#pragma warning disable SKEXP0010  // OpenAIPromptExecutionSettings is [Experimental] in SK
#pragma warning disable SKEXP0001  // FunctionChoiceBehavior is [Experimental] in SK

using BikeRental_System3.AI.Interfaces;
using BikeRental_System3.AI.Plugins;
using BikeRental_System3.IService;
using BikeRental_System3.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BikeRental_System3.AI.Services
{
    /// <summary>
    /// Phase 5 — AI pipeline with RAG + Tool Calling (Semantic Kernel Plugins).
    ///
    /// Orchestrates the full request lifecycle on every chat message:
    ///
    ///   [1] Extract last user message
    ///   [2] RAG: IContextProvider.GetContextAsync → document context (pgvector)
    ///   [3] Render system prompt with {{context}}
    ///   [4] Build SK ChatHistory from conversation memory
    ///   [5] Clone Kernel + import per-request Plugin instances (scoped services)
    ///   [6] Call GetChatMessageContentAsync with FunctionChoiceBehavior.Auto()
    ///       SK automatically handles the tool-call loop:
    ///         GPT decides which tool → SK executes → result fed back → final answer
    ///   [7] Return final reply text to ChatController
    ///
    /// WHY kernel.Clone() per request?
    ///   Plugins depend on Scoped services (IBikeService, IUserService, etc.).
    ///   The Singleton Kernel cannot hold Scoped plugin instances permanently.
    ///   kernel.Clone() creates a new Kernel that SHARES the Singleton AI services
    ///   (IChatCompletionService stays the same connection) but has an INDEPENDENT
    ///   plugin collection where fresh scoped plugin instances are attached per request.
    ///   This is the SK-recommended pattern for ASP.NET Core scoped dependencies.
    ///
    /// WHY IServiceScopeFactory?
    ///   BikeRentalChatChain is Singleton. Business services are Scoped.
    ///   Singletons cannot directly hold Scoped services (captive dependency).
    ///   IServiceScopeFactory is itself Singleton and is the correct way for a
    ///   Singleton to create a short-lived scope to resolve Scoped dependencies.
    ///
    /// FunctionChoiceBehavior.Auto():
    ///   GPT autonomously decides whether to call a tool, which tool, and when
    ///   to stop calling tools and return a final answer. Zero if/switch in the
    ///   chain — all decision making belongs to Semantic Kernel + GPT.
    ///
    /// ChatController does NOT change. Conversation memory is preserved.
    /// </summary>
    public class BikeRentalChatChain : IChatChainService
    {
        // ── Dependencies ──────────────────────────────────────────────────────

        private readonly Kernel                      _kernel;
        private readonly IPromptTemplateService      _promptTemplate;
        private readonly IContextProvider            _contextProvider;
        private readonly IServiceScopeFactory        _scopeFactory;
        private readonly ILoggerFactory              _loggerFactory;
        private readonly ILogger<BikeRentalChatChain> _logger;

        // ── Constructor ───────────────────────────────────────────────────────

        public BikeRentalChatChain(
            Kernel                       kernel,
            IPromptTemplateService       promptTemplate,
            IContextProvider             contextProvider,
            IServiceScopeFactory         scopeFactory,
            ILoggerFactory               loggerFactory,
            ILogger<BikeRentalChatChain> logger)
        {
            _kernel          = kernel          ?? throw new ArgumentNullException(nameof(kernel));
            _promptTemplate  = promptTemplate  ?? throw new ArgumentNullException(nameof(promptTemplate));
            _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
            _scopeFactory    = scopeFactory    ?? throw new ArgumentNullException(nameof(scopeFactory));
            _loggerFactory   = loggerFactory   ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── IChatChainService Implementation ──────────────────────────────────

        public async Task<string> InvokeAsync(IReadOnlyList<ChatMessage> conversationHistory)
        {
            // ── Step 1: Extract the last user message ─────────────────────────
            var lastUserMessage = conversationHistory
                .LastOrDefault(m => string.Equals(
                    m.Role, "user", StringComparison.OrdinalIgnoreCase))
                ?.Content ?? string.Empty;

            _logger.LogDebug(
                "BikeRentalChatChain invoked. History: {Count} messages. Query: '{Query}'",
                conversationHistory.Count,
                lastUserMessage.Length > 80 ? lastUserMessage[..80] + "..." : lastUserMessage);

            // ── Step 2: RAG — retrieve relevant document context ──────────────
            // VectorStoreContextProvider embeds the query → cosine search pgvector
            // → returns formatted top-K chunks. Returns "" if nothing relevant found.
            var context = await _contextProvider.GetContextAsync(lastUserMessage);

            if (!string.IsNullOrWhiteSpace(context))
                _logger.LogDebug("RAG context retrieved ({Length} chars).", context.Length);

            // ── Step 3: Render system prompt ──────────────────────────────────
            var variables = new Dictionary<string, string>
            {
                { "company", "Heaven Bike Rental System" },
                { "date",    DateTime.UtcNow.ToString("yyyy-MM-dd") },
                { "context", context }
            };
            var systemPrompt = _promptTemplate.RenderSystemPrompt(variables);

            // ── Step 4: Build SK ChatHistory ──────────────────────────────────
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemPrompt);

            foreach (var msg in conversationHistory)
            {
                if (string.Equals(msg.Role, "user", StringComparison.OrdinalIgnoreCase))
                    chatHistory.AddUserMessage(msg.Content);
                else if (string.Equals(msg.Role, "assistant", StringComparison.OrdinalIgnoreCase))
                    chatHistory.AddAssistantMessage(msg.Content);
                // "system" role messages in history are skipped —
                // the system prompt is always regenerated fresh above (Step 3).
            }

            // ── Step 5: Clone kernel + import scoped plugin instances ──────────
            // kernel.Clone() → shares IChatCompletionService (Singleton AI connection)
            //                → gives independent plugin collection for this request
            using var scope   = _scopeFactory.CreateScope();
            var sp             = scope.ServiceProvider;
            var requestKernel  = _kernel.Clone();

            requestKernel.ImportPluginFromObject(
                new BikePlugin(
                    sp.GetRequiredService<IBikeService>(),
                    sp.GetRequiredService<IBikeUnitService>(),
                    _loggerFactory.CreateLogger<BikePlugin>()),
                "Bikes");

            requestKernel.ImportPluginFromObject(
                new RentalPlugin(
                    sp.GetRequiredService<IRentalRecordService>(),
                    sp.GetRequiredService<IRentalRequestService>(),
                    _loggerFactory.CreateLogger<RentalPlugin>()),
                "Rentals");

            requestKernel.ImportPluginFromObject(
                new BookingPlugin(
                    sp.GetRequiredService<IRentalRequestService>(),
                    sp.GetRequiredService<IBikeService>(),
                    _loggerFactory.CreateLogger<BookingPlugin>()),
                "Bookings");

            requestKernel.ImportPluginFromObject(
                new UserPlugin(
                    sp.GetRequiredService<IUserService>(),
                    _loggerFactory.CreateLogger<UserPlugin>()),
                "Users");

            requestKernel.ImportPluginFromObject(
                new PaymentPlugin(
                    sp.GetRequiredService<IRentalRecordService>(),
                    _loggerFactory.CreateLogger<PaymentPlugin>()),
                "Payments");

            _logger.LogDebug(
                "Request kernel ready with 5 plugin(s): Bikes, Rentals, Bookings, Users, Payments.");

            // ── Step 6: Call SK with automatic function calling ────────────────
            // FunctionChoiceBehavior.Auto():
            //   SK sends the full tool schema to OpenAI.
            //   GPT decides: answer directly OR call a tool.
            //   If tool: SK executes the KernelFunction, appends result to chat,
            //            calls GPT again. Repeats until GPT gives a text answer.
            //   Final text answer is returned from GetChatMessageContentAsync.
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens              = 1500,  // Extra space for tool results + final answer
                Temperature            = 0.7,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var chatService = requestKernel.GetRequiredService<IChatCompletionService>();

            var result = await chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                requestKernel);

            // ── Step 7: Extract and return reply text ─────────────────────────
            var replyText = result.Content?.Trim();

            if (string.IsNullOrWhiteSpace(replyText))
            {
                _logger.LogWarning(
                    "SK returned an empty response. FinishReason: {Reason}",
                    result.Metadata?.GetValueOrDefault("FinishReason"));

                return "I'm sorry, I could not generate a response. Please try again.";
            }

            _logger.LogDebug("SK response received ({Length} chars).", replyText.Length);

            return replyText;
        }
    }
}

#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0010
