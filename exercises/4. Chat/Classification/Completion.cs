using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Classification;

public static class Completion
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Category { AI, ProgrammingLanguages, Startups, History, Business, Society };
    private static int _storyCount = 10;

    public static async Task RunAsync(IChatClient chatClient)
    {
        if (_storyCount < 1)
        {
            Console.WriteLine("Request at least 1 story.");
            return;
        }
        HNStory[] stories = await HackerNews.GetTopStoriesAsync(_storyCount);
        List<CategorizedStory> categorizedStoryList = new List<CategorizedStory>();
        foreach (HNStory story in stories)
        {
            // Alternatively I can ask the model to classify all stories at once.
            // This approach however has the slight advantage that it is easier to change how big of a prompt we want it to be at a time.
            // All we have to do is JsonSerializer.Serialize(stories) if we want to prompt for multiple at a time.
            var response = await chatClient.GetResponseAsync<CategorizedStory>(
                $"Extract information from the following story from Hacker News: {story.Title}");

            if (response.TryGetResult(out var responseStory))
            {
                categorizedStoryList.Add(responseStory);
                //Console.WriteLine(JsonSerializer.Serialize(responseStory, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                Console.WriteLine("Response was not in the expected format.");
            }
        }

        // I cared too much about this
        Console.WriteLine($"Here is {(_storyCount > 1 ? "a list of" : string.Empty)} the top {(_storyCount > 1 ? _storyCount : string.Empty)} " +
            $"stor{(_storyCount > 1 ? "ies" : "y")} from Hacker News:");
        Console.WriteLine("===");
        Console.WriteLine();

        var lookup = categorizedStoryList.ToLookup(s => s.Category, s => s.Title);
        foreach (IGrouping<Category, string> group in lookup)
        {
            Console.WriteLine($"{group.Key}:");
            foreach (var story in group)
            {
                Console.WriteLine($"- {story}");
            }
            Console.WriteLine();
        }
    }

    public record CategorizedStory(string Title, Category Category);
}