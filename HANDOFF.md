# PMM Reptiles - Agent Hand-off

For the next agent picking this up. What exists, why it was built that way, and
what's left. Read `PLAN.md` (phase 1 design + locked rulings) and `XENOTYPES.md`
(phase 2 roster + locked rulings) alongside this.

## 0. The mod in one paragraph

Project Mamono Reptiles adds reptile mamonos to RimWorld as a sub-mod of the core
Project Mamono. Two mountain factions (neutral **Dragonia**, hostile lewdist
**Scalebound Broods**), a **reptilian gene** (sharp hide, warmth-loving, egg-laying,
yearly scale-shedding), a **lamia tail-grapple** gene, and eleven **xenotypes**
(Basilisk, Dragon, Lamia, Lizardman, Medusa, Wurm, Wyvern, Malef Dragon, Dragonewt,
Salamander, Bunyip). packageId `PMM.Reptiles`, assembly `PMM_Reptiles.dll`,
namespace `PMM_Reptiles`.

## 1. Repo layout & how to build

- Source: `/home/gapho/Desktop/Project Mamono Reptiles`. Git repo; commit early/often.
- `./build.sh` compiles `Source/Reptiles/*.cs` → `Assemblies/PMM_Reptiles.dll`
  (csc vs RimWorld managed DLLs, Harmony, VEF.dll, and `../Project Mamono/Assemblies/ProjectMamono.dll`).
- `Tools/mktex.py` - pure-stdlib PNG generator for all placeholder textures/icons.
- `sync.sh` copies the workspace to the live Mods folder - **the installed copy is a
  separate tree**; always sync before in-game testing or you're testing stale code.
- Sibling mods for reference patterns: `../Project Mamono` (core), `../Project Mamono Slime Faction`, `../Project Mamono Elementals`.

## 2. Dependencies (About.xml)

Hard: Harmony, Biotech, **Vanilla Expanded Framework** (VEF - its gene/hediff
comps power egg-laying), `PMM.Core`. **Ideology is a SOFT dep** (since 2026-09-05):
the only use is the Broods' forced `ProjectMamono_Meme_MonsterExtremists` meme, and
that `<li MayRequire="Ludeon.RimWorld.Ideology">` in `requiredMemes` self-gates, so
without the DLC both factions still spawn - the Broods just lose their forced meme.
Removed from `modDependencies` + `loadAfter`. Odyssey is **optional** (only gates
Salamander volcanic spawns). **No other optional mods** - the Alpha Genes / Cyanobot's
Genes cosmetic-gene soft-dep was scrapped 2026-09-05 (see §7).

## 3. Phase 1 - factions, genes, egg-laying, grapple (DONE + boot-tested)

- **Factions** (`Defs/FactionDefs/Factions_Reptile.xml`): the project's first
  FactionDefs (core's `Patches/DisableVisibleFactions.xml` wipes all vanilla
  factions, so these two dominate). Broods force the `ProjectMamono_Meme_MonsterExtremists`
  meme via `<requiredMemes>` (Misc-group meme → structureMemeWeights won't work;
  Supremacist/OutlanderRoughBase is the vanilla precedent). Medieval tech.
- **Mountain-only settlement placement** (`Source/Reptiles/FactionPlacement.cs`):
  no XML hook exists → Harmony postfix on BOTH `TileFinder.RandomSettlementTileFor`
  overloads, reentrancy-guarded re-roll, STRICT `hilliness == Mountainous`
  (user ruling: no LargeHills). TileFinder can't throw on failure (logs + returns
  PlanetTile(0)), so no soft-lock. DevMode log line proves it works.
- **Reptilian gene** (`Defs/GeneDefs/Genes_Reptile.xml`): +0.15 sharp, +10°C comfy-min,
  egg-laying via VEF, shedding via hidden hediff (`Shedding.cs`, slime-jelly pattern),
  and cold torpor instead of hypothermia (2026-09-30, below).
- **Egg-laying** (pure XML, Alpha Genes pattern): gene modExtension
  `VEF.Genes.GeneExtension.hediffToWholeBody = PMM_Hediff_EggLaying` →
  `VEF.Genes.HediffCompProperties_HumanEggLayer` (one `PMM_ReptileEggFertilized`,
  9-day lay, 15-day `CompProperties_HumanHatcher`, temp-ruinable 0-50°C).
  Egg-layer hediff made visible (egg-bearer) so Health tab shows progress.
