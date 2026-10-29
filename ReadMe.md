# Scrandle Wrapped

This is my tools to process Slack submitted Scrandle data to produce a fun end of year summary personalized for individuals

## Running

This uses C#s newer method of running scripts individually outside of a csproj.

For some of these features .NET 11 is required.

To run a script run `dotnet run <C# file>`

## Scripts

### Cache Data

This hits Slack's endpoint to scrape data and records it all in a `raw_messages.json`.
I've manually replaced the names for deactivated accounts since there were only 5, this could be a future enhancement.

To use this, copy the appsettings.json file and rename it to appsettings.Development.json and fill in the values. You can grab most of these by visiting Slack from your browser, open your browser's DevTools and inspect some of the Network calls made while navigating the desired channel.

### Review Data

Because some people enter scores that aren't what they thought (For example, going for an evil run shooting for 0/10) the ReviewMessages.html allows you to import the `raw_messages.json` file and take notes around those examples as well as delete messages that aren't scores.

This exports a `reviewed_messages.json` file with the notes and removed messages.

### Parse Data

This script takes in the `reviewed_messages.json` file, parses the data into a consistent format prompting the user for input.
This consistent format will allow us to calculate statistics on the days.

This outputs a `parsed_messages.json` file.

## TODO

- Update scripts to account for in progress data so that I don't have to do everything fresh on November 30th.
- Analyze data and start generating statistics
- Build a GitHub site to display data and show personalized stats
