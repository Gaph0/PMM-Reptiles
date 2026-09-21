# PMM Reptiles — Agent Hand-off

For the next agent picking this up. What exists, why it was built that way, and
what's left. Read `PLAN.md` (phase 1 design + locked rulings) and `XENOTYPES.md`
(phase 2 roster + locked rulings) alongside this.

## 0. The mod in one paragraph

Project Momo Reptiles adds reptile momos to RimWorld as a sub-mod of the core
Project Momo. Two mountain factions (neutral **Dragonia**, hostile lewdist
**Scalebound Broods**), a **reptilian gene** (sharp hide, warmth-loving, egg-laying,
yearly scale-shedding), a **lamia tail-grapple** gene, and eleven **xenotypes**
(Basilisk, Dragon, Lamia, Lizardman, Medusa, Wurm, Wyvern, Malef Dragon, Dragonewt,
Salamander, Bunyip). packageId `PMM.Reptiles`, assembly `PMM_Reptiles.dll`,
namespace `PMM_Reptiles`.

## 1. Repo layout & how to build

- Source: `/home/gapho/Desktop/Project Momo Reptiles`. Git repo; commit early/often.
- `./build.sh` compiles `Source/Reptiles/*.cs` → `Assemblies/PMM_Reptiles.dll`
  (csc vs RimWorld managed DLLs, Harmony, VEF.dll, and `../Project Momo/Assemblies/ProjectMomo.dll`).
- `Tools/mktex.py` — pure-stdlib PNG generator for all placeholder textures/icons.
- `sync.sh` copies the workspace to the live Mods folder — **the installed copy is a
  separate tree**; always sync before in-game testing or you're testing stale code.
- Sibling mods for reference patterns: `../Project Momo` (core), `../Project Momo Slime Faction`, `../Project Momo Elementals`.

## 2. Dependencies (About.xml)

Hard: Harmony, Biotech, **Vanilla Expanded Framework** (VEF — its gene/hediff
comps power egg-laying), `PMM.Core`. **Ideology is a SOFT dep** (since 2026-09-05):
the only use is the Broods' forced `ProjectMomo_Meme_MonsterExtremists` meme, and
that `<li MayRequire="Ludeon.RimWorld.Ideology">` in `requiredMemes` self-gates, so
without the DLC both factions still spawn — the Broods just lose their forced meme.
Removed from `modDependencies` + `loadAfter`. Odyssey is **optional** (only gates
Salamander volcanic spawns). **No other optional mods** — the Alpha Genes / Cyanobot's
Genes cosmetic-gene soft-dep was scrapped 2026-09-05 (see §7).

## 3. Phase 1 — factions, genes, egg-laying, grapple (DONE + boot-tested)

- **Factions** (`Defs/FactionDefs/Factions_Reptile.xml`): the project's first
  FactionDefs (core's `Patches/DisableVisibleFactions.xml` wipes all vanilla
  factions, so these two dominate). Broods force the `ProjectMomo_Meme_MonsterExtremists`
  meme via `<requiredMemes>` (Misc-group meme → structureMemeWeights won't work;
  Supremacist/OutlanderRoughBase is the vanilla precedent). Medieval tech.
- **Mountain-only settlement placement** (`Source/Reptiles/FactionPlacement.cs`):
  no XML hook exists → Harmony postfix on BOTH `TileFinder.RandomSettlementTileFor`
  overloads, reentrancy-guarded re-roll, STRICT `hilliness == Mountainous`
  (user ruling: no LargeHills). TileFinder can't throw on failure (logs + returns
  PlanetTile(0)), so no soft-lock. DevMode log line proves it works.
- **Reptilian gene** (`Defs/GeneDefs/Genes_Reptile.xml`): +0.15 sharp, +10°C comfy-min,
  egg-laying via VEF, shedding via hidden hediff (`Shedding.cs`, slime-jelly pattern).
