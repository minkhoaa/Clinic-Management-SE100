using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.billing_service.dto;
using ClinicManagement_API.Features.billing_service.service;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Features.billing_service.handler;

public static class BillingHandler
{
    public static Task<IResult> GetBills(string? status, Guid? clinicId, IBillingService svc)
    {
        BillStatus? billStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<BillStatus>(status, true, out var parsed))
            billStatus = parsed;
        return svc.GetBillsAsync(billStatus, clinicId);
    }

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

    // VNPay callbacks - receive ALL query string params for proper signature verification
    public static Task<IResult> ReturnUrl(HttpContext context, IBillingService svc)
    {
        // Get all query parameters as dictionary
        var allParams = context.Request.Query
            .ToDictionary(x => x.Key, x => x.Value.ToString());
        return svc.ReturnUrlAsync(allParams);
    }

    // VNPay IPN callback - receive ALL query params for proper signature verification
    public static Task<IResult> IpnUrl(HttpContext context, IBillingService svc)
    {
        var allParams = context.Request.Query
            .ToDictionary(x => x.Key, x => x.Value.ToString());
        return svc.IpnUrlAsync(allParams);
    }
}
