using Fusion.Service.Commons.Background;
using Fusion.Service.IServices;

namespace Fusion.API.Background.Jobs;

public sealed class AutoMonthlyEntitlementResetJob : IBackgroundJob
{
    public string Name => "AutoMonthlyEntitlementResetJob";
    public TimeSpan Interval => TimeSpan.FromHours(5);
    public async Task ExecuteAsync(IServiceProvider services, CancellationToken ct)
    {

        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILogger<AutoMonthlyEntitlementResetJob>>();

        var userSubService = scope.ServiceProvider
         .GetRequiredService<IUserSubscriptionService>();

        var companySubService = scope.ServiceProvider
          .GetRequiredService<ICompanySubscriptionService>();

        var now = DateTimeOffset.UtcNow;

        logger.LogInformation("[{Job}] Tick at {Now}", Name, now);

        //Khi lên production bật lại đoạn này
        if (now.Day != 1)
        {
            logger.LogInformation(
                "[{Job}] Skip because Day = {Day} != 1",
                Name, now.Day);
            return;
        }

        try
        {
            // 1) Reset auto-month cho UserSubscriptionEntitlement
            var userUpdated = await userSubService.ResetAutoMonthlyEntitlementsAsync(ct);

            // 2) Reset auto-month cho CompanySubscriptionEntitlement
            var companyUpdated = await companySubService.ResetCompanyAutoMonthlyEntitlementsAsync(ct);

            logger.LogInformation(
                 "[{Job}] Executed at {Now}, updated {UserCount} user entitlements and {CompanyCount} company entitlements.",
                 Name, now, userUpdated, companyUpdated);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[{Job}] Failed at {Now}.",
                Name, now);
        }
    }
}
