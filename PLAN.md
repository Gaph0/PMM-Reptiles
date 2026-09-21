# Project Momo Reptiles — Design Plan

Reptile momos for Project Momo: two overworld factions in the mountains, a reptilian
physiology gene (eggs + shedding), and a lamia tail-grapple gene. **No new xenotypes in
this phase** — they arrive in a later phase.

Source flavour: `MGEWiki/Reptiles/` (Lamia, Medusa, Basilisk, Lizardman, Salamander,
Dragon, Dragonewt, Wurm, Bunyip, Malef Dragon, Dragonia).

---

## 1. Mod skeleton (mirrors Slime Faction / Elementals)

```
Project Momo Reptiles/
  About/About.xml, Manifest.xml, preview.png, PublishedFileId.txt
  Assemblies/PMM_Reptiles.dll
  Defs/
    FactionDefs/Factions_Reptile.xml
    PawnKindDefs/PawnKinds_Reptile.xml
    ThingDefs/Races_ReptileMomo.xml                 (the 11 species races, 2026-09-20)
    TraderKindDefs/TraderKinds_Dragonia.xml         (shipped 2026-09-20)
    GeneDefs/Genes_Reptile.xml
    AbilityDefs/Abilities_Lamia.xml
    HediffDefs/Hediffs_Reptile.xml               (egg-laying, shedding + constricted hediffs)
    ThingDefs/Item_ReptileScale.xml
    ThingDefs/Item_ReptileEgg.xml
    BackstoryDefs/Backstories_Reptile.xml
    RulePackDefs/RulePacks_Namers_Reptile.xml
    ThoughtDefs/Thoughts_Reptile.xml             (optional "fresh scales" moodlet)
  Languages/English/
  Source/Reptiles/*.cs
  Textures/ (item icons, ability icon, world/settlement icons)
  build.sh, release.sh, CHANGELOG.md, README.md, .gitignore
```

- **packageId** `PMM.Reptiles`, **assembly** `PMM_Reptiles.dll`, **namespace** `PMM_Reptiles`.
- **Dependencies** (About.xml, same block style as Slime Faction): Harmony, Biotech,
  `PMM.Core`, and **Vanilla Expanded Framework**
  (`OskarPotocki.VanillaFactionsExpanded.Core`, workshop 2023507013 — hard requirement:
  its gene/hediff/thing comps power the egg-laying, see §3). **Ideology is a soft dep**
  (changed 2026-09-05): the only use is the Broods' forced Monster Extremists meme, and
  the `MayRequire`-gated `requiredMemes` entry degrades gracefully without the DLC.
  Odyssey only if we reuse its assets — decide later.
- `loadAfter`: Harmony, Biotech, PMM.Core, VEF.
- `build.sh`: same `csc` pattern as the Slime Faction script; references RimWorld managed
  DLLs, Harmony, `VEF.dll`, and `ProjectMomo.dll` (build core first if missing).

## 2. The two factions (first FactionDefs in the whole project)

The base mod's `Patches/DisableVisibleFactions.xml` removes all vanilla factions, so
these two will be prominent on the world map.

### Neutral faction — "Dragonia" (user-approved name)
Canon grounding: MGE's Dragonia is "a great country where dragons and humans coexist",
a "natural fortress" covered and surrounded by large mountains — a perfect fit.
- `FactionDef`: natural-enemy = false, decent initial goodwill, no sieges; can be
  befriended, traded with (settlements + later caravans), and offended.
- **Tech level: Medieval** (user ruling — MGE lizardman-mercenary feel), medieval gear tags.
- Pawn kinds: villager, warrior, archer/thrower, leader.
- Roster (xenotype phase): **normal Dragons and Lizardmen are exclusive to Dragonia**
  (user ruling); plus generic reptilians. Lamias appear in both factions (they're the
  iconic reptile momo and the grapple works for both), exact mixes decided with xenotypes.

### Hostile "Lewdist" faction — "Scalebound Broods" (user-approved name)
- Permanent enemy (pirate-style): `canStageAttacks`, `canSiege`, goodwill locked hostile.
- **Tech level: Medieval**.
- **Always spawns with the Monster Extremist meme — and no other required meme** (user
  ruling: Monster Extremist only): FactionDef `<frequentMemes>` with
  `ProjectMomo_Meme_MonsterExtremists` (defined in the core mod's
  `Defs/IdeologyDefs_MonsterExtremists.xml`, `MayRequire="Ludeon.RimWorld.Ideology"`) —
  same mechanism vanilla's `NudistTribe` uses for Nudism.
- Roster (xenotype phase, user ruling): **Malef Dragons, Dragonewts and Salamanders are
  exclusive to the Scalebound Broods**, alongside every other reptile momo **except
  normal Dragons and Lizardmen**. Raid pawn kinds lean on lamia grapplers (below) plus
  scale-warrior melee.

