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

## 2. Hard dependencies (About.xml)

Harmony, Biotech, Ideology, **Vanilla Expanded Framework** (VEF — its gene/hediff
comps power egg-laying), `PMM.Core`. Odyssey is **optional** (only gates Salamander
volcanic spawns). Alpha Genes + Cyanobot's Genes are **soft** deps (§7).

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
  11 materials on a shared Leathery base, generic art + colour tints;
  `Shedding.cs ScaleMaterialFor(xenotype)` maps species→material, generic fallback.
- **Placeholder mechanic genes** (`Defs/GeneDefs/Genes_ReptileMechanics.xml`):
  `PMM_Gene_Petrify`, `PMM_Gene_MalefCorruption` — INERT flavour genes so xenotypes
  resolve; real C# NOT yet written (§8).

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
(`PMM_DragoniaMember`, `PMM_BroodMember`), medieval gear (Neolithic+MedievalMilitary
apparel, medieval melee weapon), `factionLeader=true`. Species comes entirely from the
FactionDef roster. Wild kinds stay in `PawnKinds_ReptileWild.xml` (they have no
FactionDef, so they keep their own pawnkind-level `xenotypeSet`).

## 7. Soft-dependency cosmetic genes (`Patches/CosmeticGenes_SoftDeps.xml`)

Alpha Genes (`sarg.alphagenes`) + Cyanobot's Genes (`cyanobot.CyanobotsGenes`) via
`PatchOperationFindMod` + conditional `PatchOperationAdd` — the standard soft-dep
pattern (NO `<modDependencies>` entry; gene `<li MayRequire>` is NOT a thing). Adds
e.g. AG_DrakonoriBody/Head/Wings to dragons, Eyes_SlitPupil to all, CYB fangs/claws/
tails per species. **Fallback ruling:** every claw species also carries the always-on
core `ProjectMomo_MomoClaws`, so claws exist even without the optional mods; AG/CYB
claws are upgrades. **Untested:** possible gene exclusion-tag conflicts between AG/CYB
claws and `ProjectMomo_MomoClaws`, and some "cosmetic" genes carry melee effects —
verify per-species spawn in the log.

## 8. NOT YET DONE (next work, in priority order)

1. **Petrify** (`PetrifyPowers.cs` + `Defs/AbilityDefs/Abilities_Petrify.xml` +
   `Defs/HediffDefs/Hediffs_Petrified.xml`): `PMM_Ability_Petrify` (gene-granted,
   pawn-target ~10 tiles) → hidden `PMM_Hediff_Petrified`: downs target AND freezes
   Need_Food/Need_Rest/Need_Joy/mood ticking for 2 days (`ShouldRemove` on timer),
   7-day cooldown. Locked: NOTHING immune, frozen = simply downed (not statue-items).
   Currently an inert placeholder gene.
2. **Malef corruption** (`MalefCorruption.cs`): hook core tease knockout — a Malef
   Dragon downing a female transforms her: baseline/other-momo → `PMM_Dragonewt`,
   normal Dragon → `PMM_MalefDragon`, via core `MomoTransformation`. Dragonewts do
   NOT transform. Inert placeholder gene now.
3. **Medusa ruins ambush** (`RuinsAmbush.cs`): caravan-ambush incident near
   `AbandonedSettlement`/`AbandonedCamp`/`AbandonedLandmark` world objects
   (CaravanAmbush precedent); gate on `Any` such object, fall back to cave wander-ins.
   (Ruin-map pre-population is a later, heavier phase.)
4. **release.sh** (copy sibling mods' pattern), `About/preview.png` polish.
5. **In-game test pass** (see the testing plan in chat): boot (zero red errors),
   worldgen (1 faction each, mountains-only, icons visible, Broods ideo has Monster
   Extremists, leaders generated), egg inheritance (Stage 3, highest risk), grapple,
   shedding, wild-momo habitat gates.

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

`/home/gapho/Desktop/Project Momo Reptiles` git repo (commits through `148df69`).
Repo memory: `/memories/repo/pmm-reptiles.md` (phase 1) and
`/memories/repo/pmm-reptiles-phase2.md` (phase 2) — keep these current.
