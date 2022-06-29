using ApplicationDiscordBot.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Options;

namespace ApplicationDiscordBot;

public class Bot
{
    public DiscordClient Client { get; }

    private readonly IOptions<BotConfiguration> _config;

    public Bot(IOptions<BotConfiguration> config)
    {
        _config = config;
        
        var dsConfig = new DiscordConfiguration
        {
            Token = config.Value.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            MinimumLogLevel = LogLevel.Debug,
            Intents = DiscordIntents.AllUnprivileged
        };
        
        Client = new DiscordClient(dsConfig);
        Client.ComponentInteractionCreated += OnInteractionCreated;
        Client.ModalSubmitted += OnModalSubmitted;

    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Client.ConnectAsync(new DiscordActivity("Детектор мудаков", ActivityType.Playing));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Client.DisconnectAsync();
        Client.Dispose();
    }

    private async Task<DiscordEmbed> BakeEmbed(ModalSubmitEventArgs e, bool isAuthor = false)
    {
        var embed = new DiscordEmbedBuilder()
            .AddField("Ф. И.", e.Values["m-name"])
            .AddField("Описание ситуации и причина", e.Values["m-description"])
            .WithColor(DiscordColor.HotPink)
            .WithTimestamp(DateTime.Now)
            .WithFooter("The bot was created by Ultra_Rabbit#7118");
        
        if (!string.IsNullOrEmpty(e.Values["m-screenshot"]) && await IsImageUrl(e.Values["m-screenshot"]))
            embed.WithImageUrl(e.Values["m-screenshot"]);
        if (!string.IsNullOrWhiteSpace(e.Values["m-discord"]))
            embed.AddField("Дискорд (если есть)", e.Values["m-discord"]);
        
        if (isAuthor)
        {
            embed.WithAuthor(e.Interaction.User.Username, iconUrl: e.Interaction.User.AvatarUrl);
        }

        return embed;

    }
    private async Task<bool> IsImageUrl(string url)
    {
        var result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
                      && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        if (!result)
        {
            return result;
        }
        var client = new HttpClient();

        var res = await client.GetAsync(url);

        return res.ToString().Contains("Content-Type: image");
    }

    private async Task OnInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        if (e.Id == "my_very_cool_button")
        {
            var modal = new DiscordInteractionResponseBuilder()
                .WithTitle("Жалобная анкета")
                .WithCustomId("m-modal")
                .AddComponents(new TextInputComponent(label: "Ф.И.", customId: "m-name", value: string.Empty,
                    max_length: 100))
                .AddComponents(new TextInputComponent(label: "Описание ситуации и причина", customId: "m-description",
                    value: string.Empty,
                    min_length: 1,
                    style: TextInputStyle.Paragraph))
                .AddComponents(new TextInputComponent(label: "Дискорд (если есть)", customId: "m-discord",
                    value: string.Empty,
                    max_length: 100, required: false))
                .AddComponents(new TextInputComponent(label: "Скрин (по желанию)", customId: "m-screenshot",
                    value: string.Empty, max_length: 360, required: false));
            
            await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
        }
    }

    private async Task OnModalSubmitted(DiscordClient sender, ModalSubmitEventArgs e)
    {
        if (e.Values.ContainsKey("m-name"))
        {
            var authorEmbed = await BakeEmbed(e, true);
                
            await e.Interaction.Guild.Owner.SendMessageAsync(embed: authorEmbed);

            var embed = await BakeEmbed(e);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }

}