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
    /// exactly the mother's endogenes and her xenotype, is not flagged hybrid, and
    /// is always a girl. Mirrors BabyXenotypeInheritancePatch and
    /// BabyCustomXenotypeBirthPatch in the core mod.
    /// </summary>
    public static class MomoEggCompat
    {
        /// <summary>The hatcher currently generating a baby, or null.</summary>
        public static VEF.Genes.CompHumanHatcher ActiveHatcher;
    }

    [HarmonyPatch(typeof(VEF.Genes.CompHumanHatcher), nameof(VEF.Genes.CompHumanHatcher.Hatch))]
    public static class Patch_HumanHatcher_Bracket
    {
        public static void Prefix(VEF.Genes.CompHumanHatcher __instance)
        {
            MomoEggCompat.ActiveHatcher = __instance;
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

            // Full maternal endogenes: remove the father's contributions, add any
            // of the mother's genes the hatch mix dropped.
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
                        baby.genes.AddGene(def, true);
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
}
