# CHƯƠNG 1: TỔNG QUAN ĐỀ TÀI

## 1.1. Hiện trạng

Trong bối cảnh ngành y tế Việt Nam đang chuyển đổi số mạnh mẽ, các phòng khám tư nhân—đặc biệt là phòng khám nha khoa—vẫn đang gặp nhiều khó khăn trong công tác quản lý vận hành hàng ngày. Hiện tại, phần lớn các cơ sở nha khoa quy mô nhỏ và vừa vẫn sử dụng phương pháp thủ công hoặc các phần mềm rời rạc để quản lý thông tin bệnh nhân, lịch hẹn, hóa đơn và hồ sơ bệnh án.

Những vấn đề thực tiễn thường gặp bao gồm: (1) **Quản lý lịch hẹn thiếu đồng bộ** — bệnh nhân khó tra cứu lịch trống, dễ xảy ra trùng lịch hoặc bỏ lỡ cuộc hẹn; (2) **Hồ sơ bệnh án phân tán** — khó theo dõi lịch sử điều trị, đặc biệt với các ca điều trị nha khoa phức tạp cần theo dõi từng răng cụ thể; (3) **Thanh toán chưa linh hoạt** — thiếu tích hợp cổng thanh toán trực tuyến, gây bất tiện cho bệnh nhân; (4) **Thiếu công cụ quản lý tập trung** dành cho quản trị viên để theo dõi doanh thu, hiệu suất bác sĩ và đánh giá từ bệnh nhân.

Do đó, việc xây dựng một hệ thống quản lý phòng khám tích hợp, hiện đại là nhu cầu cấp thiết nhằm nâng cao hiệu quả vận hành và trải nghiệm bệnh nhân.

---

## 1.2. Giới thiệu

### 1.2.1. Tên đề tài

**HỆ THỐNG QUẢN LÝ PHÒNG KHÁM NHA KHOA** (Clinic Management System)

### 1.2.2. Mục tiêu

Hệ thống được xây dựng nhằm đạt các mục tiêu cốt lõi sau:

1. **Tin học hóa quy trình đặt lịch hẹn**: Cho phép bệnh nhân tự đặt lịch trực tuyến, hệ thống tự động kiểm tra slot trống của bác sĩ và gửi email xác nhận.

2. **Số hóa hồ sơ bệnh án**: Lưu trữ thông tin khám bệnh, chẩn đoán, đơn thuốc và file đính kèm (X-ray, hình ảnh) một cách có hệ thống, dễ truy xuất.

3. **Quản lý sơ đồ răng chi tiết (Dental Chart)**: Theo dõi trạng thái từng răng của bệnh nhân theo tiêu chuẩn FDI, hỗ trợ bác sĩ lập kế hoạch điều trị.

4. **Tích hợp thanh toán đa phương thức**: Hỗ trợ thanh toán tiền mặt, VNPay, thẻ và chuyển khoản, đảm bảo trải nghiệm thanh toán liền mạch.

5. **Cung cấp dashboard quản trị**: Thống kê doanh thu, số lượng bệnh nhân, hiệu suất bác sĩ và quản lý đánh giá từ bệnh nhân.

6. **Ghi nhận nhật ký thay đổi (Audit Log)**: Đảm bảo tính minh bạch và truy vết mọi thao tác sửa đổi dữ liệu quan trọng.

7. **Hỗ trợ mô hình đa phòng khám (Multi-tenant)**: Cho phép triển khai cho nhiều cơ sở phòng khám độc lập trên cùng hệ thống.

---

## 1.3. Xác định yêu cầu phần mềm

### 1.3.1. Danh sách yêu cầu chức năng

**Bảng 1.3.1: Tổng quan các nghiệp vụ hệ thống**

