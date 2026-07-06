using System.Text.Json.Serialization;

namespace BikeRental_System3.Models
{
    /// <summary>
    /// The JSON body we POST to https://api.openai.com/v1/chat/completions.
    /// OpenAI expects snake_case keys, so we use [JsonPropertyName] to map
    /// C# PascalCase properties to the correct JSON field names.
    /// </summary>
    public class OpenAIChatRequest
    {
        /// <summary>
        /// Which OpenAI model to use. Read from appsettings.json → "OpenAI:Model".
        /// Example: "gpt-4o-mini"
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// The conversation messages sent to OpenAI.
        /// At minimum two messages: a "system" prompt and the "user" message.
        /// </summary>
        [JsonPropertyName("messages")]
        public List<OpenAIMessage> Messages { get; set; } = new();

        /// <summary>
        /// Maximum number of tokens in the AI's reply.
        /// 500 tokens ≈ ~375 English words. Prevents unexpectedly long (and expensive) replies.
        /// </summary>
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 500;

        /// <summary>
        /// Controls randomness. 0.0 = deterministic, 1.0 = very creative.
        /// 0.7 is a good balance for a customer support chatbot.
        /// </summary>
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;
    }

    /// <summary>
    /// A single message in the conversation.
    /// OpenAI roles: "system" (instructions), "user" (human), "assistant" (AI reply).
    /// </summary>
    public class OpenAIMessage
    {
        /// <summary>
        /// The speaker role: "system", "user", or "assistant".
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// The actual text content of the message.
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
