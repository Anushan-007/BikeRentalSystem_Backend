using BikeRental_System3.Models;

namespace BikeRental_System3.IService
{
    /// <summary>
    /// Contract for communicating with the OpenAI Chat Completions API.
    ///
    /// Phase 1 signature: GetChatResponseAsync(string userMessage)
    ///   — only sent the current message, no history.
    ///
    /// Phase 2 signature: GetChatResponseAsync(IReadOnlyList&lt;ChatMessage&gt; history)
    ///   — sends the complete conversation (user + assistant turns).
    ///   — the service prepends the system prompt and builds the full messages array.
    ///
    /// Why return string instead of ChatResponse?
    ///   The service's job is ONLY to call OpenAI and return the raw reply text.
    ///   Building ChatResponse (with ConversationId) is the controller's job.
    ///   Single Responsibility Principle: each class does exactly one thing.
    /// </summary>
    public interface IOpenAIService
    {
        /// <summary>
        /// Sends the full conversation history to OpenAI and returns the AI reply text.
        /// The system prompt is added by the service — not by the caller.
        /// </summary>
        /// <param name="conversationHistory">
        ///   All user and assistant messages in chronological order.
        ///   The service prepends the system message automatically.
        /// </param>
        /// <returns>The plain reply text from OpenAI (not wrapped in any DTO).</returns>
        Task<string> GetChatResponseAsync(IReadOnlyList<ChatMessage> conversationHistory);
    }
}
