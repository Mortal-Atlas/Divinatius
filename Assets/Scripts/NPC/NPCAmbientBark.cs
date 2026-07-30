using System.Collections.Generic;
using UnityEngine;
using Divinatius.UI;

namespace Divinatius.NPC
{
    public enum AmbientBarkType
    {
        Greeting,
        Comment,
        OneLiner
    }

    public class NPCAmbientBark : MonoBehaviour
    {
        [Header("Proximity Distance Settings")]
        [Tooltip("Trigger distance in meters for passing by player.")]
        [SerializeField] private float triggerRadius = 6.0f;

        [Tooltip("Cooldown time in seconds before this NPC can greet/comment again.")]
        [SerializeField] private float cooldownSeconds = 25.0f;

        [Header("Ambient Reactions Config")]
        [Tooltip("Greetings when the player passes by.")]
        public List<string> greetings = new List<string>
        {
            "Greetings, traveler!",
            "Good day to you!",
            "Welcome to Divinatius!",
            "Ah, a new face in town."
        };

        [Tooltip("Comments about the town, atmosphere, or world.")]
        public List<string> comments = new List<string>
        {
            "The air feels crisp today.",
            "The market stalls are lively this afternoon.",
            "Another busy day in the town square.",
            "Keep your eyes open along the alleyways."
        };

        [Tooltip("One-liners or quick advice.")]
        public List<string> oneLiners = new List<string>
        {
            "Steel and courage win the day.",
            "Always watch your back in the shadows.",
            "Whispers say trouble brews in the north.",
            "Honor and blade go hand in hand."
        };

        private Transform _playerTransform;
        private float _lastBarkTime = -999f;
        private NPCInteractable _npcInteractable;

        private void Start()
        {
            _npcInteractable = GetComponent<NPCInteractable>();
            FindPlayer();
        }

        private void FindPlayer()
        {
            GameObject pObj = GameObject.FindWithTag("Player");
            if (pObj == null) pObj = GameObject.Find("Player_MC");
            if (pObj != null) _playerTransform = pObj.transform;
        }

        private void Update()
        {
            if (_playerTransform == null)
            {
                FindPlayer();
                return;
            }

            if (Time.time - _lastBarkTime < cooldownSeconds) return;

            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist <= triggerRadius)
            {
                TriggerAmbientBark();
            }
        }

        public void TriggerAmbientBark()
        {
            _lastBarkTime = Time.time;

            // Pick randomly between Greeting, Comment, or One-Liner
            AmbientBarkType barkType = (AmbientBarkType)Random.Range(0, 3);
            string chosenText = "";

            switch (barkType)
            {
                case AmbientBarkType.Greeting:
                    chosenText = GetRandomLine(greetings, "Greetings, traveler!");
                    break;
                case AmbientBarkType.Comment:
                    chosenText = GetRandomLine(comments, "It's a fine day in Divinatius.");
                    break;
                case AmbientBarkType.OneLiner:
                    chosenText = GetRandomLine(oneLiners, "Stay safe on your journey.");
                    break;
            }

            // Display overhead speech bubble above NPC's head
            OverheadSpeechBubble.Create(transform, chosenText, 4.0f);
        }

        private string GetRandomLine(List<string> list, string defaultLine)
        {
            if (list == null || list.Count == 0) return defaultLine;
            return list[Random.Range(0, list.Count)];
        }
    }
}
