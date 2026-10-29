#!/usr/bin/env dotnet

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

string fileContents = File.ReadAllText("reviewed_messages.json");
List<ReviewedMessage> reviewedMessages = JsonSerializer.Deserialize(fileContents, ReviewedJsonContext.Default.ListReviewedMessage) ?? [];

List<ParsedMessage> parsedMessages = [];

foreach (ReviewedMessage message in reviewedMessages)
{
    ParsedMessage parsedMessage = new()
    {
        DeletedUser = message.DeletedUser,
        User = message.User,
        RawText = message.Text,
        Timestamp = message.Timestamp
    };

    string? parsedValue = AnswerRegex.Match(message.Text).Groups.Values.FirstOrDefault()?.Value;
    short? parsedAnswer = null;

    if (!string.IsNullOrEmpty(parsedValue))
    {
        parsedValue = parsedValue.Replace(":large_green_square:", "1").Replace(":large_red_square:", "0");
        parsedAnswer = short.Parse(parsedValue, NumberStyles.BinaryNumber);
    }

    if (message.Note != string.Empty)
    {
        // Read Input
        Console.WriteLine($"Message has notes '{(parsedAnswer == null ? message.Text : parsedAnswer!.Value.ToString("B10"))}' Inverted: {message.Inverse}, Note: {message.Note}");
        string? input = Console.ReadLine();
        parsedMessage.Answers = short.Parse(input!, NumberStyles.BinaryNumber);

        if (message.Inverse)
        {
            parsedMessage.Answers ^= 0b1111111111;
        }
    }
    else
    {
        if (parsedAnswer == null)
        {
            Console.WriteLine($"Could not parse match for '{message.Text}' Inverted: {message.Inverse}");
            string? input = Console.ReadLine();
            parsedMessage.Answers = short.Parse(input!, NumberStyles.BinaryNumber);
        }
        else
        {
            parsedMessage.Answers = parsedAnswer.Value;

            if (message.Inverse)
            {
                parsedMessage.Answers ^= 0b1111111111;
            }
        }
    }

    parsedMessages.Add(parsedMessage);
}

File.WriteAllText("parsed_messages.json", JsonSerializer.Serialize(parsedMessages, ParsedJsonContext.Default.ListParsedMessage));

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
}

[JsonSerializable(typeof(List<ReviewedMessage>))]
internal partial class ReviewedJsonContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<ParsedMessage>))]
internal partial class ParsedJsonContext : JsonSerializerContext
{
}

partial class Program
{
    [GeneratedRegex("(?::large_green_square:|:large_red_square:){10}")]
    private static partial Regex AnswerRegex { get; }
}