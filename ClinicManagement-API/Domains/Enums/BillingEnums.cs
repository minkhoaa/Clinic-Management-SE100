namespace ClinicManagement_API.Domains.Enums;

public enum BillStatus : byte
{
    Pending = 0,
    Paid = 1,
    Cancelled = 2,
    Refunded = 3
}

public enum PaymentMethod : byte
{
    Cash = 1,
    Card = 2,
    Transfer = 3,
    Insurance = 4,
    Momo = 5,
    VnPay = 6
}

public enum BillItemType : byte
{
    Service = 1,
    Medicine = 2,
    Material = 3
}
