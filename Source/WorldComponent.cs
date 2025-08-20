using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DefensivePositions
{
    internal class DefensivePositionsWorldComponent : WorldComponent
    {
        internal List<PawnSavedPositionHandler> tempHandlerSavingList;
        public int lastAdvancedControlUsed;
        public List<PawnSquad> pawnSquads = new List<PawnSquad>();
        public bool advancedModeEnabled;
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
        public DefensivePositionsWorldComponent(World world) : base(world)
        {
        }

        public void Clear()
        {
            tempHandlerSavingList?.Clear();
            pawnSquads?.Clear();
        }

        public void PrepareHandlersForFlight(IntVec3 origin, HashSet<IntVec3> engineFloors, List<Pawn> crew)
        {
            tempHandlerSavingList = tempHandlerSavingList
                .Where(h => crew.Contains(h.Owner)
                && engineFloors.Contains(h.Owner.Position)
                && h.GetSavedPositions().Any(pos => engineFloors.Contains(pos)))
                .ToList();

            foreach(PawnSavedPositionHandler handler in tempHandlerSavingList)
            {
                handler.DiscardNotInSet(engineFloors);
                handler.AdjustPositionToRelative(origin);
            }
        }

        public void AdjustHandlersToNewOrigin(IntVec3 newOrigin, Rot4 rotation)
        {
            if (tempHandlerSavingList != null)
            {
                foreach (PawnSavedPositionHandler handler in tempHandlerSavingList)
                {
                    handler.AdjustPositionFromRelative(newOrigin, rotation);
                }
            }
        }

        public List<PawnSquad> PawnSquads
        {
            get { return pawnSquads; }
            set { pawnSquads = value; }
        }

        public void SaveHandlers(List<PawnSavedPositionHandler> handlers)
        {
            if (tempHandlerSavingList == null) tempHandlerSavingList = new List<PawnSavedPositionHandler>();
            tempHandlerSavingList.Clear();
            tempHandlerSavingList.AddRange(handlers);
        }

        public void SaveSquads(List<PawnSquad> squads)
        {
            if (PawnSquads == null) PawnSquads = new List<PawnSquad>();
            PawnSquads.Clear();
            PawnSquads.AddRange(squads);
        }

        public override void ExposeData()
        {
            var mode = Scribe.mode;
            Scribe_Values.Look(ref lastAdvancedControlUsed, "lastAdvancedControlUsed");
            Scribe_Values.Look(ref advancedModeEnabled, "advancedModeEnabled");
            if (mode == LoadSaveMode.Saving)
            {
                // convert to list first- we can get the keys from the handlers at load time
                PawnSquadHandler.DiscardNonSaveWorthySquads(pawnSquads);
            }
            Scribe_Collections.Look(ref tempHandlerSavingList, "savedPositions", LookMode.Deep);
            Scribe_Collections.Look(ref pawnSquads, "pawnSquads", LookMode.Deep);
            if (mode == LoadSaveMode.PostLoadInit)
            {
                if (PawnSquads == null) PawnSquads = new List<PawnSquad>();
                LastAdvancedControlUsed = Mathf.Clamp(LastAdvancedControlUsed, 0, PawnSavedPositionHandler.NumAdvancedPositionButtons - 1);
            }
        }
    }
}
