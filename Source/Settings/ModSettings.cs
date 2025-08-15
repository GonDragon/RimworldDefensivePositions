using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DefensivePositions
{
    public class DefensivePositionsSettings : ModSettings
    {
        public static DefensivePositions.HotkeyMode SlotHotkeySetting;
        public static DefensivePositions.ShiftKeyMode ShiftKeyModeSetting;
        public static int SameGroupDistance;
        public static bool JumpingSelectsNearby;

        public override void ExposeData()
        {
            Scribe_Values.Look<DefensivePositions.HotkeyMode>(ref SlotHotkeySetting, "SlotHotkey", DefensivePositions.HotkeyMode.FirstSlotOnly);
            Scribe_Values.Look<DefensivePositions.ShiftKeyMode>(ref ShiftKeyModeSetting, "ShiftKeyMode", DefensivePositions.ShiftKeyMode.AssignSlot);
            Scribe_Values.Look<int>(ref SameGroupDistance, "SameGroupDistance", 30);
            Scribe_Values.Look<bool>(ref JumpingSelectsNearby, "JumpingSelectsNearby", false);
        }

        public void RestoreDefault()
        {
            SlotHotkeySetting = DefensivePositions.HotkeyMode.FirstSlotOnly;
            ShiftKeyModeSetting = DefensivePositions.ShiftKeyMode.AssignSlot;
            SameGroupDistance = 30;
            JumpingSelectsNearby = false;

            Write();
        }
    }

    internal class DefensivePositionsMod : Mod
    {
        public static DefensivePositionsSettings settings;

        public DefensivePositionsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<DefensivePositionsSettings>();
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
