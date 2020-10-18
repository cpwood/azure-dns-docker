using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;

namespace DynamicAzureDns
{
    class Program
    {
        static void Main()
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {
            using var httpClient = new HttpClient();

            var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
            var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
            var secret = Environment.GetEnvironmentVariable("SECRET");
            var subscriptionId = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
            var resourceGroupName = Environment.GetEnvironmentVariable("RESOURCE_GROUP");
            var zoneName = Environment.GetEnvironmentVariable("ZONE_NAME");
            var recordSetName = Environment.GetEnvironmentVariable("RECORD_NAME");
            
            // Default 15 minutes
            var delay = int.Parse(Environment.GetEnvironmentVariable("DELAY") ?? "900000");
            
            // Default 5 minutes
            var ttl = int.Parse(Environment.GetEnvironmentVariable("TTL") ?? "300");
            
            var credentials = await ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, secret);
            var dnsClient = new DnsManagementClient(credentials) {SubscriptionId = subscriptionId};

            while (true)
            {
                var ipAddress = (await httpClient.GetStringAsync("https://api.ipify.org"))?.Trim();

                var recordSets = await dnsClient.RecordSets.ListByDnsZoneAsync(resourceGroupName, zoneName);
                var recordSet = recordSets.FirstOrDefault(x => x.Name == recordSetName) ??
                                new RecordSet {ARecords = new List<ARecord>()};

                recordSet.ARecords.Clear();
                recordSet.ARecords.Add(new ARecord(ipAddress));
                recordSet.TTL = ttl;

                await dnsClient.RecordSets.CreateOrUpdateAsync(resourceGroupName, zoneName, recordSetName, RecordType.A,
                    recordSet, recordSet.Etag);

                await Task.Delay(delay);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}