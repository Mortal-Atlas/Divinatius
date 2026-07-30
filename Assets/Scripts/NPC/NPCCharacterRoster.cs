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
                CreateNPC("npc_01_celeste", "Celeste", "High Priestess of the Astral Temple. Wise, calm, speaks with reverence and deep ancient knowledge of the realm.", "EXAVITQu4vr4xnSDxMaL"), // Sarah
                CreateNPC("npc_02_kael", "Kael", "A bold rogue scout. Quick-witted, skeptical of authority, always looking for high ground and treasure.", "pNInz6obpgDQGcFmaJgB"), // Adam
                CreateNPC("npc_03_ignatius", "Ignatius", "Master Blacksmith. Gruff, practical, speaks with pride about steel, weapons, and heavy armor.", "VR6AewLTigWG4xSOukaG"), // Arnold
                CreateNPC("npc_04_lyra", "Lyra", "Wandering Bard. Cheerful, poetic, loves telling stories and singing ballads of heroes and dragons.", "21m00Tcm4TlvDq8ikWAM"), // Rachel
                CreateNPC("npc_05_thorne", "Thorne", "Captain of the Town Guard. Duty-bound, strict, focused on safety and tactical threats.", "ErXwobaYiN019PkySvjV"), // Antoni
                CreateNPC("npc_06_vespera", "Vespera", "Shadow Alchemist. Secretive, analytical, fascinated by rare herbs, potions, and forbidden arcana.", "AZnzlk1XvdvUeBnXmlld"), // Domi
                CreateNPC("npc_07_orion", "Orion", "Star Gazer & Astronomer. Soft-spoken scholar, speaks in metaphors about constellations and destiny.", "ErXwobaYiN019PkySvjV"), // Antoni
                CreateNPC("npc_08_maeve", "Maeve", "Village Elder & Healer. Warm, maternal, concerned with the well-being of the townsfolk and nature.", "EXAVITQu4vr4xnSDxMaL"), // Sarah
                CreateNPC("npc_09_zephyr", "Zephyr", "Outlaw Merchant & Smuggler. Charismatic, shrewd bargainer, always knows rumors and illegal goods.", "pNInz6obpgDQGcFmaJgB") // Adam
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
