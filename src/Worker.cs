using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest.Azure.Authentication;

namespace DynamicAzureDns
{
    public class Worker : BackgroundService
    {
        private readonly IValidator<Settings> _validator;
        private readonly ILogger<Worker> _logger;
        private readonly Settings _settings;
        
        public Worker(
            IOptions<Settings> settings,
            IValidator<Settings> validator,
            ILogger<Worker> logger)
        {
            _validator = validator;
            _logger = logger;
            _settings = settings.Value;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var result = await _validator.ValidateAsync(_settings, stoppingToken);
                
                if (!result.IsValid)
                    throw new ValidationException(result.Errors);
                
                using var httpClient = new HttpClient();
                var ipAddress = string.Empty;

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogTrace("Getting current IP address..");
                    var temp = (await httpClient.GetStringAsync("https://api.ipify.org"))?.Trim();

                    if (temp != null && !temp.Equals(ipAddress, StringComparison.InvariantCulture))
                    {
                        ipAddress = temp;
                        
                        _logger.LogTrace("Logging in to Azure DNS Zone with credentials..");
                        var credentials = await ApplicationTokenProvider.LoginSilentAsync(_settings.TenantId,
                            _settings.ClientId, _settings.Secret);
                        var dnsClient = new DnsManagementClient(credentials) {SubscriptionId = _settings.SubscriptionId};
                        
                        _logger.LogTrace("Finding record sets for zone..");
                        var recordSets = await dnsClient.RecordSets.ListByDnsZoneAsync(_settings.ResourceGroup,
                            _settings.ZoneName, cancellationToken: stoppingToken);
                        var recordSet = recordSets.FirstOrDefault(x => x.Name == _settings.RecordName) ??
                                        new RecordSet {ARecords = new List<ARecord>()};

                        recordSet.ARecords.Clear();
                        recordSet.ARecords.Add(new ARecord(ipAddress));
                        recordSet.TTL = _settings.Ttl;

                        _logger.LogTrace($"Setting A-record for {_settings.RecordName}.{_settings.ZoneName} to {ipAddress} ..");
                        await dnsClient.RecordSets.CreateOrUpdateAsync(_settings.ResourceGroup, _settings.ZoneName,
                            _settings.RecordName, RecordType.A,
                            recordSet, recordSet.Etag, cancellationToken: stoppingToken);
                        
                        _logger.LogInformation($"A-record for {_settings.RecordName}.{_settings.ZoneName} set to {ipAddress} .");
                    }
                    else
                    {
                        _logger.LogTrace($"Public IP address hasn't changed (still {ipAddress}).");
                    }

                    await Task.Delay(_settings.Delay, stoppingToken);
                }
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogCritical(ex, ex.Message);
            }
        }
    }
}