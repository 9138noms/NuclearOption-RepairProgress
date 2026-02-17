using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RepairProgress
{
    [BepInPlugin("com.noms.repairprogress", "Repair Progress", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static bool showUI = true;

        private void Awake()
        {
            Log = Logger;

            var harmony = new Harmony("com.noms.repairprogress");
            harmony.PatchAll();

            var helperGo = new GameObject("RepairProgress_FrameHelper");
            UnityEngine.Object.DontDestroyOnLoad(helperGo);
            helperGo.hideFlags = HideFlags.HideAndDontSave;
            helperGo.AddComponent<RPFrameHelper>();

            Log.LogInfo("Repair Progress v1.0.0 loaded - F9 to toggle");
        }
    }

    // ========== Repair Tracker ==========

    internal static class RepairTracker
    {
        internal struct RepairEntry
        {
            public Building building;
            public float lastSeenTime;
        }

        static readonly Dictionary<Building, RepairEntry> activeRepairs = new Dictionary<Building, RepairEntry>();

        public static void RecordRepair(Building b)
        {
            if (b == null) return;
            activeRepairs[b] = new RepairEntry
            {
                building = b,
                lastSeenTime = Time.timeSinceLevelLoad
            };
        }

        public static float CalculateHealth(Building b)
        {
            var parts = b.partLookup;
            if (parts == null || parts.Count == 0) return 1f;
            float sum = 0f;
            foreach (var part in parts)
                sum += Mathf.Max(part.hitPoints, 0f);
            return sum / (parts.Count * 100f);
        }

        public static void Cleanup()
        {
            float now = Time.timeSinceLevelLoad;
            var toRemove = new List<Building>();
            foreach (var kvp in activeRepairs)
            {
                if (kvp.Key == null || now - kvp.Value.lastSeenTime > 2f || !kvp.Key.needsRepair)
                    toRemove.Add(kvp.Key);
            }
            foreach (var k in toRemove)
                activeRepairs.Remove(k);
        }

        public static Dictionary<Building, RepairEntry> GetActiveRepairs() => activeRepairs;

        public static void Clear() => activeRepairs.Clear();
    }

    // ========== Harmony Patch ==========

    [HarmonyPatch(typeof(Building), "Repair", new Type[] { typeof(Unit), typeof(float) })]
    internal class Building_Repair_Patch
    {
        static void Postfix(Building __instance)
        {
            if (__instance != null && __instance.needsRepair)
                RepairTracker.RecordRepair(__instance);
        }
    }

    // ========== FrameHelper ==========

    internal class RPFrameHelper : MonoBehaviour
    {
        private int frameCounter;
        private static FieldInfo repairerUnitToRepairField;
        private static bool reflectionInit;

        // Cached textures
        private Texture2D bgTex;
        private Texture2D whiteTex;
        private GUIStyle labelStyle;
        private bool stylesInit;

        private void InitReflection()
        {
            if (reflectionInit) return;
            reflectionInit = true;
            try
            {
                repairerUnitToRepairField = typeof(Repairer).GetField("unitToRepair",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (repairerUnitToRepairField != null)
                    Plugin.Log.LogInfo("Repairer.unitToRepair field found");
                else
                    Plugin.Log.LogWarning("Repairer.unitToRepair field NOT found");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Reflection init failed: {e.Message}");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F9))
            {
                Plugin.showUI = !Plugin.showUI;
                Plugin.Log.LogInfo($"Repair Progress UI: {(Plugin.showUI ? "ON" : "OFF")}");
            }

            if (!Plugin.showUI) return;

            frameCounter++;
            if (frameCounter % 30 == 0)
            {
                InitReflection();
                ScanRepairers();
                RepairTracker.Cleanup();
            }
        }

        private void ScanRepairers()
        {
            if (repairerUnitToRepairField == null) return;

            try
            {
                var repairers = UnityEngine.Object.FindObjectsOfType<Repairer>();
                foreach (var r in repairers)
                {
                    if (r == null) continue;
                    var target = repairerUnitToRepairField.GetValue(r) as Unit;
                    if (target != null && target is Building building && building.needsRepair)
                    {
                        RepairTracker.RecordRepair(building);
                    }
                }
            }
            catch { }
        }

        private void InitStyles()
        {
            if (stylesInit) return;
            stylesInit = true;

            bgTex = MakeTex(new Color(0.08f, 0.08f, 0.08f, 0.85f));
            whiteTex = MakeTex(Color.white);

            labelStyle = new GUIStyle
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            labelStyle.normal.textColor = Color.white;
        }

        private Texture2D MakeTex(Color c)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, c);
            tex.Apply();
            return tex;
        }

        private void OnGUI()
        {
            if (!Plugin.showUI) return;

            var repairs = RepairTracker.GetActiveRepairs();
            if (repairs.Count == 0) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            InitStyles();

            foreach (var kvp in repairs)
            {
                Building b = kvp.Key;
                if (b == null) continue;

                try
                {
                    DrawRepairBar(cam, b);
                }
                catch { }
            }
        }

        private void DrawRepairBar(Camera cam, Building b)
        {
            // Find top of building using renderer bounds
            Vector3 worldPos;
            var renderers = b.GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);
                worldPos = new Vector3(bounds.center.x, bounds.max.y + 5f, bounds.center.z);
            }
            else
            {
                worldPos = b.transform.position + Vector3.up * 50f;
            }
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

            // Behind camera
            if (screenPos.z < 0) return;

            float distance = screenPos.z;

            // Too far away
            if (distance > 3000f) return;

            // GUI coordinates (flip Y)
            float guiX = screenPos.x;
            float guiY = Screen.height - screenPos.y;

            // Fixed size bar
            float barWidth = 160f;
            float barHeight = 14f;
            int fontSize = 12;

            float healthPercent = RepairTracker.CalculateHealth(b);

            float x = guiX - barWidth / 2f;
            float y = guiY;

            // Building name
            string unitName = "Building";
            try
            {
                if (b.definition != null && !string.IsNullOrEmpty(b.definition.unitName))
                    unitName = b.definition.unitName;
            }
            catch { }

            string label = $"{unitName} - {healthPercent * 100f:F0}%";

            // Shadow text above bar
            labelStyle.fontSize = fontSize;
            var content = new GUIContent(label);
            var textSize = labelStyle.CalcSize(content);
            float textX = guiX - textSize.x / 2f;
            float textY = y - textSize.y - 2f;

            // Text shadow
            var origColor = labelStyle.normal.textColor;
            labelStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(textX + 1, textY + 1, textSize.x, textSize.y), label, labelStyle);
            labelStyle.normal.textColor = origColor;
            GUI.Label(new Rect(textX, textY, textSize.x, textSize.y), label, labelStyle);

            // Background bar
            GUI.DrawTexture(new Rect(x, y, barWidth, barHeight), bgTex);

            // Health fill (red → yellow → green)
            Color fillColor;
            if (healthPercent < 0.5f)
                fillColor = Color.Lerp(new Color(0.8f, 0.1f, 0.1f), new Color(0.9f, 0.8f, 0.1f), healthPercent * 2f);
            else
                fillColor = Color.Lerp(new Color(0.9f, 0.8f, 0.1f), new Color(0.1f, 0.85f, 0.2f), (healthPercent - 0.5f) * 2f);

            float fillWidth = (barWidth - 2f) * Mathf.Clamp01(healthPercent);
            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(x + 1, y + 1, fillWidth, barHeight - 2), whiteTex);
            GUI.color = Color.white;

            // Border outline
            float borderAlpha = 0.5f;
            GUI.color = new Color(1f, 1f, 1f, borderAlpha);
            GUI.DrawTexture(new Rect(x, y, barWidth, 1), whiteTex); // top
            GUI.DrawTexture(new Rect(x, y + barHeight - 1, barWidth, 1), whiteTex); // bottom
            GUI.DrawTexture(new Rect(x, y, 1, barHeight), whiteTex); // left
            GUI.DrawTexture(new Rect(x + barWidth - 1, y, 1, barHeight), whiteTex); // right
            GUI.color = Color.white;
        }
    }
}
