using RimWorld;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// The reptilian gene. The sharp-armour and warm-comfort offsets are pure XML
    /// stat offsets; egg-laying is the VEF egg-layer comp (see the gene's
    /// modExtension). This class adds and removes the hidden shedding hediff that
    /// drives the yearly scale drop (the slime-gel oozing pattern). IsReptile is
    /// the single check every reptile patch keys off.
    /// </summary>
    public class Gene_Reptile : Gene
    {
        /// <summary>True if the pawn has an active reptilian gene.</summary>
        public static bool IsReptile(Pawn pawn)
        {
            if (pawn?.genes == null)
            {
                return false;
            }
            Gene gene = pawn.genes.GetGene(ReptileDefOf.PMM_Gene_Reptile);
            return gene != null && gene.Active;
        }

        /// <summary>Start the shedding countdown when the gene is added.</summary>
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn?.health != null && !pawn.Dead &&
                pawn.health.hediffSet.GetFirstHediffOfDef(ReptileDefOf.PMM_Hediff_Shedding) == null)
            {
                pawn.health.AddHediff(ReptileDefOf.PMM_Hediff_Shedding);
            }
        }

        /// <summary>Stop shedding when the gene is removed.</summary>
        public override void PostRemove()
        {
            base.PostRemove();
            Hediff shed = pawn?.health?.hediffSet?.GetFirstHediffOfDef(ReptileDefOf.PMM_Hediff_Shedding);
            if (shed != null)
            {
                pawn.health.RemoveHediff(shed);
            }
        }
    }

    /// <summary>
    /// Hidden marker hediff for the yearly shed. Invisible on the Health tab and
    /// carries no effects besides the production comp (the slime-jelly-oozing
    /// pattern). The health tracker stops ticking at death, so corpses never shed.
    /// </summary>
    public class Hediff_Shedding : HediffWithComps
    {
        public override bool Visible => false;
    }

    public class HediffCompProperties_Shedding : HediffCompProperties
    {
        /// <summary>Days between sheds (60 = one RimWorld year).</summary>
        public float intervalDays = 60f;

        /// <summary>How many scales are dropped each shed.</summary>
        public IntRange scaleCount = new IntRange(10, 20);

        public HediffCompProperties_Shedding()
        {
            compClass = typeof(HediffComp_Shedding);
        }
    }

    /// <summary>
    /// Yearly shed: drops a stack of reptile scales at the pawn's feet. Only grown
    /// reptiles shed a workable hide — children skip the countdown entirely.
    /// </summary>
    public class HediffComp_Shedding : HediffComp
    {
        private int ticksUntilNextShed = -1;

        public HediffCompProperties_Shedding Props => (HediffCompProperties_Shedding)props;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            Pawn pawn = Pawn;
            if (pawn == null || pawn.Dead || !pawn.Spawned || pawn.Map == null)
            {
                return;
            }
            if (pawn.ageTracker != null && !pawn.ageTracker.Adult)
            {
                return;
            }

            if (ticksUntilNextShed < 0)
            {
                // First tick after being added: start a fresh countdown.
                ticksUntilNextShed = IntervalTicks;
            }

            ticksUntilNextShed -= delta;
            if (ticksUntilNextShed > 0)
            {
                return;
            }

            ticksUntilNextShed = IntervalTicks;
            DropScales(pawn);
        }

        private int IntervalTicks => (int)(Props.intervalDays * GenDate.TicksPerDay);

        /// <summary>
        /// The scale material this reptile sheds (locked Q5: per-species). Falls back
        /// to the generic PMM_ReptileScale for any reptile without a species entry
        /// (e.g. a reptile-gene human with no reptile xenotype).
        /// </summary>
        private ThingDef ScaleMaterialFor(Pawn pawn)
        {
            string xeno = pawn?.genes?.Xenotype?.defName;
            switch (xeno)
            {
                case "PMM_Basilisk": return ReptileDefOf.PMM_Scale_Basilisk;
                case "PMM_Dragon": return ReptileDefOf.PMM_Scale_Dragon;
                case "PMM_Lamia": return ReptileDefOf.PMM_Scale_Lamia;
                case "PMM_Lizardman": return ReptileDefOf.PMM_Scale_Lizardman;
                case "PMM_Medusa": return ReptileDefOf.PMM_Scale_Medusa;
                case "PMM_Wurm": return ReptileDefOf.PMM_Scale_Wurm;
                case "PMM_Wyvern": return ReptileDefOf.PMM_Scale_Wyvern;
                case "PMM_MalefDragon": return ReptileDefOf.PMM_Scale_MalefDragon;
                case "PMM_Dragonewt": return ReptileDefOf.PMM_Scale_Dragonewt;
                case "PMM_Salamander": return ReptileDefOf.PMM_Scale_Salamander;
                case "PMM_Bunyip": return ReptileDefOf.PMM_Scale_Bunyip;
                default: return ReptileDefOf.PMM_ReptileScale;
            }
        }

        private void DropScales(Pawn pawn)
        {
            Thing scales = ThingMaker.MakeThing(ScaleMaterialFor(pawn));
            scales.stackCount = Props.scaleCount.RandomInRange;

            IntVec3 cell = pawn.Position;
            if (!cell.Walkable(pawn.Map) && !CellFinder.TryFindRandomReachableNearbyCell(
                    cell, pawn.Map, 2f, TraverseParms.For(pawn), c => c.Walkable(pawn.Map), null, out cell))
            {
                cell = pawn.Position; // nowhere better: drop them under her anyway
            }
            GenPlace.TryPlaceThing(scales, cell, pawn.Map, ThingPlaceMode.Near);

            Messages.Message("PMM_Reptiles_ShedScales".Translate(pawn.Named("PAWN")),
                new LookTargets(scales), MessageTypeDefOf.PositiveEvent);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticksUntilNextShed, "ticksUntilNextShed", -1);
        }
    }
}
