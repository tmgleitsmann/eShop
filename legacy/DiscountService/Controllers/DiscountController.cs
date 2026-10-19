using System;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Http;
using Legacy.DiscountService.Domain;
using Newtonsoft.Json;

namespace Legacy.DiscountService.Controllers
{
    /// <summary>
    /// Discount quotes for the legacy checkout flow. All rules are read from
    /// web.config; see the 2009 handover notes for the mainframe round-trip.
    /// </summary>
    public class DiscountController : ApiController
    {
        private static readonly DiscountEngine Engine = new DiscountEngine();

        // GET api/discount?customerId=PREF-0001&amount=250.00&loyaltyYears=3
        [HttpGet]
        public DiscountResult Calculate(string customerId, decimal amount, int loyaltyYears)
        {
            if (String.IsNullOrWhiteSpace(customerId))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            DiscountResult result = Engine.Calculate(customerId, amount, loyaltyYears);

            // Simulate the mainframe round-trip the 2009 team could not remove.
            Thread.Sleep(150);

            Engine.WriteAudit(HttpContext.Current, result);
            return result;
        }

        // POST api/discount
        [HttpPost]
        public DiscountResult Calculate([FromBody] DiscountRequest request)
        {
            if (request == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            DiscountResult result = Engine.Calculate(request.CustomerId, request.Amount, request.LoyaltyYears);
            Engine.WriteAudit(HttpContext.Current, result);
            return result;
        }
    }

    public class DiscountRequest
    {
        [JsonProperty("customerId")]
        public string CustomerId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("loyaltyYears")]
        public int LoyaltyYears { get; set; }
    }
}
