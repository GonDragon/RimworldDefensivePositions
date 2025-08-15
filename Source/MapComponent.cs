using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using Verse;
using Verse.Sound;

namespace DefensivePositions
{
    public class DefensivePositionsMapComponent : MapComponent
    {
        public bool advancedModeEnabled;
        public bool AdvancedModeEnabled
        {
            get { return advancedModeEnabled; }
            set { advancedModeEnabled = value; }
        }
        public ScheduledReportManager Reporter { get; }

        internal readonly PawnSquadHandler squadHandler;
        internal readonly MiscHotkeyHandler miscHotkeys;

        public int lastAdvancedControlUsed;
        public int LastAdvancedControlUsed
        {
            get { return lastAdvancedControlUsed; }
            set { lastAdvancedControlUsed = value; }
        }       

        internal bool modeSwitchScheduled;
        internal SoundDef scheduledSound;

        public DefensivePositionsMapComponent(Map map) : base(map)
        {
            squadHandler = new PawnSquadHandler(this);
            miscHotkeys = new MiscHotkeyHandler();
            Reporter = new ScheduledReportManager(this);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            DefensivePositions.Log("DefensivePositionsGameComponent initialized.");
        }

        public override void MapComponentTick()
        {
            if (Current.ProgramState != ProgramState.Playing) return;
            if (modeSwitchScheduled)
            {
                ToggleAdvancedMode(!AdvancedModeEnabled);
                modeSwitchScheduled = false;
            }
            Reporter.Update();
            if (scheduledSound != null)
            {
                scheduledSound.PlayOneShotOnCamera();
                scheduledSound = null;
            }
        }

        public override void MapComponentOnGUI()
        {
            if (Current.ProgramState != ProgramState.Playing) return;
            squadHandler.OnGUI();
            miscHotkeys.OnGUI();
        }

        public override void MapRemoved()
        {
            foreach (var handler in handlers.Values)
            {
                handler.OnMapDiscarded(map);
            }
        }

        public List<PawnSquad> pawnSquads = new List<PawnSquad>();
        public List<PawnSquad> PawnSquads
        {
            get { return pawnSquads; }
            set { pawnSquads = value; }
        }

        private Dictionary<Pawn, PawnSavedPositionHandler> handlers = new Dictionary<Pawn, PawnSavedPositionHandler>();

        private List<PawnSavedPositionHandler> tempHandlerSavingList;

        public PawnSavedPositionHandler GetOrAddPawnHandler(Pawn pawn)
        {
            var handler = handlers.TryGetValue(pawn);
            if (handler == null)
            {
                handler = new PawnSavedPositionHandler();
                handler.AssignOwner(pawn);
                handlers.Add(pawn, handler);
            }
            return handler;
        }

        public override void ExposeData()
        {
            var mode = Scribe.mode;
            Scribe_Values.Look(ref advancedModeEnabled, "advancedModeEnabled");
            Scribe_Values.Look(ref lastAdvancedControlUsed, "lastAdvancedControlUsed");
            if (mode == LoadSaveMode.Saving)
            {
                // convert to list first- we can get the keys from the handlers at load time
                tempHandlerSavingList = HandlerListFromDictionary(handlers);
                DiscardNonSaveWorthySquads();
            }
            Scribe_Collections.Look(ref tempHandlerSavingList, "savedPositions", LookMode.Deep);
            Scribe_Collections.Look(ref pawnSquads, "pawnSquads", LookMode.Deep);
            if (mode == LoadSaveMode.PostLoadInit)
            {
                handlers = HandlerListToDictionary(tempHandlerSavingList);
                tempHandlerSavingList = null;
                if (PawnSquads == null) PawnSquads = new List<PawnSquad>();
                LastAdvancedControlUsed = Mathf.Clamp(LastAdvancedControlUsed, 0, PawnSavedPositionHandler.NumAdvancedPositionButtons - 1);
            }
        }

        private static List<PawnSavedPositionHandler> HandlerListFromDictionary(Dictionary<Pawn, PawnSavedPositionHandler> dict)
        {
            return dict.Values
                .Where(v => v.ShouldBeSaved)
                .ToList();
        }

        private static Dictionary<Pawn, PawnSavedPositionHandler> HandlerListToDictionary(List<PawnSavedPositionHandler> list)
        {
            return (list ?? Enumerable.Empty<PawnSavedPositionHandler>())
                .Where(psp => psp?.Owner != null)
                .ToDictionary(psp => psp.Owner, v => v);
        }

        private void DiscardNonSaveWorthySquads()
        {
            pawnSquads.RemoveAll(s => s == null || !s.ShouldBeSaved);
        }

        // actual switching will occur on next frame- due to possible multiple calls
        public void ScheduleAdvancedModeToggle()
        {
            modeSwitchScheduled = true;
        }

        public void ScheduleSoundOnCamera(SoundDef sound)
        {
            scheduledSound = sound;
        }

        internal void ToggleAdvancedMode(bool enable)
        {
            AdvancedModeEnabled = enable;
        }
    }
}
