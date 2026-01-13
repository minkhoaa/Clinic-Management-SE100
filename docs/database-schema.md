# 📊 Database Schema Documentation
## Clinic Management System

> **Database**: PostgreSQL  
> **ORM**: Entity Framework Core 8  
> **Generated**: 2026-01-12

---

## 📋 Tổng quan các bảng

| # | Bảng | Mô tả | Số cột |
|---|------|-------|--------|
| 1 | User | Tài khoản đăng nhập (Identity) | 7 |
| 2 | Role | Vai trò hệ thống (Identity) | 3 |
| 3 | UserRole | Quan hệ User-Role | 2 |
| 4 | Clinics | Phòng khám | 8 |
| 5 | Doctors | Bác sĩ | 9 |
| 6 | Services | Dịch vụ khám/điều trị | 7 |
| 7 | DoctorServices | Liên kết Bác sĩ-Dịch vụ | 3 |
| 8 | DoctorAvailabilities | Lịch làm việc bác sĩ | 10 |
| 9 | DoctorTimeOffs | Lịch nghỉ bác sĩ | 6 |
| 10 | Patients | Bệnh nhân | 18 |
| 11 | StaffUsers | Nhân viên hệ thống | 6 |
| 12 | Appointments | Lịch hẹn khám | 16 |
| 13 | AppointmentTokens | Token xác thực lịch hẹn | 5 |
| 14 | MedicalRecords | Hồ sơ bệnh án | 12 |
| 15 | MedicalRecordAttachments | File đính kèm hồ sơ | 7 |
| 16 | Bills | Hóa đơn | 17 |
| 17 | BillItems | Chi tiết hóa đơn | 10 |
| 18 | Medicines | Thuốc & Vật tư | 11 |
| 19 | PrescriptionTemplates | Mẫu đơn thuốc | 7 |
| 20 | PrescriptionTemplateMedicines | Chi tiết mẫu đơn thuốc | 5 |
| 21 | ToothRecords | Sơ đồ răng (Dental Chart) | 12 |
| 22 | Reviews | Đánh giá từ bệnh nhân | 8 |
| 23 | AuditLogs | Nhật ký thay đổi | 13 |

---

## 1️⃣ User (Bảng Identity)
**Mô tả**: Lưu thông tin tài khoản đăng nhập hệ thống

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| Id | GUID (PK) | Mã tài khoản |
| UserName | VARCHAR(256) | Tên đăng nhập |
| NormalizedUserName | VARCHAR(256) | Tên đăng nhập (uppercase) |
| Email | VARCHAR(256) | Email |
| PasswordHash | TEXT | Mật khẩu đã hash |
| SecurityStamp | TEXT | Token bảo mật |
| PhoneNumber | VARCHAR(20) | Số điện thoại |

**Quan hệ**: 
- 1:1 với `Patients` (FK: UserId)
- N:N với `Role` qua `UserRole`

---

## 2️⃣ Role (Bảng Identity)
**Mô tả**: Danh sách vai trò hệ thống

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| Id | GUID (PK) | Mã vai trò |
| Name | VARCHAR(256) | Tên vai trò |
| NormalizedName | VARCHAR(256) | Tên vai trò (uppercase) |

**Giá trị**: `ADMIN`, `DOCTOR`, `RECEPTIONIST`, `PATIENT`

---

## 3️⃣ UserRole (Bảng trung gian)
**Mô tả**: Quan hệ N:N giữa User và Role

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| UserId | GUID (FK) | Mã tài khoản |
| RoleId | GUID (FK) | Mã vai trò |

**PK**: (UserId, RoleId)

---

## 4️⃣ Clinics (Phòng khám)
**Mô tả**: Thông tin phòng khám - đơn vị gốc của hệ thống multi-tenant

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| ClinicId | GUID (PK) | Mã phòng khám |
| Code | VARCHAR(20) | Mã định danh (unique) |
| Name | VARCHAR(200) | Tên phòng khám |
| TimeZone | VARCHAR(50) | Múi giờ (default: Asia/Ho_Chi_Minh) |
| Phone | VARCHAR(20) | Số điện thoại |
| Email | VARCHAR(256) | Email liên hệ |
| CreatedAt | TIMESTAMP | Ngày tạo |
| UpdatedAt | TIMESTAMP | Ngày cập nhật |

**Index**: UNIQUE(Code)

---

