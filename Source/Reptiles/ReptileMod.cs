using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Entry point. Applies Harmony patches when the mod loads.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ReptileMod
    {
        static ReptileMod()
        {
            var harmony = new Harmony("PMM.Reptiles");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [DefOf]
    public static class ReptileDefOf
    {
        public static GeneDef PMM_Gene_Reptile;
        public static GeneDef PMM_Gene_LamiaTail;
        public static GeneDef PMM_Gene_Petrify;
        public static GeneDef PMM_Gene_MalefCorruption;
        public static HediffDef PMM_Hediff_Shedding;
        public static HediffDef PMM_Hediff_Constricted;
        public static HediffDef PMM_Hediff_Petrified;
        public static ThingDef PMM_ReptileScale;
        public static ThingDef PMM_ReptileEggFertilized;
        public static AbilityDef PMM_Ability_TailGrapple;
        public static AbilityDef PMM_Ability_Petrify;
        public static FactionDef PMM_DragoniaFaction;
        public static FactionDef PMM_ScaleboundBroodsFaction;

        // Xenotypes the Malef corruption switches between (XENOTYPES.md §5).
        public static XenotypeDef PMM_Dragon;
        public static XenotypeDef PMM_MalefDragon;
        public static XenotypeDef PMM_Dragonewt;

        // Per-species scale materials (locked Q5).
        public static ThingDef PMM_Scale_Basilisk;
        public static ThingDef PMM_Scale_Dragon;
        public static ThingDef PMM_Scale_Lamia;
        public static ThingDef PMM_Scale_Lizardman;
        public static ThingDef PMM_Scale_Medusa;
        public static ThingDef PMM_Scale_Wurm;
        public static ThingDef PMM_Scale_Wyvern;
        public static ThingDef PMM_Scale_MalefDragon;
        public static ThingDef PMM_Scale_Dragonewt;
        public static ThingDef PMM_Scale_Salamander;
        public static ThingDef PMM_Scale_Bunyip;

        static ReptileDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ReptileDefOf));
        }
    }
}
