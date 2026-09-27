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
    /// pattern). Without the Broods there is no ambush at all - medusas stay a
    /// cave wander-in (the locked fallback).
    ///
    /// Ruins are matched by defName; a missing DLC's names simply never resolve. Two
    /// kinds count. World objects are Core's AbandonedSettlement and AbandonedCamp,
    /// Odyssey's AbandonedLandmark and Ideology's AbandonedArchotechStructures. Odyssey's
    /// landmarks are NOT world objects - they are tile mutators whose defName matches the
    /// landmark's - so they are matched against a tile's mutator list, on the caravan's
    /// tile or within RuinProximityTiles of it.
    /// Landmark maps are left undressed: Odyssey draws its own art there, and our broken
    /// walls and stone victims on top of an ancient garrison read as clutter (user ruling
    /// 2026-09-24), so MedusaRuinFill still keys on world objects only.
    /// </summary>
    public class IncidentWorker_MedusaRuinsAmbush : IncidentWorker_Ambush
    {
        /// <summary>
        /// Ruin world objects, by exact defName: Core's AbandonedSettlement and
        /// AbandonedCamp, Odyssey's AbandonedLandmark, Ideology's
        /// AbandonedArchotechStructures. The old comment here said Core + Odyssey and
        /// missed Ideology's entirely - corrected 2026-09-24.
        /// </summary>
        private static readonly string[] RuinDefNames =
            { "AbandonedSettlement", "AbandonedCamp", "AbandonedLandmark", "AbandonedArchotechStructures" };

        /// <summary>
        /// Odyssey landmark names, matched against a tile's mutator list rather than
        /// against world objects, because a landmark is a tile feature and the mutator
        /// carries the same defName as the landmark. The six ancient structures, plus
        /// AncientUplink - which has no LandmarkDef at all, only a mutator, a prefab and a
        /// thing - plus Odyssey's two terrain ruins. The three vents (heat, smoke, toxic)
        /// are deliberately absent: hazards, not ruins (user ruling 2026-09-24).
        /// </summary>
        private static readonly string[] LandmarkNames =
        {
            "AncientGarrison", "AncientChemfuelRefinery", "AncientInfestedSettlement",
            "AncientLaunchSite", "AncientQuarry", "AncientWarehouse", "AncientUplink",
            "Ruins", "FrozenRuins"
        };

        /// <summary>
        /// How close to a ruin the caravan must be, in world tiles. Kept at 5 for landmarks
        /// too, which makes a landmark ambush more likely rather than less (user ruling
        /// 2026-09-24).
        /// </summary>
        private const int RuinProximityTiles = 5;

        /// <summary>
        /// Exactly one, not the 1-3 this used to roll. She is a single strong threat - a
        /// melee fighter with a 7-day gaze - and two or three of them could petrify a whole
        /// caravan party while the owner watched (user ruling 2026-09-24). One medusa also
        /// means one petrify per ambush: the gaze takes one woman out for two days and the
        /// rest of the party has to cope.
        /// </summary>
        private const int AmbushMedusas = 1;

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
                return false; // no Broods faction - the cave wander-in fallback stands
            }
            return RuinOrLandmarkNear(caravan.Tile);
        }

        protected override List<Pawn> GeneratePawns(IncidentParms parms)
        {
            // The ambush map is generated one step after this, inside
            // CaravanIncidentUtility.SetupCaravanAttackMap. This flag tells the postfix
            // there (MedusaRuinFill) that the map about to be built is ours to dress -
            // world-object ruins only, since a landmark map already has Odyssey's art.
            MedusaRuinFill.Pending = RuinNear(parms.target.Tile) != null;
            Faction broods = Broods;
            parms.faction = broods; // the base class builds the lord from parms.faction
            PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamed("PMM_Reptile_WildMedusa");
            var pawns = new List<Pawn>(AmbushMedusas);
            for (int i = 0; i < AmbushMedusas; i++)
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

        /// <summary>
        /// The two strings below have a {0} in them, and the base returns them untouched,
        /// so {0} reached the player unresolved, with a literal {0} in the letter. Filling
        /// it with the caravan's name here is the whole fix (user's choice, 2026-09-24).
        /// Both hooks take (Pawn anyPawn, IncidentParms parms) and return a string.
        ///
        /// The fill is Formatted(), never Translate(). A def's letter strings are finished
        /// sentences, not translation keys, so Translate() misses its lookup - and with Dev
        /// Mode on that miss comes back through Translator.PseudoTranslated(), which is the
        /// accented "ĥàṡ ẉàŀķèḍ" text the owner reported in game (2026-09-24). Formatted()
        /// only substitutes the argument, and leaves the key alone. Vanilla fills its
        /// caravan ambush letter (IncidentWorker_Ambush_ManhunterPack) the same way.
        /// </summary>
        protected override string GetLetterLabel(Pawn anyPawn, IncidentParms parms)
        {
            return LetterWithCaravan(def.letterLabel, parms);
        }

        protected override string GetLetterText(Pawn anyPawn, IncidentParms parms)
        {
            return LetterWithCaravan(def.letterText, parms);
        }

        private static string LetterWithCaravan(string text, IncidentParms parms)
        {
            // Labelless, like the vanilla call: the {0} in the def resolves by position, so
            // a translation stays free to move it.
            NamedArgument caravanName = parms?.target is Caravan caravan && !caravan.Name.NullOrEmpty()
                ? new NamedArgument(caravan.Name, null)
                : new NamedArgument("yourCaravan".TranslateSimple(), null);
            return text.Formatted(caravanName);
        }

        protected override LordJob CreateLordJob(List<Pawn> generatedPawns, IncidentParms parms)
        {
            // She lies in wait among the stones - and strikes: an assault lord
            // drives the medusas at the caravan (the EnemyFaction ambush pattern).
            // canKidnap true: the Broods descend to claim men, and with it false the
            // ambushers had nothing to do at all once the party was downed - they do not
            // steal (canSteal) and an assault lord with no standing target just wanders
            // (the owner's report, 2026-09-24). They now carry the downed off the map.
            return new LordJob_AssaultColony(parms.faction, canKidnap: true, canTimeoutOrFlee: true, canSteal: false);
        }

        private static Faction Broods => Find.FactionManager.FirstFactionOfDef(ReptileDefOf.PMM_ScaleboundBroodsFaction);

        /// <summary>
        /// The ruin this tile is next to, if any. Also used by MedusaRuinFill's map
        /// postfix, which re-checks the real map tile before it decorates anything.
        /// </summary>
        public static WorldObject RuinNear(PlanetTile tile)
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

        /// <summary>
        /// A ruin by either measure: a ruin world object, or one of the landmark mutators
        /// on a tile within range. The incident gate and its knock-out test use this.
        /// </summary>
        public static bool RuinOrLandmarkNear(PlanetTile tile)
        {
            return RuinNear(tile) != null || LandmarkNear(tile);
        }

        /// <summary>
        /// Is a landmark mutator on this tile, or on any tile within RuinProximityTiles?
        /// Walked outward by rings through the world grid's neighbours rather than by
        /// scanning the planet, so the check stays cheap enough to run from an incident
        /// gate (a ring of depth 5 is around ninety tiles).
        /// </summary>
        public static bool LandmarkNear(PlanetTile tile)
        {
            WorldGrid grid = Find.WorldGrid;
            if (grid == null)
            {
                return false;
            }

            var seen = new HashSet<int> { tile.tileId };
            var frontier = new List<PlanetTile> { tile };

            for (int depth = 0; depth <= RuinProximityTiles; depth++)
            {
                var next = new List<PlanetTile>();
                foreach (PlanetTile current in frontier)
                {
                    if (TileHasLandmark(grid, current))
                    {
                        return true;
                    }
                    if (depth == RuinProximityTiles)
                    {
                        continue;
                    }
                    int count = grid.GetTileNeighborCount(current);
                    for (int i = 0; i < count; i++)
                    {
                        PlanetTile neighbor = grid.GetTileNeighbor(current, i);
                        if (seen.Add(neighbor.tileId))
                        {
                            next.Add(neighbor);
                        }
                    }
                }
                frontier = next;
            }
            return false;
        }

        /// <summary>Does this tile carry a landmark mutator we count as a ruin?</summary>
        private static bool TileHasLandmark(WorldGrid grid, PlanetTile tile)
        {
            IList<TileMutatorDef> mutators = grid[tile]?.Mutators;
            if (mutators == null)
            {
                return false;
            }
            for (int i = 0; i < mutators.Count; i++)
            {
                string name = mutators[i]?.defName;
                if (name != null && LandmarkNames.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
