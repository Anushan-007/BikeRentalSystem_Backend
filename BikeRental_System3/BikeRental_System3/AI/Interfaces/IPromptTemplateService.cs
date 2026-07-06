namespace BikeRental_System3.AI.Interfaces
{
    /// <summary>
    /// LangChain Concept: PromptTemplate
    ///
    /// Python LangChain equivalent:
    ///   template = PromptTemplate(
    ///       template="You are {role} for {company}. Context: {context}",
    ///       input_variables=["role", "company", "context"]
    ///   )
    ///   rendered = template.format(role="assistant", company="Heaven Bike Rental", context="")
    ///
    /// .NET Semantic Kernel equivalent (this interface):
    ///   IPromptTemplateService.RenderSystemPrompt(variables) → rendered string
    ///
    /// Why an interface?
    ///   Dependency Inversion Principle (SOLID — D).
    ///   BikeRentalChatChain depends on IPromptTemplateService, not the concrete class.
    ///   In Phase 4, we can introduce a RagPromptTemplate that injects retrieved
    ///   bike inventory data into {{context}} — without changing BikeRentalChatChain.
    ///
    /// Variable convention used in template files:
    ///   {{variable_name}}  →  double curly braces (same as LangChain / Handlebars)
    ///
    /// Current variables (Phase 3):
    ///   {{company}}  →  "Heaven Bike Rental System"
    ///   {{date}}     →  current date (e.g., "2026-07-06")
    ///   {{context}}  →  empty string (Phase 4: filled with RAG-retrieved data)
    /// </summary>
    public interface IPromptTemplateService
    {
        /// <summary>
        /// Renders the system prompt by substituting all {{variable}} placeholders
        /// with the provided values.
        ///
        /// Phase 3 call:
        ///   RenderSystemPrompt()  →  uses all defaults
        ///   RenderSystemPrompt(new Dictionary { { "date", "2026-07-06" } })
        ///
        /// Phase 4 call (RAG):
        ///   RenderSystemPrompt(new Dictionary {
        ///       { "context", "Bike #1: Mountain Hawk, ₹500/day\nBike #2: ..." }
        ///   })
        ///
        /// Unrecognized variables are ignored (no exception thrown).
        /// Missing variables use their default values.
        /// </summary>
        /// <param name="variables">
        ///   Key-value pairs to substitute into the template.
        ///   Pass null or empty to use all default values.
        /// </param>
        /// <returns>
        ///   The fully rendered system prompt string, ready to send to the LLM.
        /// </returns>
        string RenderSystemPrompt(IReadOnlyDictionary<string, string>? variables = null);

        /// <summary>
        /// Returns the list of variable names this template expects.
        ///
        /// Example return: ["company", "date", "context"]
        ///
        /// Used by:
        ///   - Unit tests to verify all variables are handled
        ///   - Phase 4 RAG layer to know which variable to fill with retrieved data
        ///   - Logging / observability tooling
        /// </summary>
        IReadOnlyList<string> GetRequiredVariables();
    }
}
