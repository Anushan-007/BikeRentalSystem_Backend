namespace BikeRental_System3.DTOs.Request
{
    /// <summary>
    /// The request body Angular sends to POST /api/chat.
    /// Angular serializes { "message": "Hello" } into this class.
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// The user's typed message. Defaults to empty string to avoid null checks.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
