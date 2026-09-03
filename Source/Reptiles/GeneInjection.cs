using HarmonyLib;
using ProjectMomo;
using RimWorld;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Interim gene wiring for the faction phase. Xenotypes arrive in a later
    /// phase, and PawnKindDef.xenotypeSet only references xenotypes, so this
    /// postfix on pawn generation gives every pawn of our two factions the momo
    /// and reptilian genes (plus the lamia tail gene for coil sisters) as
    /// endogenes. The whole shim is deleted when the xenotype phase lands and
    /// the pawnkinds switch to xenotypeSet entries.
    ///
    /// Keys off PawnKindDef.defaultFactionDef, so future pawnkinds of these
    /// factions are covered automatically. All faction pawns are women (the
    /// pawnkinds pin fixedGender Female), as momos always are.
    /// </summary>
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) })]
    public static class Patch_GeneratePawn_ReptileGenes
    {
        public static void Postfix(PawnGenerationRequest request, Pawn __result)
        {
            Pawn pawn = __result;
            if (pawn?.genes == null || pawn.RaceProps == null || !pawn.RaceProps.Humanlike)
            {
                return;
            }
            PawnKindDef kind = request.KindDef;
            FactionDef factionDef = kind?.defaultFactionDef;
            if (factionDef != ReptileDefOf.PMM_DragoniaFaction
                && factionDef != ReptileDefOf.PMM_ScaleboundBroodsFaction)
            {
                return;
            }

            EnsureGene(pawn, ProjectMomo_DefOf.ProjectMomo_Momo);
            EnsureGene(pawn, ReptileDefOf.PMM_Gene_Reptile);
            if (kind == ReptileDefOf.PMM_BroodGrappler)
            {
                EnsureGene(pawn, ReptileDefOf.PMM_Gene_LamiaTail);
            }
        }

        private static void EnsureGene(Pawn pawn, GeneDef def)
        {
            if (def != null && pawn.genes.GetGene(def) == null)
            {
                pawn.genes.AddGene(def, false);
            }
        }
    }
}
