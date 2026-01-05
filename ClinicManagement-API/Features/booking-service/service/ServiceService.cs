using ClinicManagement_API.Contracts;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.booking_service.service
{
    public interface IServiceService
    {
        Task<IResult> CreateService(CreateServiceRequest request);

        Task<IResult> GetAllService();

        Task<IResult> UpdateService(Guid serviceId, CreateServiceRequest request);

        Task<IResult> DeleteService(Guid serviceId);
    }

    public class ServiceService : IServiceService
    {
        private readonly ClinicDbContext _context;

        public ServiceService(ClinicDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> GetAllService()
        {
            var services = await _context.Services.AsNoTracking()
                .Select(service => new ServiceDto(service.ServiceId, service.Code, service.Name, service.DefaultDurationMin, service.DefaultPrice, service.IsActive, service.ClinicId))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<ServiceDto>>(true, "Services retrieved successfully", services));
        }

        public async Task<IResult> CreateService(CreateServiceRequest request)
        {

            var service = new Service
            {
                Code = request.Code,
                Name = request.Name,
                DefaultDurationMin = request.DefaultDurationMin,
                DefaultPrice = request.DefaultPrice,
                IsActive = request.IsActive,
                ClinicId = request.ClinicId
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            var serviceDto = new ServiceDto(
                service.ServiceId,
                service.Code,
                service.Name,
                service.DefaultDurationMin,
                service.DefaultPrice,
                service.IsActive,
                service.ClinicId
            );
            return Results.Created($"/services", new ApiResponse<ServiceDto>(true, "Service created successfully", serviceDto));
        }

        public async Task<IResult> UpdateService(Guid serviceId, CreateServiceRequest request)
        {
            var affectedRows = await _context.Services.AsNoTracking().Where(x => x.ServiceId == serviceId).ExecuteUpdateAsync(x => x.SetProperty(a =>
                    a.Code, request.Code)
                .SetProperty(a => a.Name, request.Name)
                .SetProperty(a => a.DefaultDurationMin, request.DefaultDurationMin)
                .SetProperty(a => a.DefaultPrice, request.DefaultPrice)
                .SetProperty(a => a.IsActive, request.IsActive)
                .SetProperty(a => a.ClinicId, request.ClinicId)
            );
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Service updated successfully {affectedRows}", null))
                : Results.NoContent();
        }

        public async Task<IResult> DeleteService(Guid serviceId)
        {
            var affectedRows = await _context.Services.AsNoTracking().Where(x => x.ServiceId == serviceId)
                .ExecuteDeleteAsync();
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Deleted {affectedRows} row(s)", null))
                : Results.NoContent();
        }
    }
}


