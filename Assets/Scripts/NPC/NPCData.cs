using System;
using System.Collections.Generic;
using UnityEngine;

namespace Divinatius.NPC
{
    [Serializable]
    public class DialogueMessage
    {
        public string speakerRole; // "player" or "model"
        public string text;
        public long timestamp;
    }

    [Serializable]
    public class NPCMemoryData
    {
        public string characterId;
        public string characterName;
        public List<string> memoryTags = new List<string>();
        public List<DialogueMessage> conversationHistory = new List<DialogueMessage>();
        public int relationshipScore = 50;
    }

    [CreateAssetMenu(fileName = "NewNPCProfile", menuName = "Divinatius/NPC Profile")]
    public class NPCProfileSO : ScriptableObject
    {
        [Header("Identity")]
        public string characterId; // e.g. "npc_01_celeste"
        public string characterName;
        [TextArea(3, 8)] public string systemPersonalityPrompt;
        
        [Header("ElevenLabs Voice Settings")]
        public string elevenLabsVoiceId = "21m00Tcm4TlvDq8ikWAM"; // Default Rachel or custom voice ID
        
        [Header("Visual Novel Portraits")]
        public Sprite npcPortraitSprite;
        public Sprite playerMcPortraitSprite;
    }
}
