using ClinicManagement_API.Features.billing_service.dto;

namespace ClinicManagement_API.Features.billing_service.service;

public interface IBillingService
{
    Task<IResult> GetBillsAsync(string? status, string? search, Guid? clinicId);
    Task<IResult> GetBillDetailAsync(Guid id);
    Task<IResult> CreateBillAsync(CreateBillRequest request);
    Task<IResult> PayBillAsync(Guid id, PayBillRequest request);
    Task<IResult> CancelBillAsync(Guid id);
    Task<IResult> GetBillingStatsAsync(string? date, Guid? clinicId);
}

public class BillingService : IBillingService
{
    public Task<IResult> GetBillsAsync(string? status, string? search, Guid? clinicId)
    {
        throw new NotImplementedException();
    }

    public Task<IResult> GetBillDetailAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IResult> CreateBillAsync(CreateBillRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<IResult> PayBillAsync(Guid id, PayBillRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<IResult> CancelBillAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IResult> GetBillingStatsAsync(string? date, Guid? clinicId)
    {
        throw new NotImplementedException();
    }
}
