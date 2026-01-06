using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Features.booking_service.service;
using ClinicManagement_API.Features.patient_service.dto;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement_API.Features.booking_service.handler;

public static class AdminHandler
{
    public static Task<IResult> CreateClinic(IClinicService svc, CreateClinicRequest request)
        => svc.CreateClinicAsync(request);

    public static Task<IResult> UpdateClinic(IClinicService svc, Guid clinicId, UpdateClinicRequest request)
        => svc.UpdateClinicAsync(clinicId, request);

    public static Task<IResult> DeleteClinic(IClinicService svc, Guid clinicId)
        => svc.DeleteClinicAsync(clinicId);

    public static Task<IResult> GetAllClinics(IClinicService svc)
        => svc.GetAllClinicAsync();

    public static Task<IResult> CreateDoctor(IDoctorService svc, CreateDoctorRequest request)
        => svc.CreateDoctorAsync(request);

    public static Task<IResult> UpdateDoctor(IDoctorService svc, Guid doctorId, UpdateDoctorRequest request)
        => svc.UpdateDoctorAsync(doctorId, request);

    public static Task<IResult> DeleteDoctor(IDoctorService svc, Guid doctorId)
        => svc.DeleteDoctorAsync(doctorId);
    
    public static Task<IResult> AddDoctorTimeOff(IDoctorService svc, AddDoctorTimeOffRequest request) 
        => svc.AddDoctorTimeOffAsync(request);
    
    public static Task<IResult> UpdateDoctorTimeOff( Guid timeOffId, IDoctorService svc, AddDoctorTimeOffRequest request) 
        => svc.UpdateDoctorTimeOffAsync(timeOffId, request);
    
    public static Task<IResult> DeleteDoctorTimeOff(IDoctorService svc, Guid timeOffId) 
        => svc.DeleteDoctorTimeOffAsync(timeOffId);

    public static Task<IResult> GetAllDoctors(IDoctorService svc)
        => svc.GetAllDoctorAsync();
    
    public static Task<IResult> CreateService(IServiceService svc, CreateServiceRequest request)
        => svc.CreateService(request);
    public static Task<IResult> CreateAvailability(IUserService svc, CreateDoctorAvailability request)
        => svc.CreateAvailabilityAsync(request);
    public static Task<IResult> UpdateAvailability(IUserService svc, Guid availId, UpdateDoctorAvailability request)
        => svc.UpdateAvailability(availId, request);

    public static Task<IResult> UpdateService(IServiceService svc, Guid serviceId, CreateServiceRequest request)
        => svc.UpdateService(serviceId, request);

    public static Task<IResult> DeleteService(IServiceService svc, Guid serviceId)
        => svc.DeleteService(serviceId);

    public static Task<IResult> GetAllServices(IServiceService svc)
        => svc.GetAllService();
    
    public static Task<IResult> CreateStaffUser(IAdminService svc, CreateStaffUserDto request) => svc.CreateStaffAsync(request);
    
    public static Task<IResult> UpdateStaffUser(IAdminService svc, Guid userId, CreateStaffUserDto req) => svc.UpdateStaffAsync(userId, req);


    public static Task<IResult> DeleteStaffUser(IAdminService svc, Guid userId) => svc.DeleteStaffAsync(userId);

    public static Task<IResult> CreatePatient(IAdminService svc, CreatePatientDto request) => svc.CreatePatientAsync(request);

    public static Task<IResult> UpdatePatient(IAdminService svc, Guid patientId, CreatePatientDto request) => svc.UpdatePatientAsync(patientId, request);

    public static Task<IResult> DeletePatient(IAdminService svc, Guid patientId) => svc.DeletePatientAsync(patientId);
}

