#!/usr/bin/env dotnet

#:include SlackGateway.cs
#:package Microsoft.Extensions.Configuration.Json@10.0.13

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using ScrandleWrapped;

IConfigurationBuilder builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json");

IConfiguration config = builder.Build();

SlackGateway slackGateway = new(config);

List<UserResult> users = await slackGateway.GetUsers();
Dictionary<string, string> userLookup = users.ToDictionary(u => u.ID, u => u.Profile.RealName);

bool keepParsing = true;
List<ParsedMessage> messages = [];
DateTimeOffset? latest = null;

// Check if we already have data and add on to it
if (File.Exists("data/raw_messages.json"))
{
    string fileContents = File.ReadAllText("data/raw_messages.json");
    messages = JsonSerializer.Deserialize(fileContents, OutputJsonContext.Default.ListParsedMessage) ?? [];

    latest = messages.First().Timestamp;
}

do
{
    SlackResponse response = await slackGateway.GetNextSetOfData();

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
            ParsedMessage parsedMessage = new()
            {
                DeletedUser = deletedUser,
                User = username ?? message.User,
                Text = message.Text,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds((long)parsedTimestamp).ToLocalTime()
            };

            if (parsedMessage.Timestamp.Year < DateTimeOffset.Now.Year || parsedMessage.Timestamp == latest)
            {
                // Only parse this year
                keepParsing = false;
                break;
            }

            messages.Add(parsedMessage);
        }
        else
        {
            throw new Exception($"Could not parse '{message.TS}' as a timestamp.");
        }
    }
}
while (keepParsing);

File.WriteAllText("data/raw_messages.json", JsonSerializer.Serialize(messages.OrderByDescending(m => m.Timestamp).ToList(), OutputJsonContext.Default.ListParsedMessage));

class ParsedMessage
{
    public bool DeletedUser { get; set; }
    public string User { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(SlackResponse))]
[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(UserListRequest))]
[JsonSerializable(typeof(List<UserResult>))]
internal partial class SlackJsonContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<ParsedMessage>))]
internal partial class OutputJsonContext : JsonSerializerContext
{
}
