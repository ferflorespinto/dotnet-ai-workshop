using System.Net.Http.Json;

namespace Classification;

public static class HackerNews
{
    public static async Task<HNStory[]> GetTopStoriesAsync(int count)
    {
        const string baseUrl = "https://hacker-news.firebaseio.com/v0";
        using var client = new HttpClient();
        var storyIds = await client.GetFromJsonAsync<int[]>($"{baseUrl}/topstories.json");
        var resultTasks = storyIds!.Take(count).Select(id => client.GetFromJsonAsync<HNStory>($"{baseUrl}/item/{id}.json")).ToArray();
        return (await Task.WhenAll(resultTasks))!;
    }
}

public record HNStory (int Id, string Title);