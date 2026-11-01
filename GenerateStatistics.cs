#!/usr/bin/env dotnet

#:include Models.cs

using System.Collections.Frozen;
using System.Text.Json;
using Models;

string slackFileContents = File.ReadAllText("data/parsed_messages.json");
List<ParsedMessage> parsedMessages = JsonSerializer.Deserialize(slackFileContents, ScrandleWrappedJsonContext.Default.ListParsedMessage) ?? [];
var capsherDictByUser = parsedMessages.GroupBy(mes => mes.User).ToFrozenDictionary(g => g.Key);
var capsherDictByDay = parsedMessages.GroupBy(mes => mes.Timestamp.Date).ToFrozenDictionary(g => g.Key);

string communityFileContents = File.ReadAllText("data/community_data.json");
List<ParsedCommunityDay> parsedCommunityData = JsonSerializer.Deserialize(communityFileContents, ScrandleJsonContext.Default.ListParsedCommunityDay) ?? [];
var communityDataDict = parsedCommunityData.ToFrozenDictionary(ks => ks.Day);

Statistics statistics = new();

var capsherDates = parsedMessages.GroupBy(m => m.Timestamp.Date).ToList();
statistics.CapsherAverage = parsedMessages.Average(m => m.Score);
statistics.CommunityAverage = parsedCommunityData.Average(d => d.Avg);
statistics.CapsherAveragePerDay = capsherDates.ToFrozenDictionary(g => g.Key, g => g.ToList().Average(m => m.Score));
statistics.CommunityAveragePerDay = communityDataDict.ToFrozenDictionary(kp => kp.Key, kp => kp.Value.Avg);
statistics.DaysCapsherBeatCommunity = statistics.CapsherAveragePerDay.Count(cday => cday.Value > communityDataDict[DateOnly.FromDateTime(cday.Key)].Avg);
statistics.CapsherHardestDay = DateOnly.FromDateTime(statistics.CapsherAveragePerDay.MinBy(kp => kp.Value).Key);
statistics.CapsherEasiestDay = DateOnly.FromDateTime(statistics.CapsherAveragePerDay.MaxBy(kp => kp.Value).Key);
statistics.CommunityHardestDay = parsedCommunityData.MinBy(kp => kp.Avg)!.Day;
statistics.CommunityEasiestDay = parsedCommunityData.MaxBy(kp => kp.Avg)!.Day;

foreach (var (user, userMessages) in capsherDictByUser.OrderBy(kp => kp.Key))
{
    PlayerStatistics playerStatistic = new()
    {
        DeletedUser = userMessages.First().DeletedUser,
        User = user,
        DaysPlayed = userMessages.Count(),
        Average = userMessages.Average(u => u.Score)
    };

    int currentPerfectStreak = 0;
    DateTime? nextDay = null;
    int currentStreak = 1;
    double currentBestDiff = -10;
    double currentWorstDiff = 10;

    foreach (ParsedMessage message in userMessages)
    {
        var messageDay = DateOnly.FromDateTime(message.Timestamp.Date);

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

        if (message.Score > statistics.CapsherAveragePerDay[message.Timestamp.Date])
        {
            playerStatistic.BeatCapsherAverage++;
        }

        if (message.Score > communityDataDict[messageDay].Avg)
        {
            playerStatistic.BeatCommunity++;
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

        playerStatistic.TeamworkCount += capsherDictByDay[message.Timestamp.Date].Count(m => m.User != message.User && m.Answers == message.Answers);

        var diffToCommunityAvg = communityDataDict[messageDay].Avg - message.Score;

        if (diffToCommunityAvg > currentBestDiff)
        {
            currentBestDiff = diffToCommunityAvg;
            playerStatistic.BestDay = messageDay;
        }
        if (diffToCommunityAvg < currentWorstDiff)
        {
            currentWorstDiff = diffToCommunityAvg;
            playerStatistic.WorstDay = messageDay;
        }
    }

    statistics.PlayerStatistics.Add(playerStatistic);
}

File.WriteAllText("data/statistics.json", JsonSerializer.Serialize(statistics, ScrandleWrappedJsonContext.Default.Statistics));
