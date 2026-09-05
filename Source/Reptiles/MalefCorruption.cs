using HarmonyLib;
using ProjectMomo;
using RimWorld;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Malef corruption (XENOTYPES.md §5, locked): when a Malef Dragon downs a
    /// female human through the tease/willpower knockout, the victim is remade —
    /// a baseline or non-dragon momo rises as a Dragonewt, a normal Dragon is
    /// blackened into a Malef Dragon. Dragonewts do NOT transform (they are the
    /// result, not the vector) and Malefs are already malef.
    ///
    /// Hook: TeaseKnockout.Evaluate runs on every tease-severity change with the
    /// attacker in scope. The prefix snapshots whether the victim already carries
    /// the knockout hediff; the postfix only acts on the FRESH knockout (this
    /// call is the one that broke her will), so a Malef standing near an
    /// already-broken victim never re-triggers, and recovery ticks (attacker
    /// null) can't either.
    /// </summary>
    [HarmonyPatch(typeof(TeaseKnockout), nameof(TeaseKnockout.Evaluate))]
    public static class TeaseKnockout_MalefCorruption_Patch
    {
        /// <summary>True when the victim was already will-broken before this evaluation.</summary>
        public static void Prefix(Pawn pawn, out bool __state)
        {
            __state = pawn?.health?.hediffSet?.GetFirstHediffOfDef(
                ProjectMomo_DefOf.ProjectMomo_WillpowerBreak) != null;
        }

        public static void Postfix(Pawn pawn, Pawn attacker, bool __state)
        {
            // Only a fresh knockout counts: she was up before, broken now.
            if (__state || pawn == null || pawn.Dead || attacker == null)
            {
                return;
            }
            if (pawn.health?.hediffSet?.GetFirstHediffOfDef(ProjectMomo_DefOf.ProjectMomo_WillpowerBreak) == null)
            {
                return; // this evaluation didn't break her
            }
            if (attacker.genes?.HasActiveGene(ReptileDefOf.PMM_Gene_MalefCorruption) != true)
            {
                return; // not a Malef Dragon
            }

            XenotypeDef target = CorruptionTargetFor(pawn);
            if (target == null)
            {
                return;
            }

            if (EssenceTransfer.IsMomo(pawn))
            {
                // Already a monster: the swap path (CanEverTransform refuses her).
                MomoTransformation.ConvertXenotype(pawn, target, attacker);
            }
            else if (MomoTransformation.CanEverTransform(pawn))
            {
                // Baseline woman: the full first-corruption path (genes, thoughts,
                // join roll when the Malef is a colonist).
                MomoTransformation.ApplyXenotype(pawn, target, attacker);
            }
        }

        /// <summary>The xenotype a Malef victim becomes, or null when she is exempt.</summary>
        private static XenotypeDef CorruptionTargetFor(Pawn victim)
        {
            XenotypeDef current = victim.genes?.Xenotype;
            if (current == ReptileDefOf.PMM_MalefDragon || current == ReptileDefOf.PMM_Dragonewt)
            {
                return null; // already the vector or the result
            }
            if (current == ReptileDefOf.PMM_Dragon)
            {
                return ReptileDefOf.PMM_MalefDragon; // a dragon is blackened
            }
            return ReptileDefOf.PMM_Dragonewt; // baselines and other momos alike
        }
    }
}
