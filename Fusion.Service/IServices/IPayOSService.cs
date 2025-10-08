
using Net.payOS.Types;

namespace Fusion.Service.IServices;

public interface IPayOSService
{
    Task<string> CreatePaymentLink(Guid transactionId, CancellationToken cancellationToken = default);
    Task HandlePaymentWebHook(WebhookType webhookData, CancellationToken cancellationToken = default);
    Task<string> ConfirmWebHook(string url);
}
