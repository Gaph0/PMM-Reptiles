using ProjectMamono;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Puts every reptile mamono corpse under the family's shared "mamono corpses" line.
    ///
    /// The category def and all of the moving live in the core mod
    /// (ProjectMamono.MamonoCorpses, Defs/ThingCategoryDefs/ThingCategories_MamonoCorpses.xml), so
    /// the insects, the slimes, the elementals and the reptiles share one line instead of one
    /// line each. See MamonoCorpses for why a corpse's category cannot be set in XML.
    ///
    /// This only works because the eleven species now have race defs of their own
    /// (Defs/ThingDefs/Races_ReptileMamono.xml). Until 2026-09-20 a reptile pawn was a vanilla
    /// Human pawn wearing a xenotype, so her corpse was an ordinary human corpse - the same
    /// def as every human in the game - and could not be told apart from one.
    ///
    /// The races come from ReptileDefOf rather than from strings, so renaming a race def fails
    /// the build instead of quietly dropping that species off the line.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ReptileMamonoCorpses
    {
        static ReptileMamonoCorpses()
        {
            MamonoCorpses.Register(
                ReptileDefOf.PMM_Race_Basilisk.defName,
                ReptileDefOf.PMM_Race_Lamia.defName,
                ReptileDefOf.PMM_Race_Medusa.defName,
                ReptileDefOf.PMM_Race_Wurm.defName,
                ReptileDefOf.PMM_Race_Bunyip.defName,
                ReptileDefOf.PMM_Race_Dragon.defName,
                ReptileDefOf.PMM_Race_Wyvern.defName,
                ReptileDefOf.PMM_Race_MalefDragon.defName,
                ReptileDefOf.PMM_Race_Lizardman.defName,
                ReptileDefOf.PMM_Race_Dragonewt.defName,
                ReptileDefOf.PMM_Race_Salamander.defName);
        }
    }
}