- **CRITICAL compat** (`Source/Reptiles/EggCompat.cs`): VEF's hatcher bypasses
  `PregnancyUtility.GetInheritedGenes`/`ApplyBirthOutcome` - the seams core's mamono
  inheritance patches hook. A bracket patch on `CompHumanHatcher.Hatch` + postfix on
  `PawnGenerator.GeneratePawn` re-applies core rules (maternal endogenes, mother
  xenotype, force female). **Test this hard** - highest-risk seam.
- **Tail grapple** (`Source/Reptiles/TailGrapple.cs`): `PMM_Ability_TailGrapple`
  (hostile=true, impid precedent → raiders use it) → drags victim adjacent +
  `PMM_Hediff_Constricted` (Moving −0.95 roots without downing; tease via core's
  `TeaseApplication.TryApplyTease`; release via `ShouldRemove`).
- **Cold torpor, and the end of the hypothermia deaths** (2026-09-30,
  `Source/Reptiles/ColdTorpor.cs` + `Defs/HediffDefs/Hediffs_ColdTorpor.xml` +
  `Patches/ColdTorpor_HediffGivers.xml`). The +10 comfy-min put her **safe** minimum at
  16°C (a pawn's safe range is its comfortable range widened 10 either way,
  `GenTemperature.SafeTemperatureRange`), and vanilla's hypothermia giver builds severity
  below 16°C while only shedding it above 26°C - so on nearly every map a reptile
  accumulated a hypothermia she could never lose, and died of it. The gene now carries
  `makeImmuneTo` + `hediffGiversCannotGive` on Hypothermia, which also closes vanilla's
  frostbite roll (below 0°C, and only while hypothermia is past 0.37 severity), and a giver
  on vanilla's own `OrganicStandard` set builds `PMM_Hediff_ColdTorpor` in its place:
  shivering (hidden), sluggish, drowsy, torpid - the last one caps Consciousness, which lays
  her down where she stands, and hunger falls to a tenth so a winter asleep does not starve
  her. Rates are vanilla's own cold constants, kept so the cold bites at the old pace; the
  one deliberate departure is that it **fades as soon as she is back inside her safe range**,
  because her comfort range (26°C) is out of reach on most maps and VRE's own version would
  have left her asleep forever. VRE's `VRE_HypothermicHibernation` is the model, and vanilla
  itself gives insectoid flesh `HypothermicSlowdown` rather than Hypothermia - a cold
  hediff with no lethality at all.

## 4. Phase 2 - xenotypes & wild spawns (bulk DONE)

- **11 xenotypes** (`Defs/XenotypeDefs/Xenotypes_Reptile.xml`): each = `ProjectMamono_Mamono`
  + `PMM_Gene_Reptile` + species genes. inheritable, all-female,
  `factionlessGenerationWeight=0` (wild entry is via our incidents, not the factionless spawner).
- **Reused core genes** (user said "see main mod gene"): `ProjectMamono_MamonoVenom`
  (Basilisk), `PMM_Gene_Flight` (Dragon/Wyvern/Malef), `ProjectMamono_MamonoFiery`
  (Salamander+Dragon), `ProjectMamono_MamonoClaws` (claw species). Dragon fire-breath
  was cut (user ruling): fire is flavour text only, no ability.
