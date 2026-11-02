#:include Models.cs

using System.Text.Json;
using Models;
using Microsoft.Extensions.Configuration;

namespace ScrandleWrapped;

public class SlackGateway
{
    private string? _cursor = null;
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public SlackGateway(IConfiguration config)
    {
        _config = config;
        _client = new();
        _client.DefaultRequestHeaders.Add("Cookie", config["Cookie"]);
    }

    public async Task<SlackResponse> GetNextSetOfData()
    {
        Dictionary<string, string> formData = new()
        {
            {"token", _config["Token"]!},
            {"channel", _config["Channel"]!}
        };

        if (_cursor != null)
        {
            formData["cursor"] = _cursor;
        }

        FormUrlEncodedContent content = new(formData);

        HttpResponseMessage response = await _client.PostAsync($"https://{_config["Organization"]}.slack.com/api/conversations.history", content);

        response.EnsureSuccessStatusCode();

        SlackResponse responseBody = await response.Content.ReadFromJsonAsync(SlackJsonContext.Default.SlackResponse) ?? throw new Exception("Unable to parse messages response.");

        _cursor = responseBody.ResponseMetadata?.NextCursor;

        return responseBody;
    }

    public async Task<List<UserResult>> GetUsers()
    {
        UserListRequest request = new()
        {
            Token = _config["Token"]!,
            Channels = [_config["Channel"]!],
            Filter = "everyone AND NOT bots AND NOT apps",
            Count = 100
        };

        StringContent content = new(JsonSerializer.Serialize(request, SlackJsonContext.Default.UserListRequest));

        HttpResponseMessage response = await _client.PostAsync($"https://edgeapi.slack.com/cache/{_config["TeamId"]}/users/list", content);

        response.EnsureSuccessStatusCode();

        UserResponse responseBody = await response.Content.ReadFromJsonAsync(SlackJsonContext.Default.UserResponse) ?? throw new Exception("Unable to parse user response.");

        return responseBody.Results;
    }
}

public class ScrandleGateway()
{
    private readonly HttpClient _client = new();

    public async Task<CommunityDayData> GetDayCommunityData(DateOnly day)
    {
        HttpResponseMessage response = await _client.GetAsync($"https://scrandle.com/results/{day:yyyy-MM-dd}");

        response.EnsureSuccessStatusCode();

        CommunityDayData responseBody = await response.Content.ReadFromJsonAsync(ScrandleJsonContext.Default.CommunityDayData) ?? throw new Exception("Unable to parse scrandle response.");
        responseBody.Day = day;

        return responseBody;
    }

    public async Task<MatchupDayData> GetDayMatchupData(DateOnly day)
    {
        HttpResponseMessage response = await _client.GetAsync($"https://scrandle.com/history/{day:yyyy-MM-dd}");

        response.EnsureSuccessStatusCode();

        MatchupDayData responseBody = await response.Content.ReadFromJsonAsync(ScrandleJsonContext.Default.MatchupDayData) ?? throw new Exception("Unable to parse scrandle response.");
        responseBody.Day = day;

        return responseBody;
    }
}
