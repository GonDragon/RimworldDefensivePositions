using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DefensivePositions
{
    [HarmonyPatch(typeof(Gravship))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(Building_GravEngine) })]
    internal static class Gravship_Patch
    {
        [HarmonyPostfix]
        public static void Insert_BeforeFlightPositionSave(Gravship __instance, Building_GravEngine engine)
        {
            Map map = engine.Map;
            DefensivePositionsMapComponent mapComponent = map.GetComponent<DefensivePositionsMapComponent>();
            mapComponent.CopyToWorldComponent();

            DefensivePositionsWorldComponent tempSave = Find.World.GetComponent<DefensivePositionsWorldComponent>();

            List<Pawn> crew = map.mapPawns.AllPawnsSpawned.ToList();

            IntVec3 origin = __instance.originalPosition;

            tempSave.PrepareHandlersForFlight(
                origin,
                engine.ValidSubstructure,
                crew);

            DefensivePositions.Warning($"Lift Origin: {origin}");
        }
    }

    [HarmonyPatch(typeof(GravshipPlacementUtility), "PlaceGravshipInMap")]
    internal static class GravshipPlacementUtility_Patch
    {

        [HarmonyPostfix]
        public static void Insert_AfterFlightPositionRestore(Gravship gravship, IntVec3 root, Map map)
        {
            DefensivePositionsMapComponent mapComponent = map.GetComponent<DefensivePositionsMapComponent>();
            DefensivePositionsWorldComponent tempSave = Find.World.GetComponent<DefensivePositionsWorldComponent>();
            tempSave.AdjustHandlersToNewOrigin(root,gravship.Rotation);
            mapComponent.RestoreFromWorldComponent();

            tempSave.Clear();

            DefensivePositions.Warning($"Land Origin: {root}");
        }

    }
}
