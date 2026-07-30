using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Divinatius.VFX
{
    public enum NPCSpellType
    {
        GodRay,            // Holy Light Pillar / Blessing
        FireBurst,          // Forge Flame / Fire Arcana
        HealingAura,        // Nature / Restoration Glow
        ShadowMist,         // Purple Dark Arcana / Ethereal Mist
        SparkleShimmer,     // Bard Golden Stars / Ballad
        GoldCoins,          // Merchant Coin Shower / Trade
        CurseAoE,           // Dark Crimson/Purple Curse Burst
        PurificationAura    // Radiant Cyan/White Cleansing Cylinder
    }

    public class NPCSpellVFXManager : MonoBehaviour
    {
        public static NPCSpellVFXManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void CastSpellByName(string spellName, Transform speaker, Transform player)
        {
            if (string.IsNullOrEmpty(spellName)) return;

            string tag = spellName.Trim().ToUpper();
            if (tag.Contains("CURSE") || tag.Contains("HEX") || tag.Contains("SLOTH") || tag.Contains("MISFORTUNE"))
            {
                CastSpell(NPCSpellType.CurseAoE, speaker, player);
            }
            else if (tag.Contains("PURIFY") || tag.Contains("CLEANSE") || tag.Contains("DISPEL") || tag.Contains("CURE"))
            {
                CastSpell(NPCSpellType.PurificationAura, speaker, player);
            }
            else if (tag.Contains("GOD_RAY") || tag.Contains("BLESSING") || tag.Contains("HOLY") || tag.Contains("LIGHT"))
            {
                CastSpell(NPCSpellType.GodRay, speaker, player);
            }
            else if (tag.Contains("FIRE") || tag.Contains("FORGE") || tag.Contains("FLAME"))
            {
                CastSpell(NPCSpellType.FireBurst, speaker, player);
            }
            else if (tag.Contains("HEAL") || tag.Contains("NATURE") || tag.Contains("AURA"))
            {
                CastSpell(NPCSpellType.HealingAura, speaker, player);
            }
            else if (tag.Contains("SHADOW") || tag.Contains("MIST") || tag.Contains("DARK"))
            {
                CastSpell(NPCSpellType.ShadowMist, speaker, player);
            }
            else if (tag.Contains("SPARKLE") || tag.Contains("BARD") || tag.Contains("STAR"))
            {
                CastSpell(NPCSpellType.SparkleShimmer, speaker, player);
            }
            else if (tag.Contains("GOLD") || tag.Contains("COIN") || tag.Contains("TRADE"))
            {
                CastSpell(NPCSpellType.GoldCoins, speaker, player);
            }
            else
            {
                CastSpell(NPCSpellType.GodRay, speaker, player);
            }
        }

        public void CastSpell(NPCSpellType spellType, Transform speaker, Transform player)
        {
            if (player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.transform;
                else
                {
                    var pc = FindFirstObjectByType<Divinatius.Player.PlayerController>();
                    if (pc != null) player = pc.transform;
                }
            }

            Vector3 originPos = speaker != null ? speaker.position : Vector3.zero;
            Vector3 targetPos = player != null ? player.position : originPos + Vector3.forward * 1.5f;

            Debug.Log($"[NPCSpellVFXManager] Casting Spell VFX/SFX: {spellType} at target {targetPos}");

            switch (spellType)
            {
                case NPCSpellType.GodRay:
                    StartCoroutine(CreateGodRayVFXCoroutine(targetPos));
                    break;
                case NPCSpellType.FireBurst:
                    StartCoroutine(CreateFireBurstVFXCoroutine(originPos));
                    break;
                case NPCSpellType.HealingAura:
                    StartCoroutine(CreateHealingAuraVFXCoroutine(targetPos));
                    break;
                case NPCSpellType.ShadowMist:
                    StartCoroutine(CreateShadowMistVFXCoroutine(originPos));
                    break;
                case NPCSpellType.SparkleShimmer:
                    StartCoroutine(CreateSparkleShimmerVFXCoroutine(targetPos));
                    break;
                case NPCSpellType.GoldCoins:
                    StartCoroutine(CreateGoldCoinsVFXCoroutine(targetPos));
                    break;
                case NPCSpellType.CurseAoE:
                    StartCoroutine(CreateCurseVFXCoroutine(targetPos));
                    break;
                case NPCSpellType.PurificationAura:
                    StartCoroutine(CreatePurificationVFXCoroutine(targetPos));
                    break;
            }
        }

        private static Material CreateRuntimeVFXMaterial(Color color, float smoothness = 0.9f, bool isTransparent = true)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("URP/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.color = color;

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            if (isTransparent)
            {
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1); // Transparent
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0);     // Alpha
            }

            return mat;
        }

        private IEnumerator CreateGodRayVFXCoroutine(Vector3 targetPos)
        {
            // Create God Ray Parent centered exactly at target location (player's feet)
            GameObject rayRoot = new GameObject("VFX_GodRay_Root");
            rayRoot.transform.position = targetPos;

            // 1. Overhead Spot Light beaming straight down onto the player
            GameObject lightObj = new GameObject("GodRaySpotLight");
            lightObj.transform.SetParent(rayRoot.transform, false);
            lightObj.transform.localPosition = new Vector3(0, 16f, 0);
            lightObj.transform.rotation = Quaternion.Euler(90f, 0, 0);

            Light spotLight = lightObj.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.color = new Color(1.0f, 0.94f, 0.6f); // Warm Holy Gold
            spotLight.intensity = 24.0f;
            spotLight.range = 25.0f;
            spotLight.spotAngle = 45.0f;

            // 2. Central Ground Light Glow at player's feet
            GameObject groundLightObj = new GameObject("GodRayGroundPointLight");
            groundLightObj.transform.SetParent(rayRoot.transform, false);
            groundLightObj.transform.localPosition = new Vector3(0, 0.5f, 0);

            Light groundLight = groundLightObj.AddComponent<Light>();
            groundLight.type = LightType.Point;
            groundLight.color = new Color(1.0f, 0.9f, 0.4f);
            groundLight.intensity = 15.0f;
            groundLight.range = 6.0f;

            // 3. Broad Vertical Light Cylinder Beam centered directly over player
            GameObject beamCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beamCylinder.name = "GodRayBeamCylinder";
            beamCylinder.transform.SetParent(rayRoot.transform, false);
            beamCylinder.transform.localPosition = new Vector3(0, 8.0f, 0);
            beamCylinder.transform.localScale = new Vector3(3.2f, 8.0f, 3.2f); // 3.2m wide cylinder surrounding player

            Collider col = beamCylinder.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Material mat = CreateRuntimeVFXMaterial(new Color(1f, 0.95f, 0.55f, 0.55f), 0.95f, true);
            beamCylinder.GetComponent<Renderer>().sharedMaterial = mat;

            // Play Divine Chime Audio
            PlayProceduralSFX(targetPos, 520f, 880f, 3.5f);

            // Extended duration: 7.0 seconds with smooth fade-in and fade-out
            float totalDuration = 7.0f;
            float timer = 0f;

            while (timer < totalDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / totalDuration;

                float alpha = 1.0f;
                if (progress < 0.15f) alpha = progress / 0.15f; // Fade in over 1s
                else if (progress > 0.8f) alpha = (1.0f - progress) / 0.2f; // Fade out over last 1.4s

                spotLight.intensity = (Mathf.PingPong(timer * 5f, 8f) + 18f) * alpha;
                groundLight.intensity = (Mathf.PingPong(timer * 4f, 6f) + 10f) * alpha;

                yield return null;
            }

            Destroy(rayRoot);
        }

        private IEnumerator CreateCurseVFXCoroutine(Vector3 pos)
        {
            Vector3 sfxCenterPos = pos + Vector3.up * 1.2f;
            GameObject curseObj = new GameObject("VFX_CurseAoELight");
            curseObj.transform.position = sfxCenterPos;

            Light light = curseObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.85f, 0.05f, 0.2f);
            light.intensity = 18.0f;
            light.range = 9.0f;

            PlayProceduralSFX(sfxCenterPos, 90f, 130f, 2.2f);

            float timer = 0f;
            while (timer < 3.5f)
            {
                timer += Time.deltaTime;
                light.intensity = Mathf.PingPong(timer * 8f, 18f);
                yield return null;
            }

            Destroy(curseObj);
        }

        private IEnumerator CreatePurificationVFXCoroutine(Vector3 pos)
        {
            GameObject purifyObj = new GameObject("VFX_PurificationAuraLight");
            purifyObj.transform.position = pos + Vector3.up * 8f;
            purifyObj.transform.rotation = Quaternion.Euler(90f, 0, 0);

            Light light = purifyObj.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(0.3f, 0.95f, 1.0f);
            light.intensity = 18.0f;
            light.range = 20.0f;
            light.spotAngle = 30.0f;

            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "PurifyBeamCylinder";
            beam.transform.SetParent(purifyObj.transform, false);
            beam.transform.localPosition = new Vector3(0, 0, 8f);
            beam.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            beam.transform.localScale = new Vector3(2.8f, 8f, 2.8f);

            Collider col = beam.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Material mat = CreateRuntimeVFXMaterial(new Color(0.4f, 0.95f, 1f, 0.5f), 0.95f, true);
            beam.GetComponent<Renderer>().sharedMaterial = mat;

            PlayProceduralSFX(pos, 700f, 1400f, 3.0f);

            float timer = 0f;
            while (timer < 5.0f)
            {
                timer += Time.deltaTime;
                light.intensity = Mathf.Lerp(18f, 0f, timer / 5.0f);
                yield return null;
            }

            Destroy(purifyObj);
        }

        private IEnumerator CreateFireBurstVFXCoroutine(Vector3 pos)
        {
            GameObject fireObj = new GameObject("VFX_FireBurstLight");
            fireObj.transform.position = pos + Vector3.up * 1.2f;

            Light light = fireObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.35f, 0.05f);
            light.intensity = 18.0f;
            light.range = 9.0f;

            PlayProceduralSFX(pos, 150f, 300f, 1.2f);

            float timer = 0f;
            while (timer < 2.5f)
            {
                timer += Time.deltaTime;
                light.intensity = Mathf.Lerp(18f, 0f, timer / 2.5f);
                yield return null;
            }

            Destroy(fireObj);
        }

        private IEnumerator CreateHealingAuraVFXCoroutine(Vector3 pos)
        {
            GameObject healObj = new GameObject("VFX_HealingAuraLight");
            healObj.transform.position = pos + Vector3.up * 1.0f;

            Light light = healObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.2f, 0.95f, 0.4f);
            light.intensity = 14.0f;
            light.range = 8.0f;

            PlayProceduralSFX(pos, 440f, 880f, 2.0f);

            float timer = 0f;
            while (timer < 4.0f)
            {
                timer += Time.deltaTime;
                light.intensity = Mathf.PingPong(timer * 4f, 14f);
                yield return null;
            }

            Destroy(healObj);
        }

        private IEnumerator CreateShadowMistVFXCoroutine(Vector3 pos)
        {
            GameObject shadowObj = new GameObject("VFX_ShadowMistLight");
            shadowObj.transform.position = pos + Vector3.up * 1.0f;

            Light light = shadowObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.6f, 0.15f, 0.85f);
            light.intensity = 14.0f;
            light.range = 8.0f;

            PlayProceduralSFX(pos, 110f, 220f, 2.0f);

            float timer = 0f;
            while (timer < 4.0f)
            {
                timer += Time.deltaTime;
                light.intensity = Mathf.Lerp(14f, 0f, timer / 4.0f);
                yield return null;
            }

            Destroy(shadowObj);
        }

        private IEnumerator CreateSparkleShimmerVFXCoroutine(Vector3 pos)
        {
            GameObject starObj = new GameObject("VFX_SparkleShimmerLight");
            starObj.transform.position = pos + Vector3.up * 1.5f;

            Light light = starObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.85f, 0.25f);
            light.intensity = 12.0f;
            light.range = 7.0f;

            PlayProceduralSFX(pos, 600f, 1200f, 1.8f);

            float timer = 0f;
            while (timer < 3.5f)
            {
                timer += Time.deltaTime;
                light.intensity = Mathf.Lerp(12f, 0f, timer / 3.5f);
                yield return null;
            }

            Destroy(starObj);
        }

        private IEnumerator CreateGoldCoinsVFXCoroutine(Vector3 pos)
        {
            GameObject coinObj = new GameObject("VFX_GoldCoinsLight");
            coinObj.transform.position = pos + Vector3.up * 1.2f;

            Light light = coinObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.8f, 0.1f);
            light.intensity = 15.0f;
            light.range = 7.0f;

            PlayProceduralSFX(pos, 800f, 1600f, 1.5f);

            float timer = 0f;
            while (timer < 3.0f)
            {
                timer += Time.deltaTime;
                light.intensity = Mathf.Lerp(15f, 0f, timer / 3.0f);
                yield return null;
            }

            Destroy(coinObj);
        }

        private void PlayProceduralSFX(Vector3 pos, float startFreq, float endFreq, float duration)
        {
            GameObject audioObj = new GameObject("ProceduralSFX_AudioSource");
            audioObj.transform.position = pos;
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.spatialBlend = 0.2f;
            source.minDistance = 2f;
            source.maxDistance = 25f;

            int sampleRate = 44100;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(startFreq, endFreq, t / duration);
                float env = Mathf.Sin(t / duration * Mathf.PI); // Envelope fade in/out
                data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.35f;
            }

            AudioClip clip = AudioClip.Create("ProceduralSFX", samples, 1, sampleRate, false);
            clip.SetData(data, 0);

            source.clip = clip;
            source.Play();
            Destroy(audioObj, duration + 0.2f);
        }
    }
}
