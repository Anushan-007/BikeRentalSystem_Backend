using BikeRental_System3.AI.Interfaces;

namespace BikeRental_System3.AI.Services
{
    /// <summary>
    /// Phase 3 implementation of IContextProvider — intentionally empty (no-op).
    ///
    /// WHAT IT DOES:
    ///   Returns string.Empty for every query.
    ///   The {{context}} placeholder in the prompt template stays blank.
    ///   GPT answers from its own training data + the conversation history.
    ///   Behaviour is identical to Phase 2.
    ///
    /// WHY IT EXISTS:
    ///   BikeRentalChatChain depends on IContextProvider (not a concrete class).
    ///   This empty class satisfies that dependency in Phase 3.
    ///   No conditional checks needed inside the chain ("if RAG is enabled...").
    ///   The chain is always clean and context-unaware.
    ///
    /// PHASE 4 UPGRADE:
    ///   In Program.cs, replace this registration:
    ///     builder.Services.AddSingleton<IContextProvider, ContextProvider>();
    ///   with:
    ///     builder.Services.AddSingleton<IContextProvider, VectorStoreContextProvider>();
    ///
    ///   VectorStoreContextProvider will:
    ///     1. Accept user query
    ///     2. Generate embedding via SK ITextEmbeddingGenerationService
    ///     3. Query IVectorStore for similar documents
    ///     4. Format results as a context string
    ///     5. Return to BikeRentalChatChain for {{context}} injection
    ///
    ///   BikeRentalChatChain is never modified.
    ///   ChatController is never modified.
    ///   Only Program.cs changes (one line swap).
    ///
    /// This is the Strategy design pattern applied to RAG retrieval.
    /// </summary>
    public class ContextProvider : IContextProvider
    {
        /// <summary>
        /// Phase 3: returns empty string immediately.
        ///
        /// The Task.FromResult avoids any async overhead —
        /// there is nothing to await in Phase 3.
        /// </summary>
        public Task<string> GetContextAsync(
            string userQuery,
            CancellationToken cancellationToken = default)
        {
            // Phase 3: No retrieval. Context is empty.
            // Phase 4: VectorStoreContextProvider handles retrieval here.
            return Task.FromResult(string.Empty);
        }
    }
}
