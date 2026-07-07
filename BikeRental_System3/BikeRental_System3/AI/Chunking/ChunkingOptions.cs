namespace BikeRental_System3.AI.Chunking
{
    /// <summary>
    /// Configuration for the chunking step of the RAG pipeline.
    ///
    /// CHUNKING VISUALISED (ChunkSize=10, Overlap=3, Step=7):
    ///
    ///   Text:   "ABCDEFGHIJKLMNOPQRST" (20 chars)
    ///   Chunk0: "ABCDEFGHIJ"  [0..10]
    ///   Chunk1: "HIJKLMNOPQ"  [7..17]   ← 3-char overlap: HIJ
    ///   Chunk2: "NOPQRST"    [14..20]   ← shorter last chunk
    ///
    /// WHY OVERLAP IMPROVES RETRIEVAL:
    ///   Without overlap, a sentence spanning two chunk boundaries is fragmented.
    ///   Neither chunk contains the complete thought → embedding misses it.
    ///   With overlap, boundary text is duplicated so the sentence fully appears
    ///   in at least one chunk → retrieval succeeds.
    ///
    ///   Rule of thumb: Overlap ≈ 10-20% of ChunkSize.
    ///   ChunkSize=512, Overlap=64 → Step=448 (our default)
    ///
    /// INVARIANT: Step = ChunkSize - ChunkOverlap must be > 0.
    ///   ChunkOverlap >= ChunkSize → Step ≤ 0 → infinite loop → ArgumentException.
    /// </summary>
    public class ChunkingOptions
    {
        public const int DefaultChunkSize    = 512;
        public const int DefaultChunkOverlap = 64;

        /// <summary>Maximum number of characters per chunk. Must be > ChunkOverlap.</summary>
        public int ChunkSize { get; init; } = DefaultChunkSize;

        /// <summary>Characters shared between consecutive chunks. Must be >= 0 and &lt; ChunkSize.</summary>
        public int ChunkOverlap { get; init; } = DefaultChunkOverlap;

        /// <summary>New characters advanced per chunk: ChunkSize - ChunkOverlap.</summary>
        public int Step => ChunkSize - ChunkOverlap;

        public static ChunkingOptions Default => new();
        public static ChunkingOptions Small   => new() { ChunkSize = 256, ChunkOverlap = 32  };
        public static ChunkingOptions Large   => new() { ChunkSize = 1024, ChunkOverlap = 128 };

        public void Validate()
        {
            if (ChunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(ChunkSize), ChunkSize,
                    $"ChunkSize must be > 0. Got: {ChunkSize}");
            if (ChunkOverlap < 0)
                throw new ArgumentOutOfRangeException(nameof(ChunkOverlap), ChunkOverlap,
                    $"ChunkOverlap must be >= 0. Got: {ChunkOverlap}");
            if (ChunkOverlap >= ChunkSize)
                throw new ArgumentOutOfRangeException(nameof(ChunkOverlap), ChunkOverlap,
                    $"ChunkOverlap ({ChunkOverlap}) must be < ChunkSize ({ChunkSize}). " +
                    $"Step = {ChunkSize - ChunkOverlap} \u2264 0 would cause an infinite loop.");
        }
    }
}
