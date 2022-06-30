using ApplicationDiscordBot.Contracts;

namespace ApplicationDiscordBot.Services;

public class BotHostedService : IHostedService
{
    private readonly IBotService _botService;

    public BotHostedService(BotService botService)
    {
        _botService = botService;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _botService.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _botService.StopAsync(cancellationToken);
    }
    
}