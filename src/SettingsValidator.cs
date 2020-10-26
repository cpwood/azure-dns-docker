using FluentValidation;

namespace DynamicAzureDns
{
    public class SettingsValidator : AbstractValidator<Settings>
    {
        public SettingsValidator()
        {
            RuleFor(x => x.TenantId).NotEmpty();
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.Secret).NotEmpty();
            RuleFor(x => x.SubscriptionId).NotEmpty();
            RuleFor(x => x.ResourceGroup).NotEmpty();
            RuleFor(x => x.ZoneName).NotEmpty();
            RuleFor(x => x.RecordName).NotEmpty();
            RuleFor(x => x.Delay).GreaterThan(0);
            RuleFor(x => x.Ttl).GreaterThan(0);
        }
    }
}