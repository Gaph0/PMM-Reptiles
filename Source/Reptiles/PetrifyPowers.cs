using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PMM_Reptiles
{
    public class CompProperties_AbilityPetrify : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityPetrify()
        {
            compClass = typeof(CompAbilityEffect_Petrify);
        }
    }

    /// <summary>
    /// Medusa's petrifying gaze: applies PMM_Hediff_Petrified to the target.
    /// Locked ruling (XENOTYPES.md §3): NOTHING is immune - any pawn can be
    /// petrified, mechanoids included; frozen pawns are simply downed for the
    /// duration (not carried, not statue-items). Target rules live in Valid; the
    /// vanilla ability system runs targeting, warmup and the 7-day cooldown.
    /// </summary>
    public class CompAbilityEffect_Petrify : CompAbilityEffect
    {
        /// <summary>2 days frozen.</summary>
        private const int PetrifyTicks = 120000;

        /// <summary>Sight a victim needs for the gaze to catch them.</summary>
        private const float MinSightToCatch = 0.2f;

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn victim = target.Pawn;
            if (victim == null || victim.Dead || victim == parent.pawn)
            {
                return false;
            }
            if (victim.health?.hediffSet?.GetFirstHediffOfDef(ReptileDefOf.PMM_Hediff_Petrified) != null)
            {
                return false; // already stone
            }
            if (!CanMeetGaze(victim))
            {
                if (throwMessages)
                {
                    // Names both of them: the point is that she cannot find his eyes.
                    Messages.Message(
                        "PMM_Reptiles_PetrifyNoEyes".Translate(parent.pawn.Named("MEDUSA"), victim.Named("PAWN")),
                        new LookTargets(victim), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            return base.Valid(target, throwMessages);
        }

        /// <summary>
        /// The AI path is separate from Valid, so the eye rule has to be repeated here
        /// - otherwise a raider medusa keeps choosing masked pawns and burns her 7-day
        /// cooldown on a gaze that cannot land.
        /// </summary>
        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            Pawn victim = target.Pawn;
            return victim != null && CanMeetGaze(victim) && base.AICanTargetNow(target);
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
            if (!CanMeetGaze(victim))
            {
                // Something changed between targeting and the cast - a helmet went on,
                // or the eyes were lost. Nothing for the stone to catch.
                return;
            }

            victim.jobs?.StopAll();

            Hediff_Petrified stone = (Hediff_Petrified)HediffMaker.MakeHediff(
                ReptileDefOf.PMM_Hediff_Petrified, victim);
            stone.ticksRemaining = PetrifyTicks;
            victim.health.AddHediff(stone);

            if (victim.Spawned)
            {
                FleckMaker.ThrowDustPuff(victim.Position, victim.Map, 1.6f);
            }
            Messages.Message("PMM_Reptiles_PetrifyCaught".Translate(victim.Named("VICTIM"), caster.Named("MEDUSA")),
                new LookTargets(victim), MessageTypeDefOf.NegativeEvent);
        }

        /// <summary>
        /// The gaze needs a working, uncovered eye. Both halves use the game's own
        /// measures rather than new defs: Sight is the capacity vanilla already uses for
        /// how much a pawn can see (a blinded or eyeless pawn sits at 0), and apparel
        /// coverage is the same body part group test the armor system runs against a
        /// headshot. The human eye parts carry the FullHead and Eyes groups, so a war
        /// mask or veil hides them while an open-faced helmet does not - vanilla's simple
        /// and advanced helmets are UpperHead.
        /// Ruling updated 2026-09-24: nothing is immune by species, but the stone needs an
        /// eye to catch.
        /// </summary>
        private static bool CanMeetGaze(Pawn victim)
        {
            if (victim?.health?.capacities == null)
            {
                return true; // nothing to judge by; the other checks still apply
            }
            if (victim.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < MinSightToCatch)
            {
                return false;
            }
            return !EyesCovered(victim);
        }

        /// <summary>Is any worn apparel covering the FullHead or Eyes groups?</summary>
        private static bool EyesCovered(Pawn victim)
        {
            Pawn_ApparelTracker apparel = victim.apparel;
            if (apparel == null || apparel.WornApparelCount == 0)
            {
                return false;
            }
            foreach (Apparel worn in apparel.WornApparel)
            {
                List<BodyPartGroupDef> groups = worn?.def?.apparel?.bodyPartGroups;
                if (groups == null)
                {
                    continue;
                }
                for (int i = 0; i < groups.Count; i++)
                {
                    BodyPartGroupDef group = groups[i];
                    if (group == BodyPartGroupDefOf.FullHead || group == BodyPartGroupDefOf.Eyes)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Turned to living stone. The XML stage downs the victim (Consciousness
    /// capped at 10% - the anesthesia precedent; Moving capped at 0 so pawns
    /// without consciousness, e.g. mechanoids, still drop). While the hediff is
    /// present the Need.IsFrozen postfix below holds every need (food, rest,
    /// joy, mood - all Need subclasses gate their NeedInterval on IsFrozen).
    /// After 2 days the timer runs out, a thaw letter/message fires, and
    /// ShouldRemove lets the health tracker remove the hediff between ticks -
    /// never mid-iteration (the Hediff_Constricted pattern).
    /// </summary>
    public class Hediff_Petrified : HediffWithComps
    {
        /// <summary>Ticks left until the stone lets go (2 days = 120000).</summary>
        public int ticksRemaining = 120000;

        private bool thawAnnounced;

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            ticksRemaining -= delta;
            if (ticksRemaining <= 0 && !thawAnnounced)
            {
                thawAnnounced = true;
                if (!pawn.Dead)
                {
                    LookTargets look = new LookTargets(pawn);
                    if (pawn.IsColonist || pawn.IsPrisonerOfColony || (pawn.Faction?.IsPlayer ?? false))
                    {
                        Find.LetterStack.ReceiveLetter(
                            "PMM_Reptiles_PetrifyThawedLabel".Translate(),
                            "PMM_Reptiles_PetrifyThawedText".Translate(pawn.Named("PAWN")),
                            LetterDefOf.NeutralEvent, look);
                    }
                    else if (pawn.Spawned)
                    {
                        Messages.Message("PMM_Reptiles_PetrifyThawedText".Translate(pawn.Named("PAWN")),
                            look, MessageTypeDefOf.PositiveEvent);
                    }
                }
            }
        }

        public override bool ShouldRemove => base.ShouldRemove || ticksRemaining <= 0;

        public override string TipStringExtra
        {
            get
            {
                string s = base.TipStringExtra;
                string time = "PMM_Reptiles_PetrifyThawsIn".Translate(ticksRemaining.ToStringTicksToPeriod());
                return s.NullOrEmpty() ? (string)time : s + "\n" + time;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 120000);
            Scribe_Values.Look(ref thawAnnounced, "thawAnnounced", false);
        }
    }

    /// <summary>
    /// The stasis half of petrification. In 1.6 every Need subclass's
    /// NeedInterval opens with an IsFrozen check (vanilla uses it for sleeping,
    /// dormancy and off-map pawns), so one postfix on the getter freezes food,
    /// rest, joy, mood and every other need at once. Only Need_Authority
    /// overrides the getter, and it is irrelevant here.
    /// (IsFrozen is protected in 1.6 - hence the string name, not nameof.)
    /// </summary>
    [HarmonyPatch(typeof(Need), "IsFrozen", MethodType.Getter)]
    public static class Need_IsFrozen_Petrified_Patch
    {
        /// <summary>Need.pawn is protected in 1.6 - cached field reference.
        /// The getter is called from every need's interval tick, so the extra
        /// hediff scan only runs while a pawn actually has needs ticking
        /// (and bails immediately when the need is already frozen).</summary>
        private static readonly AccessTools.FieldRef<Need, Pawn> NeedPawn =
            AccessTools.FieldRefAccess<Need, Pawn>("pawn");

        public static void Postfix(Need __instance, ref bool __result)
        {
            if (!__result
                && NeedPawn(__instance)?.health?.hediffSet?.GetFirstHediffOfDef(ReptileDefOf.PMM_Hediff_Petrified) != null)
            {
                __result = true;
            }
        }
    }
}
