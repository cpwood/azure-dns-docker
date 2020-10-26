namespace DynamicAzureDns
{
    public class Settings
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceGroup { get; set; }
        public string ZoneName { get; set; }
        public string RecordName { get; set; }
        public int Delay { get; set; } = 90000;
        public int Ttl { get; set; } = 300;
    }
}