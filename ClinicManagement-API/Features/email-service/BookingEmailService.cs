using FluentEmail.Core;

namespace ClinicManagement_API.Features.email_service;

public interface IBookingEmailService
{
    Task SendBookingConfirmationAsync(BookingEmailDto booking);
}

public class BookingEmailService : IBookingEmailService
{
    private readonly IFluentEmail _email;
    private readonly string _frontendBaseUrl;

    public BookingEmailService(IFluentEmail email, IConfiguration config)
    {
        _email = email;
        _frontendBaseUrl = Environment.GetEnvironmentVariable("EMAIL__FRONTENDBASEURL")
                           ?? config["Email:FrontendBaseUrl"] ?? "http://localhost:3000";
    }

    public async Task SendBookingConfirmationAsync(BookingEmailDto booking)
    {
        var viewUrl = $"{_frontendBaseUrl}/appointment/{booking.AppointmentId}";
        var cancelUrl = $"{_frontendBaseUrl}/appointment/cancel?token={booking.CancelToken}";
        var rescheduleUrl = $"{_frontendBaseUrl}/appointment/reschedule?token={booking.RescheduleToken}";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: 'Georgia', 'Times New Roman', serif;
            line-height: 1.8;
            color: #2c3e50;
            background-color: #f5f5f5;
            margin: 0;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border: 1px solid #d4d4d4;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            background-color: #1a3a5c;
            color: #ffffff;
            padding: 30px;
            text-align: center;
            border-bottom: 4px solid #c9a227;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
            font-weight: normal;
            letter-spacing: 1px;
        }}
        .content {{
            padding: 30px;
        }}
        .greeting {{
            font-size: 16px;
            margin-bottom: 20px;
        }}
        .confirmation-box {{
            background-color: #f8f9fa;
            border: 1px solid #e9ecef;
            padding: 25px;
            margin: 25px 0;
        }}
        .confirmation-title {{
            font-size: 14px;
            text-transform: uppercase;
            letter-spacing: 2px;
            color: #6c757d;
            margin-bottom: 20px;
            border-bottom: 1px solid #dee2e6;
            padding-bottom: 10px;
        }}
        .info-table {{
            width: 100%;
            border-collapse: collapse;
        }}
        .info-table tr {{
            border-bottom: 1px solid #e9ecef;
        }}
        .info-table tr:last-child {{
            border-bottom: none;
        }}
        .info-table td {{
            padding: 12px 0;
            vertical-align: top;
        }}
        .info-table .label {{
            width: 120px;
            color: #6c757d;
            font-size: 13px;
        }}
        .info-table .value {{
            color: #2c3e50;
            font-weight: 500;
        }}
        .actions {{
            margin-top: 30px;
            text-align: center;
            padding-top: 20px;
            border-top: 1px solid #e9ecef;
        }}
        .btn {{
            display: inline-block;
            padding: 12px 30px;
            margin: 8px;
            text-decoration: none;
            font-size: 13px;
            letter-spacing: 1px;
            text-transform: uppercase;
            border: 2px solid;
            transition: all 0.3s ease;
        }}
        .btn-primary {{
            background-color: #1a3a5c;
            color: #ffffff;
            border-color: #1a3a5c;
        }}
        .btn-secondary {{
            background-color: #ffffff;
            color: #1a3a5c;
            border-color: #1a3a5c;
        }}
        .btn-outline {{
            background-color: #ffffff;
            color: #6c757d;
            border-color: #6c757d;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px 30px;
            text-align: center;
            font-size: 12px;
            color: #6c757d;
            border-top: 1px solid #e9ecef;
        }}
        .footer p {{
            margin: 5px 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{booking.ClinicName.ToUpper()}</h1>
        </div>
        
        <div class='content'>
            <p class='greeting'>Kính gửi Quý khách <strong>{booking.PatientName}</strong>,</p>
            
            <p>Chúng tôi xin xác nhận lịch hẹn khám của Quý khách đã được đặt thành công. Dưới đây là thông tin chi tiết:</p>
            
            <div class='confirmation-box'>
                <div class='confirmation-title'>Thông tin lịch hẹn</div>
                <table class='info-table'>
                    <tr>
                        <td class='label'>Phòng khám</td>
                        <td class='value'>{booking.ClinicName}</td>
                    </tr>
                    <tr>
                        <td class='label'>Bác sĩ</td>
                        <td class='value'>{booking.DoctorName}</td>
                    </tr>
                    <tr>
                        <td class='label'>Dịch vụ</td>
                        <td class='value'>{booking.ServiceName}</td>
                    </tr>
                    <tr>
                        <td class='label'>Ngày khám</td>
                        <td class='value'>{booking.StartAt:dddd, dd/MM/yyyy}</td>
                    </tr>
                    <tr>
                        <td class='label'>Giờ khám</td>
                        <td class='value'>{booking.StartAt:HH:mm} - {booking.EndAt:HH:mm}</td>
                    </tr>
                </table>
            </div>
            
            <p>Quý khách vui lòng đến trước giờ hẹn 15 phút để hoàn tất thủ tục đăng ký.</p>
            
            <div class='actions'>
                <a href='{viewUrl}' class='btn btn-primary'>Xem Chi Tiết</a>
                <a href='{rescheduleUrl}' class='btn btn-secondary'>Đổi Lịch Hẹn</a>
                <a href='{cancelUrl}' class='btn btn-outline'>Huỷ Lịch</a>
            </div>
        </div>
        
        <div class='footer'>
            <p><strong>{booking.ClinicName}</strong></p>
            <p>Email này được gửi tự động. Vui lòng không trả lời trực tiếp.</p>
            <p>Nếu cần hỗ trợ, vui lòng liên hệ trực tiếp với phòng khám.</p>
        </div>
    </div>
</body>
</html>";

        await _email
            .To(booking.Email)
            .Subject($"Xác nhận lịch hẹn khám - {booking.ClinicName}")
            .Body(htmlBody, isHtml: true)
            .SendAsync();
    }
}

public record BookingEmailDto(
    Guid AppointmentId,
    string PatientName,
    string Email,
    string ClinicName,
    string DoctorName,
    string ServiceName,
    DateTime StartAt,
    DateTime EndAt,
    string CancelToken,
    string RescheduleToken
);
