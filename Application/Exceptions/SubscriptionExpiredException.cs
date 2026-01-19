namespace Application.Exceptions;

public class SubscriptionExpiredException : Exception
{
    public DateTime ExpiredAt { get; }
    public string TenantName  { get; }

    public SubscriptionExpiredException(DateTime expiredAt, string tenantName)
        : base("Subscription expired. Please renew to continue.")
    {
        ExpiredAt = expiredAt;
        TenantName = tenantName;
    }
}