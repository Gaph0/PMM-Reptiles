using System.Collections.Generic;
using HarmonyLib;
using ProjectMamono;
using RimWorld;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// The wedding collar (2026-09-23). Dragonia's gift for a married dragon: a collar she
    /// wears with pride because it means she found a man worth submitting to. In game terms,
    /// while it is worn it nullifies the greedy and arrogant traits, gives her the kind and
    /// masochistic ones, and lifts her mood a little.
    ///
    /// It is worn exactly like vanilla's slave collar (Same bodyPartGroup and layer), but it
    /// works the other way round: the vanilla collar suppresses its wearer, and this one is a
    /// wedding gift she wants to wear.
    ///
    /// How the three effects are done:
    ///
    ///  * MOOD is a situational thought (Thoughts_WeddingCollar.xml), not code - a hediff stage
    ///    has no mood field in this build.
    ///  * GREEDY and ARROGANT are suppressed by the collar's own gene (Genes_WeddingCollar.xml),
    ///    which is the only way to do it: `suppressedTraits` exists on GeneDef alone, and the
    ///    engine reads it for both the trait checks and the greyed-out rows on the Social tab.
    ///    Nothing is ever removed from the pawn, so the whole effect lifts the second the
    ///    collar comes off, and a save cannot be left holding a half-changed pawn.
    ///  * KIND and MASOCHISTIC are really added, not reported, because the trait list the
    ///    player reads has to agree with the behaviour. The comp records which of the two the
    ///    collar itself granted, so an unlucky woman who was already masochistic keeps hers
    ///    when the collar comes off.
    ///
    /// The hediff and the gene are the two halves of "is she wearing one": both are added and
    /// removed by Sync, which is idempotent and called from dressing, undressing and spawning
    /// (so loading a save repairs any drift).
    /// </summary>
    public static class WeddingCollar
    {
        /// <summary>Pawn_ApparelTracker keeps its pawn private.</summary>
        private static readonly AccessTools.FieldRef<Pawn_ApparelTracker, Pawn> TrackerOwner =
            AccessTools.FieldRefAccess<Pawn_ApparelTracker, Pawn>("pawn");

        /// <summary>
        /// Masochist is a vanilla trait, but this build's TraitDefOf has no entry for it, so it
        /// is looked up by name. Null only if a mod renames or removes it - both callers guard.
        /// </summary>
        public static TraitDef Masochist => DefDatabase<TraitDef>.GetNamedSilentFail("Masochist");

        public static Pawn OwnerOf(Pawn_ApparelTracker tracker) => tracker == null ? null : TrackerOwner(tracker);

        /// <summary>True while the pawn wears a wedding collar.</summary>
        public static bool Worn(Pawn pawn)
        {
            List<Apparel> worn = pawn?.apparel?.WornApparel;
            if (worn == null)
            {
                return false;
            }
            for (int i = 0; i < worn.Count; i++)
            {
                if (worn[i].def == ReptileDefOf.PMM_WeddingCollar)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Brings the collar's effect into line with what the pawn is wearing. Idempotent, so
        /// every hook that could change either side can just call it.
        ///
        /// Two things move together: the hediff (mood row plus the kind/masochistic swap) and
        /// the gene (which suppresses greedy and arrogant). The gene is the half that makes the
        /// traits grey out - see Genes_WeddingCollar.xml for why that has to be a gene.
        /// </summary>
        public static void Sync(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null || pawn.genes == null)
            {
                return;
            }
            bool worn = Worn(pawn);
            Hediff collar = pawn.health.hediffSet.GetFirstHediffOfDef(ReptileDefOf.PMM_Hediff_WeddingCollar);
            Gene collarGene = pawn.genes.GetGene(ReptileDefOf.PMM_Gene_WeddingCollar);
            if (worn)
            {
                if (collarGene == null)
                {
                    pawn.genes.AddGene(ReptileDefOf.PMM_Gene_WeddingCollar, xenogene: true);
                }
                if (collar == null)
                {
                    pawn.health.AddHediff(ReptileDefOf.PMM_Hediff_WeddingCollar);
                    PMMLog.Message($"[PMM_Reptiles] wedding collar: {pawn.LabelShort} put one on");
                }
                return;
            }
            if (collar == null && collarGene == null)
            {
                return;
            }
            // Hand the traits back before the hediff goes: the record of what to return lives
            // in its comp, so it has to be read while the hediff still exists.
            (collar as HediffWithComps)?.GetComp<HediffComp_WeddingCollar>()?.RestoreTraits();
            if (collar != null)
            {
                pawn.health.RemoveHediff(collar);
            }
            if (collarGene != null)
            {
                pawn.genes.RemoveGene(collarGene);
            }
            PMMLog.Message($"[PMM_Reptiles] wedding collar: {pawn.LabelShort} took hers off");
        }
    }

    public class HediffCompProperties_WeddingCollar : HediffCompProperties
    {
        public HediffCompProperties_WeddingCollar()
        {
            compClass = typeof(HediffComp_WeddingCollar);
        }
    }

    /// <summary>
    /// Hands the wearer the kind and masochistic traits while the collar is on, and returns
    /// exactly the ones the collar added when it comes off.
    /// </summary>
    public class HediffComp_WeddingCollar : HediffComp
    {
        public bool addedKind;
        public bool addedMasochist;

        /// <summary>
        /// Keeps the collar off the Health tab. The hediff is bookkeeping plus the mood, and the
        /// mood is read on the Needs tab as a thought, so a health row saying "wedding collar"
        /// would sit next to real ailments as clutter. The gene row is the visible trace for
        /// anyone who looks, and a GeneDef cannot be hidden at all (nowhere in the def schema),
        /// so the gene is where the explanation belongs.
        /// </summary>
        public override bool CompDisallowVisible() => true;

        public override void CompPostMake() => AddTraits();

        public override void CompPostPostRemoved() => RestoreTraits();

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref addedKind, "addedKind");
            Scribe_Values.Look(ref addedMasochist, "addedMasochist");
        }

        private void AddTraits()
        {
            Pawn pawn = Pawn;
            if (pawn?.story?.traits == null)
            {
                return;
            }
            if (!pawn.story.traits.HasTrait(TraitDefOf.Kind))
            {
                pawn.story.traits.GainTrait(new Trait(TraitDefOf.Kind, 0));
                addedKind = true;
            }
            TraitDef masochist = WeddingCollar.Masochist;
            if (masochist != null && !pawn.story.traits.HasTrait(masochist))
            {
                pawn.story.traits.GainTrait(new Trait(masochist, 0));
                addedMasochist = true;
            }
        }

        /// <summary>Gives back exactly the traits this comp handed out, and no others.</summary>
        public void RestoreTraits()
        {
            Pawn pawn = Pawn;
            if (pawn?.story?.traits == null)
            {
                return;
            }
            if (addedKind)
            {
                pawn.story.traits.RemoveTrait(new Trait(TraitDefOf.Kind, 0));
                addedKind = false;
            }
            TraitDef masochist = WeddingCollar.Masochist;
            if (addedMasochist && masochist != null)
            {
                pawn.story.traits.RemoveTrait(new Trait(masochist, 0));
                addedMasochist = false;
            }
        }
    }

    // The old Patch_WeddingCollarNullifies lived here: a postfix on TraitSet.HasTrait that
    // reported greedy and arrogant absent while a collar was worn. It muted the traits but
    // could never grey them out, because the trait list reads the pawn's real traits - and
    // suppressedTraits exists only on GeneDef (checked against the generated def schema), so
    // the engine's own route won (Genes_WeddingCollar.xml). Deleted rather than kept: two
    // mechanisms for one rule is a bug waiting to happen, and this one was also the source of
    // two static-constructor crashes.

    /// <summary>
    /// The collar's small lift in mood (Thoughts_WeddingCollar.xml). No state to keep: the
    /// thought is active exactly while a collar is worn.
    /// </summary>
    public class ThoughtWorker_WeddingCollar : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p) =>
            WeddingCollar.Worn(p) ? ThoughtState.ActiveAtStage(0) : ThoughtState.Inactive;
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelAdded))]
    public static class Patch_WeddingCollarPutOn
    {
        public static void Postfix(Pawn_ApparelTracker __instance) =>
            WeddingCollar.Sync(WeddingCollar.OwnerOf(__instance));
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelRemoved))]
    public static class Patch_WeddingCollarTakenOff
    {
        public static void Postfix(Pawn_ApparelTracker __instance) =>
            WeddingCollar.Sync(WeddingCollar.OwnerOf(__instance));
    }

    /// <summary>Spawning covers loading a save, where the collar is already on.</summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class Patch_WeddingCollarOnSpawn
    {
        public static void Postfix(Pawn __instance) => WeddingCollar.Sync(__instance);
    }
}