| STT | Nghiệp vụ | Biểu mẫu | Quy định | Ghi chú |
|:---:|:----------|:---------|:---------|:--------|
| 1 | Đăng ký / Đăng nhập tài khoản | BM1, BM2 | QĐ1 | Hỗ trợ đăng nhập bệnh nhân & nhân viên |
| 2 | Quản lý thông tin phòng khám | BM3 | QĐ2 | Admin cập nhật thông tin phòng khám |
| 3 | Quản lý bác sĩ | BM4 | QĐ3 | Thông tin bác sĩ, chuyên khoa, trạng thái |
| 4 | Quản lý dịch vụ khám | BM5 | QĐ4 | Danh mục dịch vụ, giá, thời gian |
| 5 | Quản lý lịch làm việc bác sĩ | BM6, BM7 | QĐ5 | Lịch tuần, lịch nghỉ phép |
| 6 | Quản lý thông tin bệnh nhân | BM8 | QĐ6 | Hồ sơ bệnh nhân đầy đủ |
| 7 | Đặt lịch hẹn khám | BM9 | QĐ7 | Online booking với xác nhận email |
| 8 | Check-in bệnh nhân | BM10 | QĐ8 | Lễ tân xác nhận đến khám |
| 9 | Khám bệnh & Tạo hồ sơ bệnh án | BM11, BM12 | QĐ9 | Chẩn đoán, điều trị, đơn thuốc |
| 10 | Quản lý sơ đồ răng | BM13 | QĐ10 | Dental chart theo chuẩn FDI |
| 11 | Quản lý thuốc & vật tư | BM14 | QĐ11 | Danh mục, tồn kho, hạn sử dụng |
| 12 | Quản lý đơn thuốc mẫu | BM15 | QĐ12 | Template đơn thuốc tái sử dụng |
| 13 | Tạo hóa đơn thanh toán | BM16 | QĐ13 | Chi tiết dịch vụ, thuốc, bảo hiểm |
| 14 | Thanh toán (Cash/VNPay) | BM17 | QĐ14 | Đa phương thức, tích hợp cổng TT |
| 15 | Đánh giá dịch vụ | BM18 | QĐ15 | Bệnh nhân đánh giá sau khám |
| 16 | Xem báo cáo thống kê | BM19 | QĐ16 | Dashboard admin |
| 17 | Tra cứu nhật ký hệ thống | BM20 | QĐ17 | Audit log cho admin |

---

### 1.3.2. Danh sách các biểu mẫu và quy định

---

#### **Biểu mẫu BM1 và Quy định QĐ1: Đăng ký tài khoản**

**Bảng 1.3.2.1: Biểu mẫu Đăng ký tài khoản (BM1)**

| Mã BM | Form Đăng ký Bệnh nhân |
|:------|:-----------------------|
| **Họ và tên** | Họ tên đầy đủ |
| **Email** | Địa chỉ email |
| **Số điện thoại** | Số điện thoại chính |
| **Mật khẩu** | Mật khẩu đăng nhập |
| **Xác nhận mật khẩu** | Nhập lại mật khẩu |
| **Giới tính** | Nam / Nữ / Khác |
| **Ngày sinh** | dd/mm/yyyy |

**Bảng 1.3.2.2: Biểu mẫu Đăng nhập (BM2)**

| Mã BM | Form Đăng nhập |
|:------|:---------------|
| **Email/Username** | Tên đăng nhập hoặc email |
| **Mật khẩu** | Mật khẩu tài khoản |

**QĐ1: Quy định về xác thực tài khoản**
- Email phải đúng định dạng và chưa tồn tại trong hệ thống.
- Mật khẩu tối thiểu 8 ký tự, bao gồm chữ hoa, chữ thường và số.
- Số điện thoại phải bắt đầu bằng 0 và có 10-11 chữ số.
- Hệ thống sử dụng JWT token với thời hạn 24 giờ.
- Phân quyền theo vai trò: ADMIN, DOCTOR, RECEPTIONIST, PATIENT.

---

#### **Biểu mẫu BM3 và Quy định QĐ2: Quản lý Phòng khám**

**Bảng 1.3.2.3: Biểu mẫu Thông tin Phòng khám (BM3)**

| Mã BM | Form Thông tin Phòng khám |
|:------|:--------------------------|
| **Mã phòng khám** | Mã định danh (auto/unique) |
| **Tên phòng khám** | Tên hiển thị |
| **Số điện thoại** | Hotline liên hệ |
| **Email** | Email phòng khám |
| **Múi giờ** | Asia/Ho_Chi_Minh |

