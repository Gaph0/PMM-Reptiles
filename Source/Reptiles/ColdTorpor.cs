using RimWorld;
using UnityEngine;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Cold torpor for the reptile mamonos, and the end of their hypothermia deaths.
    ///
    /// Why this exists. The reptilian gene shifts the whole comfort band 10 degrees warmer, and
    /// vanilla's cold is harsher than that number sounds. A pawn's SAFE range is its COMFORTABLE
    /// range widened by 10 degrees either way (`GenTemperature.SafeTemperatureRange`), so a
    /// reptile is comfortable from 26 C up (human 16, +10) and in danger below 16 - and vanilla's
    /// hypothermia giver adds severity whenever the air is below that 16 while only taking it away
    /// above the 26 (`HediffGiver_Hypothermia`, read out of the 1.6 assembly). On nearly every map
    /// in the game the air sits between those two numbers all year, so a reptile mamono
    /// accumulated hypothermia she could never shed, and died of it. The frostbite injuries follow
    /// the same hediff: vanilla rolls for them below 0 C only while hypothermia is above 0.37.
    ///
    /// What replaces it. The gene is immune to Hypothermia (Genes_Reptile.xml), so the fatal
    /// hediff - and with it the frostbite path - can never land. This giver builds a torpor
    /// instead: she slows, dozes, and finally sleeps where she stands, and wakes when the air is
    /// warm again. Vanilla has the same idea for insectoid flesh, which gets `HypothermicSlowdown`
    /// - a hediff with capacity penalties and no lethality at all - where a human gets
    /// Hypothermia; ours simply carries the stages further and ends in sleep.
    ///
    /// The rates below are vanilla's own constants, kept deliberately: cold should still bite at
    /// the pace it always did, so only the outcome changes. The one real departure is the wake-up
    /// condition. Vanilla (and VRE's own copy of it) fades the cold only above the COMFORTABLE
    /// minimum - 26 C for a reptile, out of reach on most maps, which is what made the old
    /// hypothermia permanent. This fades as soon as she is back inside her SAFE range, so a warm
    /// afternoon undoes a cold night.
    ///
    /// Registered on vanilla's own OrganicStandard giver set, because a `GeneDef` has no field for
    /// a hediff giver and a gene therefore cannot bring its own; see
    /// Patches/ColdTorpor_HediffGivers.xml. The gene check in the first line is what keeps this
    /// off every other pawn in the game.
    /// </summary>
    public class HediffGiver_ColdTorpor : HediffGiver
    {
        /// <summary>Vanilla's build rate: severity per interval per degree below the safe range.</summary>
        private const float SeverityPerDegree = 6.45E-05f;

        /// <summary>Vanilla's floor, so even a barely-cold day builds something up.</summary>
        private const float MinSeverityPerInterval = 0.00075f;

        /// <summary>Vanilla's fade rate: a fraction of current severity, clamped both ways.</summary>
        private const float FadeFraction = 0.027f;

        private const float MinFadePerInterval = 0.0015f;
        private const float MaxFadePerInterval = 0.015f;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (pawn?.genes == null || !pawn.genes.HasActiveGene(ReptileDefOf.PMM_Gene_Reptile))
            {
                return;
            }

            float ambient = pawn.AmbientTemperature;
            float safeMinimum = pawn.SafeTemperatureRange().min;
            HediffSet hediffs = pawn.health.hediffSet;

            // Hypothermia cannot reach a reptile through the gene any more, but a pawn who had it
            // already - one loaded from an older save, or one whose genes changed under her -
            // would keep it, and it is still the hediff that kills. Clear it here, the same
            // belt-and-braces VRE's hibernation giver wears.
            Hediff hypothermia = hediffs.GetFirstHediffOfDef(HediffDefOf.Hypothermia);
            if (hypothermia != null)
            {
                pawn.health.RemoveHediff(hypothermia);
            }

            Hediff torpor = hediffs.GetFirstHediffOfDef(hediff);
            if (ambient < safeMinimum)
            {
                float added = Mathf.Max(Mathf.Abs(ambient - safeMinimum) * SeverityPerDegree, MinSeverityPerInterval);
                HealthUtility.AdjustSeverity(pawn, hediff, added);
                if (pawn.Dead)
                {
                    return;
                }
            }
            else if (torpor != null)
            {
                torpor.Severity -= Mathf.Clamp(torpor.Severity * FadeFraction, MinFadePerInterval, MaxFadePerInterval);
            }
        }
    }
}
