# 📋 Billing API - Frontend Integration Guide

Tài liệu hướng dẫn Frontend sử dụng Billing API và VNPay Payment.

---

## 📌 Base URL

```
http://localhost:5000
```

---

## 🔄 Complete Payment Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  BƯỚC 1          BƯỚC 2          BƯỚC 3          BƯỚC 4          BƯỚC 5    │
│                                                                              │
│  Tạo Bill   →   Lấy Detail   →   Tạo VNPay   →   User thanh   →   Verify   │
│  (POST)         (GET)            URL (POST)      toán tại        Bill       │
│                                       │          VNPay           (GET)      │
│                                       ▼                                      │
│                                  Redirect user  ──────────────▶             │
│                                  đến VNPay                                   │
│                                                                              │
│                                       ◀────────────────────────             │
│                                  VNPay redirect về ReturnUrl                │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📝 API Details

### 1️⃣ Tạo Bill

**Request:**
```http
POST /api/receptionist/bills
Content-Type: application/json

{
  "clinicId": "cfad8f91-94e5-4ba6-bb7b-a859c810c313",
  "patientId": "9bdfb933-cec7-4fe9-8256-59902bf3c56a",
  "appointmentId": null,
  "items": [
    {
      "type": 0,
      "name": "Khám tổng quát",
      "quantity": 1,
      "unit": "lần",
      "unitPrice": 200000,
      "serviceId": null,
      "toothNumber": null,
      "notes": null
    }
  ],
  "discount": 0,
  "discountPercent": null,
  "notes": null
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Bill created successfully",
  "data": {
    "billId": "a1b2c3d4-1234-5678-9abc-def012345678",
    "invoiceNumber": "HD-2026-0001"
  }
}
```

**Lưu ý:** Lưu lại `billId` để sử dụng cho các bước tiếp theo.

---

### 2️⃣ Xem Chi Tiết Bill

**Request:**
```http
GET /api/receptionist/bills/{billId}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Bill detail retrieved",
  "data": {
    "id": "a1b2c3d4-1234-5678-9abc-def012345678",
    "invoiceNumber": "HD-2026-0001",
    "createdAt": "2026-01-09T11:00:00Z",
    "paymentDate": null,
    "status": "Pending",
    "patient": {
      "id": "9bdfb933-cec7-4fe9-8256-59902bf3c56a",
      "name": "Nguyễn Văn A",
      "phone": "0901234567",
      "email": "a@example.com",
      "address": "123 Đường ABC"
    },
    "items": [
      {
        "id": "item-id",
        "type": "service",
        "name": "Khám tổng quát",
        "quantity": 1,
        "unit": "lần",
        "unitPrice": 200000,
        "amount": 200000,
        "toothNumber": null,
        "notes": null
      }
    ],
    "subtotal": 200000,
    "discount": 0,
    "totalAmount": 200000,
    "paymentMethod": null,
    "paidAmount": null,
    "changeAmount": null
  }
}
```

---

### 3️⃣ Tạo VNPay Payment URL

**Request:**
```http
POST /api/vnpay/create
Content-Type: application/json

{
  "billId": "a1b2c3d4-1234-5678-9abc-def012345678"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Payment URL created",
  "data": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount=20000000&vnp_Command=pay&vnp_CreateDate=20260109110000&vnp_CurrCode=VND&vnp_IpAddr=127.0.0.1&vnp_Locale=vn&vnp_OrderInfo=Thanh+toan+hoa+don+HD-2026-0001&vnp_OrderType=billpayment&vnp_ReturnUrl=http%3A%2F%2Flocalhost%3A3000%2Fpayment%2Fresult&vnp_TmnCode=DEMO&vnp_TxnRef=a1b2c3d4-1234-5678-9abc-def012345678&vnp_Version=2.1.0&vnp_SecureHash=abc123..."
}
```

---

### 4️⃣ Redirect User đến VNPay

**Frontend Code:**
```javascript
const response = await fetch('/api/vnpay/create', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ billId: billId })
});

const result = await response.json();

if (result.isSuccess) {
  // Redirect user đến VNPay
  window.location.href = result.data;
  
  // ⚠️ User sẽ rời khỏi app!
  // User thanh toán tại VNPay
  // VNPay sẽ redirect về ReturnUrl
}
```

---

### 5️⃣ VNPay Redirect về ReturnUrl

Sau khi user thanh toán, VNPay redirect browser về:

