-- Clinic Management Sample Data
-- Run this script against your PostgreSQL database

-- 1. Insert Clinics
INSERT INTO "Clinics" ("ClinicId", "Code", "Name", "TimeZone", "Phone", "Email")
VALUES 
    ('a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'NKH-HCM', 'Nha Khoa Hoàng Gia - HCM', 'Asia/Ho_Chi_Minh', '028-1234-5678', 'hcm@hoanggia.vn'),
    ('b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e', 'NKH-HN', 'Nha Khoa Hoàng Gia - Hà Nội', 'Asia/Ho_Chi_Minh', '024-9876-5432', 'hanoi@hoanggia.vn')
ON CONFLICT ("ClinicId") DO NOTHING;

-- 2. Insert Doctors
INSERT INTO "Doctors" ("DoctorId", "ClinicId", "Code", "FullName", "Specialty", "Phone", "Email", "IsActive")
VALUES 
    ('d1111111-1111-1111-1111-111111111111', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'DR001', 'BS. Nguyễn Văn An', 'Nha khoa tổng quát', '0901234567', 'an.nguyen@hoanggia.vn', true),
    ('d2222222-2222-2222-2222-222222222222', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'DR002', 'BS. Trần Thị Bình', 'Chỉnh nha', '0907654321', 'binh.tran@hoanggia.vn', true),
    ('d3333333-3333-3333-3333-333333333333', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'DR003', 'BS. Lê Minh Cường', 'Phẫu thuật miệng', '0909876543', 'cuong.le@hoanggia.vn', true),
    ('d4444444-4444-4444-4444-444444444444', 'b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e', 'DR004', 'BS. Phạm Thị Dung', 'Nha khoa thẩm mỹ', '0903456789', 'dung.pham@hoanggia.vn', true)
ON CONFLICT ("DoctorId") DO NOTHING;

-- 3. Insert Services
INSERT INTO "Services" ("ServiceId", "ClinicId", "Code", "Name", "DefaultDurationMin", "DefaultPrice", "IsActive")
VALUES 
    ('s1111111-1111-1111-1111-111111111111', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'SV001', 'Khám tổng quát', 30, 200000, true),
    ('s2222222-2222-2222-2222-222222222222', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'SV002', 'Trám răng', 45, 350000, true),
    ('s3333333-3333-3333-3333-333333333333', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'SV003', 'Nhổ răng', 60, 500000, true),
    ('s4444444-4444-4444-4444-444444444444', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'SV004', 'Tẩy trắng răng', 90, 1500000, true),
    ('s5555555-5555-5555-5555-555555555555', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'SV005', 'Niềng răng', 60, 25000000, true),
    ('s6666666-6666-6666-6666-666666666666', 'b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e', 'SV006', 'Cấy ghép Implant', 120, 15000000, true)
ON CONFLICT ("ServiceId") DO NOTHING;

-- 4. Insert DoctorServices (link doctors to services)
INSERT INTO "DoctorServices" ("DoctorId", "ServiceId", "IsEnabled")
VALUES 
    ('d1111111-1111-1111-1111-111111111111', 's1111111-1111-1111-1111-111111111111', true),
    ('d1111111-1111-1111-1111-111111111111', 's2222222-2222-2222-2222-222222222222', true),
    ('d1111111-1111-1111-1111-111111111111', 's3333333-3333-3333-3333-333333333333', true),
    ('d2222222-2222-2222-2222-222222222222', 's1111111-1111-1111-1111-111111111111', true),
    ('d2222222-2222-2222-2222-222222222222', 's5555555-5555-5555-5555-555555555555', true),
    ('d3333333-3333-3333-3333-333333333333', 's3333333-3333-3333-3333-333333333333', true),
    ('d3333333-3333-3333-3333-333333333333', 's4444444-4444-4444-4444-444444444444', true)
ON CONFLICT ("DoctorId", "ServiceId") DO NOTHING;

-- 5. Insert DoctorAvailability (weekly schedule)
INSERT INTO "DoctorAvailability" ("AvailabilityId", "ClinicId", "DoctorId", "DayOfWeek", "StartTime", "EndTime", "SlotSizeMin", "IsActive", "EffectiveFrom", "EffectiveTo")
VALUES 
    -- Dr. An: Mon-Fri 8:00-12:00 and 13:30-17:30
    ('a1111111-1111-1111-1111-111111111111', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 1, '08:00:00', '12:00:00', 30, true, NULL, NULL),
    ('a1111111-1111-1111-1111-111111111112', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 1, '13:30:00', '17:30:00', 30, true, NULL, NULL),
    ('a1111111-1111-1111-1111-111111111113', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 2, '08:00:00', '12:00:00', 30, true, NULL, NULL),
    ('a1111111-1111-1111-1111-111111111114', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 2, '13:30:00', '17:30:00', 30, true, NULL, NULL),
    ('a1111111-1111-1111-1111-111111111115', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 3, '08:00:00', '12:00:00', 30, true, NULL, NULL),
    ('a1111111-1111-1111-1111-111111111116', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 3, '13:30:00', '17:30:00', 30, true, NULL, NULL),
    ('a1111111-1111-1111-1111-111111111117', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 4, '08:00:00', '12:00:00', 30, true, NULL, NULL),
    ('a1111111-1111-1111-1111-111111111118', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 4, '13:30:00', '17:30:00', 30, true, NULL, NULL),
    ('a1111111-1111-1111-1111-111111111119', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 5, '08:00:00', '12:00:00', 30, true, NULL, NULL),
    ('a1111111-1111-1111-1111-111111111120', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 5, '13:30:00', '17:30:00', 30, true, NULL, NULL),
    -- Dr. Bình: Tue, Thu, Sat 9:00-17:00
    ('a2222222-2222-2222-2222-222222222221', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd2222222-2222-2222-2222-222222222222', 2, '09:00:00', '17:00:00', 45, true, NULL, NULL),
    ('a2222222-2222-2222-2222-222222222222', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd2222222-2222-2222-2222-222222222222', 4, '09:00:00', '17:00:00', 45, true, NULL, NULL),
    ('a2222222-2222-2222-2222-222222222223', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd2222222-2222-2222-2222-222222222222', 6, '09:00:00', '12:00:00', 45, true, NULL, NULL),
    -- Dr. Cường: Mon, Wed, Fri 14:00-20:00
    ('a3333333-3333-3333-3333-333333333331', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd3333333-3333-3333-3333-333333333333', 1, '14:00:00', '20:00:00', 60, true, NULL, NULL),
    ('a3333333-3333-3333-3333-333333333332', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd3333333-3333-3333-3333-333333333333', 3, '14:00:00', '20:00:00', 60, true, NULL, NULL),
    ('a3333333-3333-3333-3333-333333333333', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd3333333-3333-3333-3333-333333333333', 5, '14:00:00', '20:00:00', 60, true, NULL, NULL)
ON CONFLICT ("AvailabilityId") DO NOTHING;

-- 6. Insert Patients
INSERT INTO "Patients" ("PatientId", "ClinicId", "PatientCode", "FullName", "Gender", "Dob", "PrimaryPhone", "Email", "AddressLine1", "Note", "Allergy", "ChronicDisease", "EmergencyName", "EmergencyPhone", "BloodGroup", "InsuranceType", "InsuranceNumber", "CreatedAt", "UpdatedAt")
VALUES 
    ('p1111111-1111-1111-1111-111111111111', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'BN001', 'Nguyễn Thị Hoa', 1, '1990-05-15', '0912345678', 'hoa.nguyen@gmail.com', '123 Lê Lợi, Q1, TP.HCM', 'Khách hàng VIP', 'Penicillin', 'Tiểu đường type 2', 'Nguyễn Văn Minh', '0987654321', 'A+', 'BHYT', 'HS4012345678', NOW(), NOW()),
    ('p2222222-2222-2222-2222-222222222222', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'BN002', 'Trần Văn Nam', 0, '1985-10-20', '0923456789', 'nam.tran@gmail.com', '456 Nguyễn Huệ, Q1, TP.HCM', NULL, NULL, 'Cao huyết áp', 'Trần Thị Mai', '0976543210', 'O+', 'Bảo Việt', 'BV123456789', NOW(), NOW()),
    ('p3333333-3333-3333-3333-333333333333', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'BN003', 'Lê Thị Lan', 1, '1995-03-08', '0934567890', 'lan.le@gmail.com', '789 Pasteur, Q3, TP.HCM', NULL, 'Thuốc tê lidocaine', NULL, NULL, NULL, 'B+', NULL, NULL, NOW(), NOW()),
    ('p4444444-4444-4444-4444-444444444444', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'BN004', 'Phạm Quốc Tuấn', 0, '1978-12-01', '0945678901', 'tuan.pham@gmail.com', '321 CMT8, Q10, TP.HCM', 'Cần theo dõi đặc biệt', NULL, 'Tim mạch', 'Phạm Thị Hương', '0965432109', 'AB+', 'BHYT', 'HS4098765432', NOW(), NOW())
ON CONFLICT ("PatientId") DO NOTHING;

-- 7. Insert Sample Bookings (future dates - next week)
INSERT INTO "Bookings" ("BookingId", "ClinicId", "DoctorId", "ServiceId", "PatientId", "StartAt", "EndAt", "FullName", "Phone", "Email", "Notes", "Channel", "Status", "CreatedAt", "UpdatedAt")
VALUES 
    ('b1111111-1111-1111-1111-111111111111', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 's1111111-1111-1111-1111-111111111111', 'p1111111-1111-1111-1111-111111111111', 
     '2026-01-13 08:00:00+07', '2026-01-13 08:30:00+07', 'Nguyễn Thị Hoa', '0912345678', 'hoa.nguyen@gmail.com', 'Khám định kỳ', 'Web', 'Confirmed', NOW(), NOW()),
    ('b2222222-2222-2222-2222-222222222222', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 's2222222-2222-2222-2222-222222222222', 'p2222222-2222-2222-2222-222222222222', 
     '2026-01-13 09:00:00+07', '2026-01-13 09:45:00+07', 'Trần Văn Nam', '0923456789', 'nam.tran@gmail.com', 'Trám răng sâu', 'Web', 'Pending', NOW(), NOW()),
    ('b3333333-3333-3333-3333-333333333333', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd2222222-2222-2222-2222-222222222222', 's5555555-5555-5555-5555-555555555555', 'p3333333-3333-3333-3333-333333333333', 
     '2026-01-14 09:00:00+07', '2026-01-14 10:00:00+07', 'Lê Thị Lan', '0934567890', 'lan.le@gmail.com', 'Tư vấn niềng răng', 'App', 'Pending', NOW(), NOW()),
    ('b4444444-4444-4444-4444-444444444444', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd3333333-3333-3333-3333-333333333333', 's3333333-3333-3333-3333-333333333333', 'p4444444-4444-4444-4444-444444444444', 
     '2026-01-15 14:00:00+07', '2026-01-15 15:00:00+07', 'Phạm Quốc Tuấn', '0945678901', 'tuan.pham@gmail.com', 'Nhổ răng khôn', 'Hotline', 'Pending', NOW(), NOW())
ON CONFLICT ("BookingId") DO NOTHING;

-- 8. Insert BookingTokens for the bookings
INSERT INTO "BookingTokens" ("BookingId", "Action", "Token", "ExpiresAt")
VALUES 
    ('b1111111-1111-1111-1111-111111111111', 'Cancel', 'cancel_token_hoa_12345678901234567890123456789012', '2026-01-13 08:00:00+07'),
    ('b1111111-1111-1111-1111-111111111111', 'Reschedule', 'reschedule_token_hoa_123456789012345678901234567890', '2026-01-13 08:00:00+07'),
    ('b2222222-2222-2222-2222-222222222222', 'Cancel', 'cancel_token_nam_12345678901234567890123456789012', '2026-01-13 09:00:00+07'),
    ('b2222222-2222-2222-2222-222222222222', 'Reschedule', 'reschedule_token_nam_123456789012345678901234567890', '2026-01-13 09:00:00+07'),
    ('b3333333-3333-3333-3333-333333333333', 'Cancel', 'cancel_token_lan_12345678901234567890123456789012', '2026-01-14 09:00:00+07'),
    ('b3333333-3333-3333-3333-333333333333', 'Reschedule', 'reschedule_token_lan_123456789012345678901234567890', '2026-01-14 09:00:00+07'),
    ('b4444444-4444-4444-4444-444444444444', 'Cancel', 'cancel_token_tuan_1234567890123456789012345678901', '2026-01-15 14:00:00+07'),
    ('b4444444-4444-4444-4444-444444444444', 'Reschedule', 'reschedule_token_tuan_12345678901234567890123456789', '2026-01-15 14:00:00+07')
ON CONFLICT ("BookingId", "Action") DO NOTHING;

-- 9. Create an Appointment for the confirmed booking
INSERT INTO "Appointments" ("AppointmentId", "ClinicId", "DoctorId", "ServiceId", "BookingId", "StartAt", "EndAt", "Source", "ContactFullName", "ContactPhone", "ContactEmail", "Status", "CreatedAt", "UpdatedAt")
VALUES 
    ('ap111111-1111-1111-1111-111111111111', 'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d', 'd1111111-1111-1111-1111-111111111111', 's1111111-1111-1111-1111-111111111111', 'b1111111-1111-1111-1111-111111111111',
     '2026-01-13 08:00:00+07', '2026-01-13 08:30:00+07', 'Web', 'Nguyễn Thị Hoa', '0912345678', 'hoa.nguyen@gmail.com', 'Confirmed', NOW(), NOW())
ON CONFLICT ("AppointmentId") DO NOTHING;

-- Done!
SELECT 'Sample data inserted successfully!' as message;
