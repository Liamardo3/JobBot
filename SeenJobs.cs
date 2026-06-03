using System.Text.Json;

namespace JobBot;

public class SeenJobs
{
    private readonly string _filePath;
    private HashSet<string> _seenUrls;

    public SeenJobs(string filePath = "seen_jobs.json")
    {
        _filePath = filePath;
        _seenUrls = Load();
    }

    public bool HasSeen(string url) => _seenUrls.Contains(url);

    public void MarkSeen(IEnumerable<string> urls)
    {
        foreach (var url in urls)
            _seenUrls.Add(url);
        Save();
    }

    public int Count => _seenUrls.Count;

    private HashSet<string> Load()
    {
        if (!File.Exists(_filePath))
            return new HashSet<string>();

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<HashSet<string>>(json)
                   ?? new HashSet<string>();
        }
        catch
        {
            return new HashSet<string>();
        }
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_seenUrls, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_filePath, json);
    }
}