**QĐ2: Quy định về quản lý phòng khám**
- Mã phòng khám (Code) là duy nhất trong toàn hệ thống.
- Chỉ Admin được phép cập nhật thông tin phòng khám.
- Múi giờ mặc định là Asia/Ho_Chi_Minh nếu không được chỉ định.

---

#### **Biểu mẫu BM4 và Quy định QĐ3: Quản lý Bác sĩ**

**Bảng 1.3.2.4: Biểu mẫu Thông tin Bác sĩ (BM4)**

| Mã BM | Form Thông tin Bác sĩ |
|:------|:----------------------|
| **Mã bác sĩ** | Mã nhân viên (unique/clinic) |
| **Họ và tên** | Họ tên đầy đủ |
| **Chuyên khoa** | Chuyên môn (Nha tổng quát, Chỉnh nha...) |
| **Số điện thoại** | SĐT liên hệ |
| **Email** | Email công việc |
| **Trạng thái** | Đang hoạt động / Tạm nghỉ |
| **Dịch vụ đảm nhận** | Danh sách dịch vụ (multi-select) |

**QĐ3: Quy định về quản lý bác sĩ**
- Mã bác sĩ là duy nhất trong phạm vi phòng khám.
- Bác sĩ bị vô hiệu hóa (IsActive = false) không được hiển thị trên giao diện đặt lịch.
- Mỗi bác sĩ phải được gán ít nhất một dịch vụ để có thể nhận lịch hẹn.

---

#### **Biểu mẫu BM5 và Quy định QĐ4: Quản lý Dịch vụ**

**Bảng 1.3.2.5: Biểu mẫu Dịch vụ khám (BM5)**

| Mã BM | Form Dịch vụ |
|:------|:-------------|
| **Mã dịch vụ** | Mã định danh (unique/clinic) |
| **Tên dịch vụ** | Tên hiển thị |
| **Thời gian mặc định** | Số phút (30, 45, 60...) |
| **Giá mặc định** | VND |
| **Trạng thái** | Hoạt động / Tạm ngưng |

**QĐ4: Quy định về dịch vụ**
- Mã dịch vụ là duy nhất trong phạm vi phòng khám.
- Giá dịch vụ phải ≥ 0 VND.
- Thời gian mặc định phải ≥ 15 phút và ≤ 480 phút.
- Dịch vụ bị tạm ngưng không được chọn khi đặt lịch mới.

---

#### **Biểu mẫu BM6, BM7 và Quy định QĐ5: Quản lý Lịch làm việc**

**Bảng 1.3.2.6: Biểu mẫu Lịch làm việc tuần (BM6)**

| Mã BM | Form Lịch làm việc |
|:------|:-------------------|
| **Bác sĩ** | Chọn bác sĩ |
| **Ngày trong tuần** | Thứ 2 - Chủ nhật |
| **Giờ bắt đầu** | HH:mm |
| **Giờ kết thúc** | HH:mm |
| **Độ dài slot** | Số phút (mặc định: 30) |
| **Áp dụng từ ngày** | dd/mm/yyyy |
| **Áp dụng đến ngày** | dd/mm/yyyy (optional) |

**Bảng 1.3.2.7: Biểu mẫu Lịch nghỉ phép (BM7)**

| Mã BM | Form Đăng ký nghỉ phép |
|:------|:-----------------------|
| **Bác sĩ** | Bác sĩ đăng ký |
| **Ngày bắt đầu** | dd/mm/yyyy HH:mm |
| **Ngày kết thúc** | dd/mm/yyyy HH:mm |
| **Lý do** | Ghi chú |

**QĐ5: Quy định về lịch làm việc**
- Giờ kết thúc phải sau giờ bắt đầu ít nhất 30 phút.
- Khi bác sĩ đăng ký nghỉ phép, hệ thống tự động kiểm tra các lịch hẹn bị ảnh hưởng.
- Hệ thống gửi email thông báo đến bệnh nhân có lịch hẹn trong thời gian nghỉ của bác sĩ.
- Slot nghỉ phép không được hiển thị trong danh sách slot trống.

