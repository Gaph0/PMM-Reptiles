using RimWorld;
using Verse;
using Verse.AI;

namespace PMM_Reptiles
{
    /// <summary>
    /// Casts one hostile ability on the best attack target the pawn can see.
    ///
    /// Vanilla's <c>JobGiver_AICastAbility</c> is abstract, and both subclasses it ships are
    /// hardcoded to a single job: <c>JobGiver_AICastAbilityOnSelf</c> targets the caster, and
    /// <c>JobGiver_AICastAnimalWarcall</c> looks for a wild animal. That is why no XML in the
    /// game uses the generic class - it cannot be built from XML, so the AI has nothing that will
    /// cast an arbitrary hostile ability. This is that missing piece.
    ///
    /// The think-tree node sets <c>abilityDef</c>; <c>Ability.AICanTargetNow</c> has the final say
    /// on the target, so the ability's own rules (aiCanUse, range, target parameters, cooldown)
    /// still govern whether the cast happens.
    /// </summary>
    public class JobGiver_AICastHostileAbility : JobGiver_AICastAbility
    {
        /// <summary>
        /// The ability to cast. Public on purpose: the base class keeps its own copy of this in a
        /// protected field, and this field is the one the think-tree XML fills in.
        /// </summary>
        public AbilityDef abilityDef;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (abilityDef == null || pawn?.abilities == null)
            {
                return null;
            }

            Ability ability = pawn.abilities.GetAbility(abilityDef);
            if (ability == null)
            {
                return null;
            }

            LocalTargetInfo target = ChooseTarget(pawn, ability);
            if (!target.IsValid)
            {
                return null;
            }

            return ability.GetJob(target, target);
        }

        protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            return ChooseTarget(caster, ability);
        }

        private static LocalTargetInfo ChooseTarget(Pawn caster, Ability ability)
        {
            float range = ability.verb != null ? ability.verb.verbProps.range : 0f;
            if (range < 5f)
            {
                // A melee or self-ranged ability should still be worth walking a few cells for.
                range = 5f;
            }

            IAttackTarget found = AttackTargetFinder.BestAttackTarget(
                caster,
                TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat | TargetScanFlags.NeedLOSToAll,
                null,
                0f,
                range);

            if (found?.Thing == null)
            {
                return LocalTargetInfo.Invalid;
            }

            LocalTargetInfo target = new LocalTargetInfo(found.Thing);
            return ability.AICanTargetNow(target) ? target : LocalTargetInfo.Invalid;
        }
    }
}
