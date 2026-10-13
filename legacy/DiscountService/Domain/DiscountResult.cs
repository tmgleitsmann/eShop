using Newtonsoft.Json;

namespace Legacy.DiscountService.Domain
{
    public class DiscountResult
    {
        [JsonProperty("customerId")]
        public string CustomerId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("discountPercent")]
        public decimal DiscountPercent { get; set; }

        [JsonProperty("discountedAmount")]
        public decimal DiscountedAmount { get; set; }

        // Pipe-delimited audit format consumed by the 2012 batch import.
        public override string ToString()
        {
            return CustomerId
                + "|" + Amount.ToString("0.00")
                + "|" + DiscountPercent.ToString("0.0")
                + "|" + DiscountedAmount.ToString("0.00");
        }
    }
}
