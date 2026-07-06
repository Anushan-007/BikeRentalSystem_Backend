using BikeRental_System3.Models;

namespace BikeRental_System3.AI.Interfaces
{
    /// <summary>
    /// Contract for managing per-conversation message history.
    ///
    /// Moved from BikeRental_System3.IService → BikeRental_System3.AI.Interfaces
    /// because conversation memory is part of the AI module, not the general
    /// application service layer.
    ///
    /// Phase 3 implementation: ConversationMemoryService (in-process ConcurrentDictionary)
    ///
    /// Future upgrade paths (no controller or chain changes needed):
    ///   RedisConversationMemoryService     → distributed cache, survives restarts
    ///   SqlConversationMemoryService       → persists history to SQL Server
    ///   SemanticKernelMemoryService        → SK's built-in IChatHistoryStore
    ///
    /// Dependency Inversion Principle:
    ///   ChatController depends on this interface, not the concrete implementation.
    /// </summary>
    public interface IConversationMemoryService
    {
        /// <summary>
        /// Creates a new empty conversation and returns its unique GUID string ID.
        /// Angular stores this ID and sends it with every subsequent message.
        /// </summary>
        string CreateConversation();

        /// <summary>
        /// Appends a single message to an existing conversation.
        /// Enforces the sliding window limit (MaxMessages = 20).
        /// Throws KeyNotFoundException if conversationId was never created.
        /// </summary>
        void AddMessage(string conversationId, string role, string content);

        /// <summary>
        /// Returns all messages in a conversation, oldest first.
        /// Returns an empty list if the conversationId is unknown (graceful fallback).
        /// </summary>
        IReadOnlyList<ChatMessage> GetMessages(string conversationId);

        /// <summary>
        /// Returns true if the conversationId was previously created by CreateConversation().
        /// ChatController uses this to validate IDs received from Angular.
        /// </summary>
        bool ConversationExists(string conversationId);

        /// <summary>
        /// Removes all messages from a conversation and deletes it from memory.
        /// Called when the user clicks "New Conversation".
        /// Silent no-op if the ID does not exist (idempotent).
        /// </summary>
        void ClearConversation(string conversationId);
    }
}