- **Forced-trait genes** (`Defs/GeneDefs/Genes_ReptileTraits.xml`, Biotech
  `forcedTraits`/KindInstinct pattern, germline so wild+colony share personality):
  kept **DragonPride** (Arrogant + Greedy - `PMM_Arrogant` is our own core trait, written
  2026-09-23 because no trait mod in play ships an "Arrogant" trait; Jealous was dropped the
  same day, since vanilla makes Greedy and Jealous each other's `conflictingTrait`) and **WurmMind**
  (**SlowLearner** degree 0 - NOT TooSmart/−1; TooSmart only has degree 0 and the bad
  degree caused ~193× log spam). Scrapped (user ruling): LizardmanFury, Lustful,
  BunyipTongue - those xenotypes have no personality gene.
- **Wild-mamono wander-ins**: `Defs/PawnKindDefs/PawnKinds_ReptileWild.xml` (9 factionless
  naked/unarmed kinds, each pins one species via pawnkind `xenotypeSet`),
  `Defs/IncidentDefs/Incidents_ReptileWild.xml` (4 incidents with a
  `ReptileWildExtension` candidate list), `Source/Reptiles/ReptileWandersIn.cs`
  (`IncidentWorker_ReptileWandersIn`, slime-mod pattern). `Patch_ReptileIsWildMan`
  makes them tameable. **Habitat checks read the WORLD tile, not the map:**
  caves=`Find.World.HasCaves(tile)`, mountains=`hilliness==Mountainous`,
  desert=`PrimaryBiome` in Desert/AridShrubland/ExtremeDesert, volcanic=`PrimaryBiome=="LavaField"`
  (Odyssey-only, locked), wetland=`PrimaryBiome=="TropicalSwamp"` (locked), river/lake=
  `World.LakeDirectionAt != Invalid || CoastAngleAt(Ocean|Lake).HasValue`.
  Bunyip is wild-only (no faction). Malef/Dragonewt are NOT wild (faction/ambush only).
- **Four skin materials** (`Defs/ThingDefs/Items_ReptileScales_Species.xml`, was per-species until 2026-09-22):
  dragon / lamia / lizardman scales plus the bunyip's own wool (user ruling: she is a
  special reptile). Generic art + colour tints; `Shedding.cs ScaleMaterialFor(xenotype)`
  maps species→material, falling back to lizardman scales.
  Stats (2026-09-06) from the user's `leather chart.ods`: armour sharp/blunt/heat are
  factors on the LeatherBase values (blunt factors the SHARP base - see the file's
  header comment), insulation is absolute °C, HP/beauty are stuffProps statFactors,
  value is ×LeatherBase $2.1. Dragon-line = thrumbo tier ($16.8), lamia-line =
  plain+ tier, lizard-line = economy, salamander = heat-insulation specialist,
  **bunyip is a WOOL** (Fabric category, 1.7 flammability), not a leather.
  Unlisted species share a chart species' stat line (user ruling): medusa/wurm→lamia,
  basilisk→lizard, wyvern→dragon, dragonewt→corrupt dragon.
- **Mechanic genes** (`Defs/GeneDefs/Genes_ReptileMechanics.xml`):
  `PMM_Gene_Petrify` is LIVE (§8 item 1; grants its ability via the standard
  `<abilities>` block). `PMM_Gene_MalefCorruption` is now an INERT flavour gene -
  Malef creation moved to an item (§8 item 2).

## 5. Faction pawn generation - the two big gotchas

- **Member xenotype roster lives on the FactionDef, NOT the pawnkind.** The
  member-xenotype tooltip and the faction pawn generator read `FactionDef.xenotypeSet`
  as FRACTIONS (outlander pattern; leftover → Baseliner). I first put rosters on
  pawnkinds → tooltip showed "Baseliner 100%". Now on the FactionDefs (sums to 1.0).
- **Faction leaders need a `factionLeader=true` pawnkind in the faction's
  pawnGroupMakers.options** (confirmed in `Faction.TryGenerateNewLeader` IL).
  When I collapsed the 9 per-faction pawn types to one member kind, leaders went null
  → fixed by marking the member kinds `factionLeader=true`.

## 6. Faction pawnkinds (collapsed, user ruling)

`Defs/PawnKindDefs/PawnKinds_Reptile.xml`: ONE member kind per faction
(`PMM_Reptile_DragoniaMember`, `PMM_Reptile_BroodMember`), medieval gear (Neolithic+MedievalMilitary
apparel, medieval melee weapon), `factionLeader=true`. Species comes entirely from the
FactionDef roster. Wild kinds stay in `PawnKinds_ReptileWild.xml` (they have no
FactionDef, so they keep their own pawnkind-level `xenotypeSet`).

## 7. Soft-dependency cosmetic genes - SCRAPPED (2026-09-05)

