namespace WilliamMetalAPI.DTOs
{
    public class PaymentDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime PaidAt { get; set; }
    }

    public class CreatePaymentDto
    {
        public decimal Amount { get; set; }
        public string Method { get; set; } = "CASH";
        public string? Note { get; set; }
    }
}
