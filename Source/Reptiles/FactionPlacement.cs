using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Reptile factions only settle mountains. Vanilla offers no XML hook for
    /// per-faction settlement tile filters, so this postfix re-rolls the result
    /// of TileFinder.RandomSettlementTileFor for our two factions until the tile
    /// is Mountainous (chaining the caller's own extraValidator, so every other
    /// vanilla rule still applies). After 30 failed tries it relaxes to
    /// LargeHills, and after 30 more it accepts the original result — worldgen
    /// must never soft-lock. Overhead-mountain map areas (caves) come free on
    /// mountainous tiles, which is why the "mountains and caves" ruling reduces
    /// to a hilliness check.
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

        private static bool HillinessOk(PlanetTile tile, bool strict)
        {
            if (tile == PlanetTile.Invalid)
            {
                return false;
            }
            Tile worldTile = Find.WorldGrid[tile];
            if (worldTile == null)
            {
                return false;
            }
            return strict
                ? worldTile.hilliness == Hilliness.Mountainous
                : worldTile.hilliness >= Hilliness.LargeHills;
        }

        /// <summary>
        /// Re-roll through <paramref name="invoke"/> (the same overload the caller
        /// used) with a hilliness validator chained onto the caller's validator.
        /// Returns PlanetTile.Invalid when even the relaxed pass found nothing.
        /// </summary>
        public static PlanetTile Reroll(Func<Predicate<PlanetTile>, PlanetTile> invoke,
            Predicate<PlanetTile> extraValidator)
        {
            rerolling = true;
            try
            {
                for (int i = 0; i < 30; i++)
                {
                    PlanetTile candidate = invoke(
                        t => HillinessOk(t, true) && (extraValidator == null || extraValidator(t)));
                    if (candidate != PlanetTile.Invalid)
                    {
                        return candidate;
                    }
                }
                for (int i = 0; i < 30; i++)
                {
                    PlanetTile candidate = invoke(
                        t => HillinessOk(t, false) && (extraValidator == null || extraValidator(t)));
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
            if (result != PlanetTile.Invalid && HillinessOk(result, false))
            {
                return; // already in the hills or better
            }
            PlanetTile mountain = Reroll(invoke, extraValidator);
            if (mountain != PlanetTile.Invalid)
            {
                result = mountain;
            }
            // Nothing found at all: keep the original tile rather than break worldgen.
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
