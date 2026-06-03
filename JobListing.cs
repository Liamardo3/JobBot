namespace JobBot;

//simple data container for listing info

public class JobListing
{
    public string Title   { get; set; } = "";
    public string Company { get; set; } = "";
    public string Url     { get; set; } = "";
    public string Source  { get; set; } = "";

    //timestamp when listing was added, UTC-3/4 time
    public DateTime FoundAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(
        DateTime.UtcNow,
        TimeZoneInfo.FindSystemTimeZoneById("America/Halifax")
    );
    
    public override string ToString() =>
        $"[{Source}] {Title} @ {Company}\n  {Url}";
}