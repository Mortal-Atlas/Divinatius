using UnityEngine;

namespace Divinatius.NPC
{
    public class NPCInteractable : MonoBehaviour
    {
        [Header("NPC Profile ScriptableObject Asset (Optional)")]
        [SerializeField] private NPCProfileSO npcProfile;

        [Header("Identity & Naming")]
        [Tooltip("Unique ID for saving conversation memory for this NPC.")]
        [SerializeField] private string characterId = "npc_custom";

        [Tooltip("Display name of the character shown in Dialogue UI.")]
        [SerializeField] private string characterName = "Custom NPC";

        [Tooltip("Short Bio / Description of who this NPC is.")]
        [TextArea(2, 4)]
        [SerializeField] private string characterDescription = "A resident of Divinatius town.";

        [Header("AI Demeanor & Personality Prompt")]
        [Tooltip("Type custom system instructions and personality prompt for this individual NPC here.")]
        [TextArea(5, 12)]
        [SerializeField] private string systemPersonalityPrompt = "You are a knowledgeable guide in Divinatius. Speak with enthusiasm, offer advice, and answer questions thoughtfully.";

        [Header("Voice & Voice ID Settings")]
        [Tooltip("ElevenLabs Voice ID for text-to-speech audio synthesis.")]
        [SerializeField] private string elevenLabsVoiceId = "EXAVITQu4vr4xnSDxMaL";

        [Header("Visual Model & Color Customization")]
        [Tooltip("Optional 3D Character Model/Prefab to use for this NPC's mesh.")]
        [SerializeField] private GameObject npc3DModelPrefab;

        [Tooltip("Color tint applied to the NPC's mesh material.")]
        [SerializeField] private Color npcColor = Color.white;

        [Header("Visual Novel Portraits")]
        [SerializeField] private Sprite npcPortraitSprite;
        [SerializeField] private Sprite playerMcPortraitSprite;

        private NPCProfileSO _runtimeProfile;

        public string CharacterId => characterId;
        public string CharacterName => characterName;
        public string CharacterDescription => characterDescription;
        public string SystemPersonalityPrompt => systemPersonalityPrompt;
        public string ElevenLabsVoiceId => elevenLabsVoiceId;
        public Color NPCColor => npcColor;
        public GameObject NPC3DModelPrefab => npc3DModelPrefab;

        public void SetCharacterDetails(string name, string description, string prompt, string voiceId, Color color, GameObject modelPrefab = null)
        {
            characterName = name;
            characterDescription = description;
            systemPersonalityPrompt = prompt;
            elevenLabsVoiceId = voiceId;
            npcColor = color;
            if (modelPrefab != null) npc3DModelPrefab = modelPrefab;
            ApplyVisualChanges();
        }

        private void OnValidate()
        {
            if (npcProfile != null)
            {
                if (string.IsNullOrEmpty(characterId) || characterId == "npc_custom")
                    characterId = npcProfile.characterId;
                if (string.IsNullOrEmpty(characterName) || characterName == "Custom NPC")
                    characterName = npcProfile.characterName;
                if (string.IsNullOrEmpty(characterDescription) || characterDescription == "A resident of Divinatius town.")
                    characterDescription = npcProfile.characterDescription;
                if (string.IsNullOrEmpty(systemPersonalityPrompt) || systemPersonalityPrompt.StartsWith("You are a knowledgeable guide"))
                    systemPersonalityPrompt = npcProfile.systemPersonalityPrompt;
                if (string.IsNullOrEmpty(elevenLabsVoiceId) || elevenLabsVoiceId == "EXAVITQu4vr4xnSDxMaL")
                    elevenLabsVoiceId = npcProfile.elevenLabsVoiceId;
                if (npcColor == Color.white && npcProfile.npcColor != Color.white)
                    npcColor = npcProfile.npcColor;
                if (npc3DModelPrefab == null && npcProfile.npc3DModelPrefab != null)
                    npc3DModelPrefab = npcProfile.npc3DModelPrefab;
                if (npcPortraitSprite == null)
                    npcPortraitSprite = npcProfile.npcPortraitSprite;
                if (playerMcPortraitSprite == null)
                    playerMcPortraitSprite = npcProfile.playerMcPortraitSprite;
            }

            ApplyVisualChanges();
        }

        public void ApplyVisualChanges()
        {
            // 1. Auto-rename GameObject in Hierarchy
            if (!string.IsNullOrEmpty(characterName))
            {
                string expectedName = $"NPC_{characterName}";
                if (gameObject.name != expectedName && !gameObject.name.StartsWith($"NPC_") && !gameObject.name.Contains(characterName))
                {
                    gameObject.name = expectedName;
                }
            }

            // 2. Apply Color Tint to Renderer
            Renderer rend = GetComponent<Renderer>();
            if (rend == null) rend = GetComponentInChildren<Renderer>();
            if (rend != null && rend.sharedMaterial != null)
            {
                if (rend.sharedMaterial.HasProperty("_Color"))
                {
                    rend.sharedMaterial.color = npcColor;
                }
                if (rend.sharedMaterial.HasProperty("_BaseColor"))
                {
                    rend.sharedMaterial.SetColor("_BaseColor", npcColor);
                }
            }

            // 3. Swap 3D Model Prefab if assigned
            if (npc3DModelPrefab != null)
            {
                Transform existingModel = transform.Find("CustomModelInstance");
                if (existingModel == null)
                {
                    Renderer baseRend = GetComponent<MeshRenderer>();
                    if (baseRend != null) baseRend.enabled = false;

                    GameObject newModel = Instantiate(npc3DModelPrefab, transform);
                    newModel.name = "CustomModelInstance";
                    newModel.transform.localPosition = Vector3.zero;
                    newModel.transform.localRotation = Quaternion.identity;
                }
            }
        }

        public NPCProfileSO NPCProfile
        {
            get
            {
                if (_runtimeProfile == null)
                {
                    if (npcProfile != null)
                    {
                        _runtimeProfile = Instantiate(npcProfile);
                    }
                    else
                    {
                        _runtimeProfile = ScriptableObject.CreateInstance<NPCProfileSO>();
                    }
                }

                _runtimeProfile.characterId = string.IsNullOrEmpty(characterId) ? "npc_custom" : characterId;
                _runtimeProfile.characterName = string.IsNullOrEmpty(characterName) ? "Custom NPC" : characterName;
                _runtimeProfile.characterDescription = characterDescription;
                _runtimeProfile.systemPersonalityPrompt = systemPersonalityPrompt;
                _runtimeProfile.elevenLabsVoiceId = string.IsNullOrEmpty(elevenLabsVoiceId) ? "EXAVITQu4vr4xnSDxMaL" : elevenLabsVoiceId;
                _runtimeProfile.npcColor = npcColor;
                _runtimeProfile.npc3DModelPrefab = npc3DModelPrefab;
                _runtimeProfile.npcPortraitSprite = npcPortraitSprite != null ? npcPortraitSprite : (npcProfile != null ? npcProfile.npcPortraitSprite : null);
                _runtimeProfile.playerMcPortraitSprite = playerMcPortraitSprite != null ? playerMcPortraitSprite : (npcProfile != null ? npcProfile.playerMcPortraitSprite : null);

                return _runtimeProfile;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = npcColor != Color.white ? npcColor : Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 1f, new Vector3(0.6f, 2f, 0.6f));
        }
    }
}
