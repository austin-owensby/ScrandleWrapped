#!/usr/bin/env dotnet

#:include Models.cs

using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Models;

string fileContents = File.ReadAllText("data/reviewed_messages.json");
List<ReviewedMessage> reviewedMessages = JsonSerializer.Deserialize(fileContents, ScrandleWrappedJsonContext.Default.ListReviewedMessage) ?? [];

List<ParsedMessage> parsedMessages = [];
DateTimeOffset? latest = null;

// Check if we already have data and add on to it
if (File.Exists("data/parsed_messages.json"))
{
    fileContents = File.ReadAllText("data/parsed_messages.json");
    parsedMessages = JsonSerializer.Deserialize(fileContents, ScrandleWrappedJsonContext.Default.ListParsedMessage) ?? [];

    latest = parsedMessages.First().Timestamp;
}

foreach (ReviewedMessage message in reviewedMessages)
{
    // Only parse data we haven't parsed before
    if (message.Timestamp == latest)
    {
        break;
    }

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
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Message has notes '{(parsedAnswer == null ? message.Text : parsedAnswer!.Value.ToString("B10"))}' Inverted: {message.Inverse}, Note: {message.Note}.");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Please input the answers in binary.");
        Console.ResetColor();
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
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Could not parse match for '{message.Text}' Inverted: {message.Inverse}.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Please input the answers in binary.");
            Console.ResetColor();
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

    parsedMessage.Score = BitOperations.PopCount((uint)parsedMessage.Answers);

    parsedMessages.Add(parsedMessage);
}

File.WriteAllText("data/parsed_messages.json", JsonSerializer.Serialize([.. parsedMessages.OrderByDescending(m => m.Timestamp)], ScrandleWrappedJsonContext.Default.ListParsedMessage));


partial class Program
{
    [GeneratedRegex("(?::large_green_square:|:large_red_square:){10}")]
    private static partial Regex AnswerRegex { get; }
}
