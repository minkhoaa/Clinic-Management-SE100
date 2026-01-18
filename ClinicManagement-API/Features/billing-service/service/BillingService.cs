using System.Globalization;
using ClinicManagement_API.Contracts;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.billing_service.dto;
using ClinicManagement_API.Features.billing_service.helper;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicManagement_API.Features.billing_service.service;

public interface IBillingService
{
    Task<IResult> GetBillsAsync(BillStatus? status, Guid? clinicId);
    Task<IResult> GetBillDetailAsync(Guid id);
    Task<IResult> CreateBillAsync(CreateBillRequest request);
    Task<IResult> PayBillAsync(Guid id, PayBillRequest request);
    Task<IResult> CancelBillAsync(Guid id);
    Task<IResult> GetBillingStatsAsync(string? date, Guid? clinicId);

    //Vn Pay api
    Task<IResult> CreatePaymentUrlAsync(CreatePaymentUrlRequest request);
    Task<IResult> ReturnUrlAsync(Dictionary<string, string> vnpParams);
    Task<IResult> IpnUrlAsync(Dictionary<string, string> vnpParams);
}

public class BillingService : IBillingService
{
    private readonly ClinicDbContext _context;
    private readonly VnPayOptions _vnPayOptions;

    public BillingService(ClinicDbContext context, IOptions<VnPayOptions> options)
    {
        _context = context;
        _vnPayOptions = options.Value;
    }

    public async Task<IResult> GetBillsAsync(BillStatus? status, Guid? clinicId)
    {
        var bills = _context.Bills.AsNoTracking()
            .Include(k => k.Patient)
            .Include(k => k.BillItems)
            .ThenInclude(k => k.Service)
            .Include(k => k.BillItems)
            .ThenInclude(k => k.Medicine)
            .AsQueryable();
        if (status.HasValue)
            bills = bills.Where(k => k.Status == status);
        if (clinicId.HasValue) bills = bills.Where(k => k.ClinicId == clinicId);
        var result = await bills.OrderByDescending(k => k.PaymentDate)
            .Select(k => new BillListResponseDto(
                k.BillId, k.Patient.FullName,
                k.Patient.PrimaryPhone ?? k.Patient.EmergencyPhone ?? "",
                k.BillItems.Select(a => a.Service != null ? a.Service.Name :
                    a.Medicine != null ? a.Medicine.Name : "Unknown").ToList(),
                k.TotalAmount,
                k.CreatedAt,
                k.Status
            )).ToListAsync();
        return Results.Ok(new ApiResponse<List<BillListResponseDto>>(true, "Get bills successfully", result));
    }

    public async Task<IResult> GetBillDetailAsync(Guid id)
    {
        var bill = await _context.Bills.AsNoTracking()
            .Include(b => b.Patient)
            .Include(b => b.BillItems)
            .ThenInclude(bi => bi.Service)
            .Include(b => b.BillItems)
            .ThenInclude(bi => bi.Medicine)
            .Include(b => b.Appointment)
            .ThenInclude(a => a!.Doctor)
            .FirstOrDefaultAsync(b => b.BillId == id);

        if (bill == null)
            return Results.NotFound(new ApiResponse<object>(false, "Bill not found", null));

        var result = new BillDetailDto(
            Id: bill.BillId,
            InvoiceNumber: bill.InvoiceNumber,
            CreatedAt: bill.CreatedAt,
            PaymentDate: bill.PaymentDate,
            Status: bill.Status,
            Patient: new PatientInfoDto(
                Id: bill.Patient.PatientId,
                Name: bill.Patient.FullName,
                Phone: bill.Patient.PrimaryPhone ?? bill.Patient.EmergencyPhone ?? "",
                Email: bill.Patient.Email,
                Address: bill.Patient.AddressLine1
            ),
            Items: bill.BillItems.Select(bi => new BillItemDto(
                Id: bi.BillItemId,
                Type: bi.Type.ToString().ToLower(),
                Name: bi.Name,
                Quantity: bi.Quantity,
                Unit: bi.Unit,
                UnitPrice: bi.UnitPrice,
                Amount: bi.Amount,
                ToothNumber: bi.ToothNumber,
                Notes: bi.Notes
            )).ToList(),
            AppointmentId: bill.AppointmentId,
            MedicalRecordId: bill.MedicalRecordId,
            Doctor: bill.Appointment?.Doctor.FullName,
            Subtotal: bill.Subtotal,
            Discount: bill.Discount,
            DiscountPercent: bill.DiscountPercent,
            InsuranceCovered: bill.InsuranceCovered,
            TotalAmount: bill.TotalAmount,
            PaymentMethod: bill.PaymentMethod,
            PaidAmount: bill.PaidAmount,
            ChangeAmount: bill.ChangeAmount,
            Notes: bill.Notes,
            CreatedBy: bill.CreatedById?.ToString()
        );

        return Results.Ok(new ApiResponse<BillDetailDto>(true, "Bill detail retrieved", result));
    }

