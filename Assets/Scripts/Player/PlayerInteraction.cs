using UnityEngine;
using UnityEngine.InputSystem;
using Divinatius.NPC;
using Divinatius.Dialogue;

namespace Divinatius.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRadius = 3.0f; // Max 3 meters distance
        [SerializeField] private float maxLookAngle = 60.0f;      // Max cone of vision angle
        [SerializeField] private LayerMask npcLayerMask = ~0;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private NPCInteractable _currentNPC;
        private PlayerController _playerController;

        private void Start()
        {
            _playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            DetectNPC();

            bool isDialogueActive = DialogueUIController.Instance != null && DialogueUIController.Instance.IsDialogueActive;
            bool isPhoneOpen = PhoneUIController.Instance != null && PhoneUIController.Instance.IsPhoneOpen;
            bool isMenuOpen = isDialogueActive || isPhoneOpen;

            if (_currentNPC != null && !isMenuOpen)
            {
                string npcName = _currentNPC.NPCProfile != null ? _currentNPC.NPCProfile.characterName : "";
                InteractionPromptUI.Instance.ShowPrompt(_currentNPC.transform, npcName);
            }
            else
            {
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.HidePrompt();
                }
            }

            bool interactPressed = false;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                interactPressed = keyboard.eKey.wasPressedThisFrame;
            }

            if (_currentNPC != null && interactPressed && !isMenuOpen)
            {
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.HidePrompt();
                }
                StartDialogueWithNPC(_currentNPC);
            }
        }

        private void DetectNPC()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius, npcLayerMask);
            NPCInteractable closestNPC = null;
            float minDistance = float.MaxValue;

            Camera mainCam = Camera.main;
            Vector3 playerForward = transform.forward;
            playerForward.y = 0;

            foreach (var hit in hits)
            {
                NPCInteractable npc = hit.GetComponent<NPCInteractable>();
                if (npc == null) npc = hit.GetComponentInParent<NPCInteractable>();

                if (npc != null)
                {
                    float dist = Vector3.Distance(transform.position, npc.transform.position);
                    if (dist <= interactionRadius)
                    {
                        // Check player facing direction
                        Vector3 dirToNPC = (npc.transform.position - transform.position).normalized;
                        dirToNPC.y = 0;

                        float playerAngle = Vector3.Angle(playerForward, dirToNPC);

                        // Check camera facing direction if available
                        bool cameraLooking = true;
                        if (mainCam != null)
                        {
                            Vector3 camDirToNPC = (npc.transform.position - mainCam.transform.position).normalized;
                            float camAngle = Vector3.Angle(mainCam.transform.forward, camDirToNPC);
                            cameraLooking = camAngle <= maxLookAngle;
                        }

                        if (playerAngle <= maxLookAngle && cameraLooking)
                        {
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                closestNPC = npc;
                            }
                        }
                    }
                }
            }

            _currentNPC = closestNPC;
        }

        public void StartDialogueWithNPC(NPCInteractable npc)
        {
            if (npc == null || npc.NPCProfile == null) return;

            if (_playerController != null)
            {
                _playerController.ControlsEnabled = false;
            }

            NPCWanderer wanderer = npc.GetComponent<NPCWanderer>();
            if (wanderer != null)
            {
                wanderer.PauseWandering(transform);
            }

            if (DialogueUIController.Instance != null)
            {
                DialogueUIController.Instance.OpenDialogue(npc.NPCProfile, npc, () =>
                {
                    if (wanderer != null)
                    {
                        wanderer.ResumeWandering();
                    }
                    if (_playerController != null)
                    {
                        _playerController.ControlsEnabled = true;
                    }
                });
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
