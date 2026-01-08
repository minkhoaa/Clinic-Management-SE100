using ClinicManagement_API.Features.billing_service.handler;

namespace ClinicManagement_API.Features.billing_service.endpoint;

public static class BillingEndpoint
{
    public static void MapBillingEndpoint(this IEndpointRouteBuilder route)
    {
        var billing = route.MapGroup("/api/receptionist").WithTags("Billing");

        // Bills CRUD
        billing.MapGet("/bills", BillingHandler.GetBills);
        billing.MapGet("/bills/{id:guid}", BillingHandler.GetBillDetail);
        billing.MapPost("/bills", BillingHandler.CreateBill);
        billing.MapPut("/bills/{id:guid}/pay", BillingHandler.PayBill);
        billing.MapPut("/bills/{id:guid}/cancel", BillingHandler.CancelBill);

        // Stats
        billing.MapGet("/billing/stats", BillingHandler.GetBillingStats);

        var vnpay = route.MapGroup("/api/vnpay").WithTags("VnPay");
        vnpay.MapPost("/create", BillingHandler.CreatePaymentUrl);
        vnpay.MapPost("/return", BillingHandler.ReturnUrl);
        vnpay.MapPost("/ipn", BillingHandler.IpnUrl);
    }
}