    public async Task<IResult> CreateBillAsync(CreateBillRequest request)
    {
        if (!request.Items.Any())
            return Results.BadRequest(new ApiResponse<object>(false, "Items cannot be empty", null));
        var clinicExists = await _context.Clinics.AnyAsync(c => c.ClinicId == request.ClinicId);
        if (!clinicExists)
            return Results.NotFound(new ApiResponse<object>(false, "Clinic not found", null));
        var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == request.PatientId);
        if (!patientExists)
            return Results.NotFound(new ApiResponse<object>(false, "Patient not found", null));

        // Validate stock for medicine items
        var medicineItems = request.Items.Where(i => i.Type == BillItemType.Medicine && i.MedicineId.HasValue).ToList();
        if (medicineItems.Any())
        {
            var medicineIds = medicineItems.Select(i => i.MedicineId!.Value).Distinct().ToList();
            var medicines = await _context.Medicines
                .Where(m => medicineIds.Contains(m.MedicineId))
                .ToDictionaryAsync(m => m.MedicineId);

            foreach (var item in medicineItems)
            {
                if (!medicines.TryGetValue(item.MedicineId!.Value, out var medicine))
                    return Results.NotFound(new ApiResponse<object>(false, $"Medicine not found: {item.Name}", null));

                if (medicine.StockQuantity < item.Quantity)
                    return Results.BadRequest(new ApiResponse<object>(false,
                        $"Thuốc '{medicine.Name}' không đủ tồn kho. Còn: {medicine.StockQuantity}, Yêu cầu: {item.Quantity}",
                        null));
            }
        }

        var invoiceNumber = await GenerateInvoiceNumber(request.ClinicId);
        var subtotal = request.Items.Sum(i => i.UnitPrice * i.Quantity);
        var discount = request.Discount ?? 0;
        if (request.DiscountPercent.HasValue)
            discount = subtotal * request.DiscountPercent.Value / 100;
        var totalAmount = subtotal - discount;
        var bill = new Bill
        {
            BillId = Guid.NewGuid(),
            ClinicId = request.ClinicId,
            PatientId = request.PatientId,
            AppointmentId = request.AppointmentId,
            InvoiceNumber = invoiceNumber,
            Status = BillStatus.Pending,
            Subtotal = subtotal,
            Discount = discount,
            DiscountPercent = request.DiscountPercent,
            TotalAmount = totalAmount,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _context.Bills.AddAsync(bill);
        var items = request.Items.Select(i => new BillItem
        {
            BillItemId = Guid.NewGuid(),
            BillId = bill.BillId,
            Type = i.Type,
            Name = i.Name,
            Quantity = i.Quantity,
            Unit = i.Unit,
            UnitPrice = i.UnitPrice,
            Amount = i.UnitPrice * i.Quantity,
            ToothNumber = i.ToothNumber,
            Notes = i.Notes,
        });
        await _context.BillItems.AddRangeAsync(items);

        await _context.SaveChangesAsync();
        return Results.Ok(new ApiResponse<object>(true, "Bill created successfully", null));
    }

