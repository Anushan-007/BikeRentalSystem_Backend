using BikeRental_System3.DTOs.Response;

namespace BikeRental_System3.IService
{
    /// <summary>
    /// Contract for communicating with the OpenAI Chat Completions API.
    /// ChatController depends on this interface, not on the concrete OpenAIService class.
    /// This follows the Dependency Inversion Principle (SOLID - D).
    /// </summary>
    public interface IOpenAIService
    {
        /// <summary>
        /// Sends the user's message to OpenAI and returns the AI-generated reply.
        /// </summary>
        /// <param name="userMessage">The text the user typed in the chat UI.</param>
        /// <returns>A ChatResponse containing the AI reply text.</returns>
        Task<ChatResponse> GetChatResponseAsync(string userMessage);
    }
}
