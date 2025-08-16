using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DefensivePositions {
	public interface IDefensivePositionGizmoHandler {
		void OnBasicGizmoAction();
		void OnBasicGizmoHover();
		void OnAdvancedGizmoClick(int controlIndex);
		void OnAdvancedGizmoHover(int controlIndex);
		void OnAdvancedGizmoHotkeyDown();
		IEnumerable<FloatMenuOption> GetGizmoContextMenuOptions(int slotIndex, bool showSlotSuffix);
	}

	/// <summary>
	/// Displays and handles user interaction for both the basic and advanced Defensive Position gizmo
	/// </summary>
	public class PawnSavedPositionHandler : PawnSavedPositions, IDefensivePositionGizmoHandler {
		public const int NumAdvancedPositionButtons = NumStoredPositions;
		public const float HotkeyMultiPressTimeout = .5f;
		public const float PositionMoteEmissionInterval = .5f;

		private float lastMultiPressTime;
		private int lastMultiPressSlot;
		private float positionMoteExpireTime;

		private DefensivePositionsMapComponent MapComponent => Owner.Map.GetComponent<DefensivePositionsMapComponent>();

        public (bool success, int activatedSlot) TrySendPawnToPositionByHotkey() {
			var index = GetHotkeyControlIndex();
			var success = false;
			if (HasSavedPosition(index)) {
				DraftToPosition(GetPosition(index));
				success = true;
			}
			return (success, index);
		}

		public Command GetGizmo() {
			if (MapComponent.AdvancedModeEnabled) {
				return new Gizmo_QuadButtonPanel {
					atlasTexture = Resources.Textures.AdvancedButtonAtlas,
					iconUVsInactive = Resources.Textures.IconUVsInactive,
					iconUVsActive = Resources.Textures.IconUVsActive,
					activeIconMask = GetAdvancedIconActivationMask(),
					hotKey = Resources.Hotkeys.DefensivePositionGizmo,
					defaultLabel = "DefPos_advanced_label".Translate(),
					defaultDesc = "DefPos_advanced_desc".Translate(),
					activateSound = SoundDefOf.Tick_Tiny,
					interactionHandler = this
				};
			} else {
				var useActiveIcon = HasSavedPosition(0);
				return new Gizmo_DefensivePositionButton {
					defaultLabel = "DefPos_basic_label".Translate(),
					defaultDesc = "DefPos_basic_desc".Translate(),
					hotKey = Resources.Hotkeys.DefensivePositionGizmo,
					icon = useActiveIcon ? Resources.Textures.BasicButtonActive : Resources.Textures.BasicButton,
					hasHighPriorityIcon = useActiveIcon,
					activateSound = SoundDefOf.Tick_Tiny,
					interactionHandler = this
				};
			}
		}

		internal void SetDefensivePosition(int positionIndex) {
			var targetPos = GetOwnerDestinationOrPosition();
			SetPosition(positionIndex, targetPos);
			MapComponent.Reporter.ReportPawnInteraction(ScheduledReportManager.ReportType.SavedPosition, Owner, true, positionIndex);
		}

		internal void DiscardSavedPosition(int controlIndex) {
			var hadPosition = HasSavedPosition(controlIndex);
			DiscardPosition(controlIndex);
			MapComponent.Reporter.ReportPawnInteraction(ScheduledReportManager.ReportType.ClearedPosition, Owner, hadPosition, controlIndex);
		}

		private byte GetAdvancedIconActivationMask() {
			int mask = 0;
			for (int i = 0; i < NumAdvancedPositionButtons; i++) {
				mask |= (HasSavedPosition(i) ? 1 : 0) << i;
			}
			return (byte)mask;
		}

		private IntVec3 GetOwnerDestinationOrPosition() {
			var curJob = Owner.jobs.curJob;
			return Owner.Drafted && curJob != null && curJob.def == JobDefOf.Goto ? curJob.targetA.Cell : Owner.Position;
		}
		
		public void OnAdvancedGizmoHotkeyDown() {
			var controlToActivate = GetHotkeyControlIndex();
			HandleControlInteraction(controlToActivate);
		}

		public void OnAdvancedGizmoClick(int controlIndex) {
			MapComponent.LastAdvancedControlUsed = controlIndex;
			HandleControlInteraction(controlIndex);
		}

		private void OnBasicGizmoAction() {
			HandleControlInteraction(0);
		}

		public void OnBasicGizmoHover() {
			HighlightDefensivePositionLocation(0);
		}

		public void OnAdvancedGizmoHover(int controlIndex) {
			HighlightDefensivePositionLocation(controlIndex);
		}

		public IEnumerable<FloatMenuOption> GetGizmoContextMenuOptions(int slotIndex, bool showSlotSuffix) {
			string TranslateWithSuffix(string key) {
				return key.Translate(showSlotSuffix 
					? "DefPos_context_slotSuffix".Translate(slotIndex + 1) 
					: (TaggedString)string.Empty
				);
			}
			yield return new FloatMenuOption(TranslateWithSuffix("DefPos_context_assignPosition"), () => SetDefensivePosition(slotIndex));
			if (HasSavedPosition(slotIndex)) {
				yield return new FloatMenuOption(TranslateWithSuffix("DefPos_context_clearPosition"), () => DiscardSavedPosition(slotIndex));
			}
			yield return new FloatMenuOption(TranslateWithSuffix("DefPos_context_toggleAdvanced"), () => MapComponent.ScheduleAdvancedModeToggle());
		}

		private int GetHotkeyControlIndex() {
			switch (Settings.SlotHotkeySetting) {
				case Settings.HotkeyMode.FirstSlotOnly:
					return 0;
				case Settings.HotkeyMode.LastUsedSlot:
					return MapComponent.LastAdvancedControlUsed;
				case Settings.HotkeyMode.MultiPress:
					if (MapComponent.AdvancedModeEnabled && Time.unscaledTime - lastMultiPressTime < HotkeyMultiPressTimeout) {
						lastMultiPressSlot = (lastMultiPressSlot + 1) % NumAdvancedPositionButtons;
					} else {
						lastMultiPressSlot = 0;
					}
					lastMultiPressTime = Time.unscaledTime;
					return lastMultiPressSlot;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandleControlInteraction(int controlIndex) {
			var manager = MapComponent;
			if (Event.current.shift && Settings.ShiftKeyModeSetting == Settings.ShiftKeyMode.AssignSlot) {
				// save new spot
				SetDefensivePosition(controlIndex);
			} else if (Event.current.control) {
				// unset saved spot
				DiscardSavedPosition(controlIndex);
			} else if (Event.current.alt) {
				// switch mode
				manager.ScheduleAdvancedModeToggle();
			} else {
				// draft and send to saved spot
				if (HasSavedPosition(controlIndex)) {
					DraftToPosition(GetPosition(controlIndex));
					manager.Reporter.ReportPawnInteraction(ScheduledReportManager.ReportType.SentToSavedPosition, Owner, true, controlIndex);
				} else {
					manager.Reporter.ReportPawnInteraction(ScheduledReportManager.ReportType.SentToSavedPosition, Owner, false, controlIndex);
				}
			}
		}

		private void DraftToPosition(IntVec3 position) {
			var job = JobMaker.MakeJob(Resources.Jobs.DPDraftToPosition, position);
			Owner.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
			MapComponent.ScheduleSoundOnCamera(SoundDefOf.DraftOn);
		}

		private void HighlightDefensivePositionLocation(int controlIndex) {
			if (positionMoteExpireTime > Time.unscaledTime) return;
			if (HasSavedPosition(controlIndex)) {
				positionMoteExpireTime = Time.unscaledTime + PositionMoteEmissionInterval;
				var savedPosition = GetPosition(controlIndex);
				MoteMaker.MakeStaticMote(savedPosition, Owner.Map, Resources.Things.DPPositionMote);
			}
		}

		void IDefensivePositionGizmoHandler.OnBasicGizmoAction() {
			OnBasicGizmoAction();
		}

		public override string ToString() {
			return $"[{nameof(PawnSavedPositionHandler)} {Owner.ToStringSafe()}]";
		}
	}
}