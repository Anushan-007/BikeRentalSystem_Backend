using System.Text;
using BikeRental_System3.AI.Models;

namespace BikeRental_System3.AI.Retrieval
{
    /// <summary>
    /// Formats retrieved VectorDocuments into a structured context block for GPT.
    ///
    /// Output format (injected into {{context}} in the system prompt):
    ///
    ///   === Relevant Information from Documents ===
    ///
    ///   Document: FAQ
    ///
    ///   Chunk:
    ///   A 30% advance payment is required to confirm a booking.
    ///   Payments can be made via UPI, credit card, or cash at the counter.
    ///
    ///   --------------------------------
    ///
    ///   Document: Terms
    ///
    ///   Chunk:
    ///   The minimum rental period is 1 hour. Hourly charges apply for...
    ///
    ///   --------------------------------
    ///
    /// Truncation: chunks are appended in order until MaximumContextCharacters is reached.
    /// The most-similar chunk (index 0) is always included — truncation skips later chunks.
    ///
    /// Lifetime: Singleton — pure computation, stateless, thread-safe.
    /// </summary>
    public sealed class ContextBuilder : IContextBuilder
    {
        private const string SectionHeader = "=== Relevant Information from Documents ===";
        private const string ChunkSeparator = "--------------------------------";

        private readonly ILogger<ContextBuilder> _logger;

        public ContextBuilder(ILogger<ContextBuilder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public string Build(IReadOnlyList<VectorDocument> documents, int maxCharacters = 4000)
        {
            if (documents == null || documents.Count == 0)
            {
                _logger.LogDebug("ContextBuilder.Build: no documents provided — returning empty context.");
                return string.Empty;
            }

            var body = new StringBuilder();
            var totalChars = 0;
            var chunksIncluded = 0;

            foreach (var doc in documents)
            {
                if (string.IsNullOrWhiteSpace(doc.Content))
                    continue;

                // Format one chunk section
                var section = BuildSection(doc);

                // If adding this section exceeds the limit AND we already have content,
                // stop — always include at least the first valid chunk.
                if (chunksIncluded > 0 && totalChars + section.Length > maxCharacters)
                {
                    _logger.LogDebug(
                        "ContextBuilder: reached character limit after {Included} chunk(s). " +
                        "Skipping remaining {Remaining} chunk(s).",
                        chunksIncluded, documents.Count - chunksIncluded);
                    break;
                }

                body.Append(section);
                totalChars += section.Length;
                chunksIncluded++;
            }

            if (chunksIncluded == 0)
            {
                _logger.LogDebug("ContextBuilder: all documents had empty content.");
                return string.Empty;
            }

            _logger.LogInformation(
                "ContextBuilder: included {Included}/{Total} chunks. Context size: {Chars} chars.",
                chunksIncluded, documents.Count, totalChars);

            // Wrap with a header so GPT knows this is retrieved reference material
            return $"{SectionHeader}\n\n{body.ToString().TrimEnd()}";
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>Builds the formatted text block for a single chunk.</summary>
        private static string BuildSection(VectorDocument doc)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Document: {doc.DocumentTitle}");
            sb.AppendLine();
            sb.AppendLine("Chunk:");
            sb.AppendLine(doc.Content.Trim());
            sb.AppendLine();
            sb.AppendLine(ChunkSeparator);
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
