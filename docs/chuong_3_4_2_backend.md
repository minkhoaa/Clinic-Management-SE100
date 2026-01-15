# 3.4.2. Thiết Kế Back-End

## Mục tiêu

Hệ thống Back-End được thiết kế với các tiêu chí cốt lõi:

- **Tuân thủ kiến trúc N-Layer kết hợp Feature-based:** Áp dụng mô hình phân lớp truyền thống với sự tổ chức theo module nghiệp vụ, đảm bảo tính độc lập giữa các tính năng và dễ dàng mở rộng theo chiều ngang.

- **Bảo mật toàn diện:** Sử dụng JWT Bearer Token kết hợp ASP.NET Core Identity để xác thực, triển khai Role-based Authorization với các policy riêng biệt cho từng nhóm người dùng (Admin, Doctor, Receptionist, Patient).

- **Hiệu năng tối ưu:** Áp dụng xử lý bất đồng bộ xuyên suốt (Async/Await), kết hợp các kỹ thuật tối ưu truy vấn của Entity Framework Core như AsNoTracking, Projection và Bulk Operations.

- **Tích hợp liền mạch:** Cung cấp RESTful API theo chuẩn Minimal APIs của ASP.NET Core 8, tích hợp cổng thanh toán VNPay và dịch vụ gửi email qua FluentEmail.

---

## Kiến trúc tổng quan

Hệ thống được tổ chức theo mô hình **Modified N-Layer Architecture** với sự kết hợp của **Feature-based Organization**, bao gồm các lớp chính:

- **Domain Layer:** Chứa các Entity classes đại diện cho nghiệp vụ (Appointment, Patient, Doctor, Bill, MedicalRecord, ToothRecord) và các Enumeration types định nghĩa trạng thái (AppointmentStatus, BillStatus, PaymentMethod). Lớp này hoàn toàn độc lập với framework, chỉ chứa các POCO objects.

- **Infrastructure Layer:** Triển khai DbContext sử dụng Entity Framework Core 8 với PostgreSQL, bao gồm cấu hình relationships cho 23 bảng dữ liệu, database migrations và seeding. Ngoài ra còn chứa các external service integrations.

- **Application/Feature Layer:** Tổ chức theo các module nghiệp vụ độc lập (booking-service, billing-service, patient-service, doctor-service...). Mỗi module bao gồm đầy đủ: Service Interface và Implementation, DTO definitions, và Endpoint handlers.

- **Presentation Layer:** Triển khai Minimal APIs pattern thay cho MVC Controllers truyền thống, cấu hình Authentication/Authorization middleware, CORS policy và Swagger documentation.

---

## Cấu trúc thư mục

Cấu trúc Solution được tổ chức theo hướng Feature-based, kết hợp phân lớp theo chức năng:

- **Domains/Entities:** Chứa 20 entity classes ánh xạ trực tiếp với database tables, bao gồm các aggregate roots (Clinic, Patient, Doctor, Appointment) và các child entities (BillItem, DoctorAvailability, MedicalRecordAttachment).

- **Domains/Enums:** Chứa 6 enumeration types định nghĩa các trạng thái nghiệp vụ: AppointmentStatus (7 trạng thái từ Pending đến NoShow), BillStatus (5 trạng thái), PaymentMethod (4 phương thức), ToothStatus (14 trạng thái sơ đồ răng), AuditAction (6 loại hành động), Gender.

- **Infrastructure/Persistence:** Chứa ClinicDbContext với OnModelCreating cấu hình 446 dòng code cho relationships và constraints, kèm DbSeeder cho việc khởi tạo dữ liệu ban đầu.

- **Contracts:** Chứa ApiResponse wrapper chuẩn hóa format response cho toàn bộ API.

- **Features:** Chứa 11 module nghiệp vụ độc lập, mỗi module có cấu trúc con: dto (request/response objects), service (interface và implementation), endpoint (API route handlers), helper (utilities nếu cần).

- **Migrations:** Chứa 43 file migration quản lý schema evolution của database.

---

