using System.Collections.Generic;
using HarmonyLib;
using ProjectMomo;
using RimWorld;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Momo inheritance for egg births. VEF's CompHumanHatcher generates the hatched
    /// baby from its own stored mother/father gene mix via PawnGenerator, bypassing
    /// PregnancyUtility.GetInheritedGenes and ApplyBirthOutcome — the two seams the
    /// core mod's momo inheritance patches hook. Left alone, a momo's egg-born baby
    /// could come out a hybrid or male.
    ///
    /// These bracket patches flag while a hatch is generating its baby (Prefix sets,
    /// Finalizer clears — even on exception), and the GeneratePawn postfix applies
    /// the core rules when the egg's mother (hatcheeParent) is a momo: the baby gets
    /// exactly the mother's endogenes, keeps her xenotype, is not flagged hybrid, is
    /// always a girl, and carries no xenogenes - VEF's stored mix has to be dropped, or
    /// the baby inherits her mother's genes a second time as implants. Mirrors
    /// BabyXenotypeInheritancePatch and BabyCustomXenotypeBirthPatch in the core mod.
    /// </summary>
    public static class MomoEggCompat
    {
        /// <summary>The hatcher currently generating a baby, or null.</summary>
        public static VEF.Genes.CompHumanHatcher ActiveHatcher;

        /// <summary>The baby the current hatch generated, held until the hatch has finished
        /// placing her so her birth letter can point at a pawn who is actually on the map.
        /// Reset at the start of every hatch, so a hatch that threw can never leak one.</summary>
        public static Pawn PendingBirthLetter;
    }

    [HarmonyPatch(typeof(VEF.Genes.CompHumanHatcher), nameof(VEF.Genes.CompHumanHatcher.Hatch))]
    public static class Patch_HumanHatcher_Bracket
    {
        public static void Prefix(VEF.Genes.CompHumanHatcher __instance)
        {
            MomoEggCompat.ActiveHatcher = __instance;
            MomoEggCompat.PendingBirthLetter = null;
        }

        public static void Finalizer()
        {
            MomoEggCompat.ActiveHatcher = null;
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) })]
    public static class Patch_GeneratePawn_MomoEgg
    {
        public static void Postfix(ref Pawn __result)
        {
            VEF.Genes.CompHumanHatcher hatcher = MomoEggCompat.ActiveHatcher;
            if (hatcher == null || __result?.genes == null)
            {
                return;
            }

            Pawn mother = hatcher.hatcheeParent;
            if (!EssenceTransfer.IsMomo(mother) || mother.genes == null)
            {
                return;
            }

            Pawn baby = __result;

            // VEF's hatcher builds the baby out of the gene mix it stored at conception and
            // lays that mix down as XENOGENES. Left alone the baby carries the mother's genes
            // twice - once as her real endogenes (from the xenotype set below) and again as an
            // implant-like xenogene set - which a player sees as a duplicate row under
            // "Xenogenes" in the gene inspector. Momo genetics live in the endogenes, so the
            // whole mix goes; that is also what takes the father's contribution out.
            for (int i = baby.genes.Xenogenes.Count - 1; i >= 0; i--)
            {
                baby.genes.RemoveGene(baby.genes.Xenogenes[i]);
            }

            // Full maternal endogenes: remove anything the hatch mix contributed, then add back
            // every gene the mother carries. Added as ENDOGENES deliberately - passing
            // xenogene: true here is the other half of how the duplicate set appeared.
            List<GeneDef> motherGenes = mother.genes.Endogenes?.ConvertAll(g => g.def);
            if (motherGenes != null && motherGenes.Count > 0)
            {
                for (int i = baby.genes.Endogenes.Count - 1; i >= 0; i--)
                {
                    Gene g = baby.genes.Endogenes[i];
                    if (!motherGenes.Contains(g.def))
                    {
                        baby.genes.RemoveGene(g);
                    }
                }
                foreach (GeneDef def in motherGenes)
                {
                    if (baby.genes.GetGene(def) == null)
                    {
                        baby.genes.AddGene(def, false);
                    }
                }
            }

            // The baby keeps the mother's xenotype identity instead of "hybrid".
            baby.genes.hybrid = false;
            if (mother.genes.UniqueXenotype)
            {
                baby.genes.xenotypeName = mother.genes.xenotypeName;
                baby.genes.iconDef = mother.genes.iconDef;
            }
            else if (mother.genes.Xenotype != null)
            {
                baby.genes.SetXenotypeDirect(mother.genes.Xenotype);
            }

            // Momos are always female; their children are too.
            ForceFemale(baby);

            // Hand her to the hatch's own postfix, which runs once the hatcher has placed her
            // and can decide whether this baby is the colony's business (see
            // Patch_HumanHatcher_BirthLetter). Nothing is shown from here.
            MomoEggCompat.PendingBirthLetter = baby;
        }

        private static void ForceFemale(Pawn baby)
        {
            bool graphicsChanged = false;

            if (baby.gender != Gender.Female)
            {
                baby.gender = Gender.Female;
                graphicsChanged = true;
            }

            if (baby.story != null && baby.story.bodyType != BodyTypeDefOf.Female)
            {
                baby.story.bodyType = BodyTypeDefOf.Female;
                graphicsChanged = true;
            }

            if (graphicsChanged && baby.Spawned && baby.Drawer?.renderer != null)
            {
                baby.Drawer.renderer.SetAllGraphicsDirty();
            }
        }
    }

    /// <summary>
    /// Gives a hatched momo the popup a live birth gets. Vanilla's own ChoiceLetter_BabyBirth is
    /// what Biotech shows when a colony baby arrives: it carries the "name the baby" button and
    /// the status choice, and it opens itself. Vanilla creates it with LetterMaker.MakeLetter and
    /// LetterDefOf.BabyBirth - the def names the letter class - so reusing both means the popup
    /// our egg shows is the vanilla one, options and all, instead of an imitation of it. The
    /// letter's Start() resolves its own pawn from the look targets, which is why the baby is
    /// passed as a target rather than assigned by hand.
    ///
    /// Only a baby who belongs to the player's faction is lettered: a wild momo's egg is none of
    /// the colony's business. Runs after the hatch has placed her, so "jump to" works.
    /// </summary>
    [HarmonyPatch(typeof(VEF.Genes.CompHumanHatcher), nameof(VEF.Genes.CompHumanHatcher.Hatch))]
    public static class Patch_HumanHatcher_BirthLetter
    {
        public static void Postfix()
        {
            Pawn baby = MomoEggCompat.PendingBirthLetter;
            MomoEggCompat.PendingBirthLetter = null;
            if (baby?.Faction != Faction.OfPlayer || baby.RaceProps?.Humanlike != true)
            {
                return;
            }

            ChoiceLetter_BabyBirth letter = (ChoiceLetter_BabyBirth)LetterMaker.MakeLetter(
                "PMM_MomoEggHatchedLabel".Translate(),
                "PMM_MomoEggHatched".Translate(baby.Named("PAWN1")),
                LetterDefOf.BabyBirth,
                new LookTargets(baby));
            letter.Start();
            Find.LetterStack.ReceiveLetter(letter, null, 0, true);
        }
    }
}