`Patches/CosmeticGenes_SoftDeps.xml` is **deleted**; there is no third-party gene
integration (`Patches/` is empty). Xenotypes carry only their built-in genes (core +
species + the always-on `ProjectMamono_MamonoClaws`). Scrapped after repeated in-game
failures: `PatchOperationFindMod` matches the mod's **display name**, not packageId
(so `sarg.alphagenes` never matched); then Cyanobot's `Eyes_SlitPupil` refused to
resolve in-game for no diagnosable reason while its 7 sibling eye genes loaded fine.
The claw/fang genes turned out to be **mechanical** (grant melee attacks), not
cosmetic, which broke the design intent. Rather than keep fighting fragile cross-mod
gene refs, the user opted to drop the whole integration. If it's ever revived: match
FindMod by display name, probe the def for the gene after def-load (don't trust
`ModsConfig.IsActive` - workshop mods store as `packageid_steam`), and treat
claws/fangs as mechanical, not cosmetic.

## 8. Work log: what shipped, and the one item left

Everything in this list has shipped and been field-tested - items 1-4 on 2026-09-05, item 5's
pass on 2026-09-24, and items 6-8 with it. The one item still open is the lamia's coil rework,
tracked in `MASTER-PLAN.md` §6.1 item 21. The list is kept as the record of what each item was.

1. ~~**Petrify**~~ DONE (2026-09-05): `PetrifyPowers.cs` + `Abilities_Petrify.xml`
   + `Hediffs_Petrified.xml`. `PMM_Ability_Petrify` (gene-granted, 10 tiles,
   hostile so raiders use it) → visible `PMM_Hediff_Petrified`: stage caps
   Consciousness 0.1 + Moving 0 (downs anything, mechs included - locked ruling),
   and a postfix on **`Need.IsFrozen`** (the check every 1.6 NeedInterval opens
   with - ONE patch freezes food/rest/joy/mood at once; `Need.pawn`/`IsFrozen`
   are protected in 1.6 → cached `AccessTools.FieldRef`). 2-day timer, thaw
   letter/message, 7-day cooldown. Icon from mktex.py.
2. ~~**Malef corruption**~~ REDESIGNED (2026-09-05), CHAIN CHANGED (2026-09-22):
   the tease-knockout corruption AND the later colony offer were both scrapped
   (user ruling). A Malef is now made ONLY by an item - `PMM_DarkDragonsBlood`
   (`Defs/ThingDefs/Item_DarkDragonsBlood.xml`, a NeverForNutrition drug) whose
   `IngestionOutcomeDoer_DarkDragonsBlood` (`Source/Reptiles/DarkDragonsBlood.cs`)
   remakes the drinker in two steps, one vial each: an ordinary woman who drinks it
   rises as `PMM_Reptile_Dragonewt` (core `ApplyXenotype`; `CanEverTransform` gates
   men/children/monsters/too-young), and a dragonewt who drinks it becomes
   `PMM_Reptile_MalefDragon` (core `MamonoTransformation.ConvertXenotype`, which
   STAYS in core for this). A normal dragon still reaches Malef on her FIRST vial,
   by the same ConvertXenotype path. Malefs/men/children/non-humans unaffected. The
   core `VoluntaryTransformTargetOverride` hook was reverted (driver back to plain
   proposer-xenotype).
3. ~~**Medusa ruins ambush**~~ DONE (2026-09-05): `RuinsAmbush.cs` -
   `IncidentWorker_MedusaRuinsAmbush : IncidentWorker_Ambush` (NOTE: 1.6 has NO
   `IncidentWorker_CaravanAmbush`; the ambush flow is `IncidentWorker_Ambush` +
   subclasses, and the base does map-gen, spawning, and lord-creation from
   `parms.faction`). Gates on a ruin world object (AbandonedSettlement core /
   AbandonedCamp, AbandonedLandmark Odyssey - matched by defName) within 5 tiles
   of the caravan + Broods existing; spawns 1-3 wild-medusa kinds under Broods
   colours with a `LordJob_AssaultColony`. No ruins → cave wander-ins (fallback).
   (Ruin-map pre-population shipped 2026-09-20 - `Source/Reptiles/MedusaRuinFill.cs`,
   see item 7.)
4. ~~**release.sh**~~ DONE (2026-09-05): slime pattern, but the repo slug comes
   from the git origin remote (override via `PMM_REPO` env) because this repo
   had no remote configured yet. `About/preview.png` is the image from 2026-10-01, not
   the mktex.py placeholder.
