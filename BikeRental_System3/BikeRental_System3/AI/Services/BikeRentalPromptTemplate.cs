using BikeRental_System3.AI.Interfaces;

namespace BikeRental_System3.AI.Services
{
    /// <summary>
    /// LangChain Concept: Concrete PromptTemplate implementation.
    ///
    /// Python LangChain equivalent:
    ///   class BikeRentalPromptTemplate(PromptTemplate):
    ///       template = open("AI/Prompts/bike-rental-system.txt").read()
    ///       input_variables = ["company", "date", "context"]
    ///
    /// This class:
    ///   1. Reads the template from AI/Prompts/bike-rental-system.txt at startup.
    ///   2. At runtime, replaces {{variable}} placeholders with provided values.
    ///   3. Returns the fully rendered system prompt string.
    ///
    /// Template syntax: {{variable_name}} (double curly braces)
    ///   Same convention as LangChain (Python) and Handlebars.js.
    ///
    /// Why load from a .txt file instead of a const string?
    ///   - Prompt engineers can edit the prompt without recompiling C# code.
    ///   - Enables A/B testing of prompts by swapping files.
    ///   - Phase 4: can have multiple template files (one for RAG, one for basic chat).
    ///   - Natural .prompty / .txt file is version-controlled alongside code.
    ///
    /// Phase 3 variables:
    ///   {{company}} → "Heaven Bike Rental System"   (default)
    ///   {{date}}    → current UTC date               (default: today)
    ///   {{context}} → ""                             (default: empty — no RAG yet)
    ///
    /// Phase 4 upgrade:
    ///   The RAG layer will pass:
    ///   variables["context"] = "Bike #1: Mountain Hawk, ₹500/day\nBike #2: ..."
    ///   This class does NOT change — only the caller changes.
    /// </summary>
    public class BikeRentalPromptTemplate : IPromptTemplateService
    {
        // ── Fields ────────────────────────────────────────────────────────────

        /// <summary>
        /// The raw template text loaded once at startup.
        /// Contains {{variable}} placeholders that are replaced at runtime.
        /// </summary>
        private readonly string _templateText;

        /// <summary>
        /// All variables this template expects.
        /// Used by unit tests and the Phase 4 RAG layer.
        /// </summary>
        private static readonly IReadOnlyList<string> _requiredVariables =
            new[] { "company", "date", "context" };

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// Loads the prompt template from disk at application startup.
        ///
        /// IWebHostEnvironment.ContentRootPath = the project root directory.
        /// In development: C:\...\BikeRental_System3\
        /// In production:  the folder containing the published .dll
        ///
        /// The file must exist at: {ContentRootPath}/AI/Prompts/bike-rental-system.txt
        /// The .csproj marks this file as Content → PreserveNewest so it is
        /// copied to the output directory on build.
        /// </summary>
        public BikeRentalPromptTemplate(IWebHostEnvironment env)
        {
            var templatePath = Path.Combine(
                env.ContentRootPath,
                "AI",
                "Prompts",
                "bike-rental-system.txt");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(
                    $"Prompt template not found at: {templatePath}. " +
                    "Ensure AI/Prompts/bike-rental-system.txt exists and is " +
                    "marked as Content → PreserveNewest in the .csproj.");
            }

            _templateText = File.ReadAllText(templatePath);
        }

        // ── IPromptTemplateService Implementation ─────────────────────────────

        /// <summary>
        /// Renders the system prompt by substituting all {{variable}} placeholders.
        ///
        /// Substitution order:
        ///   1. {{company}}  → provided value or "Heaven Bike Rental System"
        ///   2. {{date}}     → provided value or today's UTC date (yyyy-MM-dd)
        ///   3. {{context}}  → provided value or "" (empty in Phase 3)
        ///
        /// Example (Phase 3 — no RAG):
        ///   Input:    variables = null
        ///   Template: "You are an assistant for {{company}}. Date: {{date}}.\n{{context}}"
        ///   Output:   "You are an assistant for Heaven Bike Rental System. Date: 2026-07-06.\n"
        ///
        /// Example (Phase 4 — with RAG):
        ///   Input:    variables = { "context": "Bike #1: Mountain Hawk, ₹500/day" }
        ///   Output:   "You are an assistant for Heaven Bike Rental System. Date: 2026-07-06.\nBike #1: Mountain Hawk, ₹500/day"
        /// </summary>
        public string RenderSystemPrompt(IReadOnlyDictionary<string, string>? variables = null)
        {
            var vars = variables ?? new Dictionary<string, string>();

            // Start with the raw template text.
            var rendered = _templateText;

            // Replace each known variable placeholder.
            // Unknown variables passed in `variables` are simply ignored.
            foreach (var variableName in _requiredVariables)
            {
                var placeholder = $"{{{{{variableName}}}}}"; // "{{variableName}}"
                var value = vars.TryGetValue(variableName, out var provided)
                    ? provided
                    : GetDefaultValue(variableName);

                rendered = rendered.Replace(placeholder, value);
            }

            return rendered;
        }

        /// <summary>
        /// Returns the variable names this template expects.
        /// Order matches the substitution order in RenderSystemPrompt.
        /// </summary>
        public IReadOnlyList<string> GetRequiredVariables() => _requiredVariables;

        // ── Private Helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Default values used when the caller does not supply a variable.
        ///
        /// Phase 3 defaults:
        ///   company → fixed business name
        ///   date    → today's UTC date (changes each day automatically)
        ///   context → empty string (no RAG data available yet)
        ///
        /// Phase 4 change:
        ///   The RAG layer will always supply "context" — this default is the fallback.
        /// </summary>
        private static string GetDefaultValue(string variableName) => variableName switch
        {
            "company" => "Heaven Bike Rental System",
            "date"    => DateTime.UtcNow.ToString("yyyy-MM-dd"),
            "context" => string.Empty,
            _         => string.Empty   // unknown variables → empty
        };
    }
}
