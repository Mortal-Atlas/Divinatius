#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Divinatius.Core;
using Divinatius.Player;
using Divinatius.NPC;
using Divinatius.Dialogue;
using Divinatius.AI;
using Divinatius.Save;

namespace Divinatius.Editor
{
    public static class SceneSetupBuilder
    {
        [MenuItem("Divinatius/Create Demo Dev Scene")]
        public static void CreateDemoDevScene()
        {
            Debug.Log("[SceneSetupBuilder] Creating Dev Demo Scene...");

            // 1. Create a new clean scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Directional Light
            GameObject lightObj = new GameObject("Directional Light");
            Light lightComp = lightObj.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.color = new Color(1.0f, 0.95f, 0.85f); // Warm sunlight
            lightComp.intensity = 1.2f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 3. Environment Ground Plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_Environment";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, 10f); // 100x100 area

            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");

            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.18f, 0.38f, 0.22f); // Vibrant grassy green
            AssetDatabase.CreateAsset(groundMat, "Assets/Materials/GroundMaterial.mat");
            ground.GetComponent<Renderer>().sharedMaterial = groundMat;

            // 4. Environment Decorations (Town Square Props)
            GameObject envProps = new GameObject("--- ENVIRONMENT DECORATIONS ---");
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2f / 12f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 22f, 1.5f, Mathf.Sin(angle) * 22f);
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = $"Pillar_Decoration_{i+1}";
                pillar.transform.position = pos;
                pillar.transform.localScale = new Vector3(1.2f, 3f, 1.2f);
                pillar.transform.SetParent(envProps.transform);

