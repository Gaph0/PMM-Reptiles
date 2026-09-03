using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Reptile factions only settle mountains — never hills (locked ruling:
    /// LargeHills are off-theme). Vanilla offers no XML hook for per-faction
    /// settlement tile filters, so this postfix re-rolls the result of
    /// TileFinder.RandomSettlementTileFor for our two factions until the tile is
    /// Mountainous (chaining the caller's own extraValidator, so every other
    /// vanilla rule still applies). 300 re-rolls × TileFinder's own 500 candidates
    /// ≈ 150k samples, which finds a mountain in any world that has them at all.
    /// TileFinder can never throw on failure — it logs and returns PlanetTile(0) —
    /// so even a mountainless world degrades gracefully rather than soft-locking.
    /// Overhead-mountain map areas (caves) come free on mountainous tiles, which
    /// is why the "mountains and caves" ruling reduces to a hilliness check.
    ///
    /// Both overloads are patched, since which one a caller uses is unspecified.
    /// A reentrancy flag keeps the postfix's own re-roll calls (which pass
    /// through the patched method again) from recursing.
    /// </summary>
    public static class MountainSettlement
    {
        private static bool rerolling;

        public static bool IsReptileFaction(Faction faction)
        {
            FactionDef def = faction?.def;
            return def == ReptileDefOf.PMM_DragoniaFaction
                || def == ReptileDefOf.PMM_ScaleboundBroodsFaction;
        }

        private static bool IsMountain(PlanetTile tile)
        {
            if (tile == PlanetTile.Invalid)
            {
                return false;
            }
            Tile worldTile = Find.WorldGrid[tile];
            return worldTile != null && worldTile.hilliness == Hilliness.Mountainous;
        }

        /// <summary>
        /// Re-roll through <paramref name="invoke"/> (the same overload the caller
        /// used) with a mountains-only validator chained onto the caller's
        /// validator. Returns PlanetTile.Invalid only when no mountain could be
        /// found at all.
        /// </summary>
        public static PlanetTile Reroll(Func<Predicate<PlanetTile>, PlanetTile> invoke,
            Predicate<PlanetTile> extraValidator)
        {
            rerolling = true;
            try
            {
                for (int i = 0; i < 300; i++)
                {
                    PlanetTile candidate = invoke(
                        t => IsMountain(t) && (extraValidator == null || extraValidator(t)));
                    if (candidate != PlanetTile.Invalid)
                    {
                        return candidate;
                    }
                }
                return PlanetTile.Invalid;
            }
            finally
            {
                rerolling = false;
            }
        }

        public static void PostfixImpl(Faction faction, Func<Predicate<PlanetTile>, PlanetTile> invoke,
            Predicate<PlanetTile> extraValidator, ref PlanetTile result)
        {
            if (rerolling || !IsReptileFaction(faction))
            {
                return;
            }
            if (result != PlanetTile.Invalid && IsMountain(result))
            {
                return; // already mountainous
            }
            PlanetTile mountain = Reroll(invoke, extraValidator);
            if (mountain != PlanetTile.Invalid)
            {
                result = mountain;
                if (Prefs.DevMode)
                {
                    Log.Message($"[PMM Reptiles] Moved {faction.def.defName} settlement to {HillinessOf(mountain)} tile {mountain}");
                }
            }
            else if (Prefs.DevMode)
            {
                Log.Warning($"[PMM Reptiles] No mountain tile found for {faction.def.defName}; keeping vanilla tile {result}");
            }
        }

        private static Hilliness HillinessOf(PlanetTile tile)
        {
            Tile worldTile = Find.WorldGrid[tile];
            return worldTile?.hilliness ?? Hilliness.Undefined;
        }
    }

    [HarmonyPatch(typeof(TileFinder), nameof(TileFinder.RandomSettlementTileFor),
        new[] { typeof(PlanetLayer), typeof(Faction), typeof(bool), typeof(Predicate<PlanetTile>) })]
    public static class Patch_RandomSettlementTileFor_Layer
    {
        public static void Postfix(PlanetLayer layer, Faction faction, bool mustBeAutoChoosable,
            Predicate<PlanetTile> extraValidator, ref PlanetTile __result)
        {
            MountainSettlement.PostfixImpl(faction,
                v => TileFinder.RandomSettlementTileFor(layer, faction, mustBeAutoChoosable, v),
                extraValidator, ref __result);
        }
    }

    [HarmonyPatch(typeof(TileFinder), nameof(TileFinder.RandomSettlementTileFor),
        new[] { typeof(Faction), typeof(bool), typeof(Predicate<PlanetTile>) })]
    public static class Patch_RandomSettlementTileFor_Faction
    {
        public static void Postfix(Faction faction, bool mustBeAutoChoosable,
            Predicate<PlanetTile> extraValidator, ref PlanetTile __result)
        {
            MountainSettlement.PostfixImpl(faction,
                v => TileFinder.RandomSettlementTileFor(faction, mustBeAutoChoosable, v),
                extraValidator, ref __result);
        }
    }
}