---

#### **Biểu mẫu BM8 và Quy định QĐ6: Quản lý Bệnh nhân**

**Bảng 1.3.2.8: Biểu mẫu Hồ sơ Bệnh nhân (BM8)**

| Mã BM | Form Thông tin Bệnh nhân |
|:------|:-------------------------|
| **Mã bệnh nhân** | Mã hệ thống (auto) |
| **Họ và tên** | Họ tên đầy đủ |
| **Giới tính** | Nam / Nữ / Khác |
| **Ngày sinh** | dd/mm/yyyy |
| **Số điện thoại** | SĐT chính |
| **Email** | Email liên hệ |
| **Địa chỉ** | Địa chỉ thường trú |
| **Nhóm máu** | A, B, AB, O |
| **Dị ứng** | Ghi chú các dị ứng |
| **Bệnh nền** | Tiểu đường, huyết áp... |
| **Người liên hệ khẩn cấp** | Họ tên |
| **SĐT khẩn cấp** | Số điện thoại |
| **Loại bảo hiểm** | BHYT, BH tư nhân |
| **Số thẻ BHYT** | Mã thẻ bảo hiểm |

**QĐ6: Quy định về bệnh nhân**
- Mã bệnh nhân được sinh tự động theo format: BN-YYYYMMDD-XXXX.
- Mỗi tài khoản User chỉ liên kết với một hồ sơ Patient.
- Thông tin dị ứng và bệnh nền hiển thị cảnh báo khi bác sĩ mở hồ sơ khám.
- Bệnh nhân có thể tự cập nhật thông tin cá nhân (trừ mã bệnh nhân).

---

#### **Biểu mẫu BM9 và Quy định QĐ7: Đặt lịch hẹn**

**Bảng 1.3.2.9: Biểu mẫu Đặt lịch khám (BM9)**

| Mã BM | Form Đặt lịch hẹn |
|:------|:------------------|
| **Bác sĩ** | Chọn bác sĩ |
| **Dịch vụ** | Chọn dịch vụ khám |
| **Ngày khám** | dd/mm/yyyy |
| **Giờ khám** | Chọn slot trống (HH:mm) |
| **Họ tên liên hệ** | Tên người đặt |
| **SĐT liên hệ** | Số điện thoại |
| **Email liên hệ** | Email nhận xác nhận |
| **Nguồn đặt** | Web / Điện thoại / Walk-in / Giới thiệu |
| **Ghi chú** | Triệu chứng, yêu cầu đặc biệt |

**QĐ7: Quy định về đặt lịch hẹn**
- Chỉ được chọn slot còn trống trong lịch làm việc của bác sĩ.
- Không được đặt lịch trùng với thời gian nghỉ phép của bác sĩ.
- Thời gian đặt lịch phải cách hiện tại ít nhất 2 giờ.
- Trạng thái mặc định sau khi đặt: **Pending** (Chờ xác nhận).
- Hệ thống gửi email xác nhận với token hủy/đổi lịch sau khi đặt thành công.
- Token hủy/đổi lịch có hiệu lực 24 giờ.

---

#### **Biểu mẫu BM10 và Quy định QĐ8: Check-in bệnh nhân**

**Bảng 1.3.2.10: Biểu mẫu Check-in (BM10)**

| Mã BM | Form Check-in |
|:------|:--------------|
| **Mã lịch hẹn** | ID hoặc mã tra cứu |
| **Họ tên bệnh nhân** | Xác nhận thông tin |
| **Thời gian check-in** | Giờ thực tế đến |
| **Ghi chú lễ tân** | Ghi chú bổ sung |

**QĐ8: Quy định về check-in**
- Chỉ lễ tân (Receptionist) được thực hiện check-in.
- Chỉ được check-in cho lịch hẹn có trạng thái **Confirmed**.
- Sau check-in, trạng thái chuyển thành **CheckedIn**.
- Thời gian check-in được ghi nhận làm ActualStartTime.

