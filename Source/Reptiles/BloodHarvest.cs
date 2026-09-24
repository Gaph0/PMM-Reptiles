using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Harvest a Malef Dragon's blood: an operation that draws a vial of Dark
    /// Dragon's Blood from a living Malef, at the cost of half her blood. Modelled
    /// on vanilla's Recipe_ExtractHemogen (itself a Recipe_Surgery): ApplyOnPawn
    /// adds the BloodLoss hediff, OnSurgerySuccess places the vial. Only valid on
    /// a living Malef (carrier of PMM_Gene_MalefCorruption) with enough blood left
    /// to give — the vanilla "extraction would kill her" guard, so the operation
    /// is refused rather than fatal.
    /// </summary>
    public class Recipe_HarvestDarkBlood : Recipe_Surgery
    {
        /// <summary>Blood taken per harvest (severity of the BloodLoss hediff).</summary>
        private const float BloodLossSeverity = 0.5f;

        /// <summary>Refuse the harvest when it would push her blood loss to lethal.</summary>
        private const float LethalBloodLoss = 0.95f;

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return thing is Pawn pawn
                && pawn.genes?.HasActiveGene(ReptileDefOf.PMM_Gene_MalefCorruption) == true
                && pawn.health?.CanBleed == true
                && base.AvailableOnNow(thing, part);
        }

        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            if (thing is Pawn pawn && !HasEnoughBlood(pawn))
            {
                return "PMM_Reptiles_NotEnoughBlood".Translate(pawn.Named("PAWN"));
            }
            return base.AvailableReport(thing, part);
        }

        public override bool CompletableEver(Pawn surgeryTarget)
        {
            return base.CompletableEver(surgeryTarget) && HasEnoughBlood(surgeryTarget);
        }

        public override void CheckForWarnings(Pawn medPawn)
        {
            base.CheckForWarnings(medPawn);
            if (!HasEnoughBlood(medPawn))
            {
                Messages.Message("PMM_Reptiles_NotEnoughBlood".Translate(medPawn.Named("PAWN")),
                    new LookTargets(medPawn), MessageTypeDefOf.NeutralEvent);
            }
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (!HasEnoughBlood(pawn))
            {
                Messages.Message("PMM_Reptiles_HarvestFailedNoBlood".Translate(pawn.Named("PAWN")),
                    new LookTargets(pawn), MessageTypeDefOf.NeutralEvent);
                return;
            }

            // Draw the blood: half her supply, as a BloodLoss hediff.
            Hediff bloodLoss = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn);
            bloodLoss.Severity = BloodLossSeverity;
            pawn.health.AddHediff(bloodLoss);

            // OnSurgerySuccess is the virtual hook, and vanilla's Recipe_ExtractHemogen calls it
            // exactly like this from its own ApplyOnPawn. Neither obvious alternative works:
            // base.ApplyOnPawn never reaches it (vanilla's own extraction made no pack that way),
            // and base.OnSurgerySuccess goes to the empty base and skips our override. Both were
            // tried in game on 2026-09-23 and produced no vial.
            OnSurgerySuccess(pawn, part, billDoer, ingredients, bill);
        }

        protected override void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.OnSurgerySuccess(pawn, part, billDoer, ingredients, bill);

            Thing vial = ThingMaker.MakeThing(ReptileDefOf.PMM_DarkDragonsBlood);
            if (!GenPlace.TryPlaceThing(vial, pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near))
            {
                Log.Error($"[PMM_Reptiles] Could not place Dark Dragon's Blood from {pawn}");
            }
        }

        /// <summary>Current blood loss plus the draw stays below the lethal line.</summary>
        private static bool HasEnoughBlood(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return false;
            }
            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            float current = existing?.Severity ?? 0f;
            return current + BloodLossSeverity < LethalBloodLoss;
        }
    }
}
