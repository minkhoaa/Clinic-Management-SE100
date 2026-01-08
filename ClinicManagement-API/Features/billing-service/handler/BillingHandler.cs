using ClinicManagement_API.Features.billing_service.dto;
using ClinicManagement_API.Features.billing_service.service;

namespace ClinicManagement_API.Features.billing_service.handler;

public static class BillingHandler
{
    public static Task<IResult> GetBills(string? status, string? search, Guid? clinicId, IBillingService svc)
        => svc.GetBillsAsync(status, search, clinicId);

    public static Task<IResult> GetBillDetail(Guid id, IBillingService svc)
        => svc.GetBillDetailAsync(id);

    public static Task<IResult> CreateBill(CreateBillRequest request, IBillingService svc)
        => svc.CreateBillAsync(request);

    public static Task<IResult> PayBill(Guid id, PayBillRequest request, IBillingService svc)
        => svc.PayBillAsync(id, request);

    public static Task<IResult> CancelBill(Guid id, IBillingService svc)
        => svc.CancelBillAsync(id);

    public static Task<IResult> GetBillingStats(string? date, Guid? clinicId, IBillingService svc)
        => svc.GetBillingStatsAsync(date, clinicId);
    
    public static Task<IResult> CreatePaymentUrl(CreatePaymentUrlRequest request, IBillingService svc)
        => svc.CreatePaymentUrlAsync(request);
    
    public static Task<IResult> ReturnUrl(ReturnUrlRequest request, IBillingService svc)
        => svc.ReturnUrlAsync(request);
    
    public static Task<IResult> IpnUrl(IpnUrlRequest request, IBillingService svc)
        => svc.IpnUrlAsync(request);
}
