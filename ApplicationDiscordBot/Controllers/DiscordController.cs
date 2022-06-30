using ApplicationDiscordBot.Contracts;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationDiscordBot.Controllers;

[ApiController]
[Route("[controller]")]
public class DiscordController : ControllerBase
{
    private readonly IBotService _botService;

    public DiscordController(IBotService botService)
    {
        _botService = botService;
    }
    [HttpPost(nameof(Application))]
    public async Task<IActionResult> Application(ulong channelId, string label, string emoji, string content)
    {
        try
        {
            var channel = await _botService.Client.GetChannelAsync(channelId);
            var myButton = _botService.GetButton(label, emoji);
                
            var builder = new DiscordMessageBuilder()
                .WithContent(content)
                .AddComponents(myButton);

            await builder.SendAsync(channel);
            return Ok();
        }
        catch (Exception)
        {
            return BadRequest(
                "Something went wrong! Please check if the bot has enough permissions to post messages and the channel itself exist");
        }
    }
}