## 5️⃣ Doctors (Bác sĩ)
**Mô tả**: Thông tin bác sĩ thuộc phòng khám

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| DoctorId | GUID (PK) | Mã bác sĩ |
| ClinicId | GUID (FK) | Mã phòng khám |
| Code | VARCHAR(20) | Mã nhân viên |
| FullName | VARCHAR(150) | Họ tên |
| Specialty | VARCHAR(150) | Chuyên khoa |
| Phone | VARCHAR(20) | Số điện thoại |
| Email | VARCHAR(256) | Email |
| IsActive | BOOLEAN | Trạng thái hoạt động |

**Index**: UNIQUE(ClinicId, Code)

---

## 6️⃣ Services (Dịch vụ)
**Mô tả**: Danh mục dịch vụ khám/điều trị

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| ServiceId | GUID (PK) | Mã dịch vụ |
| ClinicId | GUID (FK) | Mã phòng khám |
| Code | VARCHAR(30) | Mã dịch vụ |
| Name | VARCHAR(200) | Tên dịch vụ |
| DefaultDurationMin | SMALLINT | Thời gian mặc định (phút) |
| DefaultPrice | DECIMAL | Giá mặc định |
| IsActive | BOOLEAN | Trạng thái |

**Index**: UNIQUE(ClinicId, Code)

---

## 7️⃣ DoctorServices (Bác sĩ - Dịch vụ)
**Mô tả**: Liên kết bác sĩ có thể thực hiện dịch vụ nào

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| ServiceId | GUID (FK) | Mã dịch vụ |
| DoctorId | GUID (FK) | Mã bác sĩ |
| IsEnabled | BOOLEAN | Đang kích hoạt |

---

## 8️⃣ DoctorAvailabilities (Lịch làm việc)
**Mô tả**: Cấu hình lịch làm việc theo ngày trong tuần

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| AvailabilityId | GUID (PK) | Mã lịch |
| ClinicId | GUID (FK) | Mã phòng khám |
| DoctorId | GUID (FK) | Mã bác sĩ |
| DayOfWeek | TINYINT | Ngày trong tuần (0=CN, 1=T2...) |
| StartTime | TIME | Giờ bắt đầu |
| EndTime | TIME | Giờ kết thúc |
| SlotSizeMin | SMALLINT | Độ dài slot (phút, default: 30) |
| EffectiveFrom | TIMESTAMP | Áp dụng từ ngày |
| EffectiveTo | TIMESTAMP | Áp dụng đến ngày |
| IsActive | BOOLEAN | Trạng thái |

---

## 9️⃣ DoctorTimeOffs (Lịch nghỉ)
**Mô tả**: Lịch nghỉ phép của bác sĩ

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| TimeOffId | GUID (PK) | Mã lịch nghỉ |
| ClinicId | GUID (FK) | Mã phòng khám |
| DoctorId | GUID (FK) | Mã bác sĩ |
| StartAt | TIMESTAMP | Bắt đầu nghỉ |
| EndAt | TIMESTAMP | Kết thúc nghỉ |
| Reason | TEXT | Lý do |

---

## 🔟 Patients (Bệnh nhân)
**Mô tả**: Hồ sơ bệnh nhân đầy đủ

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| PatientId | GUID (PK) | Mã bệnh nhân |
| ClinicId | GUID (FK) | Mã phòng khám |
| UserId | GUID (FK) | Liên kết tài khoản |
| PatientCode | VARCHAR | Mã bệnh nhân (auto) |
| FullName | VARCHAR | Họ tên |
| Gender | ENUM | Giới tính (Male/Female/X) |
| Dob | DATE | Ngày sinh |
| PrimaryPhone | VARCHAR(20) | Số điện thoại |
| Email | VARCHAR(256) | Email |
| AddressLine1 | TEXT | Địa chỉ |
| Note | TEXT | Ghi chú |
| Allergy | TEXT | Dị ứng |
| ChronicDisease | TEXT | Bệnh nền |
| EmergencyName | VARCHAR | Người liên hệ khẩn cấp |
| EmergencyPhone | VARCHAR(20) | SĐT khẩn cấp |
| BloodGroup | VARCHAR(10) | Nhóm máu |
| InsuranceType | VARCHAR | Loại bảo hiểm |
| InsuranceNumber | VARCHAR | Số thẻ BHYT |

---

## 1️⃣1️⃣ StaffUsers (Nhân viên)
**Mô tả**: Thông tin nhân viên (Admin, Lễ tân, Bác sĩ)

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| UserId | GUID (PK/FK) | Mã tài khoản |
| ClinicId | GUID (FK) | Mã phòng khám |
| Username | VARCHAR | Tên đăng nhập |
| FullName | VARCHAR | Họ tên |
| Role | VARCHAR | Vai trò (ADMIN/DOCTOR/RECEPTIONIST) |
| IsActive | BOOLEAN | Trạng thái |

