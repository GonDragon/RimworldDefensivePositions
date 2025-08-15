using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DefensivePositions
{
    public class Settings : ModSettings
    {
        public override void ExposeData()
        {
            //Scribe_Values.Look<DefensivePositions.HotkeyMode>(ref DefensivePositions.Instance.SlotHotkeySetting, "SlotHotkey", DefensivePositionsManager.HotkeyMode.FirstSlotOnly);
            //Scribe_Values.Look<DefensivePositions.ShiftKeyMode>(ref DefensivePositions.Instance.ShiftKeyModeSetting, "ShiftKeyMode", DefensivePositionsManager.ShiftKeyMode.AssignSlot);
        }

        public static void RestoreDefault()
        {

        }
    }

    internal class DefensivePositionsMod : Mod
    {
        public static Settings settings;

        public DefensivePositionsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            float pageTittleHeight = 26f;
            float pageTittleWidth = (float)Math.Floor(inRect.width / 3);

            Rect contentRect = new Rect(inRect.x, inRect.y + pageTittleHeight, inRect.width, inRect.height - pageTittleHeight - 30f);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(inRect.x + pageTittleWidth + pageTittleHeight, inRect.y, pageTittleWidth - 2 * pageTittleHeight, pageTittleHeight), "You should write something");

            Text.Anchor = TextAnchor.UpperLeft;

            base.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }

        public override string SettingsCategory()
        {
            return DefensivePositions.Name;
        }
    }
}
