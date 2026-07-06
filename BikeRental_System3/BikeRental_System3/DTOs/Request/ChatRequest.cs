namespace BikeRental_System3.DTOs.Request
{
    /// <summary>
    /// The request body Angular sends to POST /api/chat.
    ///
    /// Phase 1: { "message": "Hello" }
    /// Phase 2: { "message": "Hello", "conversationId": "3f2504e0-..." }
    ///
    /// ConversationId flow:
    ///   First message  → Angular sends null / omits the field
    ///                  → Controller creates a new conversation
    ///                  → Response includes the new ConversationId
    ///   Next messages  → Angular sends the received ConversationId
    ///                  → Controller retrieves the existing history
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// The user's typed message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the ongoing conversation.
        /// Null on the first message — controller creates a new conversation.
        /// Required on subsequent messages to retrieve history from memory.
        /// </summary>
        public string? ConversationId { get; set; }
    }
}
