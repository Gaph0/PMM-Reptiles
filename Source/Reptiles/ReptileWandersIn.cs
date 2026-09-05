using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Per-incident candidate list. Each reptile wander-in def carries one of these
    /// with the wild pawnkinds it may spawn; the worker drops the ones whose habitat
    /// doesn't match the tile, so a single worker class serves every habitat.
    /// </summary>
    public class ReptileWildExtension : DefModExtension
    {
        public List<string> candidates = new List<string>();
    }

    /// <summary>
    /// A wild reptile momo wanders in. Modelled on the slime-mod wander-in (itself the
    /// vanilla wild-man flow): factionless, tamed like wild men, already "reached
    /// outside" so it lingers on the map. The habitat gate reads the WORLD tile (not
    /// the map) and only fires when at least one candidate species lives there:
    ///
    ///   caves      Find.World.HasCaves(tile)          (the slime-mod cave mutator check)
    ///   mountains  worldTile.hilliness == Mountainous
    ///   deserts    biome in {Desert, AridShrubland, ExtremeDesert}
    ///   volcanic   biome == LavaField                 (Odyssey only — MayRequire; without
    ///                                                  Odyssey the biome can never appear)
    ///   wetlands   biome == TropicalSwamp             (locked Q3: marsh/swamp)
    ///   river/lake World.CoastAngleAt / LakeDirectionAt finds adjacent water
    /// </summary>
    public class IncidentWorker_ReptileWandersIn : IncidentWorker_WildManWandersIn
    {
        /// <summary>Species-by-habitat: which wild kinds can appear on which tiles.</summary>
        private static bool HabitatMatches(PawnKindDef kind, RimWorld.Planet.Tile tile, Map map)
        {
            switch (kind.defName)
            {
                case "PMM_WildBasilisk": return HasCaves(map) || IsDesert(tile);
                case "PMM_WildLamia": return HasCaves(map) || IsMountain(tile);
                case "PMM_WildMedusa": return HasCaves(map);
                case "PMM_WildWurm": return IsWetland(tile) || IsMountain(tile) || HasCaves(map);
                case "PMM_WildDragon": return HasCaves(map) || IsMountain(tile);
                case "PMM_WildLizardman": return HasCaves(map);
                case "PMM_WildWyvern": return HasCaves(map) || IsMountain(tile);
                case "PMM_WildSalamander": return HasCaves(map) || IsVolcanic(tile);
                case "PMM_WildBunyip": return IsRiverOrLake(map);
                default: return false;
            }
        }

        // ----- habitat checks (all read the world tile / world, not the map) -----

        private static bool HasCaves(Map map) => Find.World.HasCaves(map.Tile);

        private static bool IsMountain(RimWorld.Planet.Tile tile) =>
            tile != null && tile.hilliness == Hilliness.Mountainous;

        private static bool IsDesert(RimWorld.Planet.Tile tile)
        {
            if (tile?.PrimaryBiome == null)
            {
                return false;
            }
            string b = tile.PrimaryBiome.defName;
            return b == "Desert" || b == "AridShrubland" || b == "ExtremeDesert";
        }

        private static bool IsVolcanic(RimWorld.Planet.Tile tile) =>
            tile?.PrimaryBiome != null && tile.PrimaryBiome.defName == "LavaField"; // Odyssey biome

        private static bool IsWetland(RimWorld.Planet.Tile tile) =>
            tile?.PrimaryBiome != null && tile.PrimaryBiome.defName == "TropicalSwamp";

        private static bool IsRiverOrLake(Map map)
        {
            // Lakes: the world reports a lake direction. Rivers/coasts: a water coast
            // angle. Either counts as "river and lake tiles" for the bunyip.
            World world = Find.World;
            return world.LakeDirectionAt(map.Tile) != Rot4.Invalid
                || world.CoastAngleAt(map.Tile, BiomeDefOf.Ocean).HasValue
                || world.CoastAngleAt(map.Tile, BiomeDefOf.Lake).HasValue;
        }

        // The slime/elemental pattern: skip the vanilla SeasonAcceptableFor(Human)
        // check (it gates on the CURRENT seasonal temperature being within a human's
        // comfy range 16-26C, which hard-blocks reptiles on hot maps — exactly where
        // desert basilisks and volcanic salamanders live) and the former-faction
        // requirement (wild reptiles are factionless creatures, not ex-faction wild
        // people). Keep the sensible environmental gates and log every refusal, so a
        // silent non-fire is never silent again.
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                Log.Message($"[PMM_Reptiles] {def.defName} blocked: target is not a map");
                return false;
            }
            if (map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
            {
                Log.Message($"[PMM_Reptiles] {def.defName} blocked: toxic fallout active");
                return false;
            }
            if (ModsConfig.BiotechActive && map.GameConditionManager.ConditionIsActive(GameConditionDefOf.NoxiousHaze))
            {
                Log.Message($"[PMM_Reptiles] {def.defName} blocked: noxious haze active");
                return false;
            }

            RimWorld.Planet.Tile tile = Find.WorldGrid[map.Tile];
            List<PawnKindDef> viable = ViableKinds(tile, map);
            if (viable.Count == 0)
            {
                Log.Message($"[PMM_Reptiles] {def.defName} blocked: no candidate species matches this tile " +
                    $"(biome={tile?.PrimaryBiome?.defName ?? "null"}, hilliness={tile?.hilliness}, caves={HasCaves(map)})");
                return false; // no candidate species lives on this tile
            }
            if (!CellFinder.TryFindRandomEdgeCellWith(
                    c => map.reachability.CanReachColony(c),
                    map, CellFinder.EdgeRoadChance_Ignore, out _))
            {
                Log.Message($"[PMM_Reptiles] {def.defName} blocked: no edge cell can reach the colony");
                return false;
            }
            return true;
        }

        private List<PawnKindDef> ViableKinds(RimWorld.Planet.Tile tile, Map map)
        {
            var viable = new List<PawnKindDef>();
            ReptileWildExtension ext = def.GetModExtension<ReptileWildExtension>();
            if (ext?.candidates == null)
            {
                return viable;
            }
            foreach (string kindName in ext.candidates)
            {
                PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(kindName);
                if (kind != null && HabitatMatches(kind, tile, map))
                {
                    viable.Add(kind);
                }
            }
            return viable;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!CellFinder.TryFindRandomEdgeCellWith(
                    c => map.reachability.CanReachColony(c),
                    map, CellFinder.EdgeRoadChance_Ignore, out IntVec3 cell))
            {
                Log.Message($"[PMM_Reptiles] {def.defName} execute failed: no edge cell can reach the colony");
                return false;
            }

            RimWorld.Planet.Tile tile = Find.WorldGrid[map.Tile];
            List<PawnKindDef> viable = ViableKinds(tile, map);
            if (viable.Count == 0)
            {
                Log.Message($"[PMM_Reptiles] {def.defName} execute failed: candidates lost between CanFireNowSub and spawn");
                return false;
            }
            PawnKindDef kind = viable.RandomElement();

            // Wild reptiles are factionless: request a FACTIONLESS pawn from the start
            // (the slime-mod pattern). The earlier version passed a random non-colony
            // faction as "former faction flavour" the way vanilla's WildMan does, then
            // stripped it after generation — but the faction is not flavour here. In this
            // world the only eligible factions are Dragonia and the Scalebound Broods, and
            // PawnGenerator generates the pawn AS a faction member (gear, ideo, relations,
            // tech-appropriate setup); SetFaction(null) afterwards then rips her out of a
            // pawn-group/lord context vanilla assumes is consistent, which is the silent
            // spawn failure. No faction, nothing to tear down.
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                kind, null, PawnGenerationContext.NonPlayer, map.Tile,
                forceGenerateNewPawn: false, allowDead: false, allowDowned: false,
                canGeneratePawnRelations: true, mustBeCapableOfViolence: false,
                colonistRelationChanceFactor: 1f, forceAddFreeWarmLayerIfNeeded: false,
                allowGay: true, allowPregnant: true, allowFood: true, allowAddictions: true,
                inhabitant: false, certainlyBeenInCryptosleep: false,
                forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false,
                biocodeWeaponChance: 0f, biocodeApparelChance: 0f,
                extraPawnForExtraRelationChance: null, relationWithExtraPawnChanceFactor: 1f,
                validatorPreGear: null, validatorPostGear: null, forcedTraits: null,
                prohibitedTraits: null, minChanceToRedressWorldPawn: null,
                fixedBiologicalAge: null, fixedChronologicalAge: null, fixedGender: null,
                fixedLastName: null, fixedBirthName: null, fixedTitle: null,
                fixedIdeo: null, forceNoIdeo: false, forceNoBackstory: false,
                forbidAnyTitle: false, forceDead: false, forcedXenogenes: null,
                forcedEndogenes: null, forcedXenotype: null, forcedCustomXenotype: null,
                allowedXenotypes: null, forceBaselinerChance: 0f,
                developmentalStages: DevelopmentalStage.Adult,
                forceNoGear: false));

            // Defensive only: with a null faction request she should already be
            // factionless. Pawn.SetFaction logs a warning (popping the debug log) when
            // the new faction equals the current one, so only clear if generation
            // somehow assigned one anyway.
            if (pawn.Faction != null)
            {
                pawn.SetFaction(null, null);
            }
            GenSpawn.Spawn(pawn, cell, map);

            // Mark as already "reached outside" so she lingers instead of marching
            // to the map edge and despawning (the slime-mod pattern; the
            // Patch_ReptileShouldNotReachOutside postfix below is the belt-and-braces
            // cover for the re-run every think tick).
            if (pawn.mindState != null)
            {
                pawn.mindState.WildManEverReachedOutside = true;
            }

            string kindLabel = pawn.KindLabel;
            TaggedString letterText = def.letterText.Formatted(kindLabel.Named("1"), pawn.Named("PAWN"))
                .AdjustedFor(pawn)
                .CapitalizeFirst();
            TaggedString letterLabel = def.letterLabel.Formatted(kindLabel.Named("0")).CapitalizeFirst();
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref letterText, ref letterLabel, pawn);
            SendStandardLetter(letterLabel, letterText, def.letterDef, parms, pawn);
            Log.Message($"[PMM_Reptiles] {def.defName} spawned {kind.defName} '{pawn.Name?.ToStringShort}' at {cell} (biome={tile?.PrimaryBiome?.defName}, hilliness={tile?.hilliness}, caves={HasCaves(map)})");
            return true;
        }
    }

    /// <summary>
    /// Wild reptile momos are tamed like wild men. The slime-mod pattern: report the
    /// pawn as a wild man for taming purposes when it carries a reptile wild kind.
    /// </summary>
    [HarmonyLib.HarmonyPatch(typeof(WildManUtility), nameof(WildManUtility.IsWildMan))]
    public static class Patch_ReptileIsWildMan
    {
        public static void Postfix(Pawn p, ref bool __result)
        {
            if (!__result && p?.kindDef != null && !p.IsSubhuman
                && p.kindDef.defName.StartsWith("PMM_Wild"))
            {
                __result = true;
            }
        }
    }

    /// <summary>
    /// Stop wild reptile momos from marching to the map edge and despawning the
    /// instant they spawn. Vanilla <see cref="WildManUtility.WildManShouldReachOutsideNow"/>
    /// returns true for any wild man who hasn't "reached outside", which makes the
    /// pawn walk to the nearest edge and leave. Setting WildManEverReachedOutside at
    /// spawn only covers the spawn tick — the check re-runs every think tick, and a
    /// reptile that spawns in an unseen corner is never arrested/tamed before the
    /// walk begins, so she just walks off. Report wild reptiles as already reached
    /// outside so the edge-walk never triggers (the slime-mod Patch_SlimeShouldNotReachOutside
    /// pattern, which the reptile port had missed).
    /// </summary>
    [HarmonyLib.HarmonyPatch(typeof(WildManUtility), nameof(WildManUtility.WildManShouldReachOutsideNow))]
    public static class Patch_ReptileShouldNotReachOutside
    {
        public static void Postfix(Pawn p, ref bool __result)
        {
            if (__result && p?.kindDef != null && p.kindDef.defName.StartsWith("PMM_Wild"))
            {
                __result = false;
            }
        }
    }
}
