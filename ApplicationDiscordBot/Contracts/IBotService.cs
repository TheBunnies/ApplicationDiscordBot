using DSharpPlus;
using DSharpPlus.Entities;

namespace ApplicationDiscordBot.Contracts;

public interface IBotService
{
    DiscordClient Client { get; }
    DiscordComponent GetButton(string label, string emoji = "");
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}