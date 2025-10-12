using Verse;
using Verse.AI;

namespace DefensivePositions
{
    [StaticConstructorOnStartup]
    public static class GearUpAndGoHelper
    {
        internal static bool integrationEnabled = false;

        static GearUpAndGoHelper()
        {
            if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageId == "jungooji.gearupandgo"))
            {
                PawnSavedPositionHandler.DraftToPosition_Delegate = DraftToPosition_GearUpAndGo;
                integrationEnabled = true;
                DefensivePositions.Log("GearUpAndGo Integration Enabled");
            }
        }

        static void DraftToPosition_GearUpAndGo(Pawn pawn, IntVec3 position)
        {
            if (pawn.IsColonist)
            {
                GearUpAndGo.GearUpPolicyComp.comp.Set(GearUpAndGo.Mod.settings.betterPawnControlBattlePolicy);
                pawn.jobs.TryTakeOrderedJob(new Job(GearUpAndGo.GearUpAndGoJobDefOf.GearUpAndGo, position), JobTag.DraftedOrder);
            }
            else
            {
                PawnSavedPositionHandler.DraftToPosition_Base(pawn, position);
            }
        }

        internal static void UndraftReturnsPolicy()
        {   if(GearUpAndGo.GearUpPolicyComp.comp.IsOn()) GearUpAndGo.Command_GearUpAndGo.End();
        }
    }
}