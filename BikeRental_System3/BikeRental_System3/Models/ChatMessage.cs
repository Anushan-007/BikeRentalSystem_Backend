namespace BikeRental_System3.Models
{
    /// <summary>
    /// Represents a single message in a conversation.
    /// This is a DOMAIN model — it is not tied to OpenAI or any external API.
    ///
    /// Why a separate model from OpenAIMessage?
    ///   OpenAIMessage is an internal HTTP model for the OpenAI REST API.
    ///   ChatMessage is the domain model used by ConversationMemoryService.
    ///   If we switch AI providers in Phase 3, only OpenAIService changes.
    ///   ChatMessage, ConversationMemoryService, and ChatController stay the same.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Who sent the message.
        /// Allowed values: "user" | "assistant"
        /// ("system" messages are added by OpenAIService directly — not stored here)
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// The text of the message.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// When this message was added to the conversation.
        /// Useful for debugging and future Phase (persistence, TTL cleanup).
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
