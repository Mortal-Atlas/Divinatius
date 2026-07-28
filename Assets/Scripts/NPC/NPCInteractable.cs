using UnityEngine;

namespace Divinatius.NPC
{
    public class NPCInteractable : MonoBehaviour
    {
        [Header("NPC Profile Configuration")]
        [SerializeField] private NPCProfileSO npcProfile;

        public NPCProfileSO NPCProfile => npcProfile;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 1f, new Vector3(0.6f, 2f, 0.6f));
        }
    }
}
