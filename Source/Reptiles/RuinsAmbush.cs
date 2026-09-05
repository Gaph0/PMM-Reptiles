using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace PMM_Reptiles
{
    /// <summary>
    /// Medusa ruins ambush (XENOTYPES.md §4a, locked): when a player caravan
    /// travels near a ruin world object, medusas lie in wait among the stones.
    /// Built on the vanilla IncidentWorker_Ambush flow: for a caravan target the
    /// base DoExecute generates the ambush map (CaravanIncidentUtility.
    /// SetupCaravanAttackMap), spawns the pawns GeneratePawns returns, and gives
    /// them the lord CreateLordJob builds.
    ///
    /// The ambushers are wild-medusa kinds flying Broods colours (medusas are on
    /// the Broods roster): factionless pawns would just wander the map instead
    /// of ambushing, and the base class builds the lord from parms.faction, so
    /// GeneratePawns stamps the faction into the parms (the EnemyFaction
    /// pattern). Without the Broods there is no ambush at all — medusas stay a
    /// cave wander-in (the locked fallback).
    ///
    /// Ruins are matched by defName so Core (AbandonedSettlement) and Odyssey
    /// (AbandonedCamp, AbandonedLandmark) ruins both count; a missing DLC's
    /// names simply never resolve. (Ruin-map pre-population — medusas LIVING in
    /// the ruin — is the later, heavier phase.)
    /// </summary>
    public class IncidentWorker_MedusaRuinsAmbush : IncidentWorker_Ambush
    {
        /// <summary>Ruin world-object defNames (Core + Odyssey).</summary>
        private static readonly string[] RuinDefNames = { "AbandonedSettlement", "AbandonedCamp", "AbandonedLandmark" };

        /// <summary>How close to a ruin the caravan must be, in world tiles.</summary>
        private const int RuinProximityTiles = 5;

        private const int MinMedusas = 1;
        private const int MaxMedusas = 3;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            if (!(parms.target is Caravan caravan))
            {
                return false; // ruins ambushes only stalk caravans
            }
            if (Broods == null)
            {
                return false; // no Broods faction — the cave wander-in fallback stands
            }
            return RuinNear(caravan.Tile) != null;
        }

        protected override List<Pawn> GeneratePawns(IncidentParms parms)
        {
            Faction broods = Broods;
            parms.faction = broods; // the base class builds the lord from parms.faction
            PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamed("PMM_WildMedusa");
            int count = Rand.RangeInclusive(MinMedusas, MaxMedusas);
            var pawns = new List<Pawn>(count);
            for (int i = 0; i < count; i++)
            {
                pawns.Add(PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    kind, broods, PawnGenerationContext.NonPlayer, parms.target.Tile,
                    forceGenerateNewPawn: true, allowDead: false, allowDowned: false,
                    canGeneratePawnRelations: false, mustBeCapableOfViolence: true,
                    colonistRelationChanceFactor: 0f, forceAddFreeWarmLayerIfNeeded: false,
                    allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true,
                    developmentalStages: DevelopmentalStage.Adult)));
            }
            return pawns;
        }

        protected override LordJob CreateLordJob(List<Pawn> generatedPawns, IncidentParms parms)
        {
            // She lies in wait among the stones — and strikes: an assault lord
            // drives the medusas at the caravan (the EnemyFaction ambush pattern).
            return new LordJob_AssaultColony(parms.faction, canKidnap: false, canTimeoutOrFlee: true, canSteal: false);
        }

        private static Faction Broods => Find.FactionManager.FirstFactionOfDef(ReptileDefOf.PMM_ScaleboundBroodsFaction);

        private static WorldObject RuinNear(PlanetTile tile)
        {
            WorldGrid grid = Find.WorldGrid;
            foreach (WorldObject obj in Find.WorldObjects.AllWorldObjects)
            {
                if (obj?.def == null || !RuinDefNames.Contains(obj.def.defName))
                {
                    continue;
                }
                if (grid.ApproxDistanceInTiles(tile, obj.Tile) <= RuinProximityTiles)
                {
                    return obj;
                }
            }
            return null;
        }
    }
}
