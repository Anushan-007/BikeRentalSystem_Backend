using BikeRental_System3.DTOs.Request;
using BikeRental_System3.IService;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental_System3.Controllers
{
    /// <summary>
    /// Handles AI chat requests from the Angular frontend.
    ///
    /// Responsibility of this controller (and ONLY this):
    ///   1. Receive the HTTP request from Angular.
    ///   2. Validate the input.
    ///   3. Delegate to IOpenAIService.
    ///   4. Return the HTTP response.
    ///
    /// The controller does NOT know about HttpClient, OpenAI, or JSON serialization.
    /// That is the service's responsibility.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        // ── Dependencies ──────────────────────────────────────────────────────

        // IOpenAIService is the interface — never the concrete OpenAIService class.
        // This is Dependency Inversion: depend on abstraction, not implementation.
        private readonly IOpenAIService _openAIService;

        // ILogger<ChatController> is built into ASP.NET Core — no extra registration needed.
        // It writes to the console during development so we can trace errors.
        private readonly ILogger<ChatController> _logger;

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// ASP.NET Core DI resolves IOpenAIService → OpenAIService automatically
        /// because we registered it in Program.cs with AddHttpClient<IOpenAIService, OpenAIService>().
        /// </summary>
        public ChatController(IOpenAIService openAIService, ILogger<ChatController> logger)
        {
            _openAIService = openAIService;
            _logger        = logger;
        }

        // ── Endpoints ─────────────────────────────────────────────────────────

        /// <summary>
        /// POST /api/chat
        ///
        /// Request body:  { "message": "Hello" }
        /// Response body: { "reply": "Hello! How can I help with bike rental?" }
        ///
        /// HTTP Status Codes:
        ///   200 OK          — successful AI response
        ///   400 Bad Request — empty message sent
        ///   502 Bad Gateway — OpenAI API call failed (network/auth error)
        ///   500 Internal    — unexpected server error
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            // ── Validation ────────────────────────────────────────────────────
            // Reject empty messages before calling the AI service.
            // This saves an unnecessary API call and returns a clear error to Angular.
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message cannot be empty." });
            }

            try
            {
                // ── Call Service ──────────────────────────────────────────────
                // This is the ONLY line the controller needs — delegate everything to the service.
                // await suspends execution here until OpenAI responds.
                var response = await _openAIService.GetChatResponseAsync(request.Message);

                // ── Return Success ────────────────────────────────────────────
                // Ok() serializes ChatResponse to: { "reply": "..." }
                // Angular receives this and displays it in the chat UI.
                return Ok(response);
            }
            catch (HttpRequestException ex)
            {
                // HttpRequestException is thrown by OpenAIService when the OpenAI
                // API returns a non-success status (401, 429, 500, etc.).
                // 502 Bad Gateway = our server called an upstream service (OpenAI) and it failed.
                _logger.LogError(ex, "OpenAI API call failed for message: {Message}", request.Message);
                return StatusCode(502, new { error = "AI service is unavailable. Please try again later." });
            }
            catch (Exception ex)
            {
                // Catch-all for any unexpected error (serialization bug, null ref, etc.).
                // We log the full exception but return only a generic message to the client
                // to avoid leaking internal details.
                _logger.LogError(ex, "Unexpected error in ChatController.");
                return StatusCode(500, new { error = "An unexpected error occurred. Please try again." });
            }
        }
    }
}
