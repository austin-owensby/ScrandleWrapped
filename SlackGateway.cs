using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ScrandleWrapped;

public class SlackGateway(IConfiguration config)
{
    private string? Cursor = null;

    public async Task<SlackResponse> GetNextSetOfData()
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("Cookie", config["Cookie"]);

        Dictionary<string, string> formData = new()
        {
            {"token", config["Token"]!},
            {"channel", config["Channel"]!}
        };

        if (Cursor != null)
        {
            formData["cursor"] = Cursor;
        }

        FormUrlEncodedContent content = new(formData);

        HttpResponseMessage response = await client.PostAsync($"https://{config["Organization"]}.slack.com/api/conversations.history", content);

        response.EnsureSuccessStatusCode();

        SlackResponse responseBody = await response.Content.ReadFromJsonAsync(SlackJsonContext.Default.SlackResponse) ?? throw new Exception("Unable to parse messages response.");

        Cursor = responseBody.ResponseMetadata?.NextCursor;

        return responseBody;
    }

    public async Task<List<UserResult>> GetUsers()
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("Cookie", config["Cookie"]);

        UserListRequest request = new()
        {
            Token = config["Token"]!,
            Channels = [config["Channel"]!],
            Filter = "everyone AND NOT bots AND NOT apps",
            Count = 100
        };

        StringContent content = new(JsonSerializer.Serialize(request, SlackJsonContext.Default.UserListRequest));

        HttpResponseMessage response = await client.PostAsync($"https://edgeapi.slack.com/cache/{config["TeamId"]}/users/list", content);

        response.EnsureSuccessStatusCode();

        UserResponse responseBody = await response.Content.ReadFromJsonAsync(SlackJsonContext.Default.UserResponse) ?? throw new Exception("Unable to parse user response.");

        return responseBody.Results;
    }
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