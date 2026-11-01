using System.Text.Json.Serialization;

namespace Models;

class Statistics
{
    public List<PlayerStatistics> PlayerStatistics { get; set; } = [];
    public double CapsherAverage { get; set; }
    public double CommunityAverage { get; set; } = new();
    public Dictionary<DateTime, double> CapsherAveragePerDay { get; set; } = [];
    public DateOnly CapsherHardestDay { get; set; } = new();
    public DateOnly CapsherEasiestDay { get; set; } = new();
    public DateOnly CommunityHardestDay { get; set; } = new();
    public DateOnly CommunityEasiestDay { get; set; } = new();
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<PlayerStatistics>))]
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
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public short Answers { get; set; } = 0b0000000000;
    public int Score { get; set; }
}

[JsonSerializable(typeof(List<ParsedMessage>))]
internal partial class ParsedJsonContext : JsonSerializerContext
{
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
[JsonSerializable(typeof(List<ParsedCommunityDay>))]
internal partial class OutputJsonContext : JsonSerializerContext
{
}

class ParsedCommunityDay
{
    public DateOnly Day { get; set; }
    public double Avg { get; set; }
}

public class UserListRequest
{
    public string Token { get; set; } = string.Empty;
    public List<string> Channels { get; set; } = [];
    public string Filter { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class SlackResponse
{
    public ResponseMetadata? ResponseMetadata { get; set; }
    public List<Message> Messages { get; set; } = [];
}

public class ResponseMetadata
{
    public string? NextCursor { get; set; }
}

public class Message
{
    public string User { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string TS { get; set; } = string.Empty;
}

public class UserResponse
{
    public List<UserResult> Results { get; set; } = [];
}

public class UserResult
{
    public string ID { get; set; } = string.Empty;
    public Profile Profile { get; set; } = new();
}

public class Profile
{
    public string RealName { get; set; } = string.Empty;
}

public class CommunityDataDay
{
    public double Avg { get; set; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(CommunityDataDay))]
internal partial class ScrandleJsonContext : JsonSerializerContext
{
}

class ReviewedMessage
{
    public bool DeletedUser { get; set; }
    public string User { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string Note { get; set; } = string.Empty;
    public bool Inverse { get; set; }
}

[JsonSerializable(typeof(List<ReviewedMessage>))]
internal partial class ReviewedJsonContext : JsonSerializerContext
{
}