---

## 1️⃣2️⃣ Appointments (Lịch hẹn)
**Mô tả**: Lịch hẹn khám bệnh

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| AppointmentId | GUID (PK) | Mã lịch hẹn |
| ClinicId | GUID (FK) | Mã phòng khám |
| DoctorId | GUID (FK) | Mã bác sĩ |
| ServiceId | GUID (FK) | Mã dịch vụ |
| PatientId | GUID (FK) | Mã bệnh nhân |
| StartAt | TIMESTAMP | Giờ bắt đầu |
| EndAt | TIMESTAMP | Giờ kết thúc |
| Source | ENUM | Nguồn (Web/Phone/WalkIn/Referral) |
| ContactFullName | VARCHAR | Tên liên hệ |
| ContactPhone | VARCHAR | SĐT liên hệ |
| ContactEmail | VARCHAR | Email liên hệ |
| Notes | TEXT | Ghi chú |
| Status | ENUM | Trạng thái |
| ActualStartTime | TIMESTAMP | Giờ khám thực tế |
| ActualEndTime | TIMESTAMP | Giờ kết thúc thực tế |

**Trạng thái (Status)**:
- `Pending` - Chờ xác nhận
- `Confirmed` - Đã xác nhận
- `CheckedIn` - Đã check-in
- `InProgress` - Đang khám
- `Completed` - Hoàn thành
- `Cancelled` - Đã hủy
- `NoShow` - Không đến

---

## 1️⃣3️⃣ AppointmentTokens
**Mô tả**: Token dùng để hủy/đổi lịch hẹn qua email

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| TokenId | GUID (PK) | Mã token |
| AppointmentId | GUID (FK) | Mã lịch hẹn |
| Action | VARCHAR | Hành động (Cancel/Reschedule) |
| Token | VARCHAR | Chuỗi token |
| ExpiresAt | TIMESTAMP | Hết hạn |

---

## 1️⃣4️⃣ MedicalRecords (Hồ sơ bệnh án)
**Mô tả**: Hồ sơ khám bệnh của bệnh nhân

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| RecordId | GUID (PK) | Mã hồ sơ |
| PatientId | GUID (FK) | Mã bệnh nhân |
| ClinicId | GUID (FK) | Mã phòng khám |
| DoctorId | GUID (FK) | Bác sĩ khám |
| AppointmentId | GUID (FK) | Lịch hẹn liên quan |
| Title | VARCHAR | Tiêu đề ("Trám răng", "Nhổ răng") |
| RecordDate | DATE | Ngày khám |
| Diagnosis | TEXT | Chẩn đoán |
| Treatment | TEXT | Điều trị |
| Prescription | TEXT | Đơn thuốc |
| Notes | TEXT | Ghi chú |
| CreatedAt | TIMESTAMP | Ngày tạo |

---

## 1️⃣5️⃣ MedicalRecordAttachments
**Mô tả**: File đính kèm (X-Ray, ảnh, tài liệu)

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| AttachmentId | GUID (PK) | Mã file |
| RecordId | GUID (FK) | Mã hồ sơ |
| FileName | VARCHAR | Tên file gốc |
| StoredFileName | VARCHAR | Tên file lưu trữ |
| ContentType | VARCHAR | MIME type |
| FileSize | BIGINT | Kích thước (bytes) |
| Description | TEXT | Mô tả |

---

## 1️⃣6️⃣ Bills (Hóa đơn)
**Mô tả**: Hóa đơn thanh toán

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| BillId | GUID (PK) | Mã hóa đơn |
| ClinicId | GUID (FK) | Mã phòng khám |
| PatientId | GUID (FK) | Mã bệnh nhân |
| AppointmentId | GUID (FK) | Lịch hẹn |
| MedicalRecordId | GUID (FK) | Hồ sơ bệnh án |
| InvoiceNumber | VARCHAR | Số hóa đơn (HD-2025-0001) |
| Status | ENUM | Trạng thái |
| Subtotal | DECIMAL | Tổng tiền dịch vụ |
| Discount | DECIMAL | Giảm giá (VND) |
| DiscountPercent | DECIMAL | % giảm giá |
| InsuranceCovered | DECIMAL | BH chi trả |
| TotalAmount | DECIMAL | Tổng phải trả |
| PaymentMethod | ENUM | Phương thức (Cash/VNPay/Card) |
| PaidAmount | DECIMAL | Số tiền đã trả |
| ChangeAmount | DECIMAL | Tiền thừa |
| Notes | TEXT | Ghi chú |
| PaymentDate | TIMESTAMP | Ngày thanh toán |

