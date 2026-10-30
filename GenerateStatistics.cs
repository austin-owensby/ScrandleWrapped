#!/usr/bin/env dotnet

using System.Text.Json;
using System.Text.Json.Serialization;

string fileContents = File.ReadAllText("parsed_messages.json");
List<ParsedMessage> parsedMessages = JsonSerializer.Deserialize(fileContents, ParsedJsonContext.Default.ListParsedMessage) ?? [];

Statistics statistics = new();

var dates = parsedMessages.GroupBy(m => m.Timestamp.Date).ToList();
statistics.CapsherAveragePerDay = dates.ToDictionary(g => g.Key, g => g.ToList().Average(m => m.Score));

List<string> users = parsedMessages.Select(m => m.User).Distinct().Order().ToList();

statistics.CapsherAverage = parsedMessages.Average(m => m.Score);

foreach (string user in users)
{
    List<ParsedMessage> userMessages = parsedMessages.Where(m => m.User == user).ToList();

    PlayerStatistics playerStatistic = new()
    {
        DeletedUser = userMessages.First().DeletedUser,
        User = user,
        DaysPlayed = userMessages.Count,
        Average = userMessages.Average(u => u.Score)
    };

    int currentPerfectStreak = 0;
    DateTime? nextDay = null;
    int currentStreak = 1;

    foreach (ParsedMessage message in userMessages)
    {
        if (message.Timestamp.Date.AddDays(1) == nextDay || message.Timestamp.Date.DayOfWeek == DayOfWeek.Friday && message.Timestamp.Date.AddDays(3) == nextDay)
        {
            currentStreak++;

            if (currentStreak > playerStatistic.BestStreak)
            {
                playerStatistic.BestStreak = currentStreak;
            }
        }
        else
        {
            currentStreak = 1;
        }
        nextDay = message.Timestamp.Date;

        playerStatistic.ScoreFrequency[message.Score]++;

        if (statistics.CapsherAveragePerDay[message.Timestamp.Date] < message.Score)
        {
            playerStatistic.BeatCapsherAverage++;
        }

        if (message.Score == 10)
        {
            currentPerfectStreak++;

            if (currentPerfectStreak > playerStatistic.BestPerfectStreak)
            {
                playerStatistic.BestPerfectStreak = currentPerfectStreak;
            }
        }
        else
        {
            currentPerfectStreak = 0;
        }

        if (message.Answers == 0b1111111110)
        {
            playerStatistic.BlewItCount++;
        }

        playerStatistic.TeamworkCount += parsedMessages.Count(m => m.User != message.User && m.Timestamp.Date == message.Timestamp.Date && m.Answers == message.Answers);
    }

    statistics.PlayerStatistics.Add(playerStatistic);
}

File.WriteAllText("statistics.json", JsonSerializer.Serialize(statistics, StatisticsJsonContext.Default.Statistics));

class Statistics
{
    public List<PlayerStatistics> PlayerStatistics { get; set; } = [];
    public double CapsherAverage { get; set; }
    public Dictionary<DateTime, double> CapsherAveragePerDay { get; set; } = [];
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
    public int BestStreak { get; set; } = 1;
    public int BestPerfectStreak { get; set; }
    public int BeatCapsherAverage { get; set; }
    public int BlewItCount { get; set; }
    public int TeamworkCount { get; set; }
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