                Material pillarMat = new Material(Shader.Find("Standard"));
                pillarMat.color = new Color(0.5f, 0.5f, 0.55f);
                pillar.GetComponent<Renderer>().sharedMaterial = pillarMat;
            }

            // 5. Player Character & Camera
            GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "Player_MC";
            playerObj.transform.position = new Vector3(0, 1f, 0);

            Material playerMat = new Material(Shader.Find("Standard"));
            playerMat.color = new Color(0.15f, 0.5f, 0.95f); // Bright blue MC
            AssetDatabase.CreateAsset(playerMat, "Assets/Materials/PlayerMaterial.mat");
            playerObj.GetComponent<Renderer>().sharedMaterial = playerMat;

            CharacterController cc = playerObj.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = Vector3.zero;

            PlayerController pc = playerObj.AddComponent<PlayerController>();
            PlayerInteraction pi = playerObj.AddComponent<PlayerInteraction>();

            GameObject camObj = new GameObject("Main Camera");
            Camera mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
            mainCam.transform.position = new Vector3(0, 2.8f, -3.5f);
            mainCam.transform.rotation = Quaternion.Euler(15f, 0, 0);

            // 6. Core Managers
            GameObject managersObj = new GameObject("--- MANAGERS ---");
            managersObj.AddComponent<ApiConfig>();
            managersObj.AddComponent<UnityThreadDispatcher>();
            managersObj.AddComponent<GeminiService>();
            managersObj.AddComponent<ElevenLabsService>();
            managersObj.AddComponent<CharacterMemoryManager>();
            managersObj.AddComponent<NPCCharacterRoster>();

            // 7. 9 NPCs Ring Formation
            GameObject npcParent = new GameObject("--- NPCs (9 Characters) ---");
            Vector3[] positions = new Vector3[]
            {
                new Vector3(0, 1, 10),    // 1. Celeste (North)
                new Vector3(7, 1, 7),     // 2. Kael (NorthEast)
                new Vector3(10, 1, 0),    // 3. Ignatius (East)
                new Vector3(7, 1, -7),    // 4. Lyra (SouthEast)
                new Vector3(0, 1, -10),   // 5. Thorne (South)
                new Vector3(-7, 1, -7),   // 6. Vespera (SouthWest)
                new Vector3(-10, 1, 0),   // 7. Orion (West)
                new Vector3(-7, 1, 7),    // 8. Maeve (NorthWest)
                new Vector3(0, 1, 4)      // 9. Zephyr (Center Plaza)
            };

            Color[] npcColors = new Color[]
            {
                new Color(0.95f, 0.95f, 0.95f), // Celeste - White/Gold
                new Color(0.35f, 0.35f, 0.35f), // Kael - Dark Rogue
                new Color(0.85f, 0.35f, 0.15f), // Ignatius - Iron Red
                new Color(0.95f, 0.75f, 0.25f), // Lyra - Gold Bard
                new Color(0.25f, 0.45f, 0.85f), // Thorne - Royal Blue
                new Color(0.55f, 0.25f, 0.85f), // Vespera - Purple
                new Color(0.15f, 0.75f, 0.95f), // Orion - Cyan
                new Color(0.35f, 0.85f, 0.45f), // Maeve - Forest Green
                new Color(0.75f, 0.55f, 0.25f)  // Zephyr - Bronze
            };

            var defaultRoster = NPCCharacterRoster.CreateDefaultRoster();

            for (int i = 0; i < defaultRoster.Count; i++)
            {
                var profile = defaultRoster[i];
                string assetPath = $"Assets/Resources/NPCs/{profile.characterId}.asset";

                if (!AssetDatabase.IsValidFolder("Assets/Resources/NPCs"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    AssetDatabase.CreateFolder("Assets/Resources", "NPCs");
                }

                AssetDatabase.CreateAsset(profile, assetPath);

                GameObject npcObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                npcObj.name = $"NPC_{i+1}_{profile.characterName}";
                npcObj.transform.position = positions[i];
                npcObj.transform.SetParent(npcParent.transform);

                Material mat = new Material(Shader.Find("Standard"));
                mat.color = npcColors[i];
                AssetDatabase.CreateAsset(mat, $"Assets/Materials/NPC_{profile.characterName}_Mat.mat");
                npcObj.GetComponent<Renderer>().sharedMaterial = mat;

                NPCInteractable interactable = npcObj.AddComponent<NPCInteractable>();
                SerializedObject serializedInteractable = new SerializedObject(interactable);
                serializedInteractable.FindProperty("npcProfile").objectReferenceValue = profile;
                serializedInteractable.ApplyModifiedProperties();
            }

            // 8. Visual Novel Dialogue Canvas & Controls HUD
            BuildDialogueUICanvas();
            BuildControlsHUDCanvas();

            // 9. Save Scene to Assets/Scenes/DevDemoScene.unity
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            string scenePath = "Assets/Scenes/DevDemoScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SceneSetupBuilder] Dev Demo Scene created and saved to: {scenePath}");
        }

        private static void BuildControlsHUDCanvas()
        {
            GameObject hudObj = new GameObject("ControlsHUDCanvas");
            Canvas canvas = hudObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject infoBox = new GameObject("InfoPanel");
            infoBox.transform.SetParent(hudObj.transform, false);
            RectTransform rect = infoBox.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.82f);
            rect.anchorMax = new Vector2(0.32f, 0.98f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bg = infoBox.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.65f);

            GameObject textObj = new GameObject("HUDText");
            textObj.transform.SetParent(infoBox.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            text.text = "🎮 GAMEPLAY CONTROLS:\n• WASD: Move Around\n• Shift: Sprint | Space: Jump\n• Mouse: Orbital Camera Look\n• Walk to NPC & Press 'E': Talk";
        }

        private static void BuildDialogueUICanvas()
        {
            GameObject canvasObj = new GameObject("VisualNovelCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject panelObj = new GameObject("DialoguePanelRoot");
            panelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector3.zero;
            panelRect.anchorMax = Vector3.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.45f);

            // Left MC Portrait Frame
            GameObject mcPortraitObj = new GameObject("McPortraitImage");
            mcPortraitObj.transform.SetParent(panelObj.transform, false);
            RectTransform mcRect = mcPortraitObj.AddComponent<RectTransform>();
            mcRect.anchorMin = new Vector2(0.05f, 0.22f);
            mcRect.anchorMax = new Vector2(0.25f, 0.75f);
            mcRect.offsetMin = Vector2.zero;
            mcRect.offsetMax = Vector2.zero;
            Image mcImage = mcPortraitObj.AddComponent<Image>();
            mcImage.color = new Color(0.2f, 0.5f, 0.95f, 0.9f);

            // Right NPC Portrait Frame
            GameObject npcPortraitObj = new GameObject("NpcPortraitImage");
            npcPortraitObj.transform.SetParent(panelObj.transform, false);
            RectTransform npcRect = npcPortraitObj.AddComponent<RectTransform>();
            npcRect.anchorMin = new Vector2(0.75f, 0.22f);
            npcRect.anchorMax = new Vector2(0.95f, 0.75f);
            npcRect.offsetMin = Vector2.zero;
            npcRect.offsetMax = Vector2.zero;
            Image npcImage = npcPortraitObj.AddComponent<Image>();
            npcImage.color = new Color(0.95f, 0.75f, 0.2f, 0.9f);

            // Bottom Dialogue Box
            GameObject dialogueBoxObj = new GameObject("DialogueBox");
            dialogueBoxObj.transform.SetParent(panelObj.transform, false);
            RectTransform boxRect = dialogueBoxObj.AddComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.1f, 0.04f);
            boxRect.anchorMax = new Vector2(0.9f, 0.32f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;
            Image boxBg = dialogueBoxObj.AddComponent<Image>();
            boxBg.color = new Color(0.06f, 0.06f, 0.1f, 0.95f);

            // Speaker Name Text
            GameObject nameTextObj = new GameObject("SpeakerNameText");
            nameTextObj.transform.SetParent(dialogueBoxObj.transform, false);
            RectTransform nameRect = nameTextObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.03f, 0.78f);
            nameRect.anchorMax = new Vector2(0.5f, 0.96f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            Text nameText = nameTextObj.AddComponent<Text>();
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 22;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = Color.yellow;
            nameText.text = "Character Name";

            // Dialogue Body Text
            GameObject bodyTextObj = new GameObject("DialogueBodyText");
            bodyTextObj.transform.SetParent(dialogueBoxObj.transform, false);
            RectTransform bodyRect = bodyTextObj.AddComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0.03f, 0.32f);
            bodyRect.anchorMax = new Vector2(0.97f, 0.76f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;
            Text bodyText = bodyTextObj.AddComponent<Text>();
            bodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bodyText.fontSize = 18;
            bodyText.color = Color.white;
            bodyText.text = "Dialogue response text will display here...";

            // TextInputField
            GameObject inputFieldObj = new GameObject("TextInputField");
            inputFieldObj.transform.SetParent(dialogueBoxObj.transform, false);
            RectTransform inputRect = inputFieldObj.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.03f, 0.05f);
            inputRect.anchorMax = new Vector2(0.7f, 0.27f);
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
            Image inputBg = inputFieldObj.AddComponent<Image>();
            inputBg.color = new Color(0.18f, 0.18f, 0.24f, 1f);
            InputField inputField = inputFieldObj.AddComponent<InputField>();

            GameObject inputTextObj = new GameObject("Text");
            inputTextObj.transform.SetParent(inputFieldObj.transform, false);
            RectTransform textRect = inputTextObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 0);
            textRect.offsetMax = new Vector2(-8, 0);
            Text inputText = inputTextObj.AddComponent<Text>();
            inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            inputText.fontSize = 16;
            inputText.color = Color.white;
            inputField.textComponent = inputText;

            // Send Button
            GameObject sendBtnObj = new GameObject("SendButton");
            sendBtnObj.transform.SetParent(dialogueBoxObj.transform, false);
            RectTransform sendRect = sendBtnObj.AddComponent<RectTransform>();
            sendRect.anchorMin = new Vector2(0.72f, 0.05f);
            sendRect.anchorMax = new Vector2(0.83f, 0.27f);
            sendRect.offsetMin = Vector2.zero;
            sendRect.offsetMax = Vector2.zero;
            Image sendBg = sendBtnObj.AddComponent<Image>();
            sendBg.color = new Color(0.2f, 0.6f, 0.3f, 1f);
            Button sendBtn = sendBtnObj.AddComponent<Button>();

            GameObject sendLabelObj = new GameObject("Label");
            sendLabelObj.transform.SetParent(sendBtnObj.transform, false);
            RectTransform sendLabelRect = sendLabelObj.AddComponent<RectTransform>();
            sendLabelRect.anchorMin = Vector2.zero;
            sendLabelRect.anchorMax = Vector2.one;
            Text sendText = sendLabelObj.AddComponent<Text>();
            sendText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sendText.alignment = TextAnchor.MiddleCenter;
            sendText.fontSize = 14;
            sendText.color = Color.white;
            sendText.text = "Send";

            // Mic Button
            GameObject micBtnObj = new GameObject("MicButton");
            micBtnObj.transform.SetParent(dialogueBoxObj.transform, false);
            RectTransform micRect = micBtnObj.AddComponent<RectTransform>();
            micRect.anchorMin = new Vector2(0.85f, 0.05f);
            micRect.anchorMax = new Vector2(0.97f, 0.27f);
            micRect.offsetMin = Vector2.zero;
            micRect.offsetMax = Vector2.zero;
            Image micBg = micBtnObj.AddComponent<Image>();
            micBg.color = new Color(0.8f, 0.3f, 0.2f, 1f);
            Button micBtn = micBtnObj.AddComponent<Button>();

            GameObject micLabelObj = new GameObject("Label");
            micLabelObj.transform.SetParent(micBtnObj.transform, false);
            RectTransform micLabelRect = micLabelObj.AddComponent<RectTransform>();
            micLabelRect.anchorMin = Vector2.zero;
            micLabelRect.anchorMax = Vector2.one;
            Text micText = micLabelObj.AddComponent<Text>();
            micText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            micText.alignment = TextAnchor.MiddleCenter;
            micText.fontSize = 14;
            micText.color = Color.white;
            micText.text = "🎤 Voice";

            // Attach Dialogue Controllers & Wire References
            DialogueUIController uiController = canvasObj.AddComponent<DialogueUIController>();
            DialogueInputController inputCtrl = canvasObj.AddComponent<DialogueInputController>();

            SerializedObject soInput = new SerializedObject(inputCtrl);
            soInput.FindProperty("textInputField").objectReferenceValue = inputField;
            soInput.FindProperty("sendButton").objectReferenceValue = sendBtn;
            soInput.FindProperty("micRecordButton").objectReferenceValue = micBtn;
            soInput.FindProperty("micButtonText").objectReferenceValue = micText;
            soInput.ApplyModifiedProperties();

            SerializedObject soUI = new SerializedObject(uiController);
            soUI.FindProperty("dialoguePanelRoot").objectReferenceValue = panelObj;
            soUI.FindProperty("mcPortraitImage").objectReferenceValue = mcImage;
            soUI.FindProperty("npcPortraitImage").objectReferenceValue = npcImage;
            soUI.FindProperty("speakerNameText").objectReferenceValue = nameText;
            soUI.FindProperty("dialogueBodyText").objectReferenceValue = bodyText;
            soUI.FindProperty("inputController").objectReferenceValue = inputCtrl;
            soUI.ApplyModifiedProperties();

            panelObj.SetActive(false);
        }
    }
}
#endif
