using System;
using System.Collections.Generic;
using UnityEngine;

namespace Divinatius.NPC
{
    [Serializable]
    public class PlotPoint
    {
        [Tooltip("Title of this plot or quest topic (e.g. Demon Lord Quest).")]
        public string topicName = "Demon Lord Quest";

        [Tooltip("Keywords that trigger this plot info when mentioned by player (e.g. demon, demon lord, slay, boss, spire).")]
        public List<string> keywords = new List<string> { "demon", "demon lord", "slay", "boss", "spire", "kill demon" };

        [TextArea(3, 8)]
        [Tooltip("The plot information / lore revelation provided when asked about this topic.")]
        public string plotInformation = "The Demon Lord dwells high in the Obsidian Spire beyond the northern pass. Legend says only a weapon tempered in sacred dragon flame can breach his shadow armor!";

        [Tooltip("Has this plot point been discovered by the player?")]
        public bool isDiscovered = false;
    }

    public class NPCPlotKnowledge : MonoBehaviour
    {
        [Header("Plot Points & Quest Lore Config")]
        [Tooltip("List out plot points and quest info for this NPC in the Inspector. When the player mentions keywords, the NPC will reveal the relevant info.")]
        public List<PlotPoint> plotPoints = new List<PlotPoint>();

        private void Reset()
        {
            if (plotPoints == null || plotPoints.Count == 0)
            {
                plotPoints = new List<PlotPoint>
                {
                    new PlotPoint
                    {
                        topicName = "Demon Lord Quest",
                        keywords = new List<string> { "demon", "demon lord", "slay", "boss", "spire", "kill demon" },
                        plotInformation = "The Demon Lord dwells high in the Obsidian Spire beyond the northern pass. Legend says only a weapon tempered in sacred dragon flame can breach his shadow armor!",
                        isDiscovered = false
                    }
                };
            }
        }

        public string CheckAndGetPlotLore(string playerQuery, out PlotPoint matchedPlot)
        {
            matchedPlot = null;
            if (string.IsNullOrEmpty(playerQuery) || plotPoints == null || plotPoints.Count == 0) return null;

            string lowerQuery = playerQuery.ToLower();

            foreach (var plot in plotPoints)
            {
                if (plot == null || plot.keywords == null) continue;

                foreach (var kw in plot.keywords)
                {
                    if (string.IsNullOrEmpty(kw)) continue;
                    if (lowerQuery.Contains(kw.Trim().ToLower()))
                    {
                        plot.isDiscovered = true;
                        matchedPlot = plot;
                        return plot.plotInformation;
                    }
                }
            }

            return null;
        }
    }
}