5. **In-game test pass - DONE 2026-09-24, extended 2026-10-04** (`MASTER-PLAN.md` §2 and §6.1 hold
   the results) (see the testing plan in chat): boot (zero red errors),
   worldgen (1 faction each, mountains-only, icons visible, Broods ideo has Monster
   Extremists, leaders generated), egg inheritance (Stage 3, highest risk), grapple,
   shedding, wild-mamono habitat gates. NEW since this list: petrify (cast on a
   colonist → downed, needs pinned 2 days, thaws standing up; raider medusa uses
   it), dark dragon's blood (an ordinary woman → Dragonewt on one vial, Dragonewt →
   Malef on the next; a Dragon → Malef on her first; a Malef → nothing; plus the
   harvest surgery on a living Malef), ruins ambush (caravan near a ruin).

6. **Own race defs for the 11 species - DONE 2026-09-20, field-tested 2026-09-24.**
   Every species has a race def (`Defs/ThingDefs/Races_ReptileMamono.xml`), each
   xenotype names hers with `setRace` + `forceRace`, and all 11 corpses are on the
   family's shared "mamono corpses" line. Reasoning, decisions and the test list live
   in `RACES-PLAN.md`.

7. **Trade, guests, the ruin fill and the scales moodlet - BUILT 2026-09-20,
   field-tested 2026-09-24.** The four items that were sitting in `PLAN.md` §9:
   - DRAGONIA TRADE: `Defs/TraderKindDefs/TraderKinds_Dragonia.xml` plus
     `caravanTraderKinds` on the faction (neutral realm only - the Broods are enemies).
   - RUIN FILL: `Source/Reptiles/MedusaRuinFill.cs` dresses our ambush map with broken
     stone walls, sculptures as the medusa's petrified victims, fallen rock and buried
     loot. It hangs off a postfix on `CaravanIncidentUtility.SetupCaravanAttackMap` -
     the only hook that hands us the finished map - and acts only when our own flag is
     set (from `GeneratePawns`) AND the tile really is near a ruin.
   - LAMIA GUEST - BUILT AND REMOVED 2026-09-20 (user ruling).
     `Defs/IncidentDefs/Incidents_LamiaGuest.xml` and `Source/Reptiles/LamiaGuest.cs` are
     deleted. Vanilla's own visitor-group incident covers Dragonia's guests instead: it can
     pick the realm because the faction has a `Peaceful` pawn group maker, and some of those
     visitors trade because of `<visitorTraderKinds>` below. What the custom incident added,
     and is now gone: a guaranteed lamia (vanilla rolls the faction's xenotype mix), her own
     letter, and the parting gift of shed scales. Core's guest mana work stays and applies to
     every vanilla visitor (`VisitingGuestManaPatch` fills the bar on spawn; `Need_Mana`
     floors a guest's drain at `GuestManaFloor`).
   - VISITING TRADERS: `<visitorTraderKinds>` on Dragonia plus the smaller
     `PMM_DragoniaVisitor` trader kind. Vanilla's visitor-group incident turns one adult
     visitor into a small trader 75% of the time, but only for a faction that lists
     visitor trader kinds - dragonians trade at your colony because of this list.
   - FRESH SCALES: `Defs/ThoughtDefs/Thoughts_Reptile.xml`, granted from `Shedding.cs`
     when she drops her scales (+3 mood, two days).
   Test: a caravan arrives with scales/jade and buys art; a caravan ambushed near a ruin
   finds walls, figures and loot on the map; a vanilla visitor group from Dragonia arrives
   (one of them may trade) and leaves again after a few hours (`DebugSettings.
   instantVisitorsGift` in dev mode makes visitors leave at once); shedding shows the
   moodlet.

8. **Dragon orbs - BUILT 2026-09-22, field-tested 2026-09-24.** The wiki's porch lamps, as
   furniture that is bought instead of built:
   - DEFS: `Defs/ThingDefs/Building_DragonOrb.xml` - `PMM_DragonOrb` (calling) and
     `PMM_DragonOrbDecor` (glowing), 1x1, `minifiedDef MinifiedThing`, and deliberately
     NO `designationCategory` and no `costList`, which is what keeps them out of the
     Architect menu while still letting a trader hand one over as a minified object.
     Art: `Textures/Things/Building/PMM_DragonOrb.png`, shared by both; the light tells
     them apart (calling: 8 tiles, pale lavender; glowing: 12 tiles, violet).
   - LURE: `Source/Reptiles/ReptileWandersIn.cs`. `ReptileWildExtension.orbCandidates`
     (dragons only) joins the roll when a calling orb is INSTALLED and it is night
     (`GenLocalDate.HourInteger`), entered twice so the light pulls the roll; plus the
     `PMM_DragonOrbCall` incident (baseChance 0.5), which cannot fire without a lit orb.
     When the orb's species is the one picked, the orb is destroyed and the glowing orb
     spawns in its place, silently.
   - TRADE: one calling orb (1~1) and one or two glowing orbs on `PMM_DragoniaTrader`
     only, at MarketValue 420 and 150, and both defs named in
     `Patches/ExcludeDragonContentFromVanillaStock.xml` (renamed from
     `ExcludeDragoniumFromVanillaStock.xml`) so no vanilla trade rolls them.
   Test: see `MASTER-PLAN.md` §6.1 item 18.

## 9. Hard-won lessons (don't relearn these)

- **Vanilla textures in Unity bundles aren't reliably resolvable by loose path from
  mod XML** → ship own textures (mktex.py). World-icon paths (`World/WorldObjects/...`)
  DO resolve; item paths (`Things/.../Leather`, `EggBirdSmall`) did NOT.
- **FactionDefs need BOTH `factionIconPath` (UI) AND `settlementTexturePath` (world-map
  material)** - Settlement.get_Material reads the latter; null → ArgumentNullException
  on every draw (settlements invisible).
- **`startingCountAtWorldCreation` = number of FACTION INSTANCES, not settlements** (IL:
  Page_CreateWorldParams loops Add). 4/3 → duplicate factions; use 1. Settlement COUNT
  comes from world population ÷ competing factions (settlementGenerationWeight).
- **PawnKindDefs require `initialWillRange` + `initialResistanceRange`; haulable stuffs
  need explicit `<Mass>`** (config errors otherwise).
- **`tile.PrimaryBiome`** (property), NOT `tile.biome` (field, private-ish).
- **Biotech aptitude genes are runtime-generated** `Aptitude{Level}_{Skill}`
  (e.g. AptitudeStrong_Melee, AptitudeRemarkable_Social) - not literal defNames in
  Biotech XML; follow the slime mod's usage.
- **No vanilla "Arrogant"/"Aggressive" traits**; "stupid" = SlowLearner (degree 0).
- **Verify a vanilla field's meaning in IL before trusting the name** (startingCount,
  settlementTexturePath, xenotypeSet location all bit me).
- **The terminal heredoc for writing files gets mangled** - use the file-creation tool,
  not `cat <<EOF`, for anything non-trivial.
- **A def that must never be buildable must NOT inherit `FurnitureBase`**: it hard-codes
  `<designationCategory>Furniture</designationCategory>`, and that field is what puts a
  def in the Architect menu. Parent to `BuildingBase` and copy `minifiedDef MinifiedThing`
  and the `BuildingsFurniture` category out of `FurnitureBase` by hand.
- **Vanilla sells furniture through the same stock helper as everything else**, which is
  why a minifiable building can be a trade good: three vanilla trader kinds roll the
  `BuildingsFurniture` category (`TraderKinds_Base_Outlander.xml`,
  `TraderKinds_Caravan_Outlander.xml`, `TraderKinds_Orbital_Misc.xml`). None of the three
  carries an `excludedThingDefs` list, so a mod adding furniture must ADD the element, not
  append to it - hence the `match`/`nomatch` conditional in our patch.
- **`uninstallWork` is a `<building>` child**, not a top-level ThingDef field, and a
  minified building is a `MinifiedThing` in `listerThings`, never its own def.

## 10. Where the canonical state lives

`/home/gapho/Desktop/Project Mamono Reptiles` git repo (commits through `9bb782b`).
Core also changed: `MamonoTransformation.ConvertXenotype` (added `9762861`, kept) and
the reverted override hook (`3f838c8`) - Reptiles builds against that core.
Repo memory: `/memories/repo/pmm-reptiles.md` (phase 1) and
`/memories/repo/pmm-reptiles-phase2.md` (phase 2) - keep these current.

Own race defs for the 11 species: done 2026-09-20, see `RACES-PLAN.md`.