- **Egg-laying** (pure XML, Alpha Genes pattern): gene modExtension
  `VEF.Genes.GeneExtension.hediffToWholeBody = PMM_Hediff_EggLaying` →
  `VEF.Genes.HediffCompProperties_HumanEggLayer` (one `PMM_ReptileEggFertilized`,
  9-day lay, 15-day `CompProperties_HumanHatcher`, temp-ruinable 0-50°C).
  Egg-layer hediff made visible (egg-bearer) so Health tab shows progress.
- **CRITICAL compat** (`Source/Reptiles/EggCompat.cs`): VEF's hatcher bypasses
  `PregnancyUtility.GetInheritedGenes`/`ApplyBirthOutcome` — the seams core's momo
  inheritance patches hook. A bracket patch on `CompHumanHatcher.Hatch` + postfix on
  `PawnGenerator.GeneratePawn` re-applies core rules (maternal endogenes, mother
  xenotype, force female). **Test this hard** — highest-risk seam.
- **Tail grapple** (`Source/Reptiles/TailGrapple.cs`): `PMM_Ability_TailGrapple`
  (hostile=true, impid precedent → raiders use it) → drags victim adjacent +
  `PMM_Hediff_Constricted` (Moving −0.95 roots without downing; tease via core's
  `TeaseApplication.TryApplyTease`; release via `ShouldRemove`).

## 4. Phase 2 — xenotypes & wild spawns (bulk DONE)

- **11 xenotypes** (`Defs/XenotypeDefs/Xenotypes_Reptile.xml`): each = `ProjectMomo_Momo`
  + `PMM_Gene_Reptile` + species genes. inheritable, all-female,
  `factionlessGenerationWeight=0` (wild entry is via our incidents, not the factionless spawner).
- **Reused core genes** (user said "see main mod gene"): `ProjectMomo_MomoVenom`
  (Basilisk), `PMM_Gene_Flight` (Dragon/Wyvern/Malef), `ProjectMomo_MomoFiery`
  (Salamander+Dragon), `ProjectMomo_MomoClaws` (claw species). Dragon fire-breath
  was cut (user ruling): fire is flavour text only, no ability.
- **Forced-trait genes** (`Defs/GeneDefs/Genes_ReptileTraits.xml`, Biotech
  `forcedTraits`/KindInstinct pattern, germline so wild+colony share personality):
  kept **DragonPride** (Jealous+Greedy — no vanilla "Arrogant" exists) and **WurmMind**
  (**SlowLearner** degree 0 — NOT TooSmart/−1; TooSmart only has degree 0 and the bad
  degree caused ~193× log spam). Scrapped (user ruling): LizardmanFury, Lustful,
  BunyipTongue — those xenotypes have no personality gene.
