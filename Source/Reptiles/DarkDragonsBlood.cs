using System.Collections.Generic;
using ProjectMomo;
using RimWorld;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Dark Dragon's Blood: a vial of blackened dragon's blood. When a human
    /// baseliner or a normal Dragon drinks it, she is remade as a Malef Dragon.
    /// This is the ONLY way to create a Malef outside the Broods roster — the
    /// old tease-knockout corruption and the colony offer are gone (scrapped
    /// 2026-09-05 in favour of this item).
    ///
    /// A baseliner goes through ApplyXenotype (first corruption); a normal
    /// Dragon — already a monster — goes through ConvertXenotype (re-stamp).
    /// Dragonewts, Malefs, men, children and non-humans are unaffected: the
    /// blood simply doesn't take (CanEverTransform / the monster gates).
    /// </summary>
    public class IngestionOutcomeDoer_DarkDragonsBlood : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            if (pawn == null || pawn.Dead || pawn.genes == null)
            {
                return;
            }

            XenotypeDef current = pawn.genes.Xenotype;

            // Exempt: already the result (Malef) or the lesser dragon-kin (Dragonewt).
            if (current == ReptileDefOf.PMM_MalefDragon || current == ReptileDefOf.PMM_Dragonewt)
            {
                return;
            }

            if (current == ReptileDefOf.PMM_Dragon)
            {
                // A normal Dragon is blackened into her malef mirror (re-stamp —
                // ApplyXenotype refuses an already-monster pawn).
                MomoTransformation.ConvertXenotype(pawn, ReptileDefOf.PMM_MalefDragon, source: null);
                return;
            }

            // A human baseliner (or any non-dragon, non-exempt woman): the first
            // corruption. CanEverTransform gates out men, children, monsters and
            // the too-young.
            if (MomoTransformation.CanEverTransform(pawn))
            {
                MomoTransformation.ApplyXenotype(pawn, ReptileDefOf.PMM_MalefDragon, source: null);
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef ingested)
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Basics,
                "Transformation", "Malef dragon (women & dragons only)",
                "A human baseliner or a normal dragon who drinks Dark Dragon's Blood is remade as a malef dragon. Dragonewts, malef dragons, men, children and non-humans are unaffected.", 0);
        }
    }
}
