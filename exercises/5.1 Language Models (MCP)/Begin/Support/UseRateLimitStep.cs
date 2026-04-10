using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;

namespace Microsoft.Extensions.AI;

public static class UseRateLimitStep
{
    // This is an extension method that lets you add UseLanguageChatClient into a pipeline
    public static ChatClientBuilder UseRateLimit(this ChatClientBuilder builder, TimeSpan window)
    {
        return builder.Use(inner => new RateLimitedChatClient(inner, window));
    }

    // This is the actual middleware implementation
    private class RateLimitedChatClient(IChatClient next, TimeSpan window) : DelegatingChatClient(next)
    {
        // Note that this rate limit is enforced globally across all users.
        // It's not a separate rate limit for each user. We could do that but the implementation would be a bit different.
        RateLimiter rateLimiter = new FixedWindowRateLimiter(new() { Window = window, QueueLimit = 1, PermitLimit = 1 });

        public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            using var lease = await rateLimiter.AcquireAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!lease.IsAcquired)
            {
                // want to stop and tell the user "I'm busy" or something
                Console.WriteLine("Sorry, I'm too busy - please ask again later");
                throw new InvalidOperationException("Unable to acquire lease");
            }

            return await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
            // Add an extra prompt
            //var promptAugmentation = new ChatMessage(ChatRole.User, $"Always reply in the language {language}");
            //return base.GetResponseAsync([.. messages, promptAugmentation], options, cancellationToken);
        }

        public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var lease = await rateLimiter.AcquireAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!lease.IsAcquired)
            {
                // want to stop and tell the user "I'm busy" or something
                Console.WriteLine("Sorry, I'm too busy - please ask again later");
                throw new InvalidOperationException("Unable to acquire lease");
            }

            await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken).ConfigureAwait(false))
            {
                yield return update;
            }
        }

        //public override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        //{
            //var promptAugmentation = new ChatMessage(ChatRole.User, $"Always reply in the language {language}");
            //return base.GetStreamingResponseAsync([.. messages, promptAugmentation], options, cancellationToken);
        //}

    }
}