**Trạng thái (Status)**: `Pending`, `Paid`, `PartiallyPaid`, `Cancelled`, `Refunded`

---

## 1️⃣7️⃣ BillItems (Chi tiết hóa đơn)
**Mô tả**: Từng mục trong hóa đơn

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| BillItemId | GUID (PK) | Mã chi tiết |
| BillId | GUID (FK) | Mã hóa đơn |
| ServiceId | GUID (FK) | Mã dịch vụ |
| MedicineId | GUID (FK) | Mã thuốc |
| Type | ENUM | Loại (Service/Medicine/Other) |
| Name | VARCHAR | Tên |
| Quantity | INT | Số lượng |
| Unit | VARCHAR | Đơn vị (lần, viên, hộp) |
| UnitPrice | DECIMAL | Đơn giá |
| Amount | DECIMAL | Thành tiền |
| ToothNumber | VARCHAR | Số răng (nếu nha khoa) |

---

## 1️⃣8️⃣ Medicines (Thuốc & Vật tư)
**Mô tả**: Danh mục thuốc và vật tư y tế

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| MedicineId | GUID (PK) | Mã thuốc |
| ClinicId | GUID (FK) | Mã phòng khám |
| Code | VARCHAR | Mã thuốc |
| Name | VARCHAR | Tên thuốc |
| Unit | VARCHAR | Đơn vị (viên, hộp, chai) |
| Price | DECIMAL | Giá bán |
| Description | TEXT | Mô tả |
| IsActive | BOOLEAN | Trạng thái |
| **StockQuantity** | INT | 🆕 Số lượng tồn kho |
| **MinStockLevel** | INT | 🆕 Mức cảnh báo tồn kho |
| **ExpiryDate** | DATE | 🆕 Hạn sử dụng |

---

## 1️⃣9️⃣ PrescriptionTemplates (Mẫu đơn thuốc)
**Mô tả**: Mẫu đơn thuốc để tái sử dụng

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| TemplateId | GUID (PK) | Mã mẫu |
| ClinicId | GUID (FK) | Mã phòng khám |
| DoctorId | GUID (FK) | Bác sĩ tạo (null = dùng chung) |
| Name | VARCHAR | Tên mẫu |
| Category | VARCHAR | Phân loại (dental, general) |
| Notes | TEXT | Ghi chú |
| CreatedAt | TIMESTAMP | Ngày tạo |

---

## 2️⃣0️⃣ PrescriptionTemplateMedicines
**Mô tả**: Chi tiết thuốc trong mẫu đơn

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| Id | GUID (PK) | Mã chi tiết |
| TemplateId | GUID (FK) | Mã mẫu đơn |
| MedicineId | GUID (FK) | Mã thuốc |
| Dosage | VARCHAR | Liều lượng ("500mg") |
| Quantity | INT | Số lượng |
| Instructions | VARCHAR | Hướng dẫn ("3 lần/ngày sau ăn") |

---

## 2️⃣1️⃣ ToothRecords (Sơ đồ răng) 🆕
**Mô tả**: Tracking trạng thái từng răng của bệnh nhân (FDI notation)

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| ToothRecordId | GUID (PK) | Mã record |
| PatientId | GUID (FK) | Mã bệnh nhân |
| ClinicId | GUID (FK) | Mã phòng khám |
| ToothNumber | INT | Số răng (FDI: 11-48) |
| Status | ENUM | Trạng thái răng |
| PreviousStatus | ENUM | Trạng thái trước đó |
| Notes | TEXT | Ghi chú |
| LastTreatment | VARCHAR | Điều trị gần nhất |
| LastMedicalRecordId | GUID (FK) | Hồ sơ điều trị |
| LastTreatedAt | TIMESTAMP | Ngày điều trị |
| LastTreatedByDoctorId | GUID (FK) | Bác sĩ điều trị |

**Index**: UNIQUE(PatientId, ToothNumber)

**ToothStatus**: `Healthy`, `Cavity`, `Filled`, `Crown`, `Missing`, `Extracted`, `RootCanal`, `Implant`, `Bridge`, `Chipped`, `Sensitive`, `Gum`, `Orthodontic`, `PendingTreatment`

---

## 2️⃣2️⃣ Reviews (Đánh giá) 
**Mô tả**: Đánh giá từ bệnh nhân sau khám

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| ReviewId | GUID (PK) | Mã đánh giá |
| AppointmentId | GUID (FK) | Mã lịch hẹn |
| PatientId | GUID (FK) | Mã bệnh nhân |
| DoctorId | GUID (FK) | Mã bác sĩ |
| ClinicId | GUID (FK) | Mã phòng khám |
| Rating | INT | Số sao (1-5) |
| Comment | TEXT | Nhận xét |
| CreatedAt | TIMESTAMP | Ngày đánh giá |

