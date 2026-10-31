#!/usr/bin/env dotnet

#:include Models.cs

using System.Text.Json;
using Models;

string slackFileContents = File.ReadAllText("data/parsed_messages.json");
List<ParsedMessage> parsedMessages = JsonSerializer.Deserialize(slackFileContents, ParsedJsonContext.Default.ListParsedMessage) ?? [];

string communityFileContents = File.ReadAllText("data/community_data.json");
List<ParsedCommunityDay> parsedCommunityData = JsonSerializer.Deserialize(communityFileContents, OutputJsonContext.Default.ListParsedCommunityDay) ?? [];

Statistics statistics = new();

var dates = parsedMessages.GroupBy(m => m.Timestamp.Date).ToList();
statistics.CapsherAverage = parsedMessages.Average(m => m.Score);
statistics.CommunityAverage = parsedCommunityData.Average(d => d.Avg);
statistics.CapsherAveragePerDay = dates.ToDictionary(g => g.Key, g => g.ToList().Average(m => m.Score));
statistics.CapsherHardestDay = DateOnly.FromDateTime(statistics.CapsherAveragePerDay.MinBy(kp => kp.Value).Key);
statistics.CapsherEasiestDay = DateOnly.FromDateTime(statistics.CapsherAveragePerDay.MaxBy(kp => kp.Value).Key);
statistics.CommunityHardestDay = parsedCommunityData.MinBy(kp => kp.Avg)!.Day;
statistics.CommunityEasiestDay = parsedCommunityData.MaxBy(kp => kp.Avg)!.Day;

List<string> users = parsedMessages.Select(m => m.User).Distinct().Order().ToList();


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

File.WriteAllText("data/statistics.json", JsonSerializer.Serialize(statistics, StatisticsJsonContext.Default.Statistics));
