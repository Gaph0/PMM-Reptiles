using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Decorates the map of a medusa ruins ambush, so the fight happens in a place
    /// that reads as the ruin the caravan stumbled into: broken stone walls, fallen
    /// rock, stone "statues" of the medusa's petrified victims, and a little buried
    /// wealth. The medusas themselves are spawned by the incident, not here.
    ///
    /// Timing. For a caravan target, IncidentWorker_Ambush generates the map and
    /// places both groups inside CaravanIncidentUtility.SetupCaravanAttackMap, and
    /// the only hook that hands us the finished map is a postfix on that method.
    /// So:
    ///   1. IncidentWorker_MedusaRuinsAmbush.GeneratePawns (one step earlier) sets
    ///      <see cref="Pending"/>.
    ///   2. The postfix consumes it, re-checks that the map's tile really is near a
    ///      ruin, and fills the map.
    /// The second check exists so a flag left over from an aborted incident can never
    /// decorate somebody else's ambush, and so a vanilla ambush on the same map type
    /// is left alone.
    ///
    /// Placement rules: only on cells that are walkable, empty of buildings and at
    /// least 3 tiles from any pawn, so nothing is dropped on a fighter or walls
    /// anyone in. Items go through GenPlace (which slides them to a free cell instead
    /// of wiping what is there); buildings are spawned directly, because walls and
    /// sculptures must land exactly where they are placed.
    /// </summary>
    public static class MedusaRuinFill
    {
        /// <summary>
        /// Set by the medusa ambush just before the ambush map is generated, and
        /// cleared by the postfix below when it consumes it.
        /// </summary>
        public static bool Pending;

        /// <summary>Stone the ruin is built from, and the rock that fell out of it.</summary>
        private static readonly string[] StoneStuff =
        {
            "BlocksGranite", "BlocksMarble", "BlocksSlate", "BlocksLimestone", "BlocksSandstone"
        };

        private static readonly string[] ChunkDefs =
        {
            "ChunkGranite", "ChunkMarble", "ChunkSlate", "ChunkLimestone", "ChunkSandstone"
        };

        /// <summary>The petrified victims, as stone figures.</summary>
        private static readonly string[] SculptureDefs = { "SculptureSmall", "SculptureLarge" };

        private static readonly IntVec3[] Directions = { IntVec3.North, IntVec3.South, IntVec3.East, IntVec3.West };

        public static void Fill(Map map)
        {
            if (map == null)
            {
                return;
            }
            try
            {
                Walls(map, Rand.RangeInclusive(3, 5));
                Sculptures(map, Rand.RangeInclusive(4, 8));
                Scatter(map, ChunkDefs, Rand.RangeInclusive(8, 14));
                Loot(map);
            }
            catch (System.Exception ex)
            {
                // Decoration must never cost the player her ambush.
                Log.Warning("[PMM Reptiles] Medusa ruin fill failed: " + ex);
            }
        }

        /// <summary>Short broken runs of stone wall - the ruin's own footprint.</summary>
        private static void Walls(Map map, int segments)
        {
            ThingDef stuff = Named(StoneStuff);
            if (stuff == null)
            {
                return;
            }
            for (int i = 0; i < segments; i++)
            {
                if (!TryFindSpot(map, out IntVec3 start, quiet: true))
                {
                    continue;
                }
                IntVec3 dir = Directions.RandomElement();
                int length = Rand.RangeInclusive(2, 4);
                for (int step = 0; step < length; step++)
                {
                    IntVec3 cell = start + dir * step;
                    if (!cell.InBounds(map) || !cell.Walkable(map) || cell.GetEdifice(map) != null)
                    {
                        break;
                    }
                    if (AnyPawnNear(map, cell, 3f))
                    {
                        break;
                    }
                    GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Wall, stuff), cell, map, Rot4.Random);
                }
            }
        }

        /// <summary>
        /// What is left of the people the medusa caught: stone figures standing among
        /// the stones. Spawned directly (a building), with an outsider's art quality so
        /// the game does not expect a craftsman behind them.
        /// </summary>
        private static void Sculptures(Map map, int count)
        {
            for (int i = 0; i < count; i++)
            {
                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(SculptureDefs.RandomElement());
                ThingDef stuff = Named(StoneStuff);
                if (def == null || stuff == null || !TryFindSpot(map, out IntVec3 cell))
                {
                    continue;
                }
                Thing sculpture = ThingMaker.MakeThing(def, stuff);
                CompQuality quality = (sculpture as ThingWithComps)?.TryGetComp<CompQuality>();
                quality?.SetQuality(QualityCategory.Normal, ArtGenerationContext.Outsider);
                GenSpawn.Spawn(sculpture, cell, map, Rot4.Random);
            }
        }

        /// <summary>Loose things: fallen rock.</summary>
        private static void Scatter(Map map, string[] defNames, int count)
        {
            for (int i = 0; i < count; i++)
            {
                ThingDef def = Named(defNames);
                if (def == null || !TryFindSpot(map, out IntVec3 cell))
                {
                    continue;
                }
                GenPlace.TryPlaceThing(ThingMaker.MakeThing(def), cell, map, ThingPlaceMode.Near);
            }
        }

        /// <summary>
        /// What the medusas left behind: silver and jade they have no use for, and the
        /// scales they have shed.
        /// </summary>
        private static void Loot(Map map)
        {
            SpawnStack(map, ThingDefOf.Silver, Rand.RangeInclusive(60, 150));
            SpawnStack(map, ThingDefOf.Jade, Rand.RangeInclusive(20, 60));
            if (ReptileDefOf.PMM_Scale_Lamia != null)
            {
                for (int i = Rand.RangeInclusive(1, 3); i > 0; i--)
                {
                    SpawnStack(map, ReptileDefOf.PMM_Scale_Lamia, Rand.RangeInclusive(10, 30));
                }
            }
        }

        private static void SpawnStack(Map map, ThingDef def, int count)
        {
            if (def == null || count <= 0 || !TryFindSpot(map, out IntVec3 cell))
            {
                return;
            }
            Thing thing = ThingMaker.MakeThing(def);
            thing.stackCount = count;
            GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near);
        }

        /// <summary>
        /// A random cell in the middle of the map: walkable, nothing built on it, and
        /// (when <paramref name="quiet"/> is set) no pawn within three tiles.
        /// </summary>
        private static bool TryFindSpot(Map map, out IntVec3 cell, bool quiet = false)
        {
            for (int i = 0; i < 300; i++)
            {
                IntVec3 candidate = new IntVec3(
                    Rand.RangeInclusive(6, map.Size.x - 7),
                    0,
                    Rand.RangeInclusive(6, map.Size.z - 7));
                if (!candidate.InBounds(map) || !candidate.Walkable(map) || candidate.GetEdifice(map) != null)
                {
                    continue;
                }
                if (quiet && AnyPawnNear(map, candidate, 3f))
                {
                    continue;
                }
                cell = candidate;
                return true;
            }
            cell = IntVec3.Invalid;
            return false;
        }

        private static bool AnyPawnNear(Map map, IntVec3 cell, float radius)
        {
            // AllPawnsSpawned is IReadOnlyList in 1.6, not List.
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn.Spawned && pawn.Position.InHorDistOf(cell, radius))
                {
                    return true;
                }
            }
            return false;
        }

        private static ThingDef Named(string[] defNames)
        {
            return DefDatabase<ThingDef>.GetNamedSilentFail(defNames.RandomElement());
        }
    }

    /// <summary>
    /// The hook that fills the ambush map: SetupCaravanAttackMap is where
    /// IncidentWorker_Ambush builds the map and places both groups, and it hands the
    /// finished map back. Only our own ambush (the flag) on a ruin's tile (the
    /// proximity re-check) is touched.
    /// </summary>
    [HarmonyPatch(typeof(CaravanIncidentUtility), nameof(CaravanIncidentUtility.SetupCaravanAttackMap))]
    public static class Patch_SetupCaravanAttackMap_MedusaRuin
    {
        public static void Postfix(Map __result)
        {
            bool pending = MedusaRuinFill.Pending;
            MedusaRuinFill.Pending = false;
            if (!pending || __result == null)
            {
                return;
            }
            if (IncidentWorker_MedusaRuinsAmbush.RuinNear(__result.Tile) == null)
            {
                return; // stale flag: the tile is not a ruin, so leave the map alone
            }
            MedusaRuinFill.Fill(__result);
        }
    }
}
