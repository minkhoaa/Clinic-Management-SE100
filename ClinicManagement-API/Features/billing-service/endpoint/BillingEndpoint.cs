using ClinicManagement_API.Features.billing_service.handler;

namespace ClinicManagement_API.Features.billing_service.endpoint;

public static class BillingEndpoint
{
    public static void MapBillingEndpoint(this IEndpointRouteBuilder route)
    {
        var billing = route.MapGroup("/api/receptionist")
            .WithTags("Billing")
            .RequireAuthorization("ReceptionistOrAdmin");

        // Bills CRUD
        billing.MapGet("/bills", BillingHandler.GetBills);
        billing.MapGet("/bills/{id:guid}", BillingHandler.GetBillDetail);
        billing.MapPost("/bills", BillingHandler.CreateBill);
        billing.MapPut("/bills/{id:guid}/pay", BillingHandler.PayBill);
        billing.MapPut("/bills/{id:guid}/cancel", BillingHandler.CancelBill);

        // Stats
        billing.MapGet("/billing/stats", BillingHandler.GetBillingStats);

        // VNPay endpoints - no auth required as these are callbacks from VNPay
        var vnpay = route.MapGroup("/api/vnpay").WithTags("VnPay");
        vnpay.MapPost("/create", BillingHandler.CreatePaymentUrl).RequireAuthorization("ReceptionistOrAdmin");
        vnpay.MapGet("/return", BillingHandler.ReturnUrl); // No auth - VNPay redirect
        vnpay.MapGet("/ipn", BillingHandler.IpnUrl); // No auth - VNPay IPN callback
    }
}
