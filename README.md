# JobBot

A C# bot that scrapes LinkedIn daily for co-op, intern, and student 
positions in tech across Canada, then posts new listings to a Discord channel.

## Features
- Scrapes 60+ LinkedIn search combinations across software, data science, 
  AI, cybersecurity, cloud, and more
- Filters to student-level positions only (co-op, intern, student)
- Deduplicates listings so you never see the same job twice
- Posts results to Discord daily via a scheduled launchd job (macOS)
- Check-in message posted even when no new listings are found

## Tech Stack
- C# / .NET 10
- Playwright (browser automation)
- Discord.Net
- launchd (macOS scheduler)

## Setup

### Prerequisites
- .NET 10 SDK
- PowerShell (`brew install powershell/tap/powershell`)
- A Discord bot token and channel ID

### Installation
1. Clone the repo
2. Install dependencies:
```bash
   dotnet restore
   dotnet build
   pwsh bin/Debug/net10.0/playwright.ps1 install
```
3. Create a `.env` file in the project root:
DISCORD_TOKEN=your_token_here
DISCORD_CHANNEL_ID=your_channel_id_here

4. Run manually:
```bash
   dotnet run
```

### Scheduling (macOS)
To run automatically every day, see the launchd setup in the repo wiki.

## Project Structure
- `Program.cs` — entry point and orchestration
- `Scraper.cs` — LinkedIn scraping via Playwright
- `Bot.cs` — Discord bot and message posting
- `JobListing.cs` — data model
- `SeenJobs.cs` — deduplication via local JSON cache
