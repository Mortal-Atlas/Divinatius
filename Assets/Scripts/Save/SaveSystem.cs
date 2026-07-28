using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Divinatius.NPC;
using Divinatius.AI;

namespace Divinatius.Save
{
    [Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3Data(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    [Serializable]
    public class SaveDataContainer
    {
        public long saveTimestamp;
        public Vector3Data playerPosition;
        public Vector3Data playerRotation;
        public List<NPCMemoryData> characterMemories = new List<NPCMemoryData>();
        public List<string> gameProgressFlags = new List<string>();
    }

    public static class SaveSystem
    {
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "savegame.json");

        public static bool SaveGame(Vector3 playerPos, Vector3 playerRot)
        {
            try
            {
                SaveDataContainer data = new SaveDataContainer
                {
                    saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    playerPosition = new Vector3Data(playerPos),
                    playerRotation = new Vector3Data(playerRot)
                };

                if (CharacterMemoryManager.Instance != null)
                {
                    data.characterMemories = CharacterMemoryManager.Instance.GetAllMemoryData();
                }

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[SaveSystem] Game saved successfully to: {SaveFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Error saving game: {ex.Message}");
                return false;
            }
        }

        public static SaveDataContainer LoadGame()
        {
            try
            {
                if (!File.Exists(SaveFilePath))
                {
                    Debug.LogWarning($"[SaveSystem] Save file not found at: {SaveFilePath}");
                    return null;
                }

                string json = File.ReadAllText(SaveFilePath);
                SaveDataContainer data = JsonUtility.FromJson<SaveDataContainer>(json);

                if (data != null && CharacterMemoryManager.Instance != null)
                {
                    CharacterMemoryManager.Instance.LoadMemoryData(data.characterMemories);
                }

                Debug.Log($"[SaveSystem] Game state loaded successfully from: {SaveFilePath}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Error loading game: {ex.Message}");
                return null;
            }
        }

        public static bool HasSaveFile() => File.Exists(SaveFilePath);
    }
}
