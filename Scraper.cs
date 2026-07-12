using Microsoft.Playwright;

namespace JobBot;

public class Scraper
{
    public static async Task<List<JobListing>> ScrapeLinkedInAsync()
    {
        var results = new List<JobListing>();

        //searches to be used in LinkedIn
        var studentTerms = new[] { "intern", "co-op", "student" };
        var fieldTerms = new[]
        {
            "software developer", "software engineer", "computer science",
            "data science", "machine learning", "artificial intelligence",
            "cybersecurity", "cloud computing", "backend developer",
            "frontend developer", "full stack developer", "devops",
            "network engineer", "systems engineer", "database developer",
            "mobile developer", "web developer", "IT", "MLOps", "NLP"
        };

var searches = studentTerms
    .SelectMany(s => fieldTerms.Select(f => $"{f} {s} canada"))
    .ToArray();

Console.WriteLine($"Running {searches.Length} searches...");

        //start up Playwright, hold it in playwright, and automatically shut it down when we're done.
        using var playwright = await Playwright.CreateAsync();

        // launch a Chromium browser with some options, store it in browser
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });

        //Create a new browser context
        var context = await browser.NewContextAsync(new()
        {
            UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) " +
                        "AppleWebKit/537.36 (KHTML, like Gecko) " +
                        "Chrome/124.0.0.0 Safari/537.36",
            ViewportSize = new() { Width = 1280, Height = 800 }
        });

        //new tab
        var page = await context.NewPageAsync();

        foreach (var query in searches)
        {
            //URL encode the search query
            var encoded = Uri.EscapeDataString(query);

            //inject search terms, filter to Canada, sort by newest, and last 24 hours
            var url = $"https://www.linkedin.com/jobs/search/?keywords={encoded}&location=Canada&sortBy=DD&f_TPR=r172800";

            try
            {
                //Navigates the browser to the URL, wait for DOM
                await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.DOMContentLoaded });

                //anti-bot behaviour
                await Task.Delay(Random.Shared.Next(2500, 4500));

                //Queries the page for every HTML element with the CSS class base-card
                var cards = await page.QuerySelectorAllAsync(".base-card");

                foreach (var card in cards)
                {
                    //Within each card, searche for three specific child elements
                    var titleEl   = await card.QuerySelectorAsync(".base-search-card__title");
                    var companyEl = await card.QuerySelectorAsync(".base-search-card__subtitle");
                    var linkEl    = await card.QuerySelectorAsync("a.base-card__full-link");

                    //get text within the element and trim whitespace, or set null value to empty string
                    var title   = titleEl   != null ? (await titleEl.InnerTextAsync()).Trim()   : "";
                    var company = companyEl != null ? (await companyEl.InnerTextAsync()).Trim() : "";

                    //get url from  href attribute
                    var href    = linkEl    != null ? await linkEl.GetAttributeAsync("href")    : "";

                    //if there's no URL, skip this card
                    if (string.IsNullOrWhiteSpace(href)) continue;

                    //checks whether the job title matches what we're looking for
                    if (!IsRelevant(title)) continue;

                    //remove LinkedIn tracking bit
                    var cleanUrl = href.Split('?')[0];

                    //add listing only if that url has not been used before
                    if (!results.Any(r => r.Url == cleanUrl))
                        results.Add(new JobListing
                        {
                            Title   = title,
                            Company = company,
                            Url     = cleanUrl,
                            Source  = "LinkedIn"
                        });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LinkedIn error ({query}): {ex.Message}");
            }

            //anti-bot behaviour
            await Task.Delay(Random.Shared.Next(3000, 6000));
        }

        //wait for browser to close and return scraped listings
        await browser.CloseAsync();
        return results;
    }

    private static bool IsRelevant(string title)
    {
        var t = title.ToLower();

        // must be a student-level position
        var studentLevel = new[] {
            "co-op", "coop", "intern", "internship", "student"
        };

        // must be in a relevant field
        var relevantField = new[] {
            "software", "developer", "engineer", "data", "machine learning",
            "ai", "artificial intelligence", "cyber", "security", "cloud",
            "devops", "backend", "frontend", "full stack", "fullstack",
            "computer", "it ", "information technology", "database",
            "network", "systems", "web", "mobile", "programming",
            "analytics", "business intelligence", "bi ", "sap",
            "infrastructure", "platform", "sre", "mlops", "nlp"
        };

        // always reject these regardless of anything else
        var hardReject = new[] {
            "accountant", "accounting", "supply chain", "electrical assembler",
            "mechanical", "geotechnical", "civil engineer", "structural",
            "tutor", "sales", "marketing", "hr ", "human resources",
            "legal", "biology", "physics", "chemistry", "policy",
            "communications", "finance intern", "financial intern",
            "investment intern", "accounting intern", "people analyst",
            "corporate accounting", "accounts payable", "treasury",
            "management consulting", "resource evaluation"
        };

        if (hardReject.Any(w => t.Contains(w))) return false;
        if (!studentLevel.Any(w => t.Contains(w))) return false;
        return relevantField.Any(w => t.Contains(w));
    }
}