---

## 2️⃣3️⃣ AuditLogs (Nhật ký thay đổi) 🆕
**Mô tả**: Tracking thay đổi dữ liệu quan trọng

| Cột | Kiểu dữ liệu | Mô tả |
|-----|--------------|-------|
| AuditLogId | GUID (PK) | Mã log |
| ClinicId | GUID (FK) | Mã phòng khám |
| EntityType | ENUM | Loại entity (MedicalRecord, Bill...) |
| EntityId | GUID | ID của entity |
| Action | ENUM | Hành động (Created/Updated/Deleted) |
| UserId | GUID (FK) | Người thực hiện |
| UserName | VARCHAR | Tên người thực hiện |
| UserRole | VARCHAR | Vai trò |
| OldValues | JSON | Giá trị cũ |
| NewValues | JSON | Giá trị mới |
| ChangesSummary | VARCHAR | Tóm tắt thay đổi |
| IpAddress | VARCHAR | IP address |
| UserAgent | VARCHAR | User agent |

**Indexes**: 
- INDEX(EntityType, EntityId)
- INDEX(CreatedAt)
- INDEX(UserId)

---

## 🔗 Sơ đồ quan hệ chính

```
Clinic (1) ──────┬─────< (N) Doctor
                 │
                 ├─────< (N) Service
                 │
                 ├─────< (N) Patient
                 │
                 ├─────< (N) StaffUser
                 │
                 ├─────< (N) Medicine
                 │
                 └─────< (N) Appointment

Doctor (1) ──────┬─────< (N) DoctorAvailability
                 │
                 ├─────< (N) DoctorTimeOff
                 │
                 ├─────< (N) Appointment
                 │
                 └─────< (N) MedicalRecord

Patient (1) ─────┬─────< (N) Appointment
                 │
                 ├─────< (N) MedicalRecord
                 │
                 ├─────< (N) Bill
                 │
                 ├─────< (N) ToothRecord
                 │
                 └─────< (N) Review

Appointment (1) ─┬─────< (N) AppointmentToken
                 │
                 ├─────< (N) Bill
                 │
                 └─────< (1) Review

Bill (1) ────────┴─────< (N) BillItem

MedicalRecord (1) ────< (N) MedicalRecordAttachment
```

---

## 📌 Enums Reference

### AppointmentStatus
| Value | Name | Mô tả |
|-------|------|-------|
| 0 | Pending | Chờ xác nhận |
| 1 | Confirmed | Đã xác nhận |
| 2 | CheckedIn | Đã check-in |
| 3 | InProgress | Đang khám |
| 4 | Completed | Hoàn thành |
| 5 | Cancelled | Đã hủy |
| 6 | NoShow | Không đến |

### BillStatus
| Value | Name | Mô tả |
|-------|------|-------|
| 0 | Pending | Chờ thanh toán |
| 1 | Paid | Đã thanh toán |
| 2 | PartiallyPaid | Thanh toán một phần |
| 3 | Cancelled | Đã hủy |
| 4 | Refunded | Đã hoàn tiền |

### PaymentMethod
| Value | Name | Mô tả |
|-------|------|-------|
| 0 | Cash | Tiền mặt |
| 1 | VNPay | VNPay |
| 2 | Card | Thẻ |
| 3 | Transfer | Chuyển khoản |

### ToothStatus
| Value | Name | Mô tả |
|-------|------|-------|
| 0 | Healthy | Khỏe mạnh |
| 1 | Cavity | Sâu răng |
| 2 | Filled | Đã trám |
| 3 | Crown | Bọc sứ |
| 4 | Missing | Mất răng |
| 5 | Extracted | Đã nhổ |
| 6 | RootCanal | Điều trị tủy |
| 7 | Implant | Cấy ghép |
| 8 | Bridge | Cầu răng |
| 9 | Chipped | Mẻ răng |
| 10 | Sensitive | Nhạy cảm |
| 11 | Gum | Vấn đề nướu |
| 12 | Orthodontic | Đang niềng |
| 13 | PendingTreatment | Cần điều trị |

### AuditAction
| Value | Name | Mô tả |
|-------|------|-------|
| 1 | Created | Tạo mới |
| 2 | Updated | Cập nhật |
| 3 | Deleted | Xóa |
| 4 | Viewed | Xem |
| 5 | Exported | Xuất dữ liệu |
| 6 | StatusChanged | Đổi trạng thái |
