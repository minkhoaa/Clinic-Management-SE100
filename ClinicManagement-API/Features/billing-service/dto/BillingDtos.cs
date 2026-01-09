using ClinicManagement_API.Domains.Enums;

namespace ClinicManagement_API.Features.billing_service.dto;

// Bill list item for receptionist
public record BillListResponseDto(
    Guid Id,
    string PatientName,
    string Phone,
    List<string> Services,
    decimal TotalAmount,
    DateTime CreatedAt,
    BillStatus Status
);

// Bill detail
public record BillDetailDto(
    Guid Id,
    string InvoiceNumber,
    DateTime CreatedAt,
    DateTime? PaymentDate,
    BillStatus Status,

    // Patient info
    PatientInfoDto Patient,

    // Items
    List<BillItemDto> Items,

    // Related info
    Guid? AppointmentId,
    Guid? MedicalRecordId,
    string? Doctor,

    // Financials
    decimal Subtotal,
    decimal Discount,
    decimal? DiscountPercent,
    decimal? InsuranceCovered,
    decimal TotalAmount,

    // Payment
    PaymentMethod? PaymentMethod,
    decimal? PaidAmount,
    decimal? ChangeAmount,
    string? Notes,
    string? CreatedBy
);

public record PatientInfoDto(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    string? Address
);

public record BillItemDto(
    Guid Id,
    string Type, // "service" | "medicine" | "material"
    string Name,
    int Quantity,
    string Unit,
    decimal UnitPrice,
    decimal Amount,
    string? ToothNumber,
    string? Notes
);

// Create bill request
public record CreateBillRequest(
    Guid ClinicId,
    Guid PatientId,
    Guid? AppointmentId,
    List<CreateBillItemRequest> Items,
    decimal? Discount,
    decimal? DiscountPercent,
    string? Notes
);

public record CreateBillItemRequest(
    Guid? ServiceId,
    BillItemType Type,
    string Name,
    int Quantity,
    string Unit,
    decimal UnitPrice,
    string? ToothNumber,
    string? Notes
);

// Pay bill request
public record PayBillRequest(
    PaymentMethod PaymentMethod,
    decimal Amount,
    decimal? Discount,
    string? Notes
);

// Billing stats
public record BillingStatsDto(
    decimal TotalPending,
    decimal TotalPaid,
    decimal TotalCancelled,
    decimal TotalRefunded
);

// VNPay DTOs
public record CreatePaymentUrlRequest(
    Guid BillId,
    string? ReturnUrl,
    string? BankCode // Optional: VNPAYQR, VNBANK, INTCARD, or specific bank code
);

public record ReturnUrlRequest(
    string VnpTxnRef,
    string VnpResponseCode,
    string VnpSecureHash,
    string? VnpTransactionNo,
    decimal? VnpAmount
);

public record IpnUrlRequest(
    string VnpTxnRef,
    string VnpResponseCode,
    string VnpSecureHash,
    string? VnpTransactionNo,
    decimal? VnpAmount
);