---

#### **Biểu mẫu BM11, BM12 và Quy định QĐ9: Khám bệnh & Hồ sơ bệnh án**

**Bảng 1.3.2.11: Biểu mẫu Bắt đầu khám (BM11)**

| Mã BM | Form Bắt đầu phiên khám |
|:------|:------------------------|
| **Lịch hẹn** | Chọn lịch hẹn CheckedIn |
| **Bác sĩ khám** | Xác nhận bác sĩ |
| **Thời gian bắt đầu** | Giờ thực tế bắt đầu |

**Bảng 1.3.2.12: Biểu mẫu Hồ sơ bệnh án (BM12)**

| Mã BM | Form Hồ sơ bệnh án |
|:------|:-------------------|
| **Tiêu đề** | Tiêu đề hồ sơ (VD: "Trám răng #36") |
| **Ngày khám** | dd/mm/yyyy |
| **Chẩn đoán** | Mô tả chẩn đoán |
| **Điều trị** | Mô tả phương pháp điều trị |
| **Đơn thuốc** | Ghi chú đơn thuốc (text) |
| **Ghi chú bác sĩ** | Ghi chú bổ sung |
| **File đính kèm** | Ảnh X-ray, tài liệu |

**QĐ9: Quy định về khám bệnh**
- Chỉ bác sĩ được tạo và cập nhật hồ sơ bệnh án.
- Khi bắt đầu khám, trạng thái lịch hẹn chuyển thành **InProgress**.
- Khi hoàn thành khám, trạng thái chuyển thành **Completed**.
- Mọi thay đổi trên hồ sơ bệnh án được ghi nhận vào Audit Log.
- File đính kèm tối đa 10MB mỗi file, hỗ trợ: JPG, PNG, PDF.

---

#### **Biểu mẫu BM13 và Quy định QĐ10: Sơ đồ răng (Dental Chart)**

**Bảng 1.3.2.13: Biểu mẫu Cập nhật Sơ đồ răng (BM13)**

| Mã BM | Form Dental Chart |
|:------|:------------------|
| **Số răng (FDI)** | 11-48 (hệ thống FDI) |
| **Trạng thái mới** | Chọn từ danh sách |
| **Ghi chú** | Ghi chú chi tiết |
| **Điều trị gần nhất** | Mô tả điều trị |
| **Liên kết hồ sơ** | Hồ sơ bệnh án liên quan |

**QĐ10: Quy định về sơ đồ răng**
- Sử dụng hệ thống đánh số FDI (Federation Dentaire Internationale).
- Mỗi bệnh nhân có tối đa 32 răng vĩnh viễn (số 11-48).
- Trạng thái răng: Healthy, Cavity, Filled, Crown, Missing, Extracted, RootCanal, Implant, Bridge, Chipped, Sensitive, Gum, Orthodontic, PendingTreatment.
- Khi cập nhật trạng thái, trạng thái cũ được lưu vào PreviousStatus.
- Chỉ bác sĩ được cập nhật sơ đồ răng.

---

#### **Biểu mẫu BM14 và Quy định QĐ11: Quản lý Thuốc & Vật tư**

**Bảng 1.3.2.14: Biểu mẫu Thuốc/Vật tư (BM14)**

| Mã BM | Form Thuốc/Vật tư |
|:------|:------------------|
| **Mã thuốc** | Mã định danh |
| **Tên thuốc** | Tên đầy đủ |
| **Đơn vị** | Viên, hộp, chai... |
| **Giá bán** | VND |
| **Mô tả** | Công dụng, thành phần |
| **Số lượng tồn kho** | Số lượng hiện có |
| **Mức cảnh báo tồn** | Mức tối thiểu |
| **Hạn sử dụng** | dd/mm/yyyy |
| **Trạng thái** | Hoạt động / Ngưng bán |

**QĐ11: Quy định về thuốc và vật tư**
- Mã thuốc là duy nhất trong phạm vi phòng khám.
- Khi tồn kho ≤ mức cảnh báo, hiển thị cảnh báo trên dashboard.
- Thuốc quá hạn sử dụng không được thêm vào đơn/hóa đơn.
- Khi bán thuốc, hệ thống tự động trừ tồn kho.

