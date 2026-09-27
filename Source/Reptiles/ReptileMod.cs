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
        public static ThingDef PMM_ReptileEggFertilized;
        public static ThingDef PMM_DarkDragonsBlood;

        // Dragon orbs (2026-09-22): the calling orb the dragonian traders sell, and
        // the glowing orb it becomes once a dragon has answered it.
        public static ThingDef PMM_DragonOrb;
        public static ThingDef PMM_DragonOrbDecor;

        // The wedding collar (2026-09-23): the jewelry that nullifies a dragon's pride, and
        // the hediff that carries its mood and its trait swap.
        public static ThingDef PMM_WeddingCollar;
        public static HediffDef PMM_Hediff_WeddingCollar;
        public static GeneDef PMM_Gene_WeddingCollar;
        public static ThoughtDef PMM_Thought_FreshScales;
        public static AbilityDef PMM_Ability_TailGrapple;
        public static AbilityDef PMM_Ability_Petrify;
        public static FactionDef PMM_DragoniaFaction;
        public static FactionDef PMM_ScaleboundBroodsFaction;

        // Xenotypes the Malef corruption switches between (XENOTYPES.md §5).
        public static XenotypeDef PMM_Reptile_Dragon;
        public static XenotypeDef PMM_Reptile_MalefDragon;
        public static XenotypeDef PMM_Reptile_Dragonewt;

        // Skin materials (four since 2026-09-22, was one per species). Bunyip wool
        // is its own material: the bunyip is a special reptile (user ruling).
        public static ThingDef PMM_Scale_Dragon;
        public static ThingDef PMM_Scale_Lamia;
        public static ThingDef PMM_Scale_Lizardman;
        public static ThingDef PMM_Scale_Bunyip;

        // The eleven species races (Defs/ThingDefs/Races_ReptileMamono.xml, 2026-09-20).
        // Each xenotype names its race through BigAndSmall.XenotypeExtension.setRace.
        public static ThingDef PMM_Race_Basilisk;
        public static ThingDef PMM_Race_Lamia;
        public static ThingDef PMM_Race_Medusa;
        public static ThingDef PMM_Race_Wurm;
        public static ThingDef PMM_Race_Bunyip;
        public static ThingDef PMM_Race_Dragon;
        public static ThingDef PMM_Race_Wyvern;
        public static ThingDef PMM_Race_MalefDragon;
        public static ThingDef PMM_Race_Lizardman;
        public static ThingDef PMM_Race_Dragonewt;
        public static ThingDef PMM_Race_Salamander;

        static ReptileDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ReptileDefOf));
        }
    }
}
