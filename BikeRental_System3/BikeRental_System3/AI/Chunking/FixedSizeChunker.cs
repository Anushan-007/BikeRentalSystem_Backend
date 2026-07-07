using BikeRental_System3.AI.Models;

namespace BikeRental_System3.AI.Chunking
{
    /// <summary>
    /// Splits a Document into fixed-size character chunks with configurable overlap.
    ///
    /// ALGORITHM (sliding window):
    ///   start = 0
    ///   while start &lt; text.Length:
    ///       end = min(start + ChunkSize, text.Length)
    ///       emit chunk: text[start..end]
    ///       if end == text.Length: break
    ///       start += Step    (Step = ChunkSize - Overlap)
    ///
    /// EXAMPLE — text = "ABCDEFGHIJKLMNOPQRST" (20 chars), C=10, O=3, Step=7:
    ///   Chunk 0: [0..10]  = "ABCDEFGHIJ"
    ///   Chunk 1: [7..17]  = "HIJKLMNOPQ"   ← overlap = "HIJ"
    ///   Chunk 2: [14..20] = "NOPQRST"       ← shorter last chunk
    ///
    /// Lifetime: Singleton — completely stateless, safe for concurrent use.
    /// </summary>
    public class FixedSizeChunker : IDocumentChunker
    {
        public IReadOnlyList<DocumentChunk> Chunk(Document document, ChunkingOptions options)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(options);
            options.Validate();

            if (!document.HasContent)
                return Array.Empty<DocumentChunk>();

            return BuildChunks(document, options);
        }

        public IReadOnlyList<DocumentChunk> ChunkAll(
            IEnumerable<Document> documents,
            ChunkingOptions options)
        {
            ArgumentNullException.ThrowIfNull(documents);
            ArgumentNullException.ThrowIfNull(options);
            options.Validate();

            var result = new List<DocumentChunk>();
            foreach (var doc in documents)
            {
                if (doc is null || !doc.HasContent) continue;
                result.AddRange(BuildChunks(doc, options));
            }
            return result.AsReadOnly();
        }

        private static List<DocumentChunk> BuildChunks(Document document, ChunkingOptions options)
        {
            var content   = document.Content;
            var chunkSize = options.ChunkSize;
            var step      = options.Step;
            var chunks    = new List<DocumentChunk>();
            var start     = 0;
            var index     = 0;

            while (start < content.Length)
            {
                var end  = Math.Min(start + chunkSize, content.Length);
                var text = content[start..end];

                chunks.Add(new DocumentChunk
                {
                    Id            = $"{document.Id}-chunk-{index}",
                    DocumentId    = document.Id,
                    DocumentTitle = document.Title,
                    Source        = document.Source,
                    ChunkIndex    = index,
                    Content       = text,
                    StartOffset   = start,
                    EndOffset     = end
                });

                if (end == content.Length) break;
                start += step;
                index++;
            }

            return chunks;
        }
    }
}