---

#### **Biểu mẫu BM15 và Quy định QĐ12: Đơn thuốc mẫu**

**Bảng 1.3.2.15: Biểu mẫu Đơn thuốc mẫu (BM15)**

| Mã BM | Form Template Đơn thuốc |
|:------|:------------------------|
| **Tên mẫu** | Tên template |
| **Phân loại** | Nha khoa / Tổng quát |
| **Bác sĩ tạo** | Bác sĩ sở hữu (null = dùng chung) |
| **Ghi chú** | Hướng dẫn sử dụng |
| **Danh sách thuốc** | Thuốc, liều lượng, số lượng, hướng dẫn |

**QĐ12: Quy định về đơn thuốc mẫu**
- Template có DoctorId = null được dùng chung cho toàn phòng khám.
- Template của bác sĩ cụ thể chỉ hiển thị cho bác sĩ đó.
- Khi sử dụng template, bác sĩ có thể điều chỉnh liều lượng trước khi lưu.

---

#### **Biểu mẫu BM16, BM17 và Quy định QĐ13, QĐ14: Hóa đơn & Thanh toán**

**Bảng 1.3.2.16: Biểu mẫu Hóa đơn (BM16)**

| Mã BM | Form Hóa đơn |
|:------|:-------------|
| **Số hóa đơn** | Auto (HD-YYYY-XXXX) |
| **Bệnh nhân** | Thông tin bệnh nhân |
| **Lịch hẹn liên quan** | Mã lịch hẹn |
| **Hồ sơ bệnh án** | Mã hồ sơ liên quan |
| **Danh sách dịch vụ** | Dịch vụ, số lượng, đơn giá |
| **Danh sách thuốc** | Thuốc, số lượng, đơn giá |
| **Tổng tiền dịch vụ** | Subtotal |
| **Giảm giá** | VND hoặc % |
| **BH chi trả** | Số tiền BH cover |
| **Tổng phải trả** | Total Amount |
| **Ghi chú** | Ghi chú hóa đơn |

**Bảng 1.3.2.17: Biểu mẫu Thanh toán (BM17)**

| Mã BM | Form Thanh toán |
|:------|:----------------|
| **Số tiền cần thanh toán** | Hiển thị tổng |
| **Phương thức** | Tiền mặt / VNPay / Thẻ / Chuyển khoản |
| **Số tiền nhận** | Tiền khách đưa (nếu cash) |
| **Tiền thừa** | Tự động tính |

**QĐ13: Quy định về hóa đơn**
- Số hóa đơn được sinh tự động theo format: HD-YYYY-XXXX (XXXX tăng dần trong năm).
- Hóa đơn chỉ được tạo sau khi lịch hẹn ở trạng thái **Completed**.
- Giảm giá phần trăm tối đa 100%.
- TotalAmount = Subtotal - Discount - InsuranceCovered.

**QĐ14: Quy định về thanh toán**
- Hỗ trợ 4 phương thức: Cash, VNPay, Card, Transfer.
- Với VNPay: Chuyển hướng đến cổng thanh toán, callback cập nhật trạng thái.
- Với Cash: Số tiền nhận phải ≥ tổng phải trả; tiền thừa tính tự động.
- Sau thanh toán thành công, trạng thái hóa đơn chuyển thành **Paid**.
- Hỗ trợ thanh toán một phần (PartiallyPaid).

---

#### **Biểu mẫu BM18 và Quy định QĐ15: Đánh giá dịch vụ**

**Bảng 1.3.2.18: Biểu mẫu Đánh giá (BM18)**

| Mã BM | Form Đánh giá |
|:------|:--------------|
| **Lịch hẹn** | Mã lịch hẹn đã hoàn thành |
| **Số sao** | 1-5 sao |
| **Nhận xét** | Nội dung đánh giá |

