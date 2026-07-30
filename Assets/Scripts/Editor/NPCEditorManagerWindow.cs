#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Divinatius.NPC;
using System.Collections.Generic;

namespace Divinatius.Editor
{
    public class NPCEditorManagerWindow : EditorWindow
    {
        private Vector2 scrollPos;
        private List<NPCInteractable> sceneNPCs = new List<NPCInteractable>();

        [MenuItem("Divinatius/NPC Manager & Renamer")]
        public static void ShowWindow()
        {
            var window = GetWindow<NPCEditorManagerWindow>("NPC Manager");
            window.minSize = new Vector2(500, 600);
            window.RefreshNPCList();
            window.Show();
        }

        private void OnEnable()
        {
            RefreshNPCList();
        }

        private void RefreshNPCList()
        {
            sceneNPCs.Clear();
            NPCInteractable[] npcs = Object.FindObjectsOfType<NPCInteractable>();
            sceneNPCs.AddRange(npcs);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Divinatius NPC Manager & Renamer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Easily rename NPCs, update their bio descriptions, ElevenLabs voice IDs, AI personality prompts, 3D character models, and mesh color tints.", MessageType.Info);

            EditorGUILayout.Space(5);
            if (GUILayout.Button("🔄 Refresh NPC List from Active Scene", GUILayout.Height(30)))
            {
                RefreshNPCList();
            }

            EditorGUILayout.Space(10);
            if (sceneNPCs.Count == 0)
            {
                EditorGUILayout.HelpBox("No NPCInteractable objects found in the active scene. Open DevDemoScene or run 'Divinatius -> Create Demo Dev Scene'.", MessageType.Warning);
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < sceneNPCs.Count; i++)
            {
                NPCInteractable npc = sceneNPCs[i];
                if (npc == null) continue;

                SerializedObject serializedObj = new SerializedObject(npc);
                serializedObj.Update();

                SerializedProperty nameProp = serializedObj.FindProperty("characterName");
                SerializedProperty descProp = serializedObj.FindProperty("characterDescription");
                SerializedProperty promptProp = serializedObj.FindProperty("systemPersonalityPrompt");
                SerializedProperty voiceProp = serializedObj.FindProperty("elevenLabsVoiceId");
                SerializedProperty modelProp = serializedObj.FindProperty("npc3DModelPrefab");
                SerializedProperty colorProp = serializedObj.FindProperty("npcColor");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"<b>NPC #{i+1}: {nameProp.stringValue}</b>", EditorStyles.label);
                if (GUILayout.Button("Select in Scene", GUILayout.Width(110)))
                {
                    Selection.activeGameObject = npc.gameObject;
                    EditorGUIUtility.PingObject(npc.gameObject);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(nameProp, new GUIContent("Display Name"));
                EditorGUILayout.PropertyField(descProp, new GUIContent("Bio / Description"));
                EditorGUILayout.PropertyField(voiceProp, new GUIContent("ElevenLabs Voice ID"));
                EditorGUILayout.PropertyField(colorProp, new GUIContent("Mesh Tint Color"));
                EditorGUILayout.PropertyField(modelProp, new GUIContent("3D Model Prefab"));
                EditorGUILayout.PropertyField(promptProp, new GUIContent("AI System Personality Prompt"));

                if (serializedObj.ApplyModifiedProperties())
                {
                    npc.ApplyVisualChanges();
                    EditorUtility.SetDirty(npc);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(8);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
