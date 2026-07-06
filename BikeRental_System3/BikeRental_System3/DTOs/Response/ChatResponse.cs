namespace BikeRental_System3.DTOs.Response
{
    /// <summary>
    /// The response body the API sends back to Angular.
    ///
    /// Phase 1: { "reply": "Hello!" }
    /// Phase 2: { "reply": "Hello!", "conversationId": "3f2504e0-..." }
    ///
    /// Angular stores the ConversationId after the first response
    /// and sends it back in every subsequent ChatRequest.
    /// </summary>
    public class ChatResponse
    {
        /// <summary>
        /// The AI-generated reply text.
        /// </summary>
        public string Reply { get; set; } = string.Empty;

        /// <summary>
        /// The conversation this reply belongs to.
        /// Angular uses this to track which conversation is active.
        /// Always populated — even for the very first message (new conversation created).
        /// </summary>
        public string ConversationId { get; set; } = string.Empty;
    }
}
