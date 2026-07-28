using System.Collections.Generic;
using UnityEngine;
using Divinatius.NPC;

namespace Divinatius.AI
{
    public class CharacterMemoryManager : MonoBehaviour
    {
        public static CharacterMemoryManager Instance { get; private set; }

        private Dictionary<string, NPCMemoryData> _memoryStore = new Dictionary<string, NPCMemoryData>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public NPCMemoryData GetOrCreateMemory(string characterId, string characterName)
        {
            if (!_memoryStore.ContainsKey(characterId))
            {
                _memoryStore[characterId] = new NPCMemoryData
                {
                    characterId = characterId,
                    characterName = characterName
                };
            }
            return _memoryStore[characterId];
        }

        public void AddMessage(string characterId, string speakerRole, string messageText)
        {
            var memory = GetOrCreateMemory(characterId, characterId);
            memory.conversationHistory.Add(new DialogueMessage
            {
                speakerRole = speakerRole,
                text = messageText,
                timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        }

        public void AddMemoryTag(string characterId, string memoryTag)
        {
            var memory = GetOrCreateMemory(characterId, characterId);
            if (!memory.memoryTags.Contains(memoryTag))
            {
                memory.memoryTags.Add(memoryTag);
            }
        }

        public string FormatContextPrompt(NPCProfileSO profile)
        {
            var memory = GetOrCreateMemory(profile.characterId, profile.characterName);
            string prompt = $"You are playing the role of '{profile.characterName}' in a 3D fantasy open-world game.\n";
            prompt += $"Personality & Background:\n{profile.systemPersonalityPrompt}\n\n";

            if (memory.memoryTags.Count > 0)
            {
                prompt += "Important Key Memories & Events:\n";
                foreach (var tag in memory.memoryTags)
                {
                    prompt += $"- {tag}\n";
                }
                prompt += "\n";
            }

            prompt += "Keep responses concise (1-3 sentences), natural in conversation, and in-character for a visual novel dialogue window.";
            return prompt;
        }

        public List<NPCMemoryData> GetAllMemoryData()
        {
            return new List<NPCMemoryData>(_memoryStore.Values);
        }

        public void LoadMemoryData(List<NPCMemoryData> savedData)
        {
            _memoryStore.Clear();
            if (savedData == null) return;

            foreach (var mem in savedData)
            {
                if (!string.IsNullOrEmpty(mem.characterId))
                {
                    _memoryStore[mem.characterId] = mem;
                }
            }
            Debug.Log($"[CharacterMemoryManager] Loaded memory state for {_memoryStore.Count} characters.");
        }
    }
}
