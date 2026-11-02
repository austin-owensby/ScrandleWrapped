using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace Models;

// From the Slack API
// Messages
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

// Users
public class UserListRequest
{
    public string Token { get; set; } = string.Empty;
    public List<string> Channels { get; set; } = [];
    public string Filter { get; set; } = string.Empty;
    public int Count { get; set; }
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

// Extracted from Slack Message
class RawMessage
{
    public bool DeletedUser { get; set; }
    public string User { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
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

class ParsedMessage
{
    public bool DeletedUser { get; set; }
    public string User { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public short Answers { get; set; } = 0b0000000000;
    public int Score { get; set; }
}

class ScrandlebotPrediction
{
    public DateOnly Day { get; set; }
    public double Prediction { get; set; }
}

// From Scrandle.com
public class CommunityDayData
{
    public DateOnly Day { get; set; }
    public double Avg { get; set; }
}

public class MatchupDayData
{
    public DateOnly Day { get; set; }
    // Expect this to be a 10x2 array
    public List<List<FoodItem>> Data { get; set; } = [];
}

public class FoodItem
{
    // We only care about the rating
    public double Rating { get; set; }
}

// Output stats
class Statistics
{
    public List<PlayerStatistics> PlayerStatistics { get; set; } = [];
    public double CapsherAverage { get; set; }
    public double CommunityAverage { get; set; } = new();
    public FrozenDictionary<DateTime, double> CapsherAveragePerDay { get; set; } = [];
    public FrozenDictionary<DateOnly, double> CommunityAveragePerDay { get; set; } = [];
    public int DaysCapsherBeatCommunity { get; set; }
    public DateOnly CapsherHardestDay { get; set; } = new();
    public DateOnly CapsherEasiestDay { get; set; } = new();
    public DateOnly CommunityHardestDay { get; set; } = new();
    public DateOnly CommunityEasiestDay { get; set; } = new();
}

class PlayerStatistics
{
    public bool DeletedUser { get; set; }
    public string User { get; set; } = string.Empty;
    public int DaysPlayed { get; set; }
    public double Average { get; set; }
    public int BestStreak { get; set; } = 1;
    public int BestPerfectStreak { get; set; }
    public int BeatCapsherAverage { get; set; } = 0;
    public int BeatCommunity { get; set; } = 0;
    public int BlewItCount { get; set; }
    public int TeamworkCount { get; set; }
    public DateOnly BestDay { get; set; } = DateOnly.MinValue;
    public DateOnly WorstDay { get; set; } = DateOnly.MinValue;
    public MatchupStatistic BestMatchup { get; set; } = new();
    public MatchupStatistic WorstMatchup { get; set; } = new();
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

class MatchupStatistic
{
    public DateOnly Day { get; set; } = DateOnly.MinValue;
    public int ItemIndex { get; set; }
    public double Difficulty { get; set; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(SlackResponse))]
[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(UserListRequest))]
[JsonSerializable(typeof(List<UserResult>))]
internal partial class SlackJsonContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, WriteIndented = true)]
[JsonSerializable(typeof(CommunityDayData))]
[JsonSerializable(typeof(List<CommunityDayData>))]
[JsonSerializable(typeof(MatchupDayData))]
[JsonSerializable(typeof(List<MatchupDayData>))]
internal partial class ScrandleJsonContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(List<PlayerStatistics>))]
[JsonSerializable(typeof(Statistics))]
[JsonSerializable(typeof(List<ReviewedMessage>))]
[JsonSerializable(typeof(List<ParsedMessage>))]
[JsonSerializable(typeof(List<RawMessage>))]
[JsonSerializable(typeof(List<ScrandlebotPrediction>))]
internal partial class ScrandleWrappedJsonContext : JsonSerializerContext
{
}
