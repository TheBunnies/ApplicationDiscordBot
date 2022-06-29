using System.ComponentModel.DataAnnotations;

namespace ApplicationDiscordBot.Models;

public class BotConfiguration
{
    [Required]
    public string? Token { get; set; }
}