**QĐ15: Quy định về đánh giá**
- Chỉ bệnh nhân đã hoàn thành lịch hẹn mới được đánh giá.
- Mỗi lịch hẹn chỉ được đánh giá 1 lần.
- Đánh giá sau khi gửi không được chỉnh sửa.
- Admin có quyền ẩn/xóa đánh giá vi phạm.

---

#### **Biểu mẫu BM19 và Quy định QĐ16: Báo cáo thống kê**

**Bảng 1.3.2.19: Biểu mẫu Báo cáo (BM19)**

| Mã BM | Dashboard Admin |
|:------|:----------------|
| **Khoảng thời gian** | From - To (dd/mm/yyyy) |
| **Loại báo cáo** | Doanh thu / Bệnh nhân / Lịch hẹn |
| **Thống kê tổng hợp** | Tổng doanh thu, số bệnh nhân mới, số lịch hẹn |
| **Biểu đồ doanh thu** | Line chart theo tháng/quý |
| **Top dịch vụ** | Dịch vụ phổ biến nhất |
| **Đánh giá trung bình** | Điểm trung bình sao |

**QĐ16: Quy định về báo cáo**
- Chỉ Admin được truy cập dashboard thống kê.
- Dữ liệu được tổng hợp theo múi giờ của phòng khám.
- Báo cáo có thể xuất ra định dạng CSV/Excel.
- Dữ liệu được cache và cập nhật mỗi 15 phút.

---

#### **Biểu mẫu BM20 và Quy định QĐ17: Nhật ký hệ thống**

**Bảng 1.3.2.20: Biểu mẫu Tra cứu Audit Log (BM20)**

| Mã BM | Form Tra cứu Audit Log |
|:------|:-----------------------|
| **Loại đối tượng** | MedicalRecord / Bill / Appointment... |
| **Hành động** | Created / Updated / Deleted / Viewed |
| **Người thực hiện** | Username hoặc UserId |
| **Khoảng thời gian** | From - To |
| **Từ khóa** | Tìm kiếm trong ChangesSummary |

**QĐ17: Quy định về nhật ký**
- Tự động ghi log khi có thay đổi trên: MedicalRecord, Bill, Appointment, Patient.
- Lưu trữ giá trị cũ (OldValues) và giá trị mới (NewValues) dạng JSON.
- Ghi nhận IP Address và User Agent của người thực hiện.
- Chỉ Admin được tra cứu Audit Log.
- Log không được xóa hoặc sửa đổi.

---

### 1.3.3. Danh sách yêu cầu phi chức năng

#### **Bảng 1.3.2: Yêu cầu về tính bảo mật & phân quyền**

| STT | Nghiệp vụ | Admin | Doctor | Receptionist | Patient | Hệ thống |
|:---:|:----------|:-----:|:------:|:------------:|:-------:|:--------:|
| 1 | Quản lý phòng khám | X | - | - | - | - |
| 2 | Quản lý bác sĩ | X | - | - | - | - |
| 3 | Quản lý dịch vụ | X | - | - | - | - |
| 4 | Quản lý lịch làm việc | X | Xem/Sửa của mình | - | - | - |
| 5 | Quản lý thuốc/vật tư | X | Xem | - | - | - |
| 6 | Xem thông tin bệnh nhân | X | X | X | Chỉ bản thân | - |
| 7 | Đặt lịch hẹn | X | - | X | X | - |
| 8 | Check-in bệnh nhân | - | - | X | - | - |
| 9 | Tạo hồ sơ bệnh án | - | X | - | - | - |
| 10 | Cập nhật sơ đồ răng | - | X | - | - | - |
| 11 | Tạo hóa đơn | X | X | X | - | - |
| 12 | Thanh toán | - | - | X | X (VNPay) | X |
| 13 | Đánh giá dịch vụ | - | - | - | X | - |
| 14 | Xem báo cáo thống kê | X | - | - | - | - |
| 15 | Tra cứu Audit Log | X | - | - | - | - |
| 16 | Gửi email tự động | - | - | - | - | X |

---

#### **Bảng 1.3.3: Yêu cầu về tính logic**