## Các Pattern và Kỹ thuật quan trọng

- **Dependency Injection:** Tuân thủ nghiêm ngặt Constructor Injection cho tất cả Service, Helper và DbContext. Toàn bộ 16 service được đăng ký với lifetime Scoped trong DI Container, đảm bảo nguyên tắc Dependency Inversion và hỗ trợ unit testing.

- **Interface/Service Pattern:** Mỗi nghiệp vụ được trừu tượng hóa qua Interface (IPatientService, IBillingService, IBookingEmailService...) và triển khai bởi concrete class. Pattern này thay thế Repository Pattern truyền thống vì EF Core DbContext đã tích hợp sẵn Unit of Work và DbSet đã đóng vai trò Repository.

- **DTO Mapping với Projection:** Dữ liệu trả về luôn qua DTO (sử dụng C# Record types) để ẩn giấu cấu trúc Domain. Việc mapping được thực hiện trực tiếp trong LINQ query thông qua Select projection, tối ưu hóa hiệu năng bằng cách chỉ truy vấn các trường cần thiết.

- **JWT Authentication Flow:** Khi đăng nhập thành công, hệ thống sinh JWT token chứa User ID và Roles, ký bằng HMAC-SHA256. Token có thời hạn 1 giờ. Các endpoint được bảo vệ bởi Authorization Policy theo vai trò: AdminOnly, DoctorOnly, StaffOnly, PatientOnly.

- **Minimal APIs Pattern:** Thay vì MVC Controllers, hệ thống sử dụng Minimal APIs của ASP.NET Core 8 với các extension methods MapXxxEndpoint. Pattern này giảm boilerplate code, tăng performance 10-15%, và cho phép inject service trực tiếp vào handler delegates.

- **VNPay Integration:** Tích hợp cổng thanh toán VNPay theo quy trình chuẩn với checksum verification, hỗ trợ cả ReturnUrl (redirect về frontend) và IpnUrl (server-to-server callback) để đảm bảo tính tin cậy của transaction.

- **Audit Trail:** Triển khai cơ chế ghi log tự động cho các thao tác quan trọng (Create/Update/Delete) trên MedicalRecord và Bill, lưu trữ old/new values dạng JSON, thông tin user và IP address để đảm bảo truy vết.

---

## Quy tắc phát triển

- **Dependency Injection bắt buộc:** Tất cả dependencies phải được inject qua constructor, không được khởi tạo trực tiếp bằng từ khóa new. Service nghiệp vụ dùng Scoped lifetime, utilities dùng Transient.

- **DTO thay vì Entity:** API không bao giờ trả về Entity trực tiếp. Mọi response phải qua DTO để kiểm soát dữ liệu expose ra ngoài, tránh circular reference và bảo mật thông tin nhạy cảm.

- **Standardized Response Format:** Tất cả API trả về ApiResponse wrapper gồm IsSuccess, Message và Data, giúp frontend xử lý response một cách nhất quán.

- **Async/Await xuyên suốt:** Mọi thao tác I/O (database, email, external API) phải dùng async methods. Tên method có hậu tố Async, return type là Task hoặc Task of T.

- **Role-based Authorization:** Mỗi endpoint nhạy cảm phải được bảo vệ bởi Authorization Policy phù hợp với nguyên tắc Least Privilege.

---

## Tổng kết

| Thành phần | Công nghệ |
|------------|-----------|
| Framework | .NET 8, ASP.NET Core Minimal APIs |
| Database | PostgreSQL với Entity Framework Core 8 |
| Authentication | JWT Bearer Token + ASP.NET Core Identity |
| Authorization | Role-based Policy (4 vai trò) |
| Payment Gateway | VNPay Sandbox/Production |
| Email Service | FluentEmail với SMTP |
| Architecture | Modified N-Layer + Feature-based Organization |
| API Documentation | Swagger/OpenAPI |

Kiến trúc này đảm bảo hệ thống có khả năng mở rộng linh hoạt theo module, dễ bảo trì và tuân thủ các best practices trong phát triển ứng dụng doanh nghiệp.
