namespace ApplicationDiscordBot.Services;

public class BotHostedService : IHostedService
{
    private readonly Bot _bot;

    public BotHostedService(Bot bot)
    {
        _bot = bot;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _bot.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _bot.StopAsync(cancellationToken);
    }
    
}