    public async Task<IResult> PayBillAsync(Guid id, PayBillRequest request)
    {
        var bill = await _context.Bills.FirstOrDefaultAsync(b => b.BillId == id);
        if (bill == null)
            return Results.NotFound(new ApiResponse<object>(false, "Bill not found", null));
        if (bill.Status != BillStatus.Pending)
            return Results.BadRequest(new ApiResponse<object>(false, "Bill is not pending", null));
        if (request.Amount < bill.TotalAmount)
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Paid amount is less than total amount", null));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            bill.Status = BillStatus.Paid;
            bill.PaymentDate = DateTime.UtcNow;
            bill.PaidAmount = request.Amount;
            bill.ChangeAmount = request.Amount - bill.TotalAmount;
            bill.PaymentMethod = request.PaymentMethod;
            bill.Notes = request.Notes;

            // Deduct stock for medicine items
            var billItems = await _context.BillItems
                .Where(bi => bi.BillId == id && bi.MedicineId.HasValue)
                .ToListAsync();

            if (billItems.Any())
            {
                var medicineIds = billItems.Select(bi => bi.MedicineId!.Value).Distinct().ToList();
                var medicines = await _context.Medicines
                    .Where(m => medicineIds.Contains(m.MedicineId))
                    .ToListAsync();

                foreach (var item in billItems)
                {
                    var medicine = medicines.FirstOrDefault(m => m.MedicineId == item.MedicineId);
                    if (medicine != null)
                    {
                        medicine.StockQuantity -= item.Quantity;
                        medicine.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Results.Ok(new ApiResponse<object>(true, "Bill paid successfully", null));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IResult> CancelBillAsync(Guid id)
    {
        var bill = await _context.Bills.FindAsync(id);
        if (bill == null)
            return Results.NotFound(new ApiResponse<object>(false, "Bill not found", null));
        if (bill.Status != BillStatus.Pending)
            return Results.BadRequest(new ApiResponse<object>(false, "Bill is not pending", null));
        bill.Status = BillStatus.Cancelled;
        bill.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Results.Ok(new ApiResponse<object>(true, "Bill cancelled successfully", null));
    }

    public async Task<IResult> GetBillingStatsAsync(string? date, Guid? clinicId)
    {
        var targetDate = DateTime.UtcNow.Date;
        if (!string.IsNullOrEmpty(date))
        {
            targetDate = DateTime.ParseExact(date, "yyyy-MM-dd", null);
        }

        var nextDay = targetDate.AddDays(1);
        var query = _context.Bills.AsQueryable();
        if (clinicId.HasValue)
            query = query.Where(b => b.ClinicId == clinicId.Value);

        var bills = await query.Where(b => b.CreatedAt >= targetDate && b.CreatedAt < nextDay).ToListAsync();

        var totalPending = bills.Where(k => k.Status == BillStatus.Pending).Sum(k => k.TotalAmount);
        var totalPaid = bills.Where(k => k.Status == BillStatus.Paid).Sum(k => k.TotalAmount);
        var totalCancelled = bills.Where(k => k.Status == BillStatus.Cancelled).Sum(k => k.TotalAmount);
        var totalRefunded = bills.Where(k => k.Status == BillStatus.Refunded).Sum(k => k.TotalAmount);

        return Results.Ok(new ApiResponse<BillingStatsDto>(true, "Billing stats retrieved successfully",
            new BillingStatsDto(totalPending, totalPaid, totalCancelled, totalRefunded)));
    }

    public async Task<IResult> CreatePaymentUrlAsync(CreatePaymentUrlRequest request)
    {
        var bill = await _context.Bills.FindAsync(request.BillId);
        if (bill == null)
            return Results.NotFound("Bill information not found");
        if (bill.Status != BillStatus.Pending)
            return Results.BadRequest(new ApiResponse<object>(false, "Bill already paid", null));


        var vnTimezone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimezone);

        // Create unique TxnRef with timestamp to avoid duplicate transactions
        var txnRef = $"{bill.BillId}_{vnNow:HHmmss}";

        var vnpayParams = new SortedDictionary<string, string>()
        {
            { "vnp_Version", _vnPayOptions.Version },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", _vnPayOptions.TmnCode },
            { "vnp_Amount", ((long)(bill.TotalAmount * 100)).ToString() }, // VNPay x100
            { "vnp_CreateDate", vnNow.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", _vnPayOptions.CurrCode },
            { "vnp_IpAddr", "127.0.0.1" },
            { "vnp_Locale", _vnPayOptions.Locale },
            { "vnp_OrderInfo", $"Thanh toan hoa don {bill.InvoiceNumber}" },
            { "vnp_OrderType", "billpayment" },
            { "vnp_ReturnUrl", request.ReturnUrl ?? _vnPayOptions.ReturnUrl },
            { "vnp_TxnRef", txnRef },
            { "vnp_ExpireDate", vnNow.AddMinutes(15).ToString("yyyyMMddHHmmss") }
        };


        if (!string.IsNullOrEmpty(request.BankCode))
        {
            vnpayParams["vnp_BankCode"] = request.BankCode;
        }

        var paymentUrl = VnPayHelper.BuildPaymentUrl(_vnPayOptions.PaymentUrl, vnpayParams, _vnPayOptions.HashSecret);
        return Results.Ok(new ApiResponse<object>(true, "Payment URL created", paymentUrl));
    }

    public async Task<IResult> ReturnUrlAsync(Dictionary<string, string> vnpParams)
    {
        // Frontend URL to redirect after processing
        var frontendUrl = "http://localhost:3000/payment/result";

        // Verify signature with ALL parameters from VNPay
        if (!VnPayHelper.VerifySignature(vnpParams, _vnPayOptions.HashSecret))
            return Results.Redirect($"{frontendUrl}?success=false&error=invalid_signature");

        // Extract required values from VNPay response
        vnpParams.TryGetValue("vnp_ResponseCode", out var responseCode);
        vnpParams.TryGetValue("vnp_TxnRef", out var txnRef);
        vnpParams.TryGetValue("vnp_TransactionNo", out var transactionNo);
        vnpParams.TryGetValue("vnp_Amount", out var amountStr);
        vnpParams.TryGetValue("vnp_OrderInfo", out var orderInfo);
        vnpParams.TryGetValue("vnp_BankCode", out var bankCode);
        vnpParams.TryGetValue("vnp_PayDate", out var payDate);
        vnpParams.TryGetValue("vnp_TransactionStatus", out var transactionStatus);

        decimal.TryParse(amountStr, out var amount);
        var actualAmount = amount / 100; // VNPay sends x100

        // Extract billId from txnRef (format: billId_timestamp)
        var billIdStr = txnRef?.Split('_')[0];

        if (responseCode != "00")
        {
            var errorMessage = Uri.EscapeDataString(GetVnPayErrorMessage(responseCode ?? "99"));
            return Results.Redirect(
                $"{frontendUrl}?success=false&code={responseCode}&message={errorMessage}&billId={billIdStr}");
        }

        // Update bill status on successful payment
        if (Guid.TryParse(billIdStr, out var billId))
        {
            var bill = await _context.Bills.FindAsync(billId);
            if (bill != null && bill.Status == BillStatus.Pending)
            {
                bill.Status = BillStatus.Paid;
                bill.PaymentMethod = PaymentMethod.VnPay;
                bill.PaidAmount = actualAmount;
                bill.ChangeAmount = 0;
                bill.PaymentDate = DateTime.UtcNow;
                bill.Notes = $"VNPay Transaction: {transactionNo}";
                bill.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // Redirect to frontend with all payment info
        var redirectParams =
            $"success=true&billId={billIdStr}&transactionNo={transactionNo}&amount={actualAmount}&bankCode={bankCode}&payDate={payDate}&orderInfo={Uri.EscapeDataString(orderInfo ?? "")}";
        return Results.Redirect($"{frontendUrl}?{redirectParams}");
    }

    public async Task<IResult> IpnUrlAsync(Dictionary<string, string> vnpParams)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Verify signature with ALL parameters from VNPay
            if (!VnPayHelper.VerifySignature(vnpParams, _vnPayOptions.HashSecret))
                return Results.Json(new { RspCode = "97", Message = "Invalid signature" });

            // Extract required values
            vnpParams.TryGetValue("vnp_TxnRef", out var txnRef);
            vnpParams.TryGetValue("vnp_ResponseCode", out var responseCode);
            vnpParams.TryGetValue("vnp_TransactionNo", out var transactionNo);
            vnpParams.TryGetValue("vnp_Amount", out var amountStr);
            decimal.TryParse(amountStr, out var amount);

            // Extract billId from txnRef (format: billId_timestamp)
            var billIdStr = txnRef?.Split('_')[0];
            if (!Guid.TryParse(billIdStr, out var billId))
                return Results.Json(new { RspCode = "01", Message = "Order not found" });

            var bill = await _context.Bills.FindAsync(billId);
            if (bill == null)
                return Results.Json(new { RspCode = "01", Message = "Order not found" });
            if (bill.Status == BillStatus.Paid)
                return Results.Json(new { RspCode = "02", Message = "Order already confirmed" });
            if (responseCode != "00")
                return Results.Json(new { RspCode = "00", Message = "Confirm Success" });

            bill.Status = BillStatus.Paid;
            bill.PaymentMethod = PaymentMethod.VnPay;
            bill.PaidAmount = amount / 100; // VNPay sends x100
            bill.ChangeAmount = 0;
            bill.PaymentDate = DateTime.UtcNow;
            bill.Notes = $"VNPay IPN Transaction: {transactionNo}";
            bill.UpdatedAt = DateTime.UtcNow;

            // Deduct stock for medicine items
            var billItems = await _context.BillItems
                .Where(bi => bi.BillId == billId && bi.MedicineId.HasValue)
                .ToListAsync();

            if (billItems.Any())
            {
                var medicineIds = billItems.Select(bi => bi.MedicineId!.Value).Distinct().ToList();
                var medicines = await _context.Medicines
                    .Where(m => medicineIds.Contains(m.MedicineId))
                    .ToListAsync();

                foreach (var item in billItems)
                {
                    var medicine = medicines.FirstOrDefault(m => m.MedicineId == item.MedicineId);
                    if (medicine != null)
                    {
                        medicine.StockQuantity -= item.Quantity;
                        medicine.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Results.Json(new { RspCode = "00", Message = "Confirm Success" });
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw new Exception(e.Message);
        }
    }

    private string GetVnPayErrorMessage(string code) => code switch
    {
        "07" => "Trừ tiền thành công nhưng giao dịch bị nghi ngờ",
        "09" => "Thẻ/Tài khoản chưa đăng ký Internet Banking",
        "10" => "Xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
        "11" => "Đã hết hạn chờ thanh toán",
        "12" => "Thẻ/Tài khoản bị khóa",
        "24" => "Khách hàng hủy giao dịch",
        "51" => "Tài khoản không đủ số dư",
        "65" => "Tài khoản đã vượt quá hạn mức giao dịch trong ngày",
        "75" => "Ngân hàng thanh toán đang bảo trì",
        "79" => "Nhập sai mật khẩu thanh toán quá số lần quy định",
        _ => "Lỗi không xác định"
    };

    private async Task<string> GenerateInvoiceNumber(Guid clinicId)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Bills
            .Where(b => b.ClinicId == clinicId && b.CreatedAt.Year == year)
            .CountAsync();

        return $"HD-{year}-{(count + 1):D4}";
        // Kết quả: "HD-2026-0001", "HD-2026-0002"...
    }
}
