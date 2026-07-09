using JobBot;

Console.WriteLine("JobBot starting...\n");

// Wait for network to be available before scraping
Console.WriteLine("Waiting for network...");
while (true)
{
    try
    {
        using var http = new HttpClient();
        http.Timeout = TimeSpan.FromSeconds(5);
        await http.GetAsync("https://www.linkedin.com");
        Console.WriteLine("Network ready.\n");
        break;
    }
    catch
    {
        Console.WriteLine("Network not ready, retrying in 30s...");
        await Task.Delay(30000);
    }
}

var seenJobs = new SeenJobs();
Console.WriteLine($"Seen jobs on record: {seenJobs.Count}");

var allJobs = await Scraper.ScrapeLinkedInAsync();
Console.WriteLine($"Scraped {allJobs.Count} relevant listings");

var newJobs = allJobs.Where(j => !seenJobs.HasSeen(j.Url)).ToList();
Console.WriteLine($"New listings (not yet posted): {newJobs.Count}\n");

var bot = new Bot();

if (newJobs.Count > 0)
{
    await bot.PostJobsAsync(newJobs);
    seenJobs.MarkSeen(newJobs.Select(j => j.Url));
    Console.WriteLine($"Posted {newJobs.Count} new listings to Discord.");
}
else
{
    await bot.PostNoNewJobsAsync();
    Console.WriteLine("No new listings — posted check-in message to Discord.");
}