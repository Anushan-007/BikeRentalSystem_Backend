using BikeRental_System3.Models;

namespace BikeRental_System3.AI.Interfaces
{
    /// <summary>
    /// LangChain Concept: Chain
    ///
    /// Python LangChain equivalent:
    ///   chain = prompt | llm | output_parser
    ///   result = chain.invoke({"history": [...], "input": "What bikes do you have?"})
    ///
    /// This interface is the single entry point the ChatController uses
    /// to get an AI response. The controller has no knowledge of:
    ///   - Prompt templates
    ///   - Context retrieval (RAG)
    ///   - Semantic Kernel internals
    ///   - ChatHistory construction
    ///
    /// SOLID principles applied:
    ///   S — Single Responsibility: the chain owns the full AI pipeline
    ///   D — Dependency Inversion: controller depends on this abstraction
    ///
    /// Phase 3 implementation: BikeRentalChatChain (uses SK + ContextProvider stub)
    /// Phase 4 implementation: same BikeRentalChatChain — only ContextProvider changes
    ///
    /// API contract (preserved across all phases):
    ///   Input  → full conversation history (user + assistant turns in order)
    ///   Output → plain reply string (controller wraps it in ChatResponse DTO)
    /// </summary>
    public interface IChatChainService
    {
        /// <summary>
        /// Runs the complete AI pipeline and returns the assistant reply.
        ///
        /// Internal pipeline (BikeRentalChatChain):
        ///   1. Extract last user message → IContextProvider.GetContextAsync()
        ///   2. IPromptTemplateService.RenderSystemPrompt(variables)
        ///   3. Build SK ChatHistory (system + all turns)
        ///   4. Call SK IChatCompletionService
        ///   5. Return reply text
        ///
        /// The ChatController calls this method exactly as it previously called
        /// IOpenAIService.GetChatResponseAsync — the swap is transparent.
        /// </summary>
        /// <param name="conversationHistory">
        ///   All messages in the conversation so far, oldest first.
        ///   The last message is always the current user input
        ///   (added by ChatController before calling this method).
        /// </param>
        /// <returns>The AI's reply text (trimmed, never null).</returns>
        Task<string> InvokeAsync(IReadOnlyList<ChatMessage> conversationHistory);
    }
}
