using Discord;
using Discord.WebSocket;
using dotenv.net;

namespace JobBot;

public class Bot
{
    private readonly DiscordSocketClient _client;
    private readonly ulong _channelId;

    /// <summary>
    /// Initializes a new "Bot" instance: loads Discord credentials from the
    /// .env file, validates them, and creates the Discord client
    /// </summary>
    public Bot()
    {
        //read .env file from disk and load each KEY - value pair into the environment variables
        DotEnv.Load();

        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN")     ?? "";
        var channelId = Environment.GetEnvironmentVariable("DISCORD_CHANNEL_ID") ?? "";

        // error check
        if (string.IsNullOrWhiteSpace(token))
            throw new Exception("DISCORD_TOKEN is missing from .env");
        if (string.IsNullOrWhiteSpace(channelId))
            throw new Exception("DISCORD_CHANNEL_ID is missing from .env");

        _channelId = ulong.Parse(channelId);

        // Create the Discord client, subscribing only to guild-level(server) gateway events
        // The bot only posts messages out, so it doesn't need message-content or message-read intents 
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
        });
    }



    /// <summary>
    /// Connects to Discord, waits until the client is ready, and
    /// returns the  message channel
    /// </summary>
    /// <returns>The configured channel, ready to post to</returns>
    /// <exception cref="Exception">
    /// Thrown when the configured channel ID can't be resolved to a message channel
    /// </exception>
    private async Task<IMessageChannel> ConnectAndGetChannelAsync()
    {
        // Set up the readiness signal BEFORE connecting. StartAsync begins the
        // connection; true readiness arrives later as the Ready event, which we can't
        // await directly. A TCS is a Task we complete by hand, so we
        // use it to bridge that event into something awaitable.
        var ready = new TaskCompletionSource<bool>();

        // Subscribe our handler(method) to Ready
        _client.Ready += () => { ready.SetResult(true);
                                return Task.CompletedTask; };

        // Authenticate as a bot, then open the connection. Each await pauses
        // this method during the network round-trip and resumes once it completes.
        await _client.LoginAsync(TokenType.Bot,
            Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
        await _client.StartAsync();

        // Suspend until the client fires Ready and our handler completes the Task 
        await ready.Task;

        // GetChannel returns a general IChannel (could be text, voice, etc.).
        // 'as IMessageChannel' narrows it to a channel we can post to, or null 
        return _client.GetChannel(_channelId) as IMessageChannel
            ?? throw new Exception($"Channel {_channelId} not found");
    
    }



    /// <summary>
    /// Connects to Discord, posts the given job listings to the configured channel
    /// as a formatted daily report, then disconnects
    /// Messages are sent one per second to avoid Discord's rate limits
    /// </summary>
    public async Task PostJobsAsync(List<JobListing> jobs)
    {
    
        var channel = await ConnectAndGetChannelAsync();

        // Message Header; formats the date (e.g. "June 30, 2026")
        // The ** are Discord markdown for bold
        await channel.SendMessageAsync(
            $"**JobBot Daily Report** — {DateTime.Now:MMMM d, yyyy}\n" +
            $"Found **{jobs.Count}** relevant listings today\n" +
            "─────────────────────────────────");

        // One message per listing
        foreach (var job in jobs)
        {
            var msg = $"**{job.Title}**\n" +
                    $"Company: {job.Company}\n" +
                    $"URL: {job.Url}";

            await channel.SendMessageAsync(msg);

            // Async pause between sends, Discord rate limits
            await Task.Delay(1000);
        }

        // Closing divider, then close the connection 
        await channel.SendMessageAsync("─────────────────────────────────\n End of today's listings");
        await _client.StopAsync();
    }

    /// <summary>
    /// Connects to Discord and posts a single "no new listings" message
    /// Confirms the bot ran even on a run that found nothing new
    /// </summary>
    public async Task PostNoNewJobsAsync()
    {
        var channel = await ConnectAndGetChannelAsync();

        // Check-in message Includes the time because
        // the point of this message is to confirm when the bot last ran
        await channel.SendMessageAsync(
            $"**JobBot checked in** — {DateTime.Now:MMMM d, yyyy h:mm tt}\n" +
            $"No new listings since last run.");

        // Close the connection 
        await _client.StopAsync();
    }

}