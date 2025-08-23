using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace DefensivePositions {
	/// <summary>
	/// Base class for <see cref="PawnSavedPositionHandler"/> that takes care of data storage
	/// </summary>
	public abstract class PawnSavedPositions : IExposable {
		protected const int NumStoredPositions = 4;

		public Pawn Owner {
			get { return owner; }
		}

		public bool ShouldBeSaved {
			get { return owner != null && !owner.Destroyed && owner.Faction == Faction.OfPlayer; }
		}

		private Pawn owner;
		private List<IntVec3> savedPositions = new List<IntVec3>();

		public void ExposeData() {
			Scribe_References.Look(ref owner, "owner");
			var mode = Scribe.mode;
			Scribe_Collections.Look(ref savedPositions, "vectorPositions", LookMode.Value);

			if (mode == LoadSaveMode.PostLoadInit && savedPositions == null) {
				savedPositions = ResetSavedPositions();
            }
        }

		public List<IntVec3> GetSavedPositions() {
            if (savedPositions != null)
			{
				return savedPositions;
			}
			return new List<IntVec3>();
        }

        internal void AssignOwner(Pawn newOwner) {
			owner = newOwner;
		}

		protected bool HasSavedPosition(int slot) {
			return GetPosition(slot).IsValid;
		}

		protected IntVec3 GetPosition(int slot) {
			CheckOwnerSpawned();
			return savedPositions.Count > slot ?
				savedPositions[slot]
                : IntVec3.Invalid;
		}

		protected void SetPosition(int slot, IntVec3 position) {
			CheckOwnerSpawned();

			if (savedPositions.Count > slot) {
				savedPositions[slot] = position;
			}
			else
			{
                ResetSavedPositions();
                savedPositions[slot] = position;
            }
		}

		protected void DiscardPosition(int slot) {
			SetPosition(slot, IntVec3.Invalid);
		}

		internal void DiscardNotInSet(HashSet<IntVec3> set) {
			CheckOwnerSpawned();
			for (int i = 0; i < savedPositions.Count; i++) {
				if (!set.Contains(savedPositions[i])) {
                    savedPositions[i] = IntVec3.Invalid;
				}
			}
        }

		internal void AdjustPositionToRelative(IntVec3 origin)
		{
			CheckOwnerSpawned();
			for (int i = 0; i < savedPositions.Count; i++) {
				if (savedPositions[i].IsValid) {
                    savedPositions[i] -= origin;
				}
            }
        }

        internal void AdjustPositionFromRelative(IntVec3 origin, Rot4 rotation)
        {
            CheckOwnerSpawned();
            for (int i = 0; i < savedPositions.Count; i++)
            {
                if (savedPositions[i].IsValid)
                {
					savedPositions[i] = PrefabUtility.GetAdjustedLocalPosition(savedPositions[i], rotation);

                    savedPositions[i] += origin;
                }
            }
        }

        private void CheckOwnerSpawned() {
			if (!owner.Spawned)
				throw new InvalidOperationException(
					$"Cannot access saved positions while owner ({owner.ToStringSafe()}) is not spawned."
				);
		}

		private List<IntVec3> ResetSavedPositions() {
            savedPositions = new List<IntVec3>(Enumerable.Repeat(IntVec3.Invalid, NumStoredPositions));
			return savedPositions;
		}

	}
}