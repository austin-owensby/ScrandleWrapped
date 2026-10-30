#!/usr/bin/env dotnet

using System.Text.Json;
using System.Text.Json.Serialization;

string fileContents = File.ReadAllText("parsed_messages.json");
List<ParsedMessage> parsedMessages = JsonSerializer.Deserialize(fileContents, ParsedJsonContext.Default.ListParsedMessage) ?? [];

Statistics statistics = new();

CalculatePlayerStats();

File.WriteAllText("statistics.json", JsonSerializer.Serialize(statistics, StatisticsJsonContext.Default.Statistics));

void CalculatePlayerStats()
{
    List<string> users = parsedMessages.Select(m => m.User).Distinct().Order().ToList();

    foreach (string user in users)
    {
        List<ParsedMessage> userMessages = parsedMessages.Where(m => m.User == user).ToList();

        PlayerStatistics playerStatistic = new()
        {
            DeletedUser = userMessages.First().DeletedUser,
            User = user,
            DaysPlayed = userMessages.Count,
            Average = (double)userMessages.Sum(u => u.Score) / userMessages.Count
        };

        foreach (ParsedMessage message in userMessages)
        {
            playerStatistic.ScoreFrequency[message.Score]++;
        }

        statistics.PlayerStatistics.Add(playerStatistic);
    }
}

class Statistics
{
    public List<PlayerStatistics> PlayerStatistics { get; set; } = [];
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Statistics))]
internal partial class StatisticsJsonContext : JsonSerializerContext
{
}

class PlayerStatistics
{
    public bool DeletedUser { get; set; }
    public string User { get; set; } = string.Empty;
    public int DaysPlayed { get; set; }
    public double Average { get; set; }
    public Dictionary<int, int> ScoreFrequency { get; set; } = new()
    {
        {0, 0},
        {1, 0},
        {2, 0},
        {3, 0},
        {4, 0},
        {5, 0},
        {6, 0},
        {7, 0},
        {8, 0},
        {9, 0},
        {10, 0}
    };
}

class ParsedMessage
{
    public bool DeletedUser { get; set; }
    public string User { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public short Answers { get; set; } = 0b0000000000;
    public int Score { get; set; }
}

[JsonSerializable(typeof(List<ParsedMessage>))]
internal partial class ParsedJsonContext : JsonSerializerContext
{
}