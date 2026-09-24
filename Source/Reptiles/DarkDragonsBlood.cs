using System.Collections.Generic;
using ProjectMomo;
using RimWorld;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Dark Dragon's Blood: a vial of blackened dragon's blood. It works in two
    /// steps, and between them they are the ONLY way to create a Malef outside the
    /// Broods roster — the old tease-knockout corruption and the colony offer are
    /// gone (scrapped 2026-09-05 in favour of this item).
    ///
    ///   1. An ordinary woman — any woman who could still be corrupted — drinks it
    ///      and rises as a Dragonewt (ApplyXenotype, the first corruption).
    ///   2. A Dragonewt drinks it and is blackened into a Malef Dragon, and a
    ///      normal Dragon skips straight there on her first vial. Both are already
    ///      monsters, so this step goes through ConvertXenotype (the re-stamp path;
    ///      ApplyXenotype refuses a monster).
    ///
    /// A Malef Dragon is the end of the chain: the blood has nothing left to give
    /// her. It does nothing for men, children or non-humans either
    /// (CanEverTransform / the monster gates).
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

            // The end of the chain: a Malef Dragon has nothing left to become.
            if (current == ReptileDefOf.PMM_Reptile_MalefDragon)
            {
                return;
            }

            // Step two: a Dragonewt and a normal Dragon both end as a malef mirror
            // of the dark queen who bled into the vial. Both are already monsters,
            // so this is the re-stamp path (ApplyXenotype refuses a monster).
            if (current == ReptileDefOf.PMM_Reptile_Dragon || current == ReptileDefOf.PMM_Reptile_Dragonewt)
            {
                MomoTransformation.ConvertXenotype(pawn, ReptileDefOf.PMM_Reptile_MalefDragon, source: null);
                return;
            }

            // Step one: an ordinary woman of any xenotype rises as a dragonewt.
            // CanEverTransform gates out men, children, monsters and the too-young,
            // and doubles as the re-transformation guard.
            if (MomoTransformation.CanEverTransform(pawn))
            {
                MomoTransformation.ApplyXenotype(pawn, ReptileDefOf.PMM_Reptile_Dragonewt, source: null);
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef ingested)
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Basics,
                "Transformation", "Dragonewt, then malef dragon",
                "An ordinary woman who drinks Dark Dragon's Blood is remade as a dragonewt; a second vial blackens her into a malef dragon. A normal dragon is blackened by her first vial. Malef dragons, men, children and non-humans are unaffected.", 0);
        }
    }
}
