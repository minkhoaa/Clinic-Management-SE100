using System.Security.Claims;
using System.Text.Json;
using ClinicManagement_API.Contracts;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.audit_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.audit_service.service;

public interface IAuditService
{
    // Create audit log entry
    Task LogAsync(
        Guid clinicId,
        AuditEntityType entityType,
        Guid entityId,
        AuditAction action,
        ClaimsPrincipal? user,
        object? oldValues = null,
        object? newValues = null,
        string? changesSummary = null,
        string? ipAddress = null,
        string? userAgent = null
    );

    // Query audit logs
    Task<IResult> GetAuditLogsAsync(ClaimsPrincipal user, GetAuditLogsRequest request);
    Task<IResult> GetAuditLogAsync(ClaimsPrincipal user, Guid auditLogId);
    Task<IResult> GetEntityHistoryAsync(ClaimsPrincipal user, AuditEntityType entityType, Guid entityId);
}

public class AuditService : IAuditService
{
    private readonly ClinicDbContext _context;

    public AuditService(ClinicDbContext context)
    {
        _context = context;
    }

    private async Task<Guid?> GetClinicIdFromUser(ClaimsPrincipal user)
    {
        var clinicIdClaim = user.FindFirst("ClinicId")?.Value;
        if (Guid.TryParse(clinicIdClaim, out var clinicId))
            return clinicId;

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return null;

        var staff = await _context.StaffUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userGuid);

        return staff?.ClinicId;
    }

    public async Task LogAsync(
        Guid clinicId,
        AuditEntityType entityType,
        Guid entityId,
        AuditAction action,
        ClaimsPrincipal? user,
        object? oldValues = null,
        object? newValues = null,
        string? changesSummary = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        Guid? userId = null;
        string? userName = null;
        string? userRole = null;

        if (user != null)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var parsedUserId))
                userId = parsedUserId;

            userName = user.FindFirst(ClaimTypes.Name)?.Value
                       ?? user.FindFirst("name")?.Value;
            userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        }

        var auditLog = new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            ClinicId = clinicId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            UserName = userName,
            UserRole = userRole,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            ChangesSummary = changesSummary,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IResult> GetAuditLogsAsync(ClaimsPrincipal user, GetAuditLogsRequest request)
    {
        var clinicId = await GetClinicIdFromUser(user);
        if (clinicId == null)
            return Results.Unauthorized();

        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId.Value);

        // Apply filters
        if (request.EntityType.HasValue)
            query = query.Where(a => a.EntityType == request.EntityType.Value);

        if (request.EntityId.HasValue)
            query = query.Where(a => a.EntityId == request.EntityId.Value);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(a => a.CreatedAt <= request.ToDate.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto(
                a.AuditLogId,
                a.EntityType,
                GetEntityTypeName(a.EntityType),
                a.EntityId,
                a.Action,
                GetActionName(a.Action),
                a.UserName,
                a.UserRole,
                a.ChangesSummary,
                a.CreatedAt
            ))
            .ToListAsync();

        var response = new AuditLogPagedResponse(items, totalCount, request.Page, request.PageSize, totalPages);
        return Results.Ok(new ApiResponse<AuditLogPagedResponse>(true, "Audit logs retrieved", response));
    }

    public async Task<IResult> GetAuditLogAsync(ClaimsPrincipal user, Guid auditLogId)
    {
        var clinicId = await GetClinicIdFromUser(user);
        if (clinicId == null)
            return Results.Unauthorized();

        var log = await _context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AuditLogId == auditLogId && a.ClinicId == clinicId);

        if (log == null)
            return Results.NotFound(new ApiResponse<object>(false, "Audit log not found", null));

        var dto = new AuditLogDetailDto(
            log.AuditLogId,
            log.EntityType,
            GetEntityTypeName(log.EntityType),
            log.EntityId,
            log.Action,
            GetActionName(log.Action),
            log.UserId,
            log.UserName,
            log.UserRole,
            log.OldValues,
            log.NewValues,
            log.ChangesSummary,
            log.IpAddress,
            log.CreatedAt
        );

        return Results.Ok(new ApiResponse<AuditLogDetailDto>(true, "Audit log retrieved", dto));
    }

    public async Task<IResult> GetEntityHistoryAsync(ClaimsPrincipal user, AuditEntityType entityType, Guid entityId)
    {
        var clinicId = await GetClinicIdFromUser(user);
        if (clinicId == null)
            return Results.Unauthorized();

        var logs = await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId && a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AuditLogDto(
                a.AuditLogId,
                a.EntityType,
                GetEntityTypeName(a.EntityType),
                a.EntityId,
                a.Action,
                GetActionName(a.Action),
                a.UserName,
                a.UserRole,
                a.ChangesSummary,
                a.CreatedAt
            ))
            .ToListAsync();

        return Results.Ok(new ApiResponse<List<AuditLogDto>>(true, "Entity history retrieved", logs));
    }

    // Helper methods
    private static string GetEntityTypeName(AuditEntityType type)
    {
        return type switch
        {
            AuditEntityType.MedicalRecord => "Hồ sơ bệnh án",
            AuditEntityType.Prescription => "Đơn thuốc",
            AuditEntityType.Appointment => "Lịch hẹn",
            AuditEntityType.Bill => "Hóa đơn",
            AuditEntityType.Patient => "Bệnh nhân",
            AuditEntityType.ToothRecord => "Sơ đồ răng",
            _ => "Không xác định"
        };
    }

    private static string GetActionName(AuditAction action)
    {
        return action switch
        {
            AuditAction.Created => "Tạo mới",
            AuditAction.Updated => "Cập nhật",
            AuditAction.Deleted => "Xóa",
            AuditAction.Viewed => "Xem",
            AuditAction.Exported => "Xuất dữ liệu",
            AuditAction.StatusChanged => "Đổi trạng thái",
            _ => "Không xác định"
        };
    }
}