### Mountain/cave-only settlement placement
Vanilla offers **no XML hook** for per-tile settlement filters → Harmony patch on
`RimWorld.Planet.TileFinder.RandomSettlementTileFor`: for our two factions, require
`WorldGrid[tile].hilliness == Hilliness.Mountainous` (re-roll with a try cap, then fall
back to LargeHills so worldgen can never soft-lock). "Caves" come free: mountainous
tiles generate overhead-mountain map areas (the same "caves" check the Slime Faction's
bubble-slime incident uses). Keep `startingCountAtWorldCreation` /
`requiredCountAtGameStart` low (2–4) so they pepper mountain ranges.

### Interim gene wiring (no xenotypes yet)
PawnKindDef `xenotypeSet` only references xenotypes, and we aren't making any. Bridge:
a small Harmony postfix on pawn generation for our faction pawn kinds adds
`PMM_Gene_Reptile` (everyone) and `PMM_Gene_LamiaTail` (lamia kinds) as **endogenes**
post-generation. This whole shim is deleted when the xenotype phase lands and the
pawnkinds switch to `xenotypeSet` entries.

## 3. Reptilian gene — `PMM_Gene_Reptile`

XML (`Defs/GeneDefs/Genes_Reptile.xml`), style-matched to `Gene_SlimeGel.xml`:
- `statOffsets`: `ArmorRating_Sharp` **+0.15** (modest), `ComfyTemperatureMin` **+10 °C**
  (cold-blooded: comfort band shifts up; vanilla handles the discomfort consequences).
- `geneClass` `PMM_Reptiles.Gene_Reptile`; `customEffectDescriptions` list egg-laying,
  shedding, sharp hide, heat-loving.
- Endogenes-only, `canGenerateInGeneSet` false (same ruling as the slime gel gene).

### Egg-laying — the Alpha Genes / VEF pattern, pure XML (user ruling)
Alpha Genes implements oviparous birth entirely with **Vanilla Expanded Framework
comps, zero custom C#** — and Slime Faction's build already references VEF.dll. We
follow that example exactly:
1. `PMM_Gene_Reptile` carries a `VEF.Genes.GeneExtension` modExtension with
   `hediffToWholeBody = PMM_Hediff_EggLaying` (the `AG_EggLaying` → `AG_HumanEggLaying`
   wiring).
2. `PMM_Hediff_EggLaying` (whole body, isBad false) with
   `VEF.Genes.HediffCompProperties_HumanEggLayer`: `eggFertilizedDef =
   PMM_ReptileEggFertilized`, `eggLayFemaleOnly = true`, `eggLayIntervalDays` ~9,
   `eggProgressUnfertilizedMax` 0.5. VEF's comp intercepts the pregnancy: no birth bed,
   no morning sickness — the mother **lays one fertilized egg** (user ruling: one egg
   only; the comp lays a single egg per pregnancy).
3. `PMM_ReptileEggFertilized` ThingDef (abstract base on `OrganicProductBase`, category
   `EggsFertilized`, stackLimit 1, `CompProperties_TemperatureRuinable` 0–50 °C,
   not edible) with `VEF.Genes.CompProperties_HumanHatcher`
   (`hatcherDaystoHatch` ~15) — the egg stores the actual baby (genetics + core's
   xenotype inheritance intact, since VEF reads the pregnancy/father data) and hatches
   it with a birth letter. Total ~24 days, matching Alpha Genes' pacing.
- The custom-`ApplyBirthOutcome`-patch approach (original draft) is the **fallback only**,
  if VEF's egg-layer proves incompatible with core momo pregnancy in testing — check
  against `BabyCustomXenotypeBirthPatch` early.
- Edge cases: egg destroyed/rotted/burned or left outside 0–50 °C → baby lost (grief
  thought optional); ruined-egg visuals come free from `CompProperties_TemperatureRuinable`.

### Yearly shedding (C#)
- Hidden hediff `PMM_Hediff_Shedding` added in `Gene_Reptile.PostAdd` — the
  `PMM_Hediff_SlimeJellyOozing` pattern (gene adds hidden hediff, hediff does the timed
  work). One shed per year (3,600,000 ticks, optionally aligned to spring).
- On shed: drop a stack of `PMM_ReptileScale` at the pawn's feet + a subtle message;
  optional short moodlet "fresh new scales" (+2, 3 days).

### Scales material — `PMM_ReptileScale`
- `stuffProps` category **Leathery** → usable in every recipe that takes leather/wool.
- Stat factors, reptile-flavoured: slightly better sharp armor than plain leather, decent
  heat insulation, poor cold insulation; market value around leather.
- One generic material now; per-species scales (lamia/dragon/etc.) in a later phase.

## 4. Lamia grapple — `PMM_Gene_LamiaTail` + `PMM_Ability_TailGrapple`

- Gene grants the ability via its `<abilities>` block — the **Dorome mud-merge / Genie
  wish pattern**; the vanilla ability system runs gizmo + cooldown. No custom gene class
  needed unless visuals demand it.
- `AbilityDef`: `Verb_CastAbility`, range ~4–6, `canTargetPawns`, hostile, warmup ~1 s,
  cooldown ~45 s.
- `CompAbilityEffect_TailGrapple` (C#, `PMM_Reptiles`):
  1. Validate target (pawn, body size cap ~2.5, no mechanoids).
  2. **Pull** the target to a cell adjacent to the caster (lamia coils drag them in).
  3. Apply `PMM_Hediff_Constricted`: Moving capacity → 0 (can't walk off), periodic
     **tease damage** ticks — ties into the core mod's TeaseDamage/essence system, so a
     constricting lamia literally drains her victim's willpower. Expires after N ticks,
     or instantly if the caster is downed/killed/teleports.
- Hostile AI: gene-granted hostile abilities are used by raider pawns (impid fire-spew
  precedent), so lewdist lamias will grapple colonists in raids with no extra AI work.

## 5. Assembly layout (`Source/Reptiles/`)

| File | Contents |
|---|---|
| `ReptileMod.cs` | `[StaticConstructorOnStartup]` entry, `Harmony("PMM.Reptiles").PatchAll()`, `ReptileDefOf` |
| `Gene_Reptile.cs` | PostAdd/PostRemove of the shedding hediff |
| `Shedding.cs` | Hidden shedding hediff: year timer, scale drop, message |
| `TailGrapple.cs` | CompProperties + CompAbilityEffect + `Hediff_Constricted` behaviour |
| `FactionPlacement.cs` | `TileFinder.RandomSettlementTileFor` patch (mountain-only) |
| `GeneInjection.cs` | Interim post-generation gene adder for faction pawnkinds (deleted in the xenotype phase) |

(Egg-laying needs no C# — pure VEF-comp XML, see §3.)

## 6. Assets needed
- Textures: reptile scales item, fertilized egg item, tail-grapple ability icon, two
  settlement/world icons, `preview.png`. Placeholders can reuse vanilla icons first.
- Rule packs for faction/pawn naming; one reptile backstory category (the
  one-childhood/one-adulthood slime pattern) if pawn kinds get backstory filters.

## 7. Build order / checklist
1. Skeleton + About.xml + build.sh compiles an empty assembly.
2. Genes + scales + egg XML (VEF comps); assembly: shedding, grapple. Dev-test on a colonist.
3. FactionDefs + pawnkinds + group makers + placement patch; new-world test.
4. Gene-injection shim; raid/trade tests; lewdist ideo meme check.
5. Textures, namers, backstories, flavour text polish; CHANGELOG; release.sh.

## 8. Test plan (dev mode)
- **Worldgen**: new world → both factions exist, settlements only on Mountainous tiles;
  lewdist faction's ideoligion contains Monster Extremists.
- **Grapple**: dev "raid with faction" → lamia pulls + constricts a colonist; constriction
  ends on timer and on caster down.
- **Eggs**: reptilian-gene colonist becomes pregnant → lays **exactly one** fertilized
  egg (~9 days), no birth bed; egg hatches (~15 days) into the correct baby (genes
  inherited per core rules — verify VEF composes with `BabyCustomXenotypeBirthPatch`);
  destroying/ruining the egg loses the baby; egg outside 0–50 °C deteriorates.
- **Shedding**: time-warp a year (or dev-trigger) → scales drop; scales accepted as
  leather at a tailoring bench.
- **Startup**: no XML red errors; assembly loads after PMM.Core.

## 9. Later phases (out of scope now)
- Xenotypes: Lamia, Medusa, Basilisk, Lizardman, Salamander, Dragonewt, Dragon, Malef
  Dragon, Wurm, Bunyip — replacing the gene-injection shim; pawnkind `xenotypeSet` mixes
  per faction. **Faction exclusivity (user ruling 2026-09-03)**: Lizardmen + normal
  Dragons → Dragonia only; Malef Dragons, Dragonewts, Salamanders → Scalebound Broods
  only; everything else shared.
- Per-species scale materials; scale apparel; ~~Dragonia trader caravans~~ (shipped
  2026-09-20: `Defs/TraderKindDefs/TraderKinds_Dragonia.xml` + `caravanTraderKinds`).
- Incidents/visitors (basilisk/medusa events), quests. Dragonia guest visits are covered by
  vanilla's own visitor-group incident, whose visitors can trade via `visitorTraderKinds`;
  the custom lamia-guest incident was built and then removed on 2026-09-20 in favour of it.

## 10. Open questions — all resolved 2026-09-03
1. Names: **Dragonia** (neutral), **Scalebound Broods** (lewdist). ✔
2. Lewdist ideoligion: **Monster Extremists only** (no Nudism). ✔
3. Roster: **Malef Dragons / Dragonewts / Salamanders exclusive to the Broods**;
   Broods include all other reptile momos **except normal Dragons and Lizardmen**
   (those two are Dragonia-only). ✔
4. Eggs: **one egg per birth**, mechanic follows **Alpha Genes' example** (VEF comps). ✔
5. Tech level: **Medieval** (both factions). ✔
