using BikeRental_System3.AI.Interfaces;
using BikeRental_System3.Models;
using System.Collections.Concurrent;

namespace BikeRental_System3.AI.Services
{
    /// <summary>
    /// In-memory conversation store — moved to AI.Services namespace.
    ///
    /// UNCHANGED from Phase 2. Only the namespace has changed:
    ///   Before: BikeRental_System3.Services
    ///   After:  BikeRental_System3.AI.Services
    ///
    /// Reason for move: conversation memory is part of the AI module.
    /// All AI-related code lives under AI/ for Phase 4+ maintainability.
    ///
    /// Implementation: ConcurrentDictionary (thread-safe in-process storage).
    /// Lifetime: Singleton — one instance for the entire app lifetime.
    ///
    /// Upgrade paths (change DI registration only, no controller/chain changes):
    ///   RedisConversationMemoryService  → survives server restarts
    ///   SqlConversationMemoryService    → persistent conversation history
    ///   SKChatHistoryStore              → SK's native chat history abstraction
    /// </summary>
    public class ConversationMemoryService : IConversationMemoryService
    {
        // ── Storage ───────────────────────────────────────────────────────────

        private readonly ConcurrentDictionary<string, List<ChatMessage>> _conversations = new();

        // ── Configuration ─────────────────────────────────────────────────────

        // Sliding window: keeps the most recent 20 messages.
        // Prevents unbounded memory growth and excessive token usage.
        private const int MaxMessages = 20;

        // ── IConversationMemoryService Implementation ─────────────────────────

        /// <summary>
        /// Creates a new conversation and returns its GUID string ID.
        /// </summary>
        public string CreateConversation()
        {
            var conversationId = Guid.NewGuid().ToString();
            _conversations.TryAdd(conversationId, new List<ChatMessage>());
            return conversationId;
        }

        /// <summary>
        /// Appends a message to the conversation with sliding window enforcement.
        /// Thread-safe: ConcurrentDictionary for the outer dictionary,
        /// lock(messages) for the inner list.
        /// </summary>
        public void AddMessage(string conversationId, string role, string content)
        {
            if (!_conversations.TryGetValue(conversationId, out var messages))
            {
                throw new KeyNotFoundException(
                    $"Conversation '{conversationId}' was not found. " +
                    "Call POST /api/chat without a ConversationId to start a new one.");
            }

            lock (messages)
            {
                messages.Add(new ChatMessage
                {
                    Role      = role,
                    Content   = content,
                    Timestamp = DateTime.UtcNow
                });

                if (messages.Count > MaxMessages)
                {
                    int overflow = messages.Count - MaxMessages;
                    messages.RemoveRange(0, overflow);
                }
            }
        }

        /// <summary>
        /// Returns a read-only snapshot of the conversation history.
        /// </summary>
        public IReadOnlyList<ChatMessage> GetMessages(string conversationId)
        {
            if (!_conversations.TryGetValue(conversationId, out var messages))
            {
                return Array.Empty<ChatMessage>();
            }

            lock (messages)
            {
                return messages.ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Returns true if the conversationId was created by CreateConversation().
        /// </summary>
        public bool ConversationExists(string conversationId)
        {
            return _conversations.ContainsKey(conversationId);
        }

        /// <summary>
        /// Removes the conversation from memory (idempotent).
        /// </summary>
        public void ClearConversation(string conversationId)
        {
            _conversations.TryRemove(conversationId, out _);
        }
    }
}
