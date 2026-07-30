using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Divinatius.Core;
using Divinatius.NPC;

namespace Divinatius.Player
{
    public class PlayerCombatController : MonoBehaviour
    {
        public static PlayerCombatController Instance { get; private set; }

        [Header("Combat Settings")]
        [SerializeField] private float attackRange = 3.5f;
        [SerializeField] private float attackAngle = 70.0f;
        [SerializeField] private float attackDamage = 35.0f;
        [SerializeField] private float attackCooldown = 0.5f;

        private float _lastAttackTime = -10f;
        private PlayerController _playerController;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            // Block combat if UI menus are active
            bool isPhoneOpen = UI.PhoneUIController.Instance != null && UI.PhoneUIController.Instance.IsPhoneOpen;
            bool isDialogueActive = Dialogue.DialogueUIController.Instance != null && Dialogue.DialogueUIController.Instance.IsDialogueActive;
            bool isShopOpen = UI.ShopUIController.Instance != null && UI.ShopUIController.Instance.IsShopOpen;

            if (isPhoneOpen || isDialogueActive || isShopOpen) return;

            var mouse = Mouse.current;
            var keyboard = Keyboard.current;

            bool leftClickPressed = mouse != null && mouse.leftButton.wasPressedThisFrame;
            bool attackKeyPressed = keyboard != null && keyboard.fKey.wasPressedThisFrame;

            if ((leftClickPressed || attackKeyPressed) && Time.time - _lastAttackTime >= attackCooldown)
            {
                ExecuteSwordSlash();
            }
        }

        public void ExecuteSwordSlash()
        {
            _lastAttackTime = Time.time;
            Debug.Log("[PlayerCombatController] Executing Sword Attack Slash!");

            // 1. Play Sword Slash Visual Arc Trail
            StartCoroutine(PlaySlashVFXCoroutine());

            // 2. Detect hits in forward cone
            Vector3 origin = transform.position + Vector3.up * 1.2f;
            Vector3 forward = transform.forward;

            Collider[] hits = Physics.OverlapSphere(origin, attackRange);
            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                Vector3 dirToTarget = (hit.transform.position - origin).normalized;
                float angle = Vector3.Angle(forward, dirToTarget);

                if (angle <= attackAngle * 0.5f)
                {
                    // Hit Target!
                    NPCInteractable npc = hit.GetComponent<NPCInteractable>();
                    if (npc == null) npc = hit.GetComponentInParent<NPCInteractable>();

                    if (npc != null)
                    {
                        Debug.LogWarning($"[PlayerCombatController] Sword Slash HIT NPC: '{npc.name}'!");

                        // Report Crime to Town Guard Wanted System!
                        if (TownGuardWantedManager.Instance != null)
                        {
                            TownGuardWantedManager.Instance.ReportCrime(CrimeType.AssaultNPC);
                        }

                        // Play Impact SFX/VFX
                        PlayImpactVFX(hit.transform.position + Vector3.up * 1.2f);
                    }
                }
            }
        }

        private IEnumerator PlaySlashVFXCoroutine()
        {
            GameObject slashObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            slashObj.name = "VFX_SwordSlashArc";
            Destroy(slashObj.GetComponent<Collider>());

            slashObj.transform.position = transform.position + transform.forward * 1.6f + Vector3.up * 1.2f;
            slashObj.transform.rotation = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, 0, -45f);
            slashObj.transform.localScale = new Vector3(2.5f, 0.4f, 1f);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.color = new Color(1.0f, 0.9f, 0.4f, 0.85f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(1.0f, 0.9f, 0.4f, 0.85f));
            slashObj.GetComponent<Renderer>().sharedMaterial = mat;

            float timer = 0f;
            while (timer < 0.25f)
            {
                timer += Time.deltaTime;
                slashObj.transform.Rotate(0, 0, 180f * Time.deltaTime * 6f);
                yield return null;
            }

            Destroy(slashObj);
        }

        private void PlayImpactVFX(Vector3 pos)
        {
            GameObject sparkObj = new GameObject("VFX_SwordImpactSpark");
            sparkObj.transform.position = pos;

            Light light = sparkObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.8f, 0.2f);
            light.intensity = 16.0f;
            light.range = 5.0f;

            Destroy(sparkObj, 0.25f);
        }
    }
}
