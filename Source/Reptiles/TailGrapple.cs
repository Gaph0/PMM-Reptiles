using ProjectMomo;
using RimWorld;
using UnityEngine;
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
    /// victim when the timer runs out, the constrictor is downed or killed, or she
    /// leaves the map, or he wrenches free of his own accord. Neither proximity nor the
    /// victim being carried is a
    /// condition: the coil lasts an hour, and demanding adjacency meant a lamia who
    /// took one step lost her hold on the very next health tick (found 2026-09-24 -
    /// the grapple landed and the hediff was gone 18 ticks later, with 2482 of its
    /// 2500 ticks still on the clock). The same-map test had the same flaw: a victim
    /// she picks up to carry off is despawned while carried, so the coil died the
    /// instant a kidnapping began.
    /// Removal goes through ShouldRemove, so the
    /// health tracker removes the hediff between ticks — never mid-iteration.
    /// </summary>
    public class Hediff_Constricted : HediffWithComps
    {
        /// <summary>The lamia holding this victim.</summary>
        public Pawn constrictor;

        /// <summary>Ticks left until she lets go.</summary>
        public int ticksRemaining = 2500;

        private int ticksSinceTease;

        /// <summary>Ticks since the last escape roll, so they come every 312.</summary>
        private int ticksSinceEscapeCheck;

        /// <summary>
        /// Tease applied every 60 ticks (one second). 0.045 a second against the core's
        /// 0.5/day fade is about +0.00074 severity per tick, so a fresh victim's will
        /// gives out - severity 0.9, the point where willpower hits the 0.1 threshold -
        /// after roughly 1150 ticks, a bit under half an in-game hour. That is the point
        /// of the cadence: at the old 0.04 every 300 ticks a fresh man needed about 2.7
        /// hours of unbroken coiling, so he could never be broken inside one coil.
        /// </summary>
        private const int TeaseInterval = 60;
        private const float TeasePerApplication = 0.045f;

        /// <summary>Escape is rolled every eighth of an in-game hour (312 ticks).</summary>
        private const int EscapeCheckTicks = 312;

        /// <summary>Baseline escape chance before STR is compared: 10%.</summary>
        private const float EscapeBaseChance = 0.10f;

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            ticksRemaining -= delta;

            ticksSinceEscapeCheck += delta;
            if (ticksSinceEscapeCheck >= EscapeCheckTicks)
            {
                ticksSinceEscapeCheck = 0;
                TryEscape();
            }

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

        /// <summary>
        /// The victim's chance to wrench free, rolled every eighth of an hour: a flat
        /// 10% plus every percent of STR he has over her. A man of 12 against her 10 is
        /// 10% + 20% = 30%; a weaker man still gets the 10%. STR comes from Isekai via
        /// IsekaiCompat, which gives mobs the base value, so a raider lamia's own STR is
        /// her floor rather than nothing. Eight rolls fit inside one coil, so an even
        /// match gets out about 57% of the time and a stronger man almost always does.
        /// </summary>
        private void TryEscape()
        {
            Pawn victim = pawn;
            if (victim == null || victim.Dead || constrictor == null || constrictor.Dead)
            {
                return;
            }

            int manStr = IsekaiCompat.Strength(victim);
            int momoStr = IsekaiCompat.Strength(constrictor);
            float advantage = momoStr > 0 ? Mathf.Max(0f, (manStr - momoStr) / (float)momoStr) : 0f;
            float chance = Mathf.Clamp01(EscapeBaseChance + advantage);

            if (Rand.Chance(chance))
            {
                if (victim.Spawned)
                {
                    Messages.Message(
                        "PMM_Reptiles_CoilEscaped".Translate(victim.Named("VICTIM"), constrictor.Named("LAMIA")),
                        new LookTargets(victim), MessageTypeDefOf.PositiveEvent);
                }
                // Let go on the next health tick - ShouldRemove, never mid-iteration.
                ticksRemaining = 0;
            }
        }

        public override bool ShouldRemove => base.ShouldRemove || ticksRemaining <= 0 || !ConstrictorValid();

        /// <summary>
        /// She is still holding on: alive, on her feet, and still on the map. Two
        /// things this deliberately does NOT test, both learned the hard way on
        /// 2026-09-24: adjacency (one step breaks it), and whether the victim is
        /// spawned on her map - a victim she has just picked up in order to carry him
        /// off is despawned, and that test used to delete the coil mid-kidnapping.
        /// </summary>
        private bool ConstrictorValid()
        {
            return constrictor != null
                && !constrictor.Dead
                && !constrictor.Downed
                && constrictor.Spawned;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref constrictor, "constrictor");
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 2500);
            Scribe_Values.Look(ref ticksSinceTease, "ticksSinceTease", 0);
            Scribe_Values.Look(ref ticksSinceEscapeCheck, "ticksSinceEscapeCheck", 0);
        }
    }
}