| STT | Nghiệp vụ | Đối tượng | Ghi chú (Business Rules) |
|:---:|:----------|:----------|:-------------------------|
| 1 | Đặt lịch hẹn | Appointment | Không được đặt slot đã có người, không được đặt vào lịch nghỉ bác sĩ |
| 2 | Hủy lịch hẹn | Appointment | Chỉ được hủy khi trạng thái là Pending hoặc Confirmed, không được hủy khi đang khám |
| 3 | Check-in | Appointment | Chỉ check-in cho lịch Confirmed, không check-in lịch đã hủy |
| 4 | Bắt đầu khám | Appointment | Chỉ bắt đầu khi đã CheckedIn |
| 5 | Hoàn thành khám | MedicalRecord | Phải có ít nhất Diagnosis hoặc Treatment mới được lưu |
| 6 | Tạo hóa đơn | Bill | Chỉ tạo sau khi lịch hẹn Completed |
| 7 | Thanh toán | Bill | Số tiền thanh toán không được vượt quá tổng còn nợ |
| 8 | Hoàn tiền | Bill | Chỉ hoàn tiền hóa đơn đã Paid, không hoàn tiền đã Cancelled |
| 9 | Đánh giá | Review | Chỉ đánh giá 1 lần/lịch hẹn, chỉ khi lịch hẹn Completed |
| 10 | Xóa bác sĩ | Doctor | Không xóa bác sĩ có lịch hẹn trong tương lai |
| 11 | Xóa dịch vụ | Service | Không xóa dịch vụ đang được sử dụng trong lịch hẹn |
| 12 | Xuất thuốc | Medicine | Không xuất thuốc đã hết hạn hoặc hết tồn kho |

---

#### **Bảng 1.3.4: Yêu cầu về tính hiệu quả**

| STT | Nghiệp vụ | Thời gian đáp ứng |
|:---:|:----------|:------------------|
| 1 | Đăng nhập hệ thống | < 1 giây |
| 2 | Tải danh sách slot trống | < 500ms |
| 3 | Đặt lịch hẹn | < 2 giây |
| 4 | Tra cứu thông tin bệnh nhân | < 500ms |
| 5 | Tải hồ sơ bệnh án | < 1 giây |
| 6 | Hiển thị sơ đồ răng | < 500ms |
| 7 | Tạo hóa đơn | < 1 giây |
| 8 | Redirect đến VNPay | < 3 giây |
| 9 | Callback VNPay | < 2 giây |
| 10 | Tải dashboard thống kê | < 3 giây |
| 11 | Tra cứu Audit Log | < 2 giây |
| 12 | Gửi email xác nhận | < 5 giây (async) |

---

#### **Bảng 1.3.5: Yêu cầu về tính tiện dụng**

| STT | Nghiệp vụ | Mức độ dễ học | Mức độ dễ sử dụng | Ghi chú |
|:---:|:----------|:-------------:|:-----------------:|:--------|
| 1 | Đặt lịch hẹn (Bệnh nhân) | Dễ | Rất dễ | Quy trình 3 bước đơn giản |
| 2 | Check-in (Lễ tân) | Trung bình | Dễ | Cần đào tạo 30 phút |
| 3 | Khám bệnh (Bác sĩ) | Trung bình | Trung bình | Cần đào tạo 2 giờ |
| 4 | Cập nhật Dental Chart | Trung bình | Dễ | Giao diện trực quan, click chọn răng |
| 5 | Tạo hóa đơn | Dễ | Dễ | Tự động điền thông tin từ lịch hẹn |
| 6 | Thanh toán VNPay | Dễ | Rất dễ | Chuyển hướng tự động |
| 7 | Xem báo cáo | Trung bình | Dễ | Dashboard trực quan với biểu đồ |
| 8 | Tra cứu Audit Log | Khó | Trung bình | Yêu cầu hiểu cấu trúc dữ liệu |
| 9 | Quản lý lịch làm việc | Trung bình | Trung bình | Giao diện calendar drag-drop |
| 10 | Tìm kiếm bệnh nhân | Dễ | Rất dễ | Tìm kiếm theo tên, SĐT, mã BN |

---

*Hết Chương 1.*
