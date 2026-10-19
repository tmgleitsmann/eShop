using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;

namespace Legacy.DiscountService.Domain
{
    /// <summary>
    /// Discount rules engine, unchanged since 2011. All rules live in
    /// web.config because the rules database project was never delivered.
    /// </summary>
    public class DiscountEngine
    {
        private static readonly object SyncRoot = new object();
        private static bool _initialized;

        public DiscountEngine()
        {
            if (_initialized)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (_initialized)
                {
                    return;
                }

                // The team did not trust ConfigurationManager caching, so the
                // appSettings section is re-validated on every cold start.
                foreach (string key in ConfigurationManager.AppSettings.AllKeys)
                {
                    if (key.StartsWith("Discount:", StringComparison.Ordinal))
                    {
                        decimal.Parse(ConfigurationManager.AppSettings[key]);
                    }
                }

                _initialized = true;
            }
        }

        public DiscountResult Calculate(string customerId, decimal amount, int loyaltyYears)
        {
            decimal percent = ReadSetting("Discount:LoyaltyYearsPercent") * loyaltyYears;

            if (customerId != null && customerId.StartsWith("PREF", StringComparison.Ordinal))
            {
                percent += ReadSetting("Discount:PreferredCustomerPercent");
            }

            if (amount >= ReadSetting("Discount:VolumeThreshold"))
            {
                percent += ReadSetting("Discount:VolumePercent");
            }

            decimal cap = ReadSetting("Discount:MaxPercent");
            if (percent > cap)
            {
                percent = cap; // hard cap requested by finance, 2014
            }

            return new DiscountResult
            {
                CustomerId = customerId,
                Amount = amount,
                DiscountPercent = percent,
                DiscountedAmount = decimal.Round(amount * (100m - percent) / 100m, 2, MidpointRounding.AwayFromZero)
            };
        }

        public void WriteAudit(HttpContext context, DiscountResult result)
        {
            lock (SyncRoot)
            {
                string path = context.Server.MapPath(ConfigurationManager.AppSettings["Audit:LogPath"]);
                string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + result + Environment.NewLine;
                File.AppendAllText(path, line, Encoding.Default);
            }
        }

        private static decimal ReadSetting(string key)
        {
            return decimal.Parse(ConfigurationManager.AppSettings[key]);
        }
    }
}
