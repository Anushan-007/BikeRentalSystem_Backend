namespace BikeRental_System3.AI.Interfaces
{
    /// <summary>
    /// LangChain Concept: Retriever / ContextProvider
    ///
    /// Python LangChain equivalent (Phase 4):
    ///   retriever = vectorstore.as_retriever()
    ///   context = retriever.invoke(user_query)
    ///
    /// PURPOSE — Phase 3 vs Phase 4:
    ///
    ///   Phase 3 (now):
    ///     ContextProvider returns "" (empty string).
    ///     The prompt template's {{context}} placeholder stays blank.
    ///     The chain works exactly like Phase 2 — no retrieval.
    ///
    ///   Phase 4 (RAG):
    ///     VectorStoreContextProvider is registered in DI instead of ContextProvider.
    ///     It queries a Vector DB with the user's message, retrieves relevant
    ///     bike inventory / policy documents, and returns them as a formatted string.
    ///     That string fills {{context}} in the prompt template.
    ///     BikeRentalChatChain does NOT change.
    ///     ChatController does NOT change.
    ///     Only the DI registration in Program.cs changes.
    ///
    /// BENEFIT of this abstraction:
    ///   Open/Closed Principle (SOLID — O):
    ///   The system is open for extension (new retriever implementations)
    ///   and closed for modification (existing chain + controller unchanged).
    ///
    ///   Future implementations:
    ///     VectorStoreContextProvider  → SK IVectorStore + ITextSearch
    ///     DatabaseContextProvider     → SQL query for live bike inventory
    ///     ApiContextProvider          → external REST API for availability
    ///     HybridContextProvider       → combine multiple sources
    /// </summary>
    public interface IContextProvider
    {
        /// <summary>
        /// Returns relevant context text for the given user query.
        ///
        /// Phase 3: always returns string.Empty (no retrieval needed).
        /// Phase 4: queries Vector DB, returns formatted document excerpts.
        ///
        /// Example Phase 4 return value:
        ///   "Available Bikes:\n
        ///    - Mountain Hawk (ID: BK-001), ₹500/day, Status: Available\n
        ///    - Road Runner (ID: BK-002), ₹400/day, Status: Available\n
        ///
        ///    Rental Policy:\n
        ///    - Minimum rental period: 1 day\n
        ///    - ID proof required at pickup\n"
        ///
        /// This text is injected into the {{context}} variable of the prompt template.
        /// GPT reads it and answers questions using this information.
        ///
        /// CancellationToken: supports request cancellation (e.g., user closes browser).
        /// Phase 3 implementation ignores it. Phase 4 passes it to async DB calls.
        /// </summary>
        /// <param name="userQuery">The current user message (used to find relevant docs).</param>
        /// <param name="cancellationToken">Cancellation support for async operations.</param>
        /// <returns>
        ///   Formatted context string to inject into the system prompt.
        ///   Returns string.Empty when no relevant context is found or in Phase 3.
        /// </returns>
        Task<string> GetContextAsync(
            string userQuery,
            CancellationToken cancellationToken = default);
    }
}
