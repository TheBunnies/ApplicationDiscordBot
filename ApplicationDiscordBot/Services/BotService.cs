using ApplicationDiscordBot.Contracts;
using ApplicationDiscordBot.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Options;

namespace ApplicationDiscordBot.Services;

public class BotService : IBotService
{
    public DiscordClient Client { get; }

    private readonly HttpClient _client;
    public BotService(IOptions<BotConfiguration> config, HttpClient client)
    {
        _client = client;
        var dsConfig = new DiscordConfiguration
        {
            Token = config.Value.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
#if DEBUG
            MinimumLogLevel = LogLevel.Debug,
#else
            MinimumLogLevel = LogLevel.Error,
#endif
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

    public DiscordComponent GetButton(string label, string emoji = "")
    {
        var myButton = new DiscordButtonComponent(
            ButtonStyle.Success,
            "my_very_cool_button",
            label,
            false,
            string.IsNullOrEmpty(emoji) ? null : new DiscordComponentEmoji(emoji));

        return myButton;
    }

    private async Task<DiscordEmbed> BakeEmbed(ModalSubmitEventArgs e, bool isAuthor = false)
    {
        var embed = new DiscordEmbedBuilder()
            .AddField("Ф. И.", e.Values["m-name"])
            .AddField("Описание ситуации и причина", e.Values["m-description"])
            .WithColor(DiscordColor.HotPink)
            .WithTimestamp(DateTime.Now);

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

        var res = await _client.GetAsync(url);
        return res.ToString().Contains("Content-Type: image");
    }

    private async Task OnInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        if (e.Id == "my_very_cool_button")
        {
            var modal = new DiscordInteractionResponseBuilder()
                .WithTitle("Жалоба на мудака")
                .WithCustomId("m-modal")
                .AddComponents(new TextInputComponent(label: "Ф.И.", customId: "m-name", value: string.Empty,
                    max_length: 100, required: true))
                .AddComponents(new TextInputComponent(label: "Описание ситуации и причина", customId: "m-description",
                    value: string.Empty,
                    required: true,
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
            
            if (e.Interaction.User.Id != e.Interaction.Guild.Owner.Id)
                await e.Interaction.Guild.Owner.SendMessageAsync(embed: authorEmbed);

            var embed = await BakeEmbed(e);
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AddComponents(GetButton("Записать мудака")));
        }
    }

}