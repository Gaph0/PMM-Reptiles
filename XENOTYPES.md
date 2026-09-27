# Project Mamono Reptiles - Xenotype & Wild-Spawn Plan (Phase 2)

Eleven reptile mamono xenotypes, wild-mamono wander-in spawns, and the wiring that
replaces the phase-1 gene-injection shim. Builds on the factions/genes/egg-laying
already in `PLAN.md` (locked rulings) and the patterns proven in the slime mod
(`PMM_SlimeFaction`) and core (`ProjectMamono`).
Since 2026-09-20 every xenotype also names its own species race (`setRace` +
`forceRace` in `Defs/XenotypeDefs/Xenotypes_Reptile.xml`), so a reptile pawn is her
species rather than a Human pawn wearing a costume. The races, their bodies, hides
and corpses are listed in `RACES-PLAN.md`.
## 0. Reusable building blocks (already exist - reference, don't rebuild)

| Asset | Where | Used for |
|---|---|---|
| `PMM_Gene_Reptile` (scales, warm, egg-laying, shedding) | Reptiles phase 1 | every reptile xenotype |
| `PMM_Gene_LamiaTail` + tail grapple | Reptiles phase 1 | Lamia, Medusa, Bunyip |
| `ProjectMamono_MamonoVenom` (non-lethal slowing venom on melee) | Core | Basilisk |
| `PMM_Gene_Flight` (leap ability + caravan flight) | Core | Dragon, Wyvern, Malef Dragon |
| `ProjectMamono_MamonoFiery` (fire/heat/lava immunity) | Core | Salamander, Dragon (fire-breath is flavour text only) |
| `ProjectMamono_MamonoClaws` (+50% tease) | Core | Dragon, Wurm, Wyvern, Bunyip |
| Core tease/essence system (`TeaseApplication`) | Core | Malef/Dragonewt tease, Medusa freeze |
| Wander-in worker `IncidentWorker_*WandersIn` + `Find.World.HasCaves(tile)` | Slime mod | all wild-mamono incidents |
| `<forcedTraits>` on a GeneDef (Biotech `KindInstinct` pattern) | Biotech | Dragon (Arrogant+Greedy), Wurm (stupid), Lizardman (aggressive), Bunyip (poor talker) |
| Faction `xenotypeSet` on pawnkinds | Reptiles phase 1 | per-faction roster mix |

## 1. Xenotype roster & faction placement

Faction exclusivity (locked 2026-09-03): **Lizardman & Dragon → Dragonia only**;
**Malef Dragon, Dragonewt, Salamander → Scalebound Broods only**; everything else shared.
Every xenotype carries `ProjectMamono_Mamono` + `PMM_Gene_Reptile` as its base, is
`inheritable`, `factionlessGenerationWeight=0` (wild entry is via our incidents,
not the factionless spawner), all-female.

| Xenotype | Signature genes / extras | Traits (forced) | Wild-mamono spawn | Faction |
|---|---|---|---|---|
| **Basilisk** | `ProjectMamono_MamonoVenom` | - | caves, deserts | both |
| **Dragon** | `ProjectMamono_MamonoClaws`, `PMM_Gene_Flight`, `ProjectMamono_MamonoFiery` (fire-breath is flavour only - no ability) | **Arrogant, Greedy** (new trait-gene) | caves, mountains | Dragonia |
| **Lamia** | `PMM_Gene_LamiaTail`, social aptitude gene | - (persuasive) | caves, mountains | both |
| **Lizardman** | melee genes (`MeleeDamage_Strong`, `Robust`, `WoundHealing_SuperFast`) | **Aggressive** | caves | Dragonia |
| **Medusa** | `PMM_Gene_LamiaTail`, new `PMM_Gene_Petrify` (petrify gizmo) | - | caves + *ruins world-objects* (§4) | both |
| **Wurm** | `ProjectMamono_MamonoClaws`, `Robust`, `MeleeDamage_Strong`, high met | **TooSmart−** (stupid) | wetlands, mountains, caves | both |
| **Wyvern** | `ProjectMamono_MamonoClaws`, `PMM_Gene_Flight`, move-speed genes | - (no arrogant/greedy) | caves, mountains | Dragonia |
| **Malef Dragon** | `PMM_Gene_Flight`, tease-amp genes (`ProjectMamono_MamonoClaws`, social/seduction), new `PMM_Gene_MalefCorruption` (transform victims) | **Lustful** | - (not wild) | Broods |
| **Dragonewt** | tease-amp genes (`ProjectMamono_MamonoClaws`, social) | **Lustful** | - (not wild) | Broods |
| **Salamander** | `ProjectMamono_MamonoFiery` | - | caves, **volcanic (Odyssey-only)** | Broods |
| **Bunyip** | `PMM_Gene_LamiaTail`, `ProjectMamono_MamonoClaws`, `Robust`, `MeleeDamage_Strong` | **Ugly/poor talker** (social penalty) | rivers & lakes | **none** (wild only) |

Tailed species carry **no** vanilla speed gene (removed 2026-09-20): Big and Small's tail
tracker already applies `MoveSpeed` -0.5, so `MoveSpeed_Quick`/`MoveSpeed_VeryQuick` only
cancelled out the cost of the tail. Wyverns keep theirs - they fly, and have no tail to pay for.

New genes to create (all in `Defs/GeneDefs/`):
- `PMM_Gene_Petrify` - Medusa petrification gizmo (§3).
- `PMM_Gene_MalefCorruption` - Malef Dragon transformation-of-victims (§5).
- Trait-genes for forced traits (Biotech `<forcedTraits>` pattern): a small set of
  single-purpose genes that force Arrogant+Greedy (Dragon), TooSmart-low (Wurm),
  Aggressive (Lizardman), Lustful (Malef/Dragonewt), poor-talker (Bunyip).

(Dragon fire-breath is NOT an ability - user ruling 2026-09-05: Dragons/Salamanders
carry only the core `ProjectMamono_MamonoFiery` immunity and their fire-breath exists in
the description/flavour text, never as a usable gizmo.)

## 2. Wild-mamono wander-in incidents (mirror slime mod)

One `IncidentWorker_ReptileWandersIn : IncidentWorker_WildManWandersIn` (the slime
`IncidentWorker_SlimeWandersIn` pattern) with a per-species `PawnKindToSpawn` and a
`CanFireNowSub` habitat gate. Habitat checks (all read the world tile, not the map):
- **Caves** - `Find.World.HasCaves(map.Tile)` (the slime-mod cave mutator check).
- **Mountains** - `Find.WorldGrid[tile].hilliness == Hilliness.Mountainous`.
- **Deserts** - biome in {Desert, AridShrubland, ExtremeDesert}.
- **Volcanic** - Odyssey lava/volcano world-tile feature, `MayRequire`d on
  Ludeon.RimWorld.Odyssey (locked Q1: volcanic spawns exist ONLY with Odyssey;
  without it Salamanders fall back to caves only).
- **Wetlands** - marsh/swamp biome (locked Q3).
- **Rivers & lakes** - tile `Rivers.Count > 0` or a water body; river = flowing water
  adjacency (the sea-slime coastal check pattern).

Wild spawn table (the "Wild-mamono spawn" column above). Wild mamonos are factionless
wild creatures, tamed like wild men (slime `IsWildMan` patch pattern), with the
xenotype mix rolled per spawn.

## 3. Medusa petrification (the most mechanical piece)

`PMM_Gene_Petrify` grants `PMM_Ability_Petrify` (pawn-targeted, ~10 tile range,
`Verb_CastAbility`, custom `CompAbilityEffect_Petrify`). On cast:
1. Apply hidden hediff `PMM_Hediff_Petrified` to the target.
2. Effect: **frozen** - the pawn is downed/immobile **and** its need ticking is
   suspended: hunger, rest, and mood never advance while frozen (harmony patch on
   the need/joy/mood ticks keyed to the hediff, or a comp that re-pins need levels
   each tick - cheapest is freezing `Need_Food`, `Need_Rest`, `Need_Joy` levels
   and suppressing mood-thought accumulation).
3. Duration **2 days**, then it drops off automatically (severity timer →
   `ShouldRemove`), with an expiry letter.
4. Cooldown **7 days** (`cooldownTicksRange`, vanilla ability system runs it).
Locked Q4 (updated 2026-09-24): **nothing is immune by species** - any pawn can be petrified,
mechanoids included - but the stone needs a **working, uncovered eye**: Sight under 20%, or eyes
hidden by `FullHead`/`Eyes` apparel (a war mask or a veil), refuses the gaze.
Frozen pawns are simply **downed** for the duration (the non-lethal "turned to
stone" read); they are not carried, not statues-as-items.

## 4. Medusa ruins spawning - feasibility (direct answer)

**Yes, feasible, with a caveat.** The world objects you mean are vanilla/Odyssey
`AbandonedSettlement`, `AbandonedCamp`, `AbandonedLandmark` (confirmed present in
Core/Odyssey WorldObjectDefs). Three viable approaches:

- **(a) Caravan-encounter incident (recommended, phased).** When a player caravan
  travels near a ruin world object, fire an incident that spawns a Medusa ambush on
  the generated map. Vanilla precedent: `IncidentWorker_CaravanAmbush` /
  `CaravanMeeting`. Detection: a patch on `Caravan.Tick` (or a world-object-proximity
  check) that finds `Find.WorldObjects.AllWorldObjects` of type `AbandonedSettlement`
  /`AbandonedCamp`/`AbandonedLandmark` within N tiles of the caravan. Medusa lies in
  wait among the stones - thematically perfect.
- **(b) Ruin map pre-population.** Patch the map generator for those world objects
  (e.g. `MapGenerator.GenerateMap` postfix checking `map.Parent.def`) to scatter a
  Medusa (and maybe petrified "statue" pawns) into the interior. Heavier but more
  atmospheric - a medusa *lives* in the ruin.
- **(c) Wander-in near player ruins.** Weakest fit; player maps rarely sit on ruins.

Recommend **(a) now, (b) later**. Caveat: abandoned-colony *sites* on the world map
aren't guaranteed to exist in every world, so gate on `Any` of those objects existing
and fall back to cave wander-ins when none do.

## 5. Malef Dragon creation - Dark Dragon's Blood (redesigned 2026-09-05)

**The tease-knockout corruption and the colony transformation offer were both
scrapped.** A Malef Dragon no longer transforms victims by combat or consent. She
is made **only by an item**: `PMM_DarkDragonsBlood` (`Defs/ThingDefs/Item_DarkDragonsBlood.xml`),
a `NeverForNutrition` drug. Its `IngestionOutcomeDoer_DarkDragonsBlood`
(`Source/Reptiles/DarkDragonsBlood.cs`) remakes the drinker in two steps, one vial
each (chain set 2026-09-22):
- **an ordinary woman** (any woman still corruptible - the drink is not limited to
  baseliners) → `PMM_Reptile_Dragonewt` via `ApplyXenotype` (core
  `CanEverTransform` gates out men, children, monsters, the too-young);
- **a Dragonewt** → `PMM_Reptile_MalefDragon` via core
  `MamonoTransformation.ConvertXenotype` (re-stamp - `ApplyXenotype` refuses an
  already-monster pawn);
- **a normal Dragon** → `PMM_Reptile_MalefDragon` on her first vial, by that same
  re-stamp path.

A Malef Dragon is the end of the chain: the blood does nothing for her, nor for men,
children and non-humans. `PMM_Gene_MalefCorruption` has no mechanic of its own - it
is the marker a Malef carries, which the blood harvest looks for
(`Source/Reptiles/BloodHarvest.cs`). No wild spawn: Malefs appear in Broods
raids/settlements, or are made by the item.

## 6. Files to add/change

```
Defs/XenotypeDefs/Xenotypes_Reptile.xml        (11 xenotypes)
Defs/GeneDefs/Genes_ReptileTraits.xml          (forced-trait genes)
Defs/GeneDefs/Genes_Petrify.xml                (Medusa gene)
Defs/GeneDefs/Genes_MalefCorruption.xml        (Malef gene - now inert flavour, see §5)
Defs/AbilityDefs/Abilities_Petrify.xml
Defs/HediffDefs/Hediffs_Petrified.xml
Defs/IncidentDefs/Incidents_ReptileWild.xml    (wander-ins per species)
Defs/RulePackDefs/RulePacks_Namers_ReptileSpecies.xml (per-species person names)
Defs/ThingDefs/Items_ReptileScales_Species.xml (per-species scale materials, generic art)
Defs/ThingDefs/Item_DarkDragonsBlood.xml       (Malef-creation drug, §5)
Source/Reptiles/ReptileWandersIn.cs            (worker + habitat gates)
Source/Reptiles/PetrifyPowers.cs               (ability + freeze hediff + need-freeze patches)
Source/Reptiles/DarkDragonsBlood.cs            (ingest-transform doer, §5)
Source/Reptiles/RuinsAmbush.cs                 (Medusa caravan-ambush incident)
```
**Edits:** pawnkinds swap the phase-1 `GeneInjection` shim for `xenotypeSet` rosters
(per-faction mixes above); delete `Source/Reptiles/GeneInjection.cs`; faction
pawnkind `xenotypeChances` per locked roster. New pawnkinds for the wild-only
Bunyip (no faction) and the per-species wild kinds.

## 7. Decisions (all locked 2026-09-05)

1. **Volcanic (Salamander):** Odyssey lava/volcano world-tile feature ONLY, gated
   `MayRequire="Ludeon.RimWorld.Odyssey"`. Without Odyssey no volcanic spawns;
   Salamanders then use caves only. ✔
2. **Traits:** combined **trait-genes** (Biotech `<forcedTraits>`, the `KindInstinct`
   pattern) - germline, so wild and colony reptiles share the personality. ✔
3. **Wetlands (Wurm):** marsh/swamp biome. ✔
4. **Petrify:** nothing immune by species; the gaze still needs a working, uncovered
   eye (Sight >= 20%, no `FullHead`/`Eyes` apparel). Frozen pawns are simply **downed** for 2 days (not
   carried, not statue-items). ✔
5. **Scales:** per-species scale materials added WITH the xenotypes, reusing the
   generic `PMM_ReptileScale` art (single texture) until a later art pass. Each
   species' shedding drops its own material. ✔
6. **Icons:** placeholder art generated via `Tools/mktex.py` for all new xenotype
   and ability icons. ✔
