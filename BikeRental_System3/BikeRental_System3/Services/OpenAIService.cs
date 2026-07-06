using BikeRental_System3.IService;
using BikeRental_System3.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BikeRental_System3.Services
{
    /// <summary>
    /// Handles all communication with the OpenAI Chat Completions REST API.
    ///
    /// Phase 1: received a single string → built 2 messages (system + user).
    /// Phase 2: receives the full conversation history → builds system + all history.
    ///
    /// This class is the ONLY place in the backend that knows about OpenAI.
    /// It returns a plain string (the reply text) — not a DTO.
    /// Building the response DTO is the controller's responsibility.
    /// </summary>
    public class OpenAIService : IOpenAIService
    {
        // ── Dependencies ──────────────────────────────────────────────────────

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        // ── Cached Config Values ──────────────────────────────────────────────

        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _baseUrl;

        // Reused across all calls — avoids allocating new options on every request.
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // The system prompt is fixed — it defines the AI's personality for this app.
        // Stored as a constant so it's easy to find and update.
        private const string SystemPrompt =
            "You are a helpful customer support assistant for Heaven Bike Rental System. " +
            "Help users with bike availability, rental pricing, booking process, " +
            "return policies, and general inquiries. " +
            "Be friendly, concise, and professional. " +
            "Remember information shared by the user during this conversation. " +
            "If asked about anything unrelated to bike rentals, politely redirect.";

        // ── Constructor ───────────────────────────────────────────────────────

        public OpenAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient    = httpClient;
            _configuration = configuration;

            _apiKey = _configuration["OpenAI:ApiKey"]
                      ?? throw new InvalidOperationException(
                             "OpenAI:ApiKey is missing from appsettings.json.");

            _model   = _configuration["OpenAI:Model"]   ?? "gpt-4o-mini";
            _baseUrl = _configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1/chat/completions";
        }

        // ── Public Method (implements IOpenAIService) ─────────────────────────

        /// <summary>
        /// Builds the OpenAI messages array from the conversation history and calls the API.
        ///
        /// Messages sent to OpenAI (Phase 2):
        ///   [0] system  → SystemPrompt (always first)
        ///   [1] user    → "Hello"
        ///   [2] assistant → "Hello! How can I help?"
        ///   [3] user    → "My name is Anushan"
        ///   [4] assistant → "Nice to meet you, Anushan!"
        ///   [5] user    → "What is my name?"   ← current message (already in history)
        ///
        /// GPT reads the entire array in order — this is how it "remembers".
        /// </summary>
        public async Task<string> GetChatResponseAsync(IReadOnlyList<ChatMessage> conversationHistory)
        {
            // ── Step 1: Build the messages array ──────────────────────────────
            // Start with the system prompt — always position [0].
            var openAIMessages = new List<OpenAIMessage>
            {
                new OpenAIMessage { Role = "system", Content = SystemPrompt }
            };

            // Append every message from the conversation history.
            // conversationHistory already contains the latest user message
            // (added by ChatController before calling this method).
            foreach (var msg in conversationHistory)
            {
                openAIMessages.Add(new OpenAIMessage
                {
                    Role    = msg.Role,     // "user" or "assistant"
                    Content = msg.Content
                });
            }

            // ── Step 2: Build the request object ──────────────────────────────
            var openAIRequest = new OpenAIChatRequest
            {
                Model       = _model,
                Messages    = openAIMessages,
                MaxTokens   = 500,
                Temperature = 0.7
            };

            // ── Step 3: Serialize to JSON ─────────────────────────────────────
            var jsonBody    = JsonSerializer.Serialize(openAIRequest);
            var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // ── Step 4: Set Authorization header ─────────────────────────────
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            // ── Step 5: POST to OpenAI ────────────────────────────────────────
            var httpResponse = await _httpClient.PostAsync(_baseUrl, httpContent);

            // ── Step 6: Handle errors ─────────────────────────────────────────
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorBody = await httpResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"OpenAI API returned {(int)httpResponse.StatusCode} " +
                    $"({httpResponse.ReasonPhrase}): {errorBody}");
            }

            // ── Step 7: Deserialize response ──────────────────────────────────
            var responseBody   = await httpResponse.Content.ReadAsStringAsync();
            var openAIResponse = JsonSerializer.Deserialize<OpenAIChatResponse>(
                                     responseBody, _jsonOptions);

            // ── Step 8: Extract and return reply text ─────────────────────────
            // Returns plain string — the controller wraps it in ChatResponse DTO.
            return openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim()
                   ?? "I'm sorry, I could not generate a response. Please try again.";
        }
    }
}
