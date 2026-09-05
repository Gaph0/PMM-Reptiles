using System.Collections.Generic;
using HarmonyLib;
using ProjectMomo;
using RimWorld;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// The Malef Dragon's voluntary corruption offer (the missing colony path).
    /// Core's "Offer transformation" (TransformProposalFloatMenuPatch) never
    /// reaches another momo — it breaks on EssenceTransfer.IsMomo(target) — and
    /// its outcome is always the PROPOSER's xenotype, so a Malef could never
    /// remake a woman by her own corruption rules. The raid knockout
    /// (MalefCorruption.cs) only fires in combat, so a colony Malef had no way
    /// to transform anyone at all.
    ///
    /// With a Malef selected, right-click a woman: baselines and other momos are
    /// offered "Offer transformation" (she rises a Dragonewt), a normal Dragon is
    /// offered "Corrupt into malef dragon" (she is blackened into a Malef).
    /// Dragonewts and Malefs are exempt (the result / the vector). The option
    /// greys out with core's reason when the pair can't propose. On acceptance
    /// the same core ceremony job runs; core's driver calls the
    /// VoluntaryTransformTargetOverride installed here to pick the outcome and
    /// uses ConvertXenotype for an already-monster target.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetFloatMenuOptions))]
    public static class MalefTransformOfferPatch
    {
        public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, Pawn __instance, Pawn selPawn)
        {
            foreach (FloatMenuOption opt in __result)
            {
                yield return opt;
            }

            Pawn target = __instance;
            if (selPawn == null || target == null || selPawn == target)
            {
                yield break;
            }
            if (!selPawn.Spawned || !target.Spawned || selPawn.Map != target.Map)
            {
                yield break;
            }
            if (!ProjectMomoModSettings.Settings.CorruptionEnabled
                || !ProjectMomoModSettings.Settings.VoluntaryCorruptionEnabled)
            {
                yield break;
            }

            // Only a Malef Dragon uses this path.
            if (selPawn.genes?.HasActiveGene(ReptileDefOf.PMM_Gene_MalefCorruption) != true)
            {
                yield break;
            }

            // The same ceremony as the consensual path: she must be upright.
            if (target.Downed)
            {
                yield break;
            }

            XenotypeDef outcome = MalefTransformRules.OutcomeFor(target);
            if (outcome == null)
            {
                yield break; // Dragonewt or already a Malef — nothing to offer
            }

            FloatMenuOption offer = BuildOption(selPawn, target, outcome);
            if (offer != null)
            {
                yield return offer;
            }
        }

        private static FloatMenuOption BuildOption(Pawn malef, Pawn target, XenotypeDef outcome)
        {
            if (!malef.Drafted && !malef.IsColonistPlayerControlled)
            {
                return null; // only let the player order their own pawns
            }

            string label = outcome == ReptileDefOf.PMM_MalefDragon
                ? "Corrupt into malef dragon"
                : "Offer transformation";
            if (!VoluntaryTransformation.CanProposeTo(malef, target, out string reason))
            {
                // Greyed out with core's reason so the player sees why not.
                return new FloatMenuOption(reason != null ? $"{label} ({reason})" : $"{label} (unavailable)", null);
            }

            return FloatMenuUtility.DecoratePrioritizedTask(
                new FloatMenuOption(label, () =>
                {
                    // Core re-validates, rolls the woman's acceptance, and orders
                    // the ceremony job onto the Malef; the outcome is decided by
                    // the override at the ceremony's end.
                    VoluntaryTransformation.TryPlayerOrderedProposal(malef, target);
                }),
                malef,
                target);
        }
    }

    /// <summary>
    /// The corruption outcome rules for a Malef's victim, shared by the offer
    /// above and the knockout path (MalefCorruption.cs), and installed as core's
    /// VoluntaryTransformTargetOverride so the ceremony applies the right
    /// xenotype instead of a copy of the proposer.
    /// </summary>
    public static class MalefTransformRules
    {
        /// <summary>What a Malef victim becomes, or null when she is exempt.</summary>
        public static XenotypeDef OutcomeFor(Pawn victim)
        {
            XenotypeDef current = victim?.genes?.Xenotype;
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

        /// <summary>
        /// Core's override hook: only a Malef's proposal changes the outcome;
        /// anyone else falls back to the default (the proposer's own xenotype).
        /// </summary>
        public static XenotypeDef OverrideFor(Pawn proposer, Pawn target)
        {
            if (proposer?.genes?.HasActiveGene(ReptileDefOf.PMM_Gene_MalefCorruption) == true)
            {
                return OutcomeFor(target);
            }
            return null;
        }
    }
}
