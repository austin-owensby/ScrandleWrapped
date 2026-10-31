# Scrandle Wrapped

This is my tools to process Slack submitted Scrandle data to produce a fun end of year summary personalized for individuals

## Running

This uses C#s newer method of running scripts individually outside of a csproj.

For some of these features .NET 11 is required.

To run a script run `dotnet run <C# file>`

## Scripts

### Cache Data

This hits Slack's endpoint to scrape data and records it all in a `raw_messages.json`. It also fetches the community data from scrandle.com and records it to `community_data.json`.
I've manually replaced the names for deactivated accounts since there were only 5, this could be a future enhancement.

To use this, copy the `appsettings.json` file and rename it to `appsettings.Development.json` and fill in the values. You can grab most of these by visiting Slack from your browser, open your browser's DevTools and inspect some of the Network calls made while navigating the desired channel.

### Review Data

Because some people enter scores that aren't what they thought (For example, going for an evil run shooting for 0/10) the ReviewMessages.html allows you to import the `raw_messages.json` file and take notes around those examples as well as delete messages that aren't scores.

This exports a `reviewed_messages.json` file with the notes and removed messages.

### Parse Data

This script takes in the `reviewed_messages.json` file, parses the data into a consistent format prompting the user for input.
This consistent format will allow us to calculate statistics on the days.

This outputs a `parsed_messages.json` file.

### Generate Statistics

This takes in the `parsed_messages.json` file and generates various statistics and outputs them to `statistics.json`

## TODO

- Grab more community statistics and matchups for statistics
- Grab Scrandlebot stats
- Flesh out statistics
  - Without community statistics, but with Scrandlebot
    - Scrandlebot average overall
    - Days beat the Scrandlebot
    - Days Capsher average beat Scrandlebot
  - With community statistics
    - Best match up (Correct answer on difficult matchup)
    - Worst match up (Incorrect answer on easy matchup)
- Build a GitHub site to display data and show personalized stats
  - Select yourself
  - Show Capsher stats (Number of puzzles, days Capsher beat the community, Capsher best day, Capsher worst day)
  - Show individual stats as a build up (Days played, average, best day, worst day, best matchup, worst matchup)
  - Give out awards for special achievements, choose most impressive and try to keep unique
  - Stats page (Capsher stats, individual stats, percent vs. total, export of raw data)
  - Share button
- Finalize results on 11/30 and update Scrandlebot to post a link.

### Scrandle data
Community result data (scrandle.com/results/DAY) comes in the form:
```json
{
    "4": 0.03439,
    "5": 0.08846,
    "3": 0.00907,
    "7": 0.25761,
    "6": 0.18387,
    "8": 0.24723,
    "9": 0.13807,
    "10": 0.03844,
    "1": 0.00074,
    "2": 0.00211,
    "0": 0,
    "avg": 7.12347
}
```

Figure out how to interpret what all the fields but `avg` mean.

If needed, price rating info is available at scrandle.com/history/DAY. There's also an `elo` field, but I don't understand what it means.
