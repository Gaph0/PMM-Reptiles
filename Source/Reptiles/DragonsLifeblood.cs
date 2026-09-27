using ProjectMamono;
using RimWorld;
using UnityEngine;
using Verse;

namespace PMM_Reptiles
{
    /// <summary>
    /// Dragon's Lifeblood, the strength-and-willpower half of the drink
    /// (Defs/ThingDefs/Item_DragonsLifeblood.xml). The alcohol, the tolerance and
    /// the chemical recreation are plain vanilla comps on the ThingDef; this doer
    /// only tops up the drinker's life energy.
    ///
    /// Which energy depends on who is drinking, and exactly one of the two needs
    /// ever exists on a pawn: the Mamono gene swaps essence out for mana, so a Mamono
    /// carries <see cref="Need_Mana"/> and an ordinary human carries
    /// <see cref="Need_Essence"/>. Both are checked, so the same flask serves a
    /// dragonian host and a human guest at her table.
    ///
    /// The amounts are set per drink in XML, so the balance is tunable without a
    /// rebuild.
    /// </summary>
    public class IngestionOutcomeDoer_DragonsLifeblood : IngestionOutcomeDoer
    {
        /// <summary>Mana restored per drink, as a fraction of a full bar.</summary>
        public float manaRefill = 0.5f;

        /// <summary>Essence restored per drink, as a fraction of a full bar.</summary>
        public float essenceRefill = 0.5f;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            if (pawn == null || pawn.Dead || pawn.needs == null)
            {
                return;
            }

            Refill(pawn.needs.TryGetNeed<Need_Mana>(), manaRefill);
            Refill(pawn.needs.TryGetNeed<Need_Essence>(), essenceRefill);
        }

        private static void Refill(Need need, float amount)
        {
            if (need == null || amount <= 0f)
            {
                return;
            }

            // Set through CurLevel: it is the public surface, and it clamps.
            need.CurLevel = Mathf.Min(need.MaxLevel, need.CurLevel + amount);
        }
    }

    /// <summary>
    /// The hidden "blood running hot" hediff a flask of Dragon's Lifeblood leaves
    /// behind. It draws no art and says nothing on the Health tab (Visible false is
    /// the shedding / slime-jelly pattern); its whole job is the vanilla
    /// <c>HediffCompProperties_GiveLovinMTBFactor</c> declared on its def, which
    /// <c>JobDriver_Lovin.GenerateRandomMinTicksToNextLovin</c> multiplies into the
    /// gap between lovin' - a factor below 1 means it comes around sooner. That is
    /// vanilla's own comp, so this needs no Harmony patch; the vanilla
    /// HediffCompProperties_SeverityPerDay on the def decays it away, and
    /// <c>Hediff.ShouldRemove</c> (severity &lt;= 0) retires it.
    /// </summary>
    public class Hediff_DragonsLifeblood : HediffWithComps
    {
        public override bool Visible => false;
    }
}
