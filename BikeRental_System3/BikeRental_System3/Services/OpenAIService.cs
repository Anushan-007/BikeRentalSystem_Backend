using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BikeRental_System3.Services
{
    /// <summary>
    /// Handles all communication with the OpenAI Chat Completions REST API.
    /// This class is the ONLY place in the backend that knows about OpenAI.
    /// ChatController calls this via IOpenAIService — it never touches HttpClient directly.
    /// </summary>
    public class OpenAIService : IOpenAIService
    {
        // ── Dependencies ──────────────────────────────────────────────────────

        // HttpClient injected by IHttpClientFactory via AddHttpClient<> in Program.cs.
        // We do NOT create new HttpClient() here — that causes socket exhaustion.
        private readonly HttpClient _httpClient;

        // IConfiguration reads values from appsettings.json at runtime.
        // We use it to read ApiKey, Model, BaseUrl, TimeoutSeconds.
        private readonly IConfiguration _configuration;

        // ── Cached Config Values ──────────────────────────────────────────────

        // Read once in constructor so we don't read config on every request.
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _baseUrl;

        // JSON serializer options — reused across calls for performance.
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true  // allows OpenAI's snake_case to map to our PascalCase
        };

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// ASP.NET Core DI automatically calls this constructor.
        /// HttpClient is provided by IHttpClientFactory (registered in Program.cs).
        /// IConfiguration is always available in DI — no registration needed.
        /// </summary>
        public OpenAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient    = httpClient;
            _configuration = configuration;

            // Read config values once. Throw immediately if ApiKey is missing
            // so we get a clear startup error instead of a cryptic runtime failure.
            _apiKey = _configuration["OpenAI:ApiKey"]
                      ?? throw new InvalidOperationException(
                             "OpenAI:ApiKey is missing from appsettings.json. Add your API key.");

            _model   = _configuration["OpenAI:Model"]   ?? "gpt-4o-mini";
            _baseUrl = _configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1/chat/completions";
        }

        // ── Public Method (implements IOpenAIService) ─────────────────────────

        /// <summary>
        /// Sends the user message to OpenAI and returns the AI reply.
        /// </summary>
        public async Task<ChatResponse> GetChatResponseAsync(string userMessage)
        {
            // ── Step 1: Build the OpenAI request body ─────────────────────────
            // OpenAI expects a list of messages. We always send two:
            //   1. "system" → secret instructions that shape the AI's personality
            //   2. "user"   → the actual message the user typed
            var openAIRequest = new OpenAIChatRequest
            {
                Model      = _model,
                MaxTokens  = 500,
                Temperature = 0.7,
                Messages   = new List<OpenAIMessage>
                {
                    new OpenAIMessage
                    {
                        Role    = "system",
                        Content = "You are a helpful customer support assistant for Heaven Bike Rental System. " +
                                  "Help users with bike availability, rental pricing, booking process, " +
                                  "return policies, and general inquiries. " +
                                  "Be friendly, concise, and professional. " +
                                  "If asked about anything unrelated to bike rentals, politely redirect."
                    },
                    new OpenAIMessage
                    {
                        Role    = "user",
                        Content = userMessage
                    }
                }
            };

            // ── Step 2: Serialize the request object to JSON ──────────────────
            // System.Text.Json converts our C# object to the JSON string
            // that OpenAI expects. [JsonPropertyName] on the model properties
            // ensures "max_tokens" (not "MaxTokens") appears in the JSON.
            var jsonBody    = JsonSerializer.Serialize(openAIRequest);
            var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // ── Step 3: Set the Authorization header ──────────────────────────
            // OpenAI requires: Authorization: Bearer sk-...
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            // ── Step 4: POST to OpenAI ────────────────────────────────────────
            // PostAsync sends the HTTP request and waits for the full response.
            // await means this thread is NOT blocked while waiting — other
            // requests can be processed by the server in the meantime.
            var httpResponse = await _httpClient.PostAsync(_baseUrl, httpContent);

            // ── Step 5: Handle HTTP-level errors ──────────────────────────────
            // 401 = wrong API key
            // 429 = rate limit hit
            // 500 = OpenAI server error
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorBody = await httpResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"OpenAI API returned {(int)httpResponse.StatusCode} " +
                    $"({httpResponse.ReasonPhrase}): {errorBody}");
            }

            // ── Step 6: Read and deserialize the response JSON ────────────────
            var responseBody   = await httpResponse.Content.ReadAsStringAsync();
            var openAIResponse = JsonSerializer.Deserialize<OpenAIChatResponse>(
                                     responseBody, _jsonOptions);

            // ── Step 7: Extract the reply text ────────────────────────────────
            // OpenAI returns: choices[0].message.content
            // ?. navigates safely — if anything is null, we return a fallback.
            var replyText = openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim()
                            ?? "I'm sorry, I could not generate a response. Please try again.";

            // ── Step 8: Return our clean ChatResponse DTO ─────────────────────
            return new ChatResponse { Reply = replyText };
        }
    }
}
