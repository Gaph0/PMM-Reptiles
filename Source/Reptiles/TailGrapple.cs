using ProjectMomo;
using RimWorld;
using Verse;

namespace PMM_Reptiles
{
    public class CompProperties_AbilityTailGrapple : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityTailGrapple()
        {
            compClass = typeof(CompAbilityEffect_TailGrapple);
        }
    }

    /// <summary>
    /// Tail grapple: drags the victim into the lamia's coils and applies the
    /// constricted hediff. Target rules live in Valid; the vanilla ability system
    /// runs targeting, warmup and cooldown.
    /// </summary>
    public class CompAbilityEffect_TailGrapple : CompAbilityEffect
    {
        /// <summary>Lamia tails can coil a human, not a thrumbo.</summary>
        private const float MaxBodySize = 2.5f;

        /// <summary>How long the constriction lasts (2500 ticks = one in-game hour).</summary>
        private const int ConstrictTicks = 2500;

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn victim = target.Pawn;
            if (victim == null || victim.Dead || victim == parent.pawn)
            {
                return false;
            }
            if (victim.RaceProps == null || victim.RaceProps.IsMechanoid)
            {
                return false; // no coils for cold steel
            }
            if (victim.BodySize > MaxBodySize)
            {
                return false;
            }
            if (victim.health?.hediffSet?.GetFirstHediffOfDef(ReptileDefOf.PMM_Hediff_Constricted) != null)
            {
                return false; // already wrapped
            }
            return base.Valid(target, throwMessages);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            Pawn victim = target.Pawn;
            if (caster?.Map == null || victim == null || victim.Dead)
            {
                return;
            }
            Map map = caster.Map;

            // Drag the victim into her coils: teleport them to a cell adjacent to
            // the lamia (the Dorome teleport, minus the mud).
            if (!victim.Position.AdjacentTo8Way(caster.Position))
            {
                IntVec3 spot = IntVec3.Invalid;
                foreach (IntVec3 c in GenAdj.CellsAdjacent8Way(new TargetInfo(caster)))
                {
                    if (c.InBounds(map) && c.Walkable(map))
                    {
                        spot = c;
                        break;
                    }
                }
                if (spot.IsValid)
                {
                    victim.jobs?.StopAll();
                    IntVec3 origin = victim.Position;
                    victim.DeSpawn();
                    GenSpawn.Spawn(victim, spot, map, victim.Rotation);
                    FleckMaker.ThrowDustPuff(origin, map, 1.2f);
                    FleckMaker.ThrowDustPuff(spot, map, 1.2f);
                }
                // No valid adjacent cell: the grapple simply fails to move them,
                // but her tail still finds them — constriction applies anyway.
            }

            victim.jobs?.StopAll();

            Hediff_Constricted constrict = (Hediff_Constricted)HediffMaker.MakeHediff(
                ReptileDefOf.PMM_Hediff_Constricted, victim);
            constrict.constrictor = caster;
            constrict.ticksRemaining = ConstrictTicks;
            victim.health.AddHediff(constrict);

            Messages.Message("PMM_Reptiles_GrappleCaught".Translate(victim.Named("VICTIM"), caster.Named("LAMIA")),
                new LookTargets(victim), MessageTypeDefOf.NegativeEvent);
        }
    }

    /// <summary>
    /// Wrapped in a lamia's coils. The XML stage roots the victim (Moving offset
    /// leaves 5%: held, but not downed — she can still fight back). This class
    /// ticks tease damage through the core mod's tease system (the same severity
    /// arithmetic as melee teasing, willpower knockout included) and releases the
    /// victim when the timer runs out, the constrictor is downed or killed, or the
    /// two are no longer adjacent. Removal goes through ShouldRemove, so the
    /// health tracker removes the hediff between ticks — never mid-iteration.
    /// </summary>
    public class Hediff_Constricted : HediffWithComps
    {
        /// <summary>The lamia holding this victim.</summary>
        public Pawn constrictor;

        /// <summary>Ticks left until she lets go.</summary>
        public int ticksRemaining = 2500;

        private int ticksSinceTease;

        /// <summary>Tease applied every 300 ticks.</summary>
        private const int TeaseInterval = 300;
        private const float TeasePerApplication = 0.04f;

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            ticksRemaining -= delta;

            ticksSinceTease += delta;
            if (ticksSinceTease >= TeaseInterval)
            {
                ticksSinceTease = 0;
                if (constrictor != null && !constrictor.Dead && !pawn.Dead)
                {
                    // The lamia is the "attacker" so the knockout and any XP credit her.
                    TeaseApplication.TryApplyTease(pawn, constrictor, TeasePerApplication);
                }
            }
        }

        public override bool ShouldRemove => base.ShouldRemove || ticksRemaining <= 0 || !ConstrictorValid();

        private bool ConstrictorValid()
        {
            return constrictor != null
                && !constrictor.Dead
                && !constrictor.Downed
                && constrictor.Spawned
                && constrictor.Map == pawn.Map
                && pawn.Position.AdjacentTo8Way(constrictor.Position);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref constrictor, "constrictor");
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 2500);
            Scribe_Values.Look(ref ticksSinceTease, "ticksSinceTease", 0);
        }
    }
}
