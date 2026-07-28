using UnityEngine;
using Divinatius.NPC;
using Divinatius.Dialogue;

namespace Divinatius.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRadius = 3.0f;
        [SerializeField] private LayerMask npcLayerMask = ~0; // Default to all layers unless specified
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

            if (_currentNPC != null && Input.GetKeyDown(interactKey))
            {
                if (DialogueUIController.Instance != null && !DialogueUIController.Instance.IsDialogueActive)
                {
                    StartDialogueWithNPC(_currentNPC);
                }
            }
        }

        private void DetectNPC()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius, npcLayerMask);
            NPCInteractable closestNPC = null;
            float minDistance = float.MaxValue;

            foreach (var hit in hits)
            {
                NPCInteractable npc = hit.GetComponent<NPCInteractable>();
                if (npc != null)
                {
                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestNPC = npc;
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

            if (DialogueUIController.Instance != null)
            {
                DialogueUIController.Instance.OpenDialogue(npc.NPCProfile, () =>
                {
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
