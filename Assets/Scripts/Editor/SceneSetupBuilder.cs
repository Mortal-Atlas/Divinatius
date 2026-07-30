#if UNITY_EDITOR
#pragma warning disable 0618
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Divinatius.Core;
using Divinatius.Player;
using Divinatius.NPC;
using Divinatius.Dialogue;
using Divinatius.AI;
using Divinatius.Save;
using System.Collections.Generic;

namespace Divinatius.Editor
{
    public struct MultiStoryBuildingDef
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 size; // width (x), height (y), depth (z)
        public int stories;  // 1, 2, or 3 stories
        public Color wallCol;
        public Color roofCol;
        public Color trimCol;
        public bool isFacade;
    }

    public static class SceneSetupBuilder
    {
        public static Shader GetURPLitShader()
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("URP/Lit");
            if (s == null) s = Shader.Find("Standard");
            return s;
        }

        public static Material GetOrCreateMaterialAsset(string materialName, Color color, float smoothness = 0.5f, bool isWater = false)
        {
            string sanitizedName = materialName.Replace(" ", "_").Replace("/", "_");
            string path = $"Assets/Materials/{sanitizedName}.mat";

            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                if (!AssetDatabase.IsValidFolder("Assets"))
                    AssetDatabase.CreateFolder("", "Assets");
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader urpShader = GetURPLitShader();

            if (existingMat != null)
            {
                existingMat.shader = urpShader;
                existingMat.color = color;
                if (existingMat.HasProperty("_BaseColor")) existingMat.SetColor("_BaseColor", color);
                if (existingMat.HasProperty("_Smoothness")) existingMat.SetFloat("_Smoothness", smoothness);
                return existingMat;
            }

            Material mat = new Material(urpShader);
            mat.name = sanitizedName;
            mat.color = color;

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", smoothness);
            }

            if (isWater)
            {
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1); // Transparent
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0);     // Alpha
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.95f);
            }
            else
            {
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 0); // Opaque
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0);
                if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 1);
            }

            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void FixCylinderCollider(GameObject obj)
        {
            CapsuleCollider cc = obj.GetComponent<CapsuleCollider>();
            if (cc != null)
            {
                Object.DestroyImmediate(cc);
            }
            if (obj.GetComponent<Collider>() == null)
            {
                obj.AddComponent<MeshCollider>();
            }
        }

        [MenuItem("Divinatius/Fix Scene View Display & Lighting")]
        public static void FixSceneViewDisplaySettings()
        {
            foreach (SceneView sv in SceneView.sceneViews)
            {
                if (sv != null)
                {
                    sv.sceneLighting = true;
                    sv.cameraMode = SceneView.GetBuiltinCameraMode(DrawCameraMode.Normal);
                    sv.Repaint();
                }
            }
            Debug.Log("[SceneSetupBuilder] Scene View set to Normal Shaded with Scene Lighting enabled.");
        }

        [MenuItem("Divinatius/Convert All Scene Materials to URP Lit")]
        public static void ConvertAllSceneMaterialsToURPLit()
        {
            Shader urpShader = GetURPLitShader();
            Renderer[] renderers = Object.FindObjectsOfType<Renderer>();
            int count = 0;

            foreach (var r in renderers)
            {
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat != null && mat.shader != urpShader)
                    {
                        mat.shader = urpShader;
                        if (mat.HasProperty("_BaseColor"))
                        {
                            mat.SetColor("_BaseColor", mat.color);
                        }
                        count++;
                    }
                }
            }

            Debug.Log($"[SceneSetupBuilder] Successfully converted {count} scene object materials to URP/Lit ({urpShader.name}).");
        }

        [MenuItem("Divinatius/Create Demo Dev Scene")]
        public static void CreateDemoDevScene()
        {
            Debug.Log("[SceneSetupBuilder] Creating Compact Multi-Story Town Street Wall Layout...");

            // 1. Create a new clean scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Directional Light (Warm Sunset Atmosphere)
            GameObject lightObj = new GameObject("Directional Light");
            Light lightComp = lightObj.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.color = new Color(1.0f, 0.92f, 0.82f);
            lightComp.intensity = 1.15f;
            lightObj.transform.rotation = Quaternion.Euler(42f, -38f, 0f);

            // 3. Base Ground Terrain
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_BaseTerrain";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(18f, 1f, 18f);
            GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.NavigationStatic);

            Material groundMat = GetOrCreateMaterialAsset("GroundMaterial", new Color(0.16f, 0.32f, 0.18f));
            ground.GetComponent<Renderer>().sharedMaterial = groundMat;

            // 4. Build Compact Multi-Story Town Layout
            GameObject townParent = new GameObject("--- COMPACT MULTI-STORY TOWN STREETS ---");
            BuildCompactTownLayout(townParent.transform);

            // 5. Player Character & Camera (Spawned at South Entrance of Main Street)
            GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "Player_MC";
            playerObj.tag = "Player"; // Tag explicitly assigned!
            playerObj.transform.position = new Vector3(0, 1.0f, -48f);

            Material playerMat = GetOrCreateMaterialAsset("PlayerMaterial", new Color(0.15f, 0.5f, 0.95f), 0.8f);
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
            mainCam.transform.position = new Vector3(0, 2.7f, -6.5f);
            mainCam.transform.rotation = Quaternion.Euler(18f, 0, 0);

            // 6. Core Managers
            GameObject managersObj = new GameObject("--- MANAGERS ---");
            managersObj.AddComponent<ApiConfig>();
            managersObj.AddComponent<UnityThreadDispatcher>();
            managersObj.AddComponent<GeminiService>();
            managersObj.AddComponent<ElevenLabsService>();
            managersObj.AddComponent<CharacterMemoryManager>();
            managersObj.AddComponent<NPCCharacterRoster>();
            managersObj.AddComponent<Divinatius.UI.MinimapUIController>();
            managersObj.AddComponent<Divinatius.VFX.NPCSpellVFXManager>();
            managersObj.AddComponent<Divinatius.Buffs.PlayerBuffManager>();

            // 7. 9 NPCs Distributed Across Main Street & Compact Alleyways
            GameObject npcParent = new GameObject("--- NPCs (9 NavMesh Wandering Agents) ---");
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-3.5f, 0.5f, -40f), // 1. Celeste (South Town Entrance)
                new Vector3(3.5f, 0.5f, -25f),  // 2. Kael (East Market Alley)
                new Vector3(-3.5f, 0.5f, -12f), // 3. Ignatius (West Forge Row)
                new Vector3(0f, 0.5f, 0f),      // 4. Lyra (Central Town Square Plaza)
                new Vector3(3.5f, 0.5f, 12f),   // 5. Thorne (East Guild Street)
                new Vector3(-3.5f, 0.5f, 25f),  // 6. Vespera (West Alchemy Row)
                new Vector3(3.5f, 0.5f, 38f),   // 7. Orion (North High Street)
                new Vector3(-16f, 0.5f, -20f),  // 8. Maeve (West Side Alley)
                new Vector3(16f, 0.5f, 20f)     // 9. Zephyr (East Side Alley)
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

            string[] npcDescriptions = new string[]
            {
                "High Priestess of the Astral Temple. Wise, calm, speaks with reverence.",
                "A bold rogue scout. Quick-witted, skeptical of authority, seeking treasure.",
                "Master Blacksmith. Gruff, practical, speaks with pride about steel and armor.",
                "Wandering Bard. Cheerful, poetic, loves telling stories and ballads of heroes.",
                "Captain of the Town Guard. Duty-bound, strict, focused on safety and threats.",
                "Shadow Alchemist. Secretive, analytical, fascinated by rare herbs and arcana.",
                "Star Gazer & Astronomer. Soft-spoken scholar who speaks in stellar metaphors.",
                "Village Elder & Healer. Warm, maternal, concerned with townsfolk well-being.",
                "Outlaw Merchant & Smuggler. Charismatic, shrewd bargainer who knows all rumors."
            };

            var defaultRoster = NPCCharacterRoster.CreateDefaultRoster();

            for (int i = 0; i < defaultRoster.Count; i++)
            {
                var profile = defaultRoster[i];
                profile.characterDescription = npcDescriptions[i];
                profile.npcColor = npcColors[i];

                string assetPath = $"Assets/Resources/NPCs/{profile.characterId}.asset";

                if (!AssetDatabase.IsValidFolder("Assets/Resources/NPCs"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    AssetDatabase.CreateFolder("Assets/Resources", "NPCs");
                }

                AssetDatabase.CreateAsset(profile, assetPath);

                GameObject npcObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                npcObj.name = $"NPC_{profile.characterName}";
                npcObj.transform.position = positions[i];
                npcObj.transform.SetParent(npcParent.transform);

                Material mat = GetOrCreateMaterialAsset($"NPC_{profile.characterName}_Mat", npcColors[i], 0.6f);
                npcObj.GetComponent<Renderer>().sharedMaterial = mat;

                NPCInteractable interactable = npcObj.AddComponent<NPCInteractable>();
                SerializedObject serializedInteractable = new SerializedObject(interactable);
                serializedInteractable.FindProperty("npcProfile").objectReferenceValue = profile;
                serializedInteractable.FindProperty("characterId").stringValue = profile.characterId;
                serializedInteractable.FindProperty("characterName").stringValue = profile.characterName;
                serializedInteractable.FindProperty("characterDescription").stringValue = npcDescriptions[i];
                serializedInteractable.FindProperty("systemPersonalityPrompt").stringValue = profile.systemPersonalityPrompt;
                serializedInteractable.FindProperty("elevenLabsVoiceId").stringValue = profile.elevenLabsVoiceId;
                serializedInteractable.FindProperty("npcColor").colorValue = npcColors[i];
                serializedInteractable.ApplyModifiedProperties();

                NavMeshAgent agent = npcObj.AddComponent<NavMeshAgent>();
                agent.speed = 2.0f;
                agent.stoppingDistance = 0.5f;
                agent.radius = 0.4f;
                agent.height = 2.0f;

                npcObj.AddComponent<NPCWanderer>();

                // Add Ambient Barks & Proximity Reactions
                npcObj.AddComponent<NPCAmbientBark>();

                // Add Inspector Plot Points & Quest Lore Knowledge
                NPCPlotKnowledge plotKnowledge = npcObj.AddComponent<NPCPlotKnowledge>();
                plotKnowledge.plotPoints = new List<PlotPoint>
                {
                    new PlotPoint
                    {
                        topicName = "Demon Lord Quest",
                        keywords = new List<string> { "demon", "demon lord", "slay", "boss", "spire", "kill demon" },
                        plotInformation = "The Demon Lord dwells high in the Obsidian Spire beyond the northern pass. Legend says only a weapon forged in sacred dragon flame can breach his shadow armor!",
                        isDiscovered = false
                    },
                    new PlotPoint
                    {
                        topicName = "Town Gate Key",
                        keywords = new List<string> { "gate", "key", "north gate", "lock", "escape" },
                        plotInformation = "The key to the North Town Gate was hidden inside the Starlight Tavern by Captain Thorne for safekeeping.",
                        isDiscovered = false
                    }
                };
            }

            // 8. Visual Novel Dialogue Canvas & Controls HUD
            BuildDialogueUICanvas();
            BuildControlsHUDCanvas();

            // 9. Convert all scene object materials to URP Lit
            ConvertAllSceneMaterialsToURPLit();

            // 10. Bake NavMesh Navigation Data
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

            // 11. Reset Scene View Draw Modes
            FixSceneViewDisplaySettings();

            // 12. Save Scene to Assets/Scenes/DevDemoScene.unity
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            string scenePath = "Assets/Scenes/DevDemoScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SceneSetupBuilder] Compact Multi-Story Town saved to: {scenePath}");
        }

        private static void BuildCompactTownLayout(Transform townParent)
        {
            Material stoneMat = GetOrCreateMaterialAsset("Town_CobbleStoneRoad", new Color(0.35f, 0.36f, 0.4f));
            Material plazaMat = GetOrCreateMaterialAsset("Town_PlazaPaver", new Color(0.72f, 0.7f, 0.65f), 0.6f);

            // Main Avenue
            GameObject mainAve = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainAve.name = "Street_MainAvenue";
            mainAve.transform.SetParent(townParent, false);
            mainAve.transform.localPosition = new Vector3(0, 0.02f, 0);
            mainAve.transform.localScale = new Vector3(11f, 0.05f, 105f);
            mainAve.GetComponent<Renderer>().sharedMaterial = stoneMat;
            GameObjectUtility.SetStaticEditorFlags(mainAve, StaticEditorFlags.NavigationStatic);

            // Plaza
            GameObject townSquare = GameObject.CreatePrimitive(PrimitiveType.Cube);
            townSquare.name = "Plaza_TownSquare";
            townSquare.transform.SetParent(townParent, false);
            townSquare.transform.localPosition = new Vector3(0, 0.03f, 0);
            townSquare.transform.localScale = new Vector3(22f, 0.05f, 22f);
            townSquare.GetComponent<Renderer>().sharedMaterial = plazaMat;
            GameObjectUtility.SetStaticEditorFlags(townSquare, StaticEditorFlags.NavigationStatic);

            // Cross Alleys
            GameObject crossSouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crossSouth.name = "Street_SouthCrossAlley";
            crossSouth.transform.SetParent(townParent, false);
            crossSouth.transform.localPosition = new Vector3(0, 0.02f, -20f);
            crossSouth.transform.localScale = new Vector3(50f, 0.05f, 7f);
            crossSouth.GetComponent<Renderer>().sharedMaterial = stoneMat;
            GameObjectUtility.SetStaticEditorFlags(crossSouth, StaticEditorFlags.NavigationStatic);

            GameObject crossNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crossNorth.name = "Street_NorthCrossAlley";
            crossNorth.transform.SetParent(townParent, false);
            crossNorth.transform.localPosition = new Vector3(0, 0.02f, 20f);
            crossNorth.transform.localScale = new Vector3(50f, 0.05f, 7f);
            crossNorth.GetComponent<Renderer>().sharedMaterial = stoneMat;
            GameObjectUtility.SetStaticEditorFlags(crossNorth, StaticEditorFlags.NavigationStatic);

            // Drainage Canals
            CreateLinearDrainageCanal(townParent, new Vector3(-5.1f, 0, -48f), new Vector3(-5.1f, 0, 48f));
            CreateLinearDrainageCanal(townParent, new Vector3(5.1f, 0, -48f), new Vector3(5.1f, 0, 48f));
            CreateLinearDrainageCanal(townParent, new Vector3(-24f, 0, -16.8f), new Vector3(24f, 0, -16.8f));
            CreateLinearDrainageCanal(townParent, new Vector3(-24f, 0, 16.8f), new Vector3(24f, 0, 16.8f));

            // Fountain & Gates
            CreateWaterFountain(townParent, Vector3.zero, 3.8f);
            CreateTownGate(townParent, "SouthTownGate", new Vector3(0, 0, -49f));
            CreateTownGate(townParent, "NorthTownGate", new Vector3(0, 0, 49f));

            // Buildings
            MultiStoryBuildingDef[] westMainRow = new MultiStoryBuildingDef[]
            {
                new MultiStoryBuildingDef { name = "BakeryShop", position = new Vector3(-9.5f, 0, -40f), rotation = Quaternion.Euler(0, 90f, 0), size = new Vector3(9.5f, 8.5f, 7.5f), stories = 2, wallCol = new Color(0.75f, 0.58f, 0.42f), roofCol = new Color(0.55f, 0.22f, 0.15f), trimCol = new Color(0.35f, 0.2f, 0.15f), isFacade = false },
                new MultiStoryBuildingDef { name = "BlacksmithForge", position = new Vector3(-9.5f, 0, -30.5f), rotation = Quaternion.Euler(0, 90f, 0), size = new Vector3(9.5f, 5.0f, 7.5f), stories = 1, wallCol = new Color(0.48f, 0.45f, 0.42f), roofCol = new Color(0.2f, 0.2f, 0.25f), trimCol = new Color(0.15f, 0.15f, 0.18f), isFacade = false },
                new MultiStoryBuildingDef { name = "ResidentialTowerW1", position = new Vector3(-9.5f, 0, -10f), rotation = Quaternion.Euler(0, 90f, 0), size = new Vector3(10f, 12.5f, 7.5f), stories = 3, wallCol = new Color(0.62f, 0.58f, 0.52f), roofCol = new Color(0.4f, 0.18f, 0.15f), trimCol = new Color(0.85f, 0.75f, 0.35f), isFacade = true },
                new MultiStoryBuildingDef { name = "Tavern_TheStarlight", position = new Vector3(-10f, 0, 10f), rotation = Quaternion.Euler(0, 90f, 0), size = new Vector3(10f, 9.0f, 8f), stories = 2, wallCol = new Color(0.52f, 0.42f, 0.32f), roofCol = new Color(0.6f, 0.25f, 0.15f), trimCol = new Color(0.3f, 0.2f, 0.12f), isFacade = false },
                new MultiStoryBuildingDef { name = "TownLibrary", position = new Vector3(-9.5f, 0, 30.5f), rotation = Quaternion.Euler(0, 90f, 0), size = new Vector3(9.5f, 12.5f, 7.5f), stories = 3, wallCol = new Color(0.68f, 0.62f, 0.55f), roofCol = new Color(0.45f, 0.18f, 0.15f), trimCol = new Color(0.25f, 0.35f, 0.5f), isFacade = false },
                new MultiStoryBuildingDef { name = "ApothecaryShop", position = new Vector3(-9.5f, 0, 40f), rotation = Quaternion.Euler(0, 90f, 0), size = new Vector3(9.5f, 8.5f, 7.5f), stories = 2, wallCol = new Color(0.48f, 0.62f, 0.55f), roofCol = new Color(0.15f, 0.4f, 0.35f), trimCol = new Color(0.2f, 0.3f, 0.25f), isFacade = true }
            };

            MultiStoryBuildingDef[] eastMainRow = new MultiStoryBuildingDef[]
            {
                new MultiStoryBuildingDef { name = "GeneralStore", position = new Vector3(9.5f, 0, -40f), rotation = Quaternion.Euler(0, -90f, 0), size = new Vector3(9.5f, 8.5f, 7.5f), stories = 2, wallCol = new Color(0.65f, 0.6f, 0.5f), roofCol = new Color(0.35f, 0.45f, 0.25f), trimCol = new Color(0.25f, 0.35f, 0.2f), isFacade = false },
                new MultiStoryBuildingDef { name = "AdventurersGuild", position = new Vector3(10f, 0, -30.5f), rotation = Quaternion.Euler(0, -90f, 0), size = new Vector3(9.5f, 12.5f, 8f), stories = 3, wallCol = new Color(0.55f, 0.5f, 0.45f), roofCol = new Color(0.18f, 0.35f, 0.55f), trimCol = new Color(0.9f, 0.75f, 0.2f), isFacade = false },
                new MultiStoryBuildingDef { name = "ResidentialTowerE1", position = new Vector3(9.5f, 0, -10f), rotation = Quaternion.Euler(0, -90f, 0), size = new Vector3(10f, 8.5f, 7.5f), stories = 2, wallCol = new Color(0.72f, 0.65f, 0.55f), roofCol = new Color(0.55f, 0.25f, 0.2f), trimCol = new Color(0.4f, 0.25f, 0.15f), isFacade = true },
                new MultiStoryBuildingDef { name = "ClothierBoutique", position = new Vector3(9.5f, 0, 10f), rotation = Quaternion.Euler(0, -90f, 0), size = new Vector3(10f, 8.5f, 7.5f), stories = 2, wallCol = new Color(0.68f, 0.58f, 0.65f), roofCol = new Color(0.4f, 0.2f, 0.45f), trimCol = new Color(0.5f, 0.25f, 0.5f), isFacade = false },
                new MultiStoryBuildingDef { name = "AlchemistLab", position = new Vector3(9.5f, 0, 30.5f), rotation = Quaternion.Euler(0, -90f, 0), size = new Vector3(9.5f, 12.5f, 7.5f), stories = 3, wallCol = new Color(0.5f, 0.55f, 0.5f), roofCol = new Color(0.25f, 0.45f, 0.25f), trimCol = new Color(0.2f, 0.4f, 0.3f), isFacade = false },
                new MultiStoryBuildingDef { name = "ScribeStudio", position = new Vector3(9.5f, 0, 40f), rotation = Quaternion.Euler(0, -90f, 0), size = new Vector3(9.5f, 5.0f, 7.5f), stories = 1, wallCol = new Color(0.65f, 0.62f, 0.55f), roofCol = new Color(0.45f, 0.3f, 0.2f), trimCol = new Color(0.35f, 0.25f, 0.15f), isFacade = true }
            };

            List<MultiStoryBuildingDef> allBuildings = new List<MultiStoryBuildingDef>();
            allBuildings.AddRange(westMainRow);
            allBuildings.AddRange(eastMainRow);

            foreach (var bDef in allBuildings)
            {
                if (bDef.isFacade) CreateMultiStoryFacadeBuilding(townParent, bDef);
                else CreateMultiStoryHollowBuilding(townParent, bDef);
            }
        }

        private static void CreateMultiStoryFacadeBuilding(Transform parent, MultiStoryBuildingDef bDef)
        {
            GameObject facadeObj = new GameObject($"Facade_{bDef.name}_{bDef.stories}Story");
            facadeObj.transform.position = bDef.position;
            facadeObj.transform.rotation = bDef.rotation;
            facadeObj.transform.SetParent(parent);

            float width = bDef.size.x;
            float height = bDef.size.y;
            float depth = bDef.size.z;

            Material wallMat = GetOrCreateMaterialAsset($"WallMat_{bDef.name}", bDef.wallCol);
            Material roofMat = GetOrCreateMaterialAsset($"RoofMat_{bDef.name}", bDef.roofCol);

            GameObject front = GameObject.CreatePrimitive(PrimitiveType.Cube);
            front.name = "FacadeFrontWall";
            front.transform.SetParent(facadeObj.transform, false);
            front.transform.localPosition = new Vector3(0, height * 0.5f, 0);
            front.transform.localScale = new Vector3(width, height, depth);
            front.GetComponent<Renderer>().sharedMaterial = wallMat;
            GameObjectUtility.SetStaticEditorFlags(front, StaticEditorFlags.NavigationStatic);

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "RoofCapTrim";
            roof.transform.SetParent(facadeObj.transform, false);
            roof.transform.localPosition = new Vector3(0, height + 0.2f, 0);
            roof.transform.localScale = new Vector3(width + 0.6f, 0.4f, depth + 0.6f);
            roof.GetComponent<Renderer>().sharedMaterial = roofMat;
        }

        private static void CreateMultiStoryHollowBuilding(Transform parent, MultiStoryBuildingDef bDef)
        {
            GameObject bldgObj = new GameObject($"Building_{bDef.name}_{bDef.stories}Story");
            bldgObj.transform.position = bDef.position;
            bldgObj.transform.rotation = bDef.rotation;
            bldgObj.transform.SetParent(parent);

            float width = bDef.size.x;
            float height = bDef.size.y;
            float depth = bDef.size.z;

            Material wallMat = GetOrCreateMaterialAsset($"WallMat_{bDef.name}", bDef.wallCol);
            Material roofMat = GetOrCreateMaterialAsset($"RoofMat_{bDef.name}", bDef.roofCol);

            GameObject front = GameObject.CreatePrimitive(PrimitiveType.Cube);
            front.name = "BuildingBody";
            front.transform.SetParent(bldgObj.transform, false);
            front.transform.localPosition = new Vector3(0, height * 0.5f, 0);
            front.transform.localScale = new Vector3(width, height, depth);
            front.GetComponent<Renderer>().sharedMaterial = wallMat;
            GameObjectUtility.SetStaticEditorFlags(front, StaticEditorFlags.NavigationStatic);
        }

        private static void CreateLinearDrainageCanal(Transform parent, Vector3 startPos, Vector3 endPos)
        {
            GameObject canalObj = new GameObject($"JapaneseWaterCanal_Linear");
            canalObj.transform.SetParent(parent, false);

            Material stoneGutterMat = GetOrCreateMaterialAsset("JapaneseCanal_StoneGutter", new Color(0.22f, 0.23f, 0.25f), 0.4f);
            Material clearWaterMat = GetOrCreateMaterialAsset("JapaneseCanal_ClearWater", new Color(0.12f, 0.6f, 0.9f), 0.95f, true);

            Vector3 delta = endPos - startPos;
            int segments = Mathf.Max(4, Mathf.RoundToInt(delta.magnitude / 2.5f));
            Vector3 stepDelta = delta / segments;
            Quaternion canalRot = Quaternion.LookRotation(delta);

            for (int i = 0; i < segments; i++)
            {
                Vector3 pos = startPos + stepDelta * (i + 0.5f);

                GameObject trough = GameObject.CreatePrimitive(PrimitiveType.Cube);
                trough.name = $"GutterTrough_{i}";
                trough.transform.SetParent(canalObj.transform, false);
                trough.transform.position = pos + new Vector3(0, 0.02f, 0);
                trough.transform.rotation = canalRot;
                trough.transform.localScale = new Vector3(0.85f, 0.12f, stepDelta.magnitude * 1.02f);
                trough.GetComponent<Renderer>().sharedMaterial = stoneGutterMat;

                GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cube);
                water.name = $"CanalWaterStream_{i}";
                water.transform.SetParent(canalObj.transform, false);
                water.transform.position = pos + new Vector3(0, 0.06f, 0);
                water.transform.rotation = canalRot;
                water.transform.localScale = new Vector3(0.6f, 0.08f, stepDelta.magnitude * 1.02f);
                water.GetComponent<Renderer>().sharedMaterial = clearWaterMat;
            }
        }

        private static void CreateTownGate(Transform parent, string gateName, Vector3 centerPos)
        {
            GameObject gateObj = new GameObject(gateName);
            gateObj.transform.SetParent(parent, false);
            gateObj.transform.position = centerPos;
            Material stoneMat = GetOrCreateMaterialAsset("CityGate_Stone", new Color(0.58f, 0.58f, 0.62f));

            GameObject postL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            postL.transform.SetParent(gateObj.transform, false);
            postL.transform.localPosition = new Vector3(-5.5f, 3.5f, 0);
            postL.transform.localScale = new Vector3(2.5f, 7f, 2.5f);
            postL.GetComponent<Renderer>().sharedMaterial = stoneMat;

            GameObject postR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            postR.transform.SetParent(gateObj.transform, false);
            postR.transform.localPosition = new Vector3(5.5f, 3.5f, 0);
            postR.transform.localScale = new Vector3(2.5f, 7f, 2.5f);
            postR.GetComponent<Renderer>().sharedMaterial = stoneMat;
        }

        private static void CreateWaterFountain(Transform parent, Vector3 centerPos, float radius)
        {
            GameObject ftnObj = new GameObject("OrnamentalWaterFountain");
            ftnObj.transform.SetParent(parent, false);
            ftnObj.transform.position = centerPos;
            Material stoneMat = GetOrCreateMaterialAsset("Fountain_StoneBasin", new Color(0.68f, 0.68f, 0.72f), 0.6f);

            GameObject basin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basin.transform.SetParent(ftnObj.transform, false);
            basin.transform.localPosition = new Vector3(0, 0.4f, 0);
            basin.transform.localScale = new Vector3(radius * 2f, 0.8f, radius * 2f);
            basin.GetComponent<Renderer>().sharedMaterial = stoneMat;
            FixCylinderCollider(basin);
        }

        private static void BuildControlsHUDCanvas()
        {
            GameObject hudObj = new GameObject("ControlsHUDCanvas");
            Canvas canvas = hudObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = hudObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject infoBox = new GameObject("InfoPanel");
            infoBox.transform.SetParent(hudObj.transform, false);
            RectTransform rect = infoBox.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.80f);
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

            Text textComp = textObj.AddComponent<Text>();
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 16;
            textComp.color = Color.white;
            textComp.horizontalOverflow = HorizontalWrapMode.Wrap;
            textComp.verticalOverflow = VerticalWrapMode.Overflow;
            textComp.resizeTextForBestFit = true;
            textComp.resizeTextMinSize = 12;
            textComp.resizeTextMaxSize = 20;
            textComp.text = "<b>Divinatius Compact Multi-Story Town</b>\n" +
                            "• [WASD]: Move Player\n" +
                            "• [Mouse]: Rotate View / Camera\n" +
                            "• [E]: Talk to Nearby Wandering NPC\n" +
                            "• [Esc]: Exit Conversation";
        }

        private static void BuildDialogueUICanvas()
        {
            GameObject canvasObj = new GameObject("DialogueCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject panelObj = new GameObject("DialogueBoxRoot");
            panelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.08f, 0.03f);
            panelRect.anchorMax = new Vector2(0.92f, 0.42f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image bg = panelObj.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.92f);

            GameObject exitBtnObj = new GameObject("ExitButton");
            exitBtnObj.transform.SetParent(panelObj.transform, false);
            RectTransform exitRect = exitBtnObj.AddComponent<RectTransform>();
            exitRect.anchorMin = new Vector2(0.90f, 0.85f);
            exitRect.anchorMax = new Vector2(0.99f, 0.97f);
            exitRect.offsetMin = Vector2.zero;
            exitRect.offsetMax = Vector2.zero;

            Image exitBg = exitBtnObj.AddComponent<Image>();
            exitBg.color = new Color(0.85f, 0.2f, 0.2f, 0.9f);
            Button exitBtn = exitBtnObj.AddComponent<Button>();

            GameObject exitTextObj = new GameObject("ExitText");
            exitTextObj.transform.SetParent(exitBtnObj.transform, false);
            RectTransform exitTextRect = exitTextObj.AddComponent<RectTransform>();
            exitTextRect.anchorMin = Vector2.zero;
            exitTextRect.anchorMax = Vector2.one;
            exitTextRect.offsetMin = Vector2.zero;
            exitTextRect.offsetMax = Vector2.zero;

            Text exitText = exitTextObj.AddComponent<Text>();
            exitText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            exitText.text = "✖ Exit (Esc)";
            exitText.alignment = TextAnchor.MiddleCenter;
            exitText.fontSize = 14;
            exitText.fontStyle = FontStyle.Bold;
            exitText.color = Color.white;

            GameObject nameObj = new GameObject("SpeakerNameText");
            nameObj.transform.SetParent(panelObj.transform, false);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.03f, 0.85f);
            nameRect.anchorMax = new Vector2(0.4f, 0.97f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            Text nameText = nameObj.AddComponent<Text>();
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 22;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = new Color(1f, 0.85f, 0.2f);
            nameText.text = "NPC Speaker";

            GameObject bodyObj = new GameObject("DialogueBodyText");
            bodyObj.transform.SetParent(panelObj.transform, false);
            RectTransform bodyRect = bodyObj.AddComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0.03f, 0.22f);
            bodyRect.anchorMax = new Vector2(0.97f, 0.83f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;

            Text bodyText = bodyObj.AddComponent<Text>();
            bodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bodyText.fontSize = 18;
            bodyText.lineSpacing = 1.1f;
            bodyText.color = Color.white;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;
            bodyText.text = "Greeting traveler! Welcome to Divinatius town.";

            GameObject inputRow = new GameObject("InputRow");
            inputRow.transform.SetParent(panelObj.transform, false);
            RectTransform inputRowRect = inputRow.AddComponent<RectTransform>();
            inputRowRect.anchorMin = new Vector2(0.03f, 0.03f);
            inputRowRect.anchorMax = new Vector2(0.97f, 0.20f);
            inputRowRect.offsetMin = Vector2.zero;
            inputRowRect.offsetMax = Vector2.zero;

            // 1. Text Input Field (0% to 68% Width)
            GameObject inputFieldObj = new GameObject("PlayerInputField");
            inputFieldObj.transform.SetParent(inputRow.transform, false);
            RectTransform ifRect = inputFieldObj.AddComponent<RectTransform>();
            ifRect.anchorMin = new Vector2(0f, 0f);
            ifRect.anchorMax = new Vector2(0.68f, 1f);
            ifRect.offsetMin = Vector2.zero;
            ifRect.offsetMax = Vector2.zero;

            Image ifBg = inputFieldObj.AddComponent<Image>();
            ifBg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            InputField inputFieldComp = inputFieldObj.AddComponent<InputField>();

            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputFieldObj.transform, false);
            RectTransform phRect = placeholderObj.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = new Vector2(10, 0);
            phRect.offsetMax = new Vector2(-10, 0);
            Text phText = placeholderObj.AddComponent<Text>();
            phText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            phText.text = "Type your response to speak...";
            phText.fontSize = 15;
            phText.fontStyle = FontStyle.Italic;
            phText.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            inputFieldComp.placeholder = phText;

            GameObject inputTextObj = new GameObject("Text");
            inputTextObj.transform.SetParent(inputFieldObj.transform, false);
            RectTransform textInputRect = inputTextObj.AddComponent<RectTransform>();
            textInputRect.anchorMin = Vector2.zero;
            textInputRect.anchorMax = Vector2.one;
            textInputRect.offsetMin = new Vector2(10, 0);
            textInputRect.offsetMax = new Vector2(-10, 0);
            Text inputText = inputTextObj.AddComponent<Text>();
            inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            inputText.fontSize = 15;
            inputText.color = Color.white;
            inputFieldComp.textComponent = inputText;

            // 2. Mic Record Voice Button (69% to 82% Width)
            GameObject micBtnObj = new GameObject("MicRecordButton");
            micBtnObj.transform.SetParent(inputRow.transform, false);
            RectTransform micRect = micBtnObj.AddComponent<RectTransform>();
            micRect.anchorMin = new Vector2(0.69f, 0f);
            micRect.anchorMax = new Vector2(0.82f, 1f);
            micRect.offsetMin = Vector2.zero;
            micRect.offsetMax = Vector2.zero;

            Image micBg = micBtnObj.AddComponent<Image>();
            micBg.color = new Color(0.8f, 0.3f, 0.3f, 1f);
            Button micBtn = micBtnObj.AddComponent<Button>();

            GameObject micTextObj = new GameObject("MicText");
            micTextObj.transform.SetParent(micBtnObj.transform, false);
            RectTransform micTextRect = micTextObj.AddComponent<RectTransform>();
            micTextRect.anchorMin = Vector2.zero;
            micTextRect.anchorMax = Vector2.one;
            micTextRect.offsetMin = Vector2.zero;
            micTextRect.offsetMax = Vector2.zero;
            Text micText = micTextObj.AddComponent<Text>();
            micText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            micText.text = "🎤 Voice";
            micText.alignment = TextAnchor.MiddleCenter;
            micText.fontSize = 14;
            micText.color = Color.white;

            // 3. Send Button (83% to 100% Width)
            GameObject sendBtnObj = new GameObject("SendButton");
            sendBtnObj.transform.SetParent(inputRow.transform, false);
            RectTransform sendRect = sendBtnObj.AddComponent<RectTransform>();
            sendRect.anchorMin = new Vector2(0.83f, 0f);
            sendRect.anchorMax = new Vector2(1f, 1f);
            sendRect.offsetMin = Vector2.zero;
            sendRect.offsetMax = Vector2.zero;

            Image sendBg = sendBtnObj.AddComponent<Image>();
            sendBg.color = new Color(0.2f, 0.6f, 0.9f, 1f);
            Button sendBtn = sendBtnObj.AddComponent<Button>();

            GameObject sendTextObj = new GameObject("SendText");
            sendTextObj.transform.SetParent(sendBtnObj.transform, false);
            RectTransform sendTextRect = sendTextObj.AddComponent<RectTransform>();
            sendTextRect.anchorMin = Vector2.zero;
            sendTextRect.anchorMax = Vector2.one;
            sendTextRect.offsetMin = Vector2.zero;
            sendTextRect.offsetMax = Vector2.zero;
            Text sendText = sendTextObj.AddComponent<Text>();
            sendText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sendText.text = "Send";
            sendText.alignment = TextAnchor.MiddleCenter;
            sendText.fontSize = 15;
            sendText.color = Color.white;

            DialogueUIController uiController = canvasObj.AddComponent<DialogueUIController>();
            DialogueInputController inputController = canvasObj.AddComponent<DialogueInputController>();

            SerializedObject serializedUI = new SerializedObject(uiController);
            serializedUI.FindProperty("dialoguePanelRoot").objectReferenceValue = panelObj;
            serializedUI.FindProperty("speakerNameText").objectReferenceValue = nameText;
            serializedUI.FindProperty("dialogueBodyText").objectReferenceValue = bodyText;
            serializedUI.FindProperty("closeButton").objectReferenceValue = exitBtn;
            serializedUI.FindProperty("inputController").objectReferenceValue = inputController;
            serializedUI.ApplyModifiedProperties();

            SerializedObject serializedInput = new SerializedObject(inputController);
            serializedInput.FindProperty("textInputField").objectReferenceValue = inputFieldComp;
            serializedInput.FindProperty("sendButton").objectReferenceValue = sendBtn;
            serializedInput.FindProperty("micRecordButton").objectReferenceValue = micBtn;
            serializedInput.FindProperty("micButtonText").objectReferenceValue = micText;
            serializedInput.ApplyModifiedProperties();
        }
    }
}
#pragma warning restore 0618
#endif
