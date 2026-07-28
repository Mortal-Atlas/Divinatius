using System.Collections.Generic;
using UnityEngine;

namespace Divinatius.NPC
{
    public class NPCCharacterRoster : MonoBehaviour
    {
        public static NPCCharacterRoster Instance { get; private set; }

        [Header("9 NPC Profiles Configured")]
        [SerializeField] private List<NPCProfileSO> characterProfiles = new List<NPCProfileSO>();

        private Dictionary<string, NPCProfileSO> _profileMap = new Dictionary<string, NPCProfileSO>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDefaultsIfEmpty();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDefaultsIfEmpty()
        {
            if (characterProfiles.Count == 0)
            {
                // Create runtime default profiles for all 9 characters if SO assets not assigned
                characterProfiles = CreateDefaultRoster();
            }

            foreach (var p in characterProfiles)
            {
                if (p != null && !string.IsNullOrEmpty(p.characterId))
                {
                    _profileMap[p.characterId] = p;
                }
            }
            Debug.Log($"[NPCCharacterRoster] Registered {_profileMap.Count} character profiles into roster.");
        }

        public NPCProfileSO GetProfile(string characterId)
        {
            if (_profileMap.TryGetValue(characterId, out var profile))
            {
                return profile;
            }
            return null;
        }

        public List<NPCProfileSO> GetAllProfiles() => characterProfiles;

        public static List<NPCProfileSO> CreateDefaultRoster()
        {
            return new List<NPCProfileSO>
            {
                CreateNPC("npc_01_celeste", "Celeste", "High Priestess of the Astral Temple. Wise, calm, speaks with reverence and deep ancient knowledge of the realm.", "21m00Tcm4TlvDq8ikWAM"),
                CreateNPC("npc_02_kael", "Kael", "A bold rogue scout. Quick-witted, skeptical of authority, always looking for high ground and treasure.", "AZnzlk1XvdvUeBnXmlld"),
                CreateNPC("npc_03_ignatius", "Ignatius", "Master Blacksmith. Gruff, practical, speaks with pride about steel, weapons, and heavy armor.", "EXAVITQu4vr4xnSDxMaL"),
                CreateNPC("npc_04_lyra", "Lyra", "Wandering Bard. Cheerful, poetic, loves telling stories and singing ballads of heroes and dragons.", "ErXwobaYiN019PkySvjV"),
                CreateNPC("npc_05_thorne", "Thorne", "Captain of the Town Guard. Duty-bound, strict, focused on safety and tactical threats.", "MF3mGyEYCl7XYWbV9V6O"),
                CreateNPC("npc_06_vespera", "Vespera", "Shadow Alchemist. Secretive, analytical, fascinated by rare herbs, potions, and forbidden arcana.", "TxGEqnHWrfWFTfGW9XjX"),
                CreateNPC("npc_07_orion", "Orion", "Star Gazer & Astronomer. Soft-spoken scholar, speaks in metaphors about constellations and destiny.", "VR6AewLTigWG4xSOukaG"),
                CreateNPC("npc_08_maeve", "Maeve", "Village Elder & Healer. Warm, maternal, concerned with the well-being of the townsfolk and nature.", "pNInz6obpgDQGcFmaJgB"),
                CreateNPC("npc_09_zephyr", "Zephyr", "Outlaw Merchant & Smuggler. Charismatic, shrewd bargainer, always knows rumors and illegal goods.", "yoZ06aGfZXNShxVf3o12")
            };
        }

        private static NPCProfileSO CreateNPC(string id, string name, string personality, string voiceId)
        {
            var profile = ScriptableObject.CreateInstance<NPCProfileSO>();
            profile.characterId = id;
            profile.characterName = name;
            profile.systemPersonalityPrompt = personality;
            profile.elevenLabsVoiceId = voiceId;
            return profile;
        }
    }
}
