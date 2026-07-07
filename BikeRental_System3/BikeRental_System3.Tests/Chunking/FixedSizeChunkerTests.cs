using BikeRental_System3.AI.Chunking;
using BikeRental_System3.AI.Models;

namespace BikeRental_System3.Tests.Chunking
{
    /// <summary>
    /// 25 unit tests for FixedSizeChunker.
    /// No external dependencies — pure in-process logic.
    /// </summary>
    public class FixedSizeChunkerTests
    {
        private readonly FixedSizeChunker _chunker = new();

        // ── helpers ──────────────────────────────────────────────────────────

        private static Document MakeDoc(string content, string id = "doc1") =>
            new() { Id = id, Title = "Test Doc", Source = "test.pdf", Content = content };

        private static ChunkingOptions Opts(int size, int overlap = 0) =>
            new() { ChunkSize = size, ChunkOverlap = overlap };

        // ── 1. Empty document returns no chunks ───────────────────────────────

        [Fact]
        public void Chunk_EmptyContent_ReturnsEmpty()
        {
            var doc    = MakeDoc("");
            var result = _chunker.Chunk(doc, Opts(10));
            Assert.Empty(result);
        }

        // ── 2. Null document throws ───────────────────────────────────────────

        [Fact]
        public void Chunk_NullDocument_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _chunker.Chunk(null!, Opts(10)));
        }

        // ── 3. Null options throws ────────────────────────────────────────────

        [Fact]
        public void Chunk_NullOptions_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _chunker.Chunk(MakeDoc("hello"), null!));
        }

        // ── 4. Content shorter than ChunkSize → single chunk ─────────────────

        [Fact]
        public void Chunk_ContentShorterThanChunkSize_ReturnsSingleChunk()
        {
            var doc    = MakeDoc("Hello");
            var result = _chunker.Chunk(doc, Opts(100));
            Assert.Single(result);
            Assert.Equal("Hello", result[0].Content);
        }

        // ── 5. Content exactly ChunkSize → single chunk ───────────────────────

        [Fact]
        public void Chunk_ContentExactlyChunkSize_ReturnsSingleChunk()
        {
            var doc    = MakeDoc("ABCDE");
            var result = _chunker.Chunk(doc, Opts(5));
            Assert.Single(result);
            Assert.Equal("ABCDE", result[0].Content);
        }

        // ── 6. No overlap — adjacent chunks cover all content ─────────────────

        [Fact]
        public void Chunk_NoOverlap_ChunksAreAdjacent()
        {
            // "ABCDEFGHIJ" size=3 overlap=0 → ABC DEF GHI J
            var doc    = MakeDoc("ABCDEFGHIJ");
            var result = _chunker.Chunk(doc, Opts(3, 0));
            Assert.Equal(4, result.Count);
            Assert.Equal("ABC", result[0].Content);
            Assert.Equal("DEF", result[1].Content);
            Assert.Equal("GHI", result[2].Content);
            Assert.Equal("J",   result[3].Content);
        }

        // ── 7. With overlap — boundary text repeated ──────────────────────────

        [Fact]
        public void Chunk_WithOverlap_BoundaryTextRepeated()
        {
            // "ABCDEFGHIJ" size=5 overlap=2 → step=3
            // Chunk0: [0..5]  = "ABCDE"
            // Chunk1: [3..8]  = "DEFGH"  (overlap = "DE")
            // Chunk2: [6..10] = "GHIJ"
            var doc    = MakeDoc("ABCDEFGHIJ");
            var result = _chunker.Chunk(doc, Opts(5, 2));
            Assert.Equal(3, result.Count);
            Assert.Equal("ABCDE", result[0].Content);
            Assert.Equal("DEFGH", result[1].Content);
            Assert.Equal("GHIJ",  result[2].Content);
        }

        // ── 8. StartOffset of first chunk is 0 ───────────────────────────────

        [Fact]
        public void Chunk_FirstChunkStartOffset_IsZero()
        {
            var result = _chunker.Chunk(MakeDoc("Hello World"), Opts(5));
            Assert.Equal(0, result[0].StartOffset);
        }

        // ── 9. Last chunk EndOffset equals content length ─────────────────────

        [Fact]
        public void Chunk_LastChunkEndOffset_EqualsContentLength()
        {
            var content = "Hello World";
            var result  = _chunker.Chunk(MakeDoc(content), Opts(4));
            Assert.Equal(content.Length, result[^1].EndOffset);
        }

        // ── 10. All characters covered (no gaps) ─────────────────────────────

        [Fact]
        public void Chunk_NoOverlap_AllCharactersCovered()
        {
            var content = "ABCDEFGHIJKLMNOP";
            var result  = _chunker.Chunk(MakeDoc(content), Opts(4, 0));
            var rebuilt = string.Concat(result.Select(c => c.Content));
            Assert.Equal(content, rebuilt);
        }

        // ── 11. ChunkIndex sequential from 0 ─────────────────────────────────

        [Fact]
        public void Chunk_ChunkIndexes_AreSequentialFromZero()
        {
            var result = _chunker.Chunk(MakeDoc("ABCDEFGHIJ"), Opts(3, 0));
            for (int i = 0; i < result.Count; i++)
                Assert.Equal(i, result[i].ChunkIndex);
        }

        // ── 12. Chunk Id contains document Id and chunk index ─────────────────

        [Fact]
        public void Chunk_ChunkId_ContainsDocIdAndIndex()
        {
            var result = _chunker.Chunk(MakeDoc("ABCDEFGHIJ", "myDoc"), Opts(3, 0));
            Assert.Equal("myDoc-chunk-0", result[0].Id);
            Assert.Equal("myDoc-chunk-1", result[1].Id);
        }

        // ── 13. DocumentId inherited from source document ─────────────────────

        [Fact]
        public void Chunk_DocumentId_InheritedFromSource()
        {
            var result = _chunker.Chunk(MakeDoc("Hello World", "src99"), Opts(5));
            Assert.All(result, c => Assert.Equal("src99", c.DocumentId));
        }

        // ── 14. DocumentTitle inherited ───────────────────────────────────────

        [Fact]
        public void Chunk_DocumentTitle_Inherited()
        {
            var doc    = new Document { Id = "d1", Title = "My PDF", Source = "x.pdf", Content = "Hello World" };
            var result = _chunker.Chunk(doc, Opts(5));
            Assert.All(result, c => Assert.Equal("My PDF", c.DocumentTitle));
        }

        // ── 15. Source inherited ──────────────────────────────────────────────

        [Fact]
        public void Chunk_Source_Inherited()
        {
            var doc    = new Document { Id = "d1", Title = "T", Source = "faq.pdf", Content = "Hello World" };
            var result = _chunker.Chunk(doc, Opts(5));
            Assert.All(result, c => Assert.Equal("faq.pdf", c.Source));
        }

        // ── 16. StartOffset / EndOffset consistent with Content ───────────────

        [Fact]
        public void Chunk_Offsets_ConsistentWithContent()
        {
            var content = "ABCDEFGHIJKLMNOPQRST";
            var result  = _chunker.Chunk(MakeDoc(content), Opts(7, 2));
            foreach (var chunk in result)
            {
                var expected = content[chunk.StartOffset..chunk.EndOffset];
                Assert.Equal(expected, chunk.Content);
            }
        }

        // ── 17. ContentLength matches Content.Length ──────────────────────────

        [Fact]
        public void Chunk_ContentLength_MatchesStringLength()
        {
            var result = _chunker.Chunk(MakeDoc("ABCDEFGHIJ"), Opts(3, 0));
            Assert.All(result, c => Assert.Equal(c.Content.Length, c.ContentLength));
        }

        // ── 18. HasContent is true for all produced chunks ────────────────────

        [Fact]
        public void Chunk_HasContent_TrueForAllChunks()
        {
            var result = _chunker.Chunk(MakeDoc("ABCDEFGHIJ"), Opts(3, 0));
            Assert.All(result, c => Assert.True(c.HasContent));
        }

        // ── 19. Zero overlap → step equals chunk size ─────────────────────────

        [Fact]
        public void Chunk_ZeroOverlap_StepEqualsChunkSize()
        {
            var opts = new ChunkingOptions { ChunkSize = 8, ChunkOverlap = 0 };
            Assert.Equal(8, opts.Step);
        }

        // ── 20. Validation: ChunkSize ≤ 0 throws ─────────────────────────────

        [Fact]
        public void ChunkingOptions_InvalidChunkSize_ThrowsOnValidate()
        {
            var opts = new ChunkingOptions { ChunkSize = 0, ChunkOverlap = 0 };
            Assert.Throws<ArgumentOutOfRangeException>(opts.Validate);
        }

        // ── 21. Validation: Overlap ≥ ChunkSize throws ───────────────────────

        [Fact]
        public void ChunkingOptions_OverlapGreaterThanOrEqualSize_ThrowsOnValidate()
        {
            var opts = new ChunkingOptions { ChunkSize = 5, ChunkOverlap = 5 };
            Assert.Throws<ArgumentOutOfRangeException>(opts.Validate);
        }

        // ── 22. Validation: Negative overlap throws ───────────────────────────

        [Fact]
        public void ChunkingOptions_NegativeOverlap_ThrowsOnValidate()
        {
            var opts = new ChunkingOptions { ChunkSize = 10, ChunkOverlap = -1 };
            Assert.Throws<ArgumentOutOfRangeException>(opts.Validate);
        }

        // ── 23. Default preset is valid ───────────────────────────────────────

        [Fact]
        public void ChunkingOptions_Default_IsValid()
        {
            var opts = ChunkingOptions.Default;
            var ex   = Record.Exception(opts.Validate);
            Assert.Null(ex);
        }

        // ── 24. ChunkAll — multiple documents ────────────────────────────────

        [Fact]
        public void ChunkAll_MultipleDocs_ReturnsChunksFromAll()
        {
            var docs = new[]
            {
                MakeDoc("ABCDE", "d1"),
                MakeDoc("FGHIJ", "d2")
            };
            var result = _chunker.ChunkAll(docs, Opts(3, 0));
            // d1: ABC DE → 2 chunks; d2: FGH IJ → 2 chunks
            Assert.Equal(4, result.Count);
            Assert.Contains(result, c => c.DocumentId == "d1");
            Assert.Contains(result, c => c.DocumentId == "d2");
        }

        // ── 25. ChunkAll — null list throws ──────────────────────────────────

        [Fact]
        public void ChunkAll_NullDocuments_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _chunker.ChunkAll(null!, Opts(10)));
        }
    }
}
