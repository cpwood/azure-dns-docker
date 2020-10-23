using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Extensions.Logging;

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
            
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });
            
            var logger = loggerFactory.CreateLogger<Program>();
            
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            logger.LogInformation($"Azure Dynamic DNS - v{version}");

            try
            {
                var tenantId = GetValue("TENANT_ID");
                var clientId = GetValue("CLIENT_ID");
                var secret = GetValue("SECRET");
                var subscriptionId = GetValue("SUBSCRIPTION_ID");
                var resourceGroupName = GetValue("RESOURCE_GROUP");
                var zoneName = GetValue("ZONE_NAME");
                var recordSetName = GetValue("RECORD_NAME");
            
                // Default 15 minutes
                var delay = int.Parse(GetValue("DELAY", false) ?? "900000");
            
                // Default 5 minutes
                var ttl = int.Parse(GetValue("TTL", false) ?? "300");
                
                var ipAddress = string.Empty;

                while (true)
                {
                    logger.LogTrace("Getting current IP address..");
                    var temp = (await httpClient.GetStringAsync("https://api.ipify.org"))?.Trim();

                    if (temp != null && !temp.Equals(ipAddress, StringComparison.InvariantCulture))
                    {
                        ipAddress = temp;
                        
                        logger.LogTrace("Logging in to Azure DNS Zone with credentials..");
                        var credentials = await ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, secret);
                        var dnsClient = new DnsManagementClient(credentials) {SubscriptionId = subscriptionId};
                        
                        logger.LogTrace("Finding record sets for zone..");
                        var recordSets = await dnsClient.RecordSets.ListByDnsZoneAsync(resourceGroupName, zoneName);
                        var recordSet = recordSets.FirstOrDefault(x => x.Name == recordSetName) ??
                                        new RecordSet {ARecords = new List<ARecord>()};

                        recordSet.ARecords.Clear();
                        recordSet.ARecords.Add(new ARecord(ipAddress));
                        recordSet.TTL = ttl;

                        logger.LogTrace($"Setting A-record for {recordSetName}.{zoneName} to {ipAddress} ..");
                        await dnsClient.RecordSets.CreateOrUpdateAsync(resourceGroupName, zoneName, recordSetName, RecordType.A,
                            recordSet, recordSet.Etag);
                        
                        logger.LogInformation($"A-record for {recordSetName}.{zoneName} set to {ipAddress} .");
                    }
                    else
                    {
                        logger.LogTrace($"Public IP address hasn't changed (still {ipAddress}).");
                    }

                    await Task.Delay(delay);
                }
            }
            catch (ArgumentNullException ex)
            {
                logger.LogCritical(ex, ex.Message);
            }
            
            // ReSharper disable once FunctionNeverReturns
        }

        static string GetValue(string name, bool required = true)
        {
            var value = Environment.GetEnvironmentVariable(name);

            if (required && string.IsNullOrEmpty(value))
                throw new ArgumentNullException(name, $"The '{name}' environment variable value must be set.");

            return value;
        }
    }
}