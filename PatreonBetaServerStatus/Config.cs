namespace PatreonBetaServerStatus;

public class Config
{
    public string GameVersion { get; set; } = string.Empty;

    public string WebhookToken { get; set; } = string.Empty;

    public ulong WebhookId { get; set; }

    public ulong MessageId { get; set; }

    public int RefreshRate { get; set; }
}
