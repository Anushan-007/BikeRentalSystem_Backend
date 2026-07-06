namespace BikeRental_System3.DTOs.Response
{
    /// <summary>
    /// The response body the API sends back to Angular.
    /// Angular reads { "reply": "..." } and displays it in the chat UI.
    /// </summary>
    public class ChatResponse
    {
        /// <summary>
        /// The AI-generated reply extracted from OpenAI's response.
        /// </summary>
        public string Reply { get; set; } = string.Empty;
    }
}
