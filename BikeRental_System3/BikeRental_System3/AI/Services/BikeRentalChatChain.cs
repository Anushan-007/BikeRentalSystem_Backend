#pragma warning disable SKEXP0010  // OpenAIPromptExecutionSettings is marked experimental in SK

using BikeRental_System3.AI.Interfaces;
using BikeRental_System3.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BikeRental_System3.AI.Services
{
    /// <summary>
    /// LangChain Concept: Concrete Chain implementation.
    ///
    /// Python LangChain equivalent:
    ///   chain = RunnableSequence(
    ///       prompt_template | chat_model | output_parser
    ///   )
    ///   result = chain.invoke({"history": [...], "context": "..."})
    ///
    /// This class is the AI pipeline.
    /// It is the ONLY class in the backend that knows about Semantic Kernel.
    /// ChatController and ConversationMemoryService are completely isolated from SK.
    ///
    /// Pipeline (executed in InvokeAsync on every chat request):
    ///
    ///   conversationHistory
    ///         │
    ///         ▼
    ///   [1] Extract last user message
    ///         │
    ///         ▼
    ///   [2] IContextProvider.GetContextAsync(userQuery)
    ///         │ Phase 3: returns ""
    ///         │ Phase 4: returns retrieved bike data from Vector DB
    ///         ▼
    ///   [3] IPromptTemplateService.RenderSystemPrompt(variables)
    ///         │ Fills {{company}}, {{date}}, {{context}} in the template
    ///         ▼
    ///   [4] Build SK ChatHistory
    ///         │ AddSystemMessage(renderedPrompt)
    ///         │ + AddUserMessage / AddAssistantMessage for each turn
    ///         ▼
    ///   [5] IChatCompletionService.GetChatMessageContentAsync(chatHistory)
    ///         │ SK sends this to OpenAI GPT-4o-mini
    ///         ▼
    ///   [6] Return reply string to ChatController
    ///
    /// Dependencies:
    ///   Kernel                  → SK orchestrator (holds IChatCompletionService)
    ///   IPromptTemplateService  → renders the system prompt
    ///   IContextProvider        → provides RAG context (empty in Phase 3)
    ///   ILogger                 → structured logging
    ///
    /// Phase 4 changes (NONE in this file):
    ///   Only IContextProvider implementation changes (VectorStoreContextProvider).
    ///   This chain is closed for modification, open for extension.
    /// </summary>
    public class BikeRentalChatChain : IChatChainService
    {
        // ── Dependencies ──────────────────────────────────────────────────────

        private readonly Kernel _kernel;
        private readonly IPromptTemplateService _promptTemplate;
        private readonly IContextProvider _contextProvider;
        private readonly ILogger<BikeRentalChatChain> _logger;

        // ── Execution Settings ────────────────────────────────────────────────

        // Controls the OpenAI API call behaviour.
        // Semantic Kernel passes these to the underlying OpenAI connector.
        //
        // MaxTokens   = 500  → ~375 words max reply length (prevents runaway costs)
        // Temperature = 0.7  → balanced creativity (0.0 = robotic, 1.0 = very creative)
        //
        // These are the same values previously set in OpenAIService (Phase 2).
        private static readonly OpenAIPromptExecutionSettings ExecutionSettings = new()
        {
            MaxTokens   = 500,
            Temperature = 0.7
        };

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// All dependencies are injected by ASP.NET Core DI.
        ///
        /// Kernel           → registered in Program.cs via builder.Services.AddKernel()
        /// IPromptTemplate  → registered as BikeRentalPromptTemplate (Singleton)
        /// IContextProvider → registered as ContextProvider (Singleton, Phase 3)
        /// ILogger          → auto-injected by ASP.NET Core logging
        /// </summary>
        public BikeRentalChatChain(
            Kernel kernel,
            IPromptTemplateService promptTemplate,
            IContextProvider contextProvider,
            ILogger<BikeRentalChatChain> logger)
        {
            _kernel          = kernel;
            _promptTemplate  = promptTemplate;
            _contextProvider = contextProvider;
            _logger          = logger;
        }

        // ── IChatChainService Implementation ──────────────────────────────────

        /// <summary>
        /// Runs the full AI pipeline and returns the assistant reply text.
        ///
        /// Called by ChatController after:
        ///   1. The user message has been saved to ConversationMemoryService
        ///   2. The full history has been retrieved from ConversationMemoryService
        ///
        /// The controller only deals with conversation lifecycle.
        /// This chain only deals with AI response generation.
        /// Clear separation of responsibilities.
        /// </summary>
        public async Task<string> InvokeAsync(IReadOnlyList<ChatMessage> conversationHistory)
        {
            // ── Step 1: Extract the last user message ─────────────────────────
            // IContextProvider needs the current user query to find relevant docs.
            // We search from the end because the latest user message is at the back
            // (ChatController adds it just before calling InvokeAsync).
            var lastUserMessage = conversationHistory
                .LastOrDefault(m => string.Equals(m.Role, "user", StringComparison.OrdinalIgnoreCase))
                ?.Content ?? string.Empty;

            _logger.LogDebug(
                "BikeRentalChatChain invoked. History length: {Count}. Query: '{Query}'",
                conversationHistory.Count,
                lastUserMessage.Length > 80 ? lastUserMessage[..80] + "..." : lastUserMessage);

            // ── Step 2: Retrieve context via IContextProvider ─────────────────
            // Phase 3: ContextProvider returns string.Empty immediately.
            // Phase 4: VectorStoreContextProvider queries the vector DB.
            //
            // This is the single extension point for RAG.
            // BikeRentalChatChain never changes when switching retrieval strategies.
            var context = await _contextProvider.GetContextAsync(lastUserMessage);

            if (!string.IsNullOrWhiteSpace(context))
            {
                _logger.LogDebug("Context retrieved ({Length} chars).", context.Length);
            }

            // ── Step 3: Render the system prompt ──────────────────────────────
            // BikeRentalPromptTemplate reads AI/Prompts/bike-rental-system.txt
            // and replaces {{company}}, {{date}}, {{context}} with these values.
            var variables = new Dictionary<string, string>
            {
                { "company", "Heaven Bike Rental System" },
                { "date",    DateTime.UtcNow.ToString("yyyy-MM-dd") },
                { "context", context }
            };

            var systemPrompt = _promptTemplate.RenderSystemPrompt(variables);

            // ── Step 4: Build SK ChatHistory ──────────────────────────────────
            // SK ChatHistory is the equivalent of LangChain's ConversationBufferMemory.
            // It holds the message list in SK's own format (ChatMessageContent objects).
            //
            // Structure sent to OpenAI:
            //   [0] system    → rendered system prompt
            //   [1] user      → "Hello"                     (first turn)
            //   [2] assistant → "Hi! How can I help you?"   (first turn reply)
            //   [3] user      → "What bikes are available?"  (current message)
            //
            // GPT reads the entire array in order — this is how it "remembers".
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemPrompt);

            foreach (var message in conversationHistory)
            {
                if (string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase))
                {
                    chatHistory.AddUserMessage(message.Content);
                }
                else if (string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase))
                {
                    chatHistory.AddAssistantMessage(message.Content);
                }
                // "system" role messages in history are skipped —
                // the system prompt is always added fresh above (Step 3).
            }

            // ── Step 5: Call Semantic Kernel ──────────────────────────────────
            // SK resolves IChatCompletionService from the Kernel's service collection.
            // The OpenAI connector translates ChatHistory → REST API call → response.
            // No HttpClient, no JSON, no API key management — SK handles all of it.
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();

            var result = await chatService.GetChatMessageContentAsync(
                chatHistory,
                ExecutionSettings,
                _kernel);

            // ── Step 6: Extract and return reply text ─────────────────────────
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

#pragma warning restore SKEXP0010
