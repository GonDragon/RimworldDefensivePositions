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
        private Dictionary<Pawn, PawnSavedPositionHandler> handlers = new Dictionary<Pawn, PawnSavedPositionHandler>();
        private List<PawnSavedPositionHandler> tempHandlerSavingList;
        internal bool modeSwitchScheduled;
        internal SoundDef scheduledSound;
        public bool advancedModeEnabled;
        public int lastAdvancedControlUsed;
        public List<PawnSquad> pawnSquads = new List<PawnSquad>();
        internal readonly PawnSquadHandler squadHandler;
        internal readonly MiscHotkeyHandler miscHotkeys;

        public bool AdvancedModeEnabled
        {
            get { return advancedModeEnabled; }
            set { advancedModeEnabled = value; }
        }
        public int LastAdvancedControlUsed
        {
            get { return lastAdvancedControlUsed; }
            set { lastAdvancedControlUsed = value; }
        }
        public List<PawnSquad> PawnSquads
        {
            get { return pawnSquads; }
            set { pawnSquads = value; }
        }
        public ScheduledReportManager Reporter { get; }

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

        public override void ExposeData()
        {
            var mode = Scribe.mode;
            Scribe_Values.Look(ref advancedModeEnabled, "advancedModeEnabled");
            Scribe_Values.Look(ref lastAdvancedControlUsed, "lastAdvancedControlUsed");
            if (mode == LoadSaveMode.Saving)
            {
                // convert to list first- we can get the keys from the handlers at load time
                tempHandlerSavingList = PawnSavedPositionHandler.HandlerListFromDictionary(handlers);
                PawnSquadHandler.DiscardNonSaveWorthySquads(pawnSquads);
            }
            Scribe_Collections.Look(ref tempHandlerSavingList, "savedPositions", LookMode.Deep);
            Scribe_Collections.Look(ref pawnSquads, "pawnSquads", LookMode.Deep);
            if (mode == LoadSaveMode.PostLoadInit)
            {
                handlers = PawnSavedPositionHandler.HandlerListToDictionary(tempHandlerSavingList);
                tempHandlerSavingList = null;
                if (PawnSquads == null) PawnSquads = new List<PawnSquad>();
                LastAdvancedControlUsed = Mathf.Clamp(LastAdvancedControlUsed, 0, PawnSavedPositionHandler.NumAdvancedPositionButtons - 1);
            }
        }

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

        /// <summary>
        /// Copies the current state of this map component to the world component.
        /// </summary>
        internal void CopyToWorldComponent()
        {
            DefensivePositionsWorldComponent worldComponent = Find.World.GetComponent<DefensivePositionsWorldComponent>();
            worldComponent.SaveHandlers(PawnSavedPositionHandler.HandlerListFromDictionary(handlers));
            worldComponent.LastAdvancedControlUsed = LastAdvancedControlUsed;
            worldComponent.SaveSquads(PawnSquads);
        }

        /// <summary>
        /// Restores the state of this map component from the world component.
        /// </summary>
        internal void RestoreFromWorldComponent()
        {
            DefensivePositionsWorldComponent worldComponent = Find.World.GetComponent<DefensivePositionsWorldComponent>();

            handlers = PawnSavedPositionHandler.HandlerListToDictionary(worldComponent.tempHandlerSavingList);
            LastAdvancedControlUsed = worldComponent.LastAdvancedControlUsed;
            PawnSquads = new List<PawnSquad>();
            PawnSquads.AddRange(worldComponent.PawnSquads);

            //worldComponent.Clear();
        }
    }
}
