#!/usr/bin/env dotnet

#:include Gateways.cs
#:package Microsoft.Extensions.Configuration.Json@10.0.13

using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ScrandleWrapped;
using Models;

IConfigurationBuilder builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json");

IConfiguration config = builder.Build();

// Get Slack data
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Fetching slack data");
Console.ResetColor();
SlackGateway slackGateway = new(config);

List<UserResult> users = await slackGateway.GetUsers();
Dictionary<string, string> userLookup = users.ToDictionary(u => u.ID, u => u.Profile.RealName);

bool keepParsing = true;
List<RawMessage> messages = [];
DateTimeOffset? latest = null;

// Check if we already have data and add on to it
if (File.Exists("data/raw_messages.json"))
{
    string fileContents = File.ReadAllText("data/raw_messages.json");
    messages = JsonSerializer.Deserialize(fileContents, ScrandleWrappedJsonContext.Default.ListRawMessage) ?? [];

    latest = messages.First().Timestamp;
}

int numMessages = 0;
do
{
    SlackResponse response = await slackGateway.GetNextSetOfData();

    numMessages += response.Messages.Count;
    Console.WriteLine($"Progress: {numMessages}");

    foreach (Message message in response.Messages)
    {
        if (message.User == string.Empty || message.User == "USLACKBOT")
        {
            // Ignore Scrandlebot and Slackbot
            continue;
        }

        if (double.TryParse(message.TS, out double parsedTimestamp))
        {
            bool deletedUser = !userLookup.TryGetValue(message.User, out string? username);
            RawMessage rawMessage = new()
            {
                DeletedUser = deletedUser,
                User = username ?? message.User,
                Text = message.Text,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds((long)parsedTimestamp).ToLocalTime()
            };

            if (rawMessage.Timestamp.Year < DateTimeOffset.Now.Year || rawMessage.Timestamp == latest)
            {
                // Only parse this year
                keepParsing = false;
                break;
            }

            messages.Add(rawMessage);
        }
        else
        {
            throw new Exception($"Could not parse '{message.TS}' as a timestamp.");
        }
    }
}
while (keepParsing);

File.WriteAllText("data/raw_messages.json", JsonSerializer.Serialize([.. messages.OrderByDescending(m => m.Timestamp)], ScrandleWrappedJsonContext.Default.ListRawMessage));

// Get Scrandle data
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Fetching community data, this takes a bit");
Console.ResetColor();
ScrandleGateway scrandleGateway = new();

DateOnly firstDayOfYear = new(DateTime.Today.Year, 1, 1);
List<(DateOnly, Task<CommunityDayData> communityTask, Task<MatchupDayData> matchupTask)> httpTasks = [.. Enumerable
    .Range(0, DateTime.Today.DayOfYear - 1)
    .Select(firstDayOfYear.AddDays)
    .Select(d => (d, scrandleGateway.GetDayCommunityData(d), scrandleGateway.GetDayMatchupData(d)))];
await Task.WhenAll(httpTasks.SelectMany<(DateOnly, Task<CommunityDayData> communityTask, Task<MatchupDayData> matchupTask), Task>(t => [t.communityTask, t.matchupTask]));

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Saving community data");
Console.ResetColor();

List<CommunityDayData> communityDayDatas = [.. httpTasks.Select(t => t.communityTask.Result)];
List<MatchupDayData> matchupDayDatas = [.. httpTasks.Select(t => t.matchupTask.Result)];

File.WriteAllText("data/community_data.json", JsonSerializer.Serialize([.. communityDayDatas.OrderByDescending(m => m.Day)], ScrandleJsonContext.Default.ListCommunityDayData));

File.WriteAllText("data/matchup_data.json", JsonSerializer.Serialize([.. matchupDayDatas.OrderByDescending(m => m.Day)], ScrandleJsonContext.Default.ListMatchupDayData));