- **Wild-momo wander-ins**: `Defs/PawnKindDefs/PawnKinds_ReptileWild.xml` (9 factionless
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
- **Per-species scales** (`Defs/ThingDefs/Items_ReptileScales_Species.xml`, locked Q5):
  11 materials, generic art + colour tints; `Shedding.cs ScaleMaterialFor(xenotype)`
  maps species→material, generic fallback (`Item_ReptileScale.xml`, standalone def).
  Stats (2026-09-06) from the user's `leather chart.ods`: armour sharp/blunt/heat are
  factors on the LeatherBase values (blunt factors the SHARP base — see the file's
  header comment), insulation is absolute °C, HP/beauty are stuffProps statFactors,
  value is ×LeatherBase $2.1. Dragon-line = thrumbo tier ($16.8), lamia-line =
  plain+ tier, lizard-line = economy, salamander = heat-insulation specialist,
  **bunyip is a WOOL** (Fabric category, 1.7 flammability), not a leather.
  Unlisted species share a chart species' stat line (user ruling): medusa/wurm→lamia,
  basilisk→lizard, wyvern→dragon, dragonewt→corrupt dragon.
- **Mechanic genes** (`Defs/GeneDefs/Genes_ReptileMechanics.xml`):
  `PMM_Gene_Petrify` is LIVE (§8 item 1; grants its ability via the standard
  `<abilities>` block). `PMM_Gene_MalefCorruption` is now an INERT flavour gene —
  Malef creation moved to an item (§8 item 2).

## 5. Faction pawn generation — the two big gotchas

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

## 7. Soft-dependency cosmetic genes — SCRAPPED (2026-09-05)

`Patches/CosmeticGenes_SoftDeps.xml` is **deleted**; there is no third-party gene
integration (`Patches/` is empty). Xenotypes carry only their built-in genes (core +
species + the always-on `ProjectMomo_MomoClaws`). Scrapped after repeated in-game
failures: `PatchOperationFindMod` matches the mod's **display name**, not packageId
(so `sarg.alphagenes` never matched); then Cyanobot's `Eyes_SlitPupil` refused to
resolve in-game for no diagnosable reason while its 7 sibling eye genes loaded fine.
The claw/fang genes turned out to be **mechanical** (grant melee attacks), not
cosmetic, which broke the design intent. Rather than keep fighting fragile cross-mod
gene refs, the user opted to drop the whole integration. If it's ever revived: match
FindMod by display name, probe the def for the gene after def-load (don't trust
`ModsConfig.IsActive` — workshop mods store as `packageid_steam`), and treat
claws/fangs as mechanical, not cosmetic.

## 8. NOT YET DONE (next work, in priority order)

1. ~~**Petrify**~~ DONE (2026-09-05): `PetrifyPowers.cs` + `Abilities_Petrify.xml`
   + `Hediffs_Petrified.xml`. `PMM_Ability_Petrify` (gene-granted, 10 tiles,
   hostile so raiders use it) → visible `PMM_Hediff_Petrified`: stage caps
   Consciousness 0.1 + Moving 0 (downs anything, mechs included — locked ruling),
   and a postfix on **`Need.IsFrozen`** (the check every 1.6 NeedInterval opens
   with — ONE patch freezes food/rest/joy/mood at once; `Need.pawn`/`IsFrozen`
   are protected in 1.6 → cached `AccessTools.FieldRef`). 2-day timer, thaw
   letter/message, 7-day cooldown. Icon from mktex.py.
2. ~~**Malef corruption**~~ REDESIGNED (2026-09-05): the tease-knockout
   corruption AND the later colony offer were both scrapped (user ruling). A Malef
   is now made ONLY by an item — `PMM_DarkDragonsBlood`
   (`Defs/ThingDefs/Item_DarkDragonsBlood.xml`, a NeverForNutrition drug) whose
   `IngestionOutcomeDoer_DarkDragonsBlood` (`Source/Reptiles/DarkDragonsBlood.cs`)
   transforms the drinker into `PMM_Reptile_MalefDragon`: baseliner → core `ApplyXenotype`
   (`CanEverTransform` gates men/children/monsters/too-young), normal Dragon →
   core `MomoTransformation.ConvertXenotype` (which STAYS in core for this).
   Dragonewts/Malefs/men/children/non-humans unaffected. The core
   `VoluntaryTransformTargetOverride` hook was reverted (driver back to plain
   proposer-xenotype).
3. ~~**Medusa ruins ambush**~~ DONE (2026-09-05): `RuinsAmbush.cs` —
   `IncidentWorker_MedusaRuinsAmbush : IncidentWorker_Ambush` (NOTE: 1.6 has NO
   `IncidentWorker_CaravanAmbush`; the ambush flow is `IncidentWorker_Ambush` +
   subclasses, and the base does map-gen, spawning, and lord-creation from
   `parms.faction`). Gates on a ruin world object (AbandonedSettlement core /
   AbandonedCamp, AbandonedLandmark Odyssey — matched by defName) within 5 tiles
   of the caravan + Broods existing; spawns 1-3 wild-medusa kinds under Broods
   colours with a `LordJob_AssaultColony`. No ruins → cave wander-ins (fallback).
   (Ruin-map pre-population shipped 2026-09-20 - `Source/Reptiles/MedusaRuinFill.cs`,
   see item 7.)
4. ~~**release.sh**~~ DONE (2026-09-05): slime pattern, but the repo slug comes
   from the git origin remote (override via `PMM_REPO` env) because this repo
   had no remote configured yet. `About/preview.png` is the mktex.py placeholder
   (mountain bands + sun) — polish optional.
5. **In-game test pass** (see the testing plan in chat): boot (zero red errors),
   worldgen (1 faction each, mountains-only, icons visible, Broods ideo has Monster
   Extremists, leaders generated), egg inheritance (Stage 3, highest risk), grapple,
   shedding, wild-momo habitat gates. NEW since this list: petrify (cast on a
   colonist → downed, needs pinned 2 days, thaws standing up; raider medusa uses
   it), malef corruption (Malef tease-KOs a baseliner → Dragonewt; a Dragon →
   Malef; a Dragonewt → nothing), ruins ambush (caravan near a ruin).

6. **Own race defs for the 11 species — DONE 2026-09-20, not yet field-tested.**
   Every species has a race def (`Defs/ThingDefs/Races_ReptileMomo.xml`), each
   xenotype names hers with `setRace` + `forceRace`, and all 11 corpses are on the
   family's shared "momo corpses" line. Reasoning, decisions and the test list live
   in `RACES-PLAN.md`.

7. **Trade, guests, the ruin fill and the scales moodlet — BUILT 2026-09-20, not yet
   field-tested.** The four items that were sitting in `PLAN.md` §9:
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

## 9. Hard-won lessons (don't relearn these)

- **Vanilla textures in Unity bundles aren't reliably resolvable by loose path from
  mod XML** → ship own textures (mktex.py). World-icon paths (`World/WorldObjects/...`)
  DO resolve; item paths (`Things/.../Leather`, `EggBirdSmall`) did NOT.
- **FactionDefs need BOTH `factionIconPath` (UI) AND `settlementTexturePath` (world-map
  material)** — Settlement.get_Material reads the latter; null → ArgumentNullException
  on every draw (settlements invisible).
- **`startingCountAtWorldCreation` = number of FACTION INSTANCES, not settlements** (IL:
  Page_CreateWorldParams loops Add). 4/3 → duplicate factions; use 1. Settlement COUNT
  comes from world population ÷ competing factions (settlementGenerationWeight).
- **PawnKindDefs require `initialWillRange` + `initialResistanceRange`; haulable stuffs
  need explicit `<Mass>`** (config errors otherwise).
- **`tile.PrimaryBiome`** (property), NOT `tile.biome` (field, private-ish).
- **Biotech aptitude genes are runtime-generated** `Aptitude{Level}_{Skill}`
  (e.g. AptitudeStrong_Melee, AptitudeRemarkable_Social) — not literal defNames in
  Biotech XML; follow the slime mod's usage.
- **No vanilla "Arrogant"/"Aggressive" traits**; "stupid" = SlowLearner (degree 0).
- **Verify a vanilla field's meaning in IL before trusting the name** (startingCount,
  settlementTexturePath, xenotypeSet location all bit me).
- **The terminal heredoc for writing files gets mangled** — use the file-creation tool,
  not `cat <<EOF`, for anything non-trivial.

## 10. Where the canonical state lives

`/home/gapho/Desktop/Project Momo Reptiles` git repo (commits through `9bb782b`).
Core also changed: `MomoTransformation.ConvertXenotype` (added `9762861`, kept) and
the reverted override hook (`3f838c8`) — Reptiles builds against that core.
Repo memory: `/memories/repo/pmm-reptiles.md` (phase 1) and
`/memories/repo/pmm-reptiles-phase2.md` (phase 2) — keep these current.

Own race defs for the 11 species: done 2026-09-20, see `RACES-PLAN.md`.
