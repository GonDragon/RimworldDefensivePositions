using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using UnityEngine;
using Verse;
using static DefensivePositions.Settings;

namespace DefensivePositions
{
    public class Settings : ModSettings
    {
        public enum HotkeyMode
        {
            FirstSlotOnly,
            LastUsedSlot,
            MultiPress
        }

        public enum ShiftKeyMode
        {
            AssignSlot,
            QueueOrder
        }

        public static HotkeyMode SlotHotkeySetting;
        public static ShiftKeyMode ShiftKeyModeSetting;
        public static int SameGroupDistance;
        public static bool JumpingSelectsNearby;

        public override void ExposeData()
        {
            Scribe_Values.Look<HotkeyMode>(ref SlotHotkeySetting, "SlotHotkey", HotkeyMode.FirstSlotOnly);
            Scribe_Values.Look<ShiftKeyMode>(ref ShiftKeyModeSetting, "ShiftKeyMode", ShiftKeyMode.AssignSlot);
            Scribe_Values.Look<int>(ref SameGroupDistance, "SameGroupDistance", 30);
            Scribe_Values.Look<bool>(ref JumpingSelectsNearby, "JumpingSelectsNearby", false);
        }

        public void RestoreDefault()
        {
            SlotHotkeySetting = HotkeyMode.FirstSlotOnly;
            ShiftKeyModeSetting = ShiftKeyMode.AssignSlot;
            SameGroupDistance = 30;
            JumpingSelectsNearby = false;

            Write();
        }
    }

    internal class DefensivePositionsMod : Mod
    {
        public static Settings settings;
        internal static string _buffer;

        public DefensivePositionsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            float pageTittleHeight = 26f;
            float pageTittleWidth = (float)Math.Floor(inRect.width / 3);

            Rect contentRect = new Rect(inRect.x, inRect.y + pageTittleHeight, inRect.width, inRect.height - pageTittleHeight - 30f);

            float columnWidth = inRect.width / 2;
            float heigth = inRect.height;
            Rect column1 = new Rect(columnWidth - columnWidth / 2, inRect.y, columnWidth, heigth).ContractedBy(2f);
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(column1);

            if (listingStandard.ButtonTextLabeled("setting_slotHotkeyMode_label".Translate(), $"setting_slotHotkeyMode_{Settings.SlotHotkeySetting.ToString()}".Translate(), tooltip: "setting_slotHotkeyMode_desc".Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (HotkeyMode mode in Enum.GetValues(typeof(HotkeyMode)))
                {
                    options.Add(new FloatMenuOption($"setting_slotHotkeyMode_{mode.ToString()}".Translate(), (Action)(() => { Settings.SlotHotkeySetting = mode; })));
                }
                Find.WindowStack.Add((Window)new FloatMenu(options));
            }

            if (listingStandard.ButtonTextLabeled("setting_shiftKeyMode_label".Translate(), $"setting_shiftKeyMode_{Settings.ShiftKeyModeSetting}".Translate(), tooltip: "setting_shiftKeyMode_desc".Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (ShiftKeyMode mode in Enum.GetValues(typeof(ShiftKeyMode)))
                {
                    options.Add(new FloatMenuOption($"setting_shiftKeyMode_{mode}".Translate(), (Action)(() => { Settings.ShiftKeyModeSetting = mode; })));
                }
                Find.WindowStack.Add((Window)new FloatMenu(options));
            }

            listingStandard.Gap();
            //listingStandard.TextFieldNumericLabeled<int>("settings_sameGroupDistance_label".Translate(), ref SameGroupDistance,ref _buffer, 0, 2000);
            Rect row = listingStandard.GetRect(Text.LineHeight); // one line, same height as other settings

            float labelWidth = row.width * 0.7f;
            Rect labelRect = new Rect(row.x, row.y, labelWidth, row.height);
            Rect inputRect = new Rect(row.x + labelWidth + 10f, row.y, row.width - labelWidth - 10f, row.height);
            Widgets.DrawHighlightIfMouseover(row);
            Widgets.Label(labelRect, "settings_sameGroupDistance_label".Translate());
            if (Mouse.IsOver(labelRect))
            {
                TooltipHandler.TipRegion(labelRect, "settings_sameGroupDistance_desc".Translate());
            }
            Widgets.TextFieldNumeric(inputRect, ref SameGroupDistance, ref _buffer, 0, 2000);

            listingStandard.Gap();

            listingStandard.CheckboxLabeled("settings_jumpingSelectsNearby_label".Translate(), ref Settings.JumpingSelectsNearby, tooltip: "settings_jumpingSelectsNearby_desc".Translate());

            listingStandard.GapLine();

            if (listingStandard.ButtonTextLabeled("setting_restoreDefault_label".Translate(), "setting_restoreDefault_buttonlabel".Translate(), tooltip: "setting_restoreDefault_desc".Translate()))
            {
                settings.RestoreDefault();
                _buffer = null;
            }

            listingStandard.End();

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
