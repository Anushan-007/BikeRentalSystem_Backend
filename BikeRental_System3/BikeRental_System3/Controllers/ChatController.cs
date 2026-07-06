using BikeRental_System3.AI.Interfaces;
using BikeRental_System3.DTOs.Request;
using BikeRental_System3.DTOs.Response;
using BikeRental_System3.IService;    // IOpenAIService — removed in Step 7
using Microsoft.AspNetCore.Mvc;

namespace BikeRental_System3.Controllers
{
    /// <summary>
    /// Handles AI chat requests from the Angular frontend.
    ///
    /// Phase 2 responsibilities (controller orchestrates, services execute):
    ///   1. Receive and validate the request.
    ///   2. Resolve the conversation (new or existing).
    ///   3. Save the user message to memory.
    ///   4. Retrieve full conversation history.
    ///   5. Call OpenAIService with the full history.
    ///   6. Save the AI reply to memory.
    ///   7. Return the reply + conversationId to Angular.
    ///
    /// The controller does NOT know about:
    ///   - ConcurrentDictionary (that's ConversationMemoryService)
    ///   - HttpClient or OpenAI API (that's OpenAIService)
    ///   - JSON serialization (that's ASP.NET Core)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        // ── Dependencies ──────────────────────────────────────────────────────

        private readonly IOpenAIService _openAIService;
        private readonly IConversationMemoryService _memoryService;
        private readonly ILogger<ChatController> _logger;

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// ASP.NET Core DI injects all three automatically.
        /// IConversationMemoryService resolves to ConversationMemoryService (Singleton).
        /// IOpenAIService resolves to OpenAIService (managed HttpClient).
        /// </summary>
        public ChatController(
            IOpenAIService openAIService,
            IConversationMemoryService memoryService,
            ILogger<ChatController> logger)
        {
            _openAIService = openAIService;
            _memoryService = memoryService;
            _logger        = logger;
        }

        // ── Endpoints ─────────────────────────────────────────────────────────

        /// <summary>
        /// POST /api/chat
        ///
        /// Phase 1 request:  { "message": "Hello" }
        /// Phase 2 request:  { "message": "Hello", "conversationId": "guid-or-null" }
        ///
        /// Phase 1 response: { "reply": "Hello!" }
        /// Phase 2 response: { "reply": "Hello!", "conversationId": "3f2504e0-..." }
        ///
        /// HTTP Status Codes:
        ///   200 OK            — success
        ///   400 Bad Request   — empty message
        ///   404 Not Found     — invalid conversationId (not created by this server)
        ///   502 Bad Gateway   — OpenAI API failed
        ///   500 Internal      — unexpected error
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            // ── Step 1: Validate message ──────────────────────────────────────
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message cannot be empty." });
            }

            // ── Step 2: Resolve the conversation ──────────────────────────────
            // If Angular sends no ConversationId (first message), create one.
            // If Angular sends an ID, validate it exists on this server.
            string conversationId;

            if (string.IsNullOrWhiteSpace(request.ConversationId))
            {
                // First message — create a brand new conversation.
                conversationId = _memoryService.CreateConversation();
                _logger.LogInformation("New conversation created: {Id}", conversationId);
            }
            else if (!_memoryService.ConversationExists(request.ConversationId))
            {
                // The client sent an ID, but we don't recognize it.
                // This happens if the server restarted (all memory is lost).
                // Return 404 — Angular will start a new conversation on next send.
                _logger.LogWarning("Unknown conversationId received: {Id}", request.ConversationId);
                return NotFound(new { error = "Conversation not found. Please start a new conversation." });
            }
            else
            {
                // Known conversation — continue it.
                conversationId = request.ConversationId;
            }

            try
            {
                // ── Step 3: Save the user's message to history ────────────────
                // Must happen BEFORE calling OpenAI so the current message
                // is included in the history sent to GPT.
                _memoryService.AddMessage(conversationId, "user", request.Message.Trim());

                // ── Step 4: Retrieve full conversation history ─────────────────
                // Includes all previous turns + the message we just added.
                // OpenAIService will prepend the system prompt to this list.
                var history = _memoryService.GetMessages(conversationId);

                // ── Step 5: Call OpenAI with the full history ─────────────────
                var replyText = await _openAIService.GetChatResponseAsync(history);

                // ── Step 6: Save the AI reply to history ──────────────────────
                // Stored as "assistant" role so GPT sees its own previous replies
                // when the next message comes in.
                _memoryService.AddMessage(conversationId, "assistant", replyText);

                // ── Step 7: Return the response to Angular ────────────────────
                // Angular stores conversationId and sends it back on the next request.
                return Ok(new ChatResponse
                {
                    Reply          = replyText,
                    ConversationId = conversationId
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "OpenAI API call failed. ConversationId: {Id}", conversationId);
                return StatusCode(502, new { error = "AI service is unavailable. Please try again later." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ChatController. ConversationId: {Id}", conversationId);
                return StatusCode(500, new { error = "An unexpected error occurred. Please try again." });
            }
        }

        /// <summary>
        /// DELETE /api/chat/{conversationId}
        ///
        /// Clears the conversation history from memory.
        /// Angular calls this when the user clicks "New Conversation".
        ///
        /// Returns 204 No Content on success.
        /// Returns 204 even if the ID doesn't exist (idempotent — safe to call twice).
        /// </summary>
        [HttpDelete("{conversationId}")]
        public IActionResult ClearConversation(string conversationId)
        {
            _memoryService.ClearConversation(conversationId);
            _logger.LogInformation("Conversation cleared: {Id}", conversationId);

            // 204 No Content — success with no body.
            // Idempotent: calling DELETE on a non-existent ID also returns 204.
            return NoContent();
        }
    }
}