```
http://localhost:3000/payment/result?
  vnp_TxnRef=a1b2c3d4-1234-5678-9abc-def012345678
  &vnp_ResponseCode=00
  &vnp_TransactionNo=14789632
  &vnp_Amount=20000000
  &vnp_SecureHash=xyz...
```

**Frontend Parse Kết Quả:**
```javascript
const params = new URLSearchParams(window.location.search);

const responseCode = params.get('vnp_ResponseCode');
const billId = params.get('vnp_TxnRef');
const transactionNo = params.get('vnp_TransactionNo');
const amount = params.get('vnp_Amount');

if (responseCode === '00') {
  console.log('✅ Thanh toán thành công!');
  console.log('Bill ID:', billId);
  console.log('Mã giao dịch:', transactionNo);
  console.log('Số tiền:', parseInt(amount) / 100, 'VND');
} else {
  console.log('❌ Thanh toán thất bại!');
  console.log('Mã lỗi:', responseCode);
}
```

---

### 6️⃣ Verify Bill Status (Optional)

**Request:**
```http
GET /api/receptionist/bills/{billId}
```

**Response (sau khi thanh toán thành công):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "a1b2c3d4-1234-5678-9abc-def012345678",
    "status": "Paid",
    "paymentDate": "2026-01-09T11:15:00Z",
    "paymentMethod": "Transfer",
    "paidAmount": 200000
  }
}
```

---

## 💵 Thanh Toán Tiền Mặt (Alternative)

**Request:**
```http
PUT /api/receptionist/bills/{billId}/pay
Content-Type: application/json

{
  "paymentMethod": "Cash",
  "amount": 200000,
  "discount": 0,
  "notes": null
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Bill paid successfully",
  "data": null
}
```

**PaymentMethod Options:**
- `"Cash"` - Tiền mặt
- `"Card"` - Thẻ
- `"Transfer"` - Chuyển khoản

---

## ❌ Huỷ Bill

**Request:**
```http
PUT /api/receptionist/bills/{billId}/cancel
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Bill cancelled successfully",
  "data": null
}
```

---

## 📊 Thống Kê Billing

**Request:**
```http
GET /api/receptionist/billing/stats?date=2026-01-09&clinicId={clinicId}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Billing stats retrieved successfully",
  "data": {
    "totalPending": 500000,
    "totalPaid": 1200000,
    "totalCancelled": 100000,
    "totalRefunded": 0
  }
}
```

---

## 📋 VNPay Response Codes

| Code | Mô tả |
|------|-------|
| `00` | ✅ Thành công |
| `07` | Trừ tiền thành công nhưng giao dịch bị nghi ngờ |
| `09` | Thẻ/Tài khoản chưa đăng ký Internet Banking |
| `10` | Xác thực thông tin thẻ không đúng quá 3 lần |
| `11` | Đã hết hạn chờ thanh toán |
| `12` | Thẻ/Tài khoản bị khóa |
| `24` | Khách hàng hủy giao dịch |
| `51` | Tài khoản không đủ số dư |
| `65` | Vượt quá hạn mức giao dịch trong ngày |
| `75` | Ngân hàng đang bảo trì |
| `79` | Nhập sai mật khẩu quá số lần quy định |

---

## 🧪 VNPay Sandbox Test

**Thông tin thẻ test:**

| Thuộc tính | Giá trị |
|------------|---------|
| Ngân hàng | NCB |
| Số thẻ | `9704198526191432198` |
| Tên chủ thẻ | `NGUYEN VAN A` |
| Ngày phát hành | `07/15` |
| OTP | `123456` |

---

## ⚙️ Frontend Route Setup

Frontend cần có route `/payment/result` để nhận redirect từ VNPay:

```javascript
// React Router
<Route path="/payment/result" element={<PaymentResult />} />

// Next.js
// pages/payment/result.tsx
```

---

## 🔄 Complete Flow Summary

```
1. POST /api/receptionist/bills          → Tạo bill, lấy billId
2. GET  /api/receptionist/bills/{id}     → Xem chi tiết (optional)
3. POST /api/vnpay/create                → Tạo payment URL
4. window.location.href = paymentUrl     → Redirect đến VNPay
5. User thanh toán tại VNPay
6. VNPay redirect về /payment/result?vnp_...
7. Parse query params → Hiển thị kết quả
8. GET  /api/receptionist/bills/{id}     → Verify status (optional)
```
