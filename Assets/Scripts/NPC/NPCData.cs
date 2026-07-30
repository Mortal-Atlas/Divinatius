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
        [Header("Identity & Naming")]
        [Tooltip("Unique ID for saving conversation memory for this NPC.")]
        public string characterId = "npc_custom";

        [Tooltip("Display Name of the NPC (e.g. Celeste the High Priestess).")]
        public string characterName = "Custom NPC";

        [Tooltip("Short Bio or Description of the NPC (e.g. High Priestess of the Astral Temple).")]
        [TextArea(2, 5)]
        public string characterDescription = "A knowledgeable character in the town of Divinatius.";

        [Header("AI Personality & Prompt")]
        [Tooltip("System instructions and AI personality prompt for Gemini API.")]
        [TextArea(5, 15)] 
        public string systemPersonalityPrompt = "You are a knowledgeable guide in Divinatius. Speak with enthusiasm, offer advice, and answer questions thoughtfully.";

        [Header("Voice & Synthesis Settings")]
        [Tooltip("ElevenLabs Voice ID for text-to-speech voice output.")]
        public string elevenLabsVoiceId = "EXAVITQu4vr4xnSDxMaL";

        [Header("Visuals & 3D Model Customization")]
        [Tooltip("Optional 3D Model/Prefab to use for this NPC character's visual body.")]
        public GameObject npc3DModelPrefab;

        [Tooltip("Color tint applied to the NPC's mesh material.")]
        public Color npcColor = Color.white;

        [Header("Visual Novel Portraits")]
        public Sprite npcPortraitSprite;
        public Sprite playerMcPortraitSprite;
    }
}
