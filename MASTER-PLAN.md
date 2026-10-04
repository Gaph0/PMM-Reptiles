# Project Mamono Reptiles - Master Plan

This file replaces the four older documents as the single entry point.
`PLAN.md` (phase 1), `XENOTYPES.md` (phase 2), `RACES-PLAN.md` (the eleven races)
and `HANDOFF.md` (agent hand-off notes) were written at different times, so they
disagree in places. This file says what is true today.

The rule for every disagreement: **the XML, the C# and the game are the truth.**
A document is only a note about them.

§7 lists every statement this file corrected from the older documents.

---

## 1. What the mod is

- Repo: `/home/gapho/Desktop/Project Mamono Reptiles`. Git repo, branch `master`.
- `packageId` `PMM.Reptiles`, assembly `PMM_Reptiles.dll`, namespace `PMM_Reptiles`.
- Hard requirements: Harmony, Biotech, Vanilla Expanded Framework (VEF),
  `PMM.Core`, Big and Small - Framework.
- Soft requirements, all self-gating: Ideology (the Broods' forced meme)
  and Odyssey (Salamander volcanic spawns).
- Build `./build.sh` (compiles `Source/Reptiles/*.cs` against the RimWorld managed
  DLLs and `../Project Mamono/Assemblies/ProjectMamono.dll`).
  `sync.sh` copies the mod into the live Mods folder. **Always sync before a test**,
  or you test yesterday's code.
- `Tools/mktex.py` makes every placeholder texture and icon.

Eleven species: basilisk, lamia, medusa, wurm, bunyip, dragon, wyvern, malef
dragon, lizardman, dragonewt, salamander.

## 2. Where things stand

| Area | State | Field-tested |
|---|---|---|
| Two factions, mountains-only placement, medieval gear | built | yes, 2026-09-24 |
| Reptilian gene (sharp hide, warmth, egg-laying, shedding) | built | yes, 2026-09-24, egg inheritance included |
| Lamia tail-grapple ability | built | yes, 2026-09-24, AI casting included |
| Eleven xenotypes, faction rosters, wild wander-ins | built | yes, 2026-09-24 |
| Medusa petrify (downs the target, freezes every need 2 days) | built | yes, 2026-09-24, AI casting and the eye rule included |
| Dark Dragon's Blood item + blood harvest surgery | built | yes, 2026-09-24 |
| Medusa ruin ambush + ruin map fill | built | yes, 2026-09-24 |
| Eleven species races, own hides, shared corpse line | built 2026-09-20 | yes, 2026-09-24 |
| Dragonia trader caravans + visiting traders | built 2026-09-20 | yes, 2026-09-24 |
| Fresh-scales moodlet | built 2026-09-20 | yes, 2026-09-23 |
| Dragon's Lifeblood, Dragonia's wine, in trader stock | built 2026-09-22 | yes, 2026-09-24 |
| Dragonium, Dragonia's metal, in trader stock | built 2026-09-22 | yes, 2026-09-24 |
| Dragon orbs, bought lamps that call a dragon at night | built 2026-09-22 | yes, 2026-09-24 |
| Cold torpor instead of hypothermia (the cold slows her, then sleeps her) | built 2026-09-30 | yes, 2026-10-04 |
| One family per woman (`PMM_MamonoFamily` on the identity genes) | built 2026-10-01 | yes, 2026-10-04 |
| Mamono rename (labels, defNames, C# types; older saves do not load) | built 2026-09-27 | yes, 2026-10-04 |
| Effect boxes carry engine lines only, no custom prose | built 2026-09-27 | yes, 2026-10-04 |
| The family's one mamono meat from every reptile corpse | built 2026-10-04 | yes, 2026-10-04 |
| Full in-game test pass (HANDOFF §8 item 5) | **done 2026-09-24**, extended 2026-10-04 | item 21 still open |

Everything through 2026-10-04 is committed and pushed: `main` sits level with `origin/main` at
`4054241`, and the dragon orbs, the wedding collar, the coil and gaze changes and these documents
went in with it.

Confirmed in game **2026-10-04**: cold torpor, the Mamono rename, the effect-box prose sweep, the
family mamono meat, the new mod-page preview image, and one family per woman. **The lamia's coil
rework (§6.1 item 21) is the only test item still open.**

## 3. What is built, and where it lives

### Factions
`Defs/FactionDefs/Factions_Reptile.xml` - neutral **Dragonia**
(`PMM_DragoniaFaction`) and hostile **Scalebound Broods** (`PMM_ScaleboundBroodsFaction`,
`permanentEnemy`, forced Monster Extremists meme). Member species come from each
faction's `xenotypeSet` roster, never from a pawnkind. `Source/Reptiles/FactionPlacement.cs`
forces mountain settlement tiles.

### Genes
- `Defs/GeneDefs/Genes_Reptile.xml` - `PMM_Gene_Reptile` (egg-laying via VEF, shedding),
  `PMM_Gene_LamiaTail` (grapple + `thingDefSwap BS_Naga` + `Flagger ShowBaseAbdomen`),
  the flight gene wiring.
- `Defs/GeneDefs/Genes_ReptileTraits.xml` - **only two trait genes**:
  `PMM_GeneTrait_DragonPride` (Arrogant + Greedy) and `PMM_GeneTrait_WurmMind`
  (SlowLearner, and the Wurm's +0.5 body size). `PMM_Arrogant` itself is core content
  (`Project Mamono/Defs/TraitDefs_Mamono.xml`), written with `commonality` 0 so only a gene can
  hand it out and no random pawn ever rolls it.
- `Defs/GeneDefs/Genes_ReptileMechanics.xml` - `PMM_Gene_Petrify` (live) and
  `PMM_Gene_MalefCorruption` (inert flavour, but it is the marker the blood
  harvest looks for).
- `Source/Reptiles/Shedding.cs` - holds `class Gene_Reptile` (the shedding hediff,
  the yearly scale drop, the fresh-scales thought).

### The eleven races
`Defs/ThingDefs/Races_ReptileMamono.xml`. Every species is a `ThingDef ParentName="Human"`.

| Species | Body | Health-tab tracker |
|---|---|---|
| Basilisk, Lamia, Medusa, Wurm, Bunyip | `BS_SnakeHuman` | `BS_Naga_Race` |
| Dragon, Wyvern, Malef Dragon | `BS_HumanoidWithWings_Body` | `BS_HumanoidWithWings_Race` |
| Lizardman, Dragonewt, Salamander | Human (inherited) | `PMM_RaceTracker_<Species>` |

- Each race sets `leatherDef` to that species' scales and `LeatherAmount` 30, so a
  corpse gives scales instead of human leather.
- The eight tailed/winged species use Big and Small's own tracker as their **only**
  tracker, set straight in `raceHediff`. One Health row per pawn.
- Each race carries only `LeatherAmount` in `statBases` on purpose. Size,
  temperature range, armour and melee tools stay on the genes, because a second
  copy on the race would stack with them.
- `Defs/XenotypeDefs/Xenotypes_Reptile.xml` gives all eleven xenotypes `setRace` +
  `forceRace`. Both are required, not decoration.
- `Source/Reptiles/ReptileMamonoCorpses.cs` puts all eleven corpses on the core
  "mamono corpses" line (`ProjectMamono.MamonoCorpses`).
- `Source/Reptiles/ReptileMod.cs` names every race, scale and related def in
  `ReptileDefOf`, so a rename breaks the build instead of the mod.

### Wild spawns and incidents
`Defs/PawnKindDefs/PawnKinds_ReptileWild.xml` (nine kinds, one species each),
`Defs/IncidentDefs/Incidents_ReptileWild.xml` with `Source/Reptiles/ReptileWandersIn.cs`,
`Defs/IncidentDefs/Incidents_RuinsAmbush.xml` with `Source/Reptiles/RuinsAmbush.cs`,
and `Source/Reptiles/MedusaRuinFill.cs` for the ruin dressing. Habitat checks read
the **world tile**, not the map.

### Trade, guests, mood
`Defs/TraderKindDefs/TraderKinds_Dragonia.xml` (`PMM_DragoniaTrader` for caravans,
`PMM_DragoniaVisitor` for visitors), wired by `caravanTraderKinds` +
`visitorTraderKinds` + a `Trader` pawn group maker on Dragonia only.
Guests use vanilla's own visitor-group incident. Core fills a visiting mamono's mana
on spawn and floors her drain (`VisitingGuestManaPatch`, `GuestManaFloor`).
`Defs/ThoughtDefs/Thoughts_Reptile.xml` holds the fresh-scales moodlet.

### Malef creation
`Defs/ThingDefs/Item_DarkDragonsBlood.xml` is a drug worked in two steps, one vial
each. An ordinary woman who drinks it rises as a Dragonewt, through core
`ApplyXenotype`. A Dragonewt who drinks it becomes a Malef Dragon, and a normal
Dragon skips straight there on her first vial; both of those steps go through
`ConvertXenotype`, the re-stamp path. A Malef Dragon is the end of the chain and the
blood does nothing for her. `Source/Reptiles/BloodHarvest.cs` +
`Defs/RecipeDefs/Recipes_HarvestDarkBlood.xml` add a surgery that draws a vial
from a living Malef at the cost of half her blood.

### Dragon's Lifeblood
`Defs/ThingDefs/Item_DragonsLifeblood.xml` is Dragonia's strong red wine, sold by
`PMM_DragoniaTrader` (15-40 flasks) and `PMM_DragoniaVisitor` (5-15). Its alcohol is
vanilla's own, copied from beer: `AlcoholHigh` and `AlcoholTolerance` as outcome
doers, plus `CompProperties_Drug` (chemical Alcohol) for tolerance, dependency and
chemical recreation. It adds two things: `IngestionOutcomeDoer_DragonsLifeblood`
(`Source/Reptiles/DragonsLifeblood.cs`) tops up mana or essence, whichever need the
drinker has, and the hidden `PMM_Hediff_DragonsLifeblood` shortens the wait between
lovin' through vanilla's `HediffCompProperties_GiveLovinMTBFactor` - no patch, and
it fades on its own. The Broods cannot trade, so their raiders and their captured
camps carry flasks as loot instead: `PMM_BroodsRaidLootMaker` (0.7 chance, 2-6
flasks) on top of the single vial of dark blood that is their signature.

### Dragonium
`Defs/ThingDefs/Item_Dragonium.xml` is Dragonia's metal: a `Metallic` stuff copied from
vanilla plasteel, with seven numbers moved (better heat armour, heat insulation, sharp
damage and beauty; worse hit points, cold insulation and blunt damage) and everything else
identical. Its art is the three-file `Graphic_StackCount` set under
`Textures/Things/Item/Resource/PMM_Dragonium/`. Sold by `PMM_DragoniaTrader` at 50-150,
the exact count vanilla's neolithic shaman-merchant and outlander bulk trader carry of
plasteel; our visitor kind carries none, as vanilla visitors carry no plasteel either.

It sits in `ResourcesRaw`, like every other metal in the game, so it shows up beside plasteel
in the storage filters. That category IS rolled by vanilla's own traders
(`Base_Outlander_Standard`, `Base_Neolithic_Standard` and `Orbital_BulkGoods` each stock raw
resources with a `StockGenerator_Category`), and that generator takes any def in the
category with no tech gate unless it asks for one - so `PMM_Dragonium` is named in all three
of their `excludedThingDefs` lists instead
(`Patches/ExcludeDragonContentFromVanillaStock.xml`, renamed 2026-09-22 when the dragon orbs
moved in beside it), the same mechanism vanilla uses to keep
`Bioferrite` and the Odyssey panels out of them. Nothing else changes: recipes take stuff
categories, and the metal is still `Metallic`. The deep-drill fields were deliberately left
off plasteel's copy, so no deep drill can find it.

The ward. Two stat pairs live in core (`Defs/Stats_Mamono.xml`): `PMM_EssenceRecoveryOffset`
(item, 0.1 a piece) summed by `PMM_EssenceRecovery` through `StatPart_GearStatOffset`, and
`PMM_ManaDrainFactor` (item, 0.95 a piece) multiplied by `PMM_ManaDrain` through
`StatPart_GearStatFactor` - vanilla's own parts, the pair eltex gear uses. Core reads them in
`Need_Essence` (gain x 1 + the bonus) and in `Need_Mana.DrainMultiplier`. A set of three pieces
therefore gives +30% essence recovery and drains mana at 86%. Only WORN apparel counts, but it
counts for any apparel made of the metal, this mod's or another's.

### Dragon orbs
`Defs/ThingDefs/Building_DragonOrb.xml` holds two 1x1 buildings that are never built, only
bought: `PMM_DragonOrb` (the calling orb) and `PMM_DragonOrbDecor` (the glowing orb). Neither
carries a `designationCategory` or a `costList`, the pair that would put it in the Architect
menu, and both carry `minifiedDef MinifiedThing`, so a trader's stock generator hands one over
as a minified object the player installs, uninstalls and carries like any furniture. Sold by
`PMM_DragoniaTrader` alone (one calling orb, one or two glowing orbs); the visitor kind carries
none, as it carries no dragonium. They sit in `BuildingsFurniture`, so they are excluded by
name from the three vanilla stock generators that roll that category, in the same patch as the
metal. Beauty is fixed, not stuffable: the player can never choose a material for a def she
cannot build, and a stuffable orb would be spawned without one.

The lure (`Source/Reptiles/ReptileWandersIn.cs`). At night, and only while an orb is INSTALLED
on a map, a calling orb adds `ReptileWildExtension.orbCandidates` to the roll whatever the
habitat says, entered twice so the light pulls the roll towards a dragon; the separate
`PMM_DragonOrbCall` incident (baseChance 0.5) gives the same orb a chance of its own, so an orb
helps on a desert or a river bank and not only on the mountains and caves dragons already walk.
When the roll picks a species the orb is what allowed, the orb spends itself: it is destroyed
and the glowing orb is spawned in its place, with no message and no letter line (user ruling:
silently). A dragon her own habitat already allowed costs the orb nothing, extra orbs change
nothing, and a minified orb in a stockpile can never call - the lister only ever returns the
building def.

### The wedding collar
`Defs/ThingDefs/Apparel_WeddingCollar.xml` is Dragonia's jewelry for a married dragon: worn on
the neck on the Overhead layer exactly like vanilla's slave collar (Ideology), but working in
reverse - a slave collar suppresses its wearer, and this one is a gift she wants to wear. Not
makeable and not stuffable, and only `PMM_DragoniaTrader` carries it, one or two at a time at
MarketValue 850 ("a dragoon's first month of pay", per the wiki).

While it is on, three things happen (`Defs/HediffDefs/Hediffs_WeddingCollar.xml`,
`Defs/GeneDefs/Genes_WeddingCollar.xml` plus `Source/Reptiles/WeddingCollar.cs`): greedy and
arrogant are suppressed by the collar's own gene, so the engine drops those two rows from her
trait list outright; kind and masochistic are really added, with the hediff's comp recording
which of the two the collar itself granted, so a woman who was already masochistic keeps hers;
and a situational thought (`Thoughts_WeddingCollar.xml`, whose worker is active while a collar
is worn) lifts mood by 3. `suppressedTraits` lives on `GeneDef` and nowhere else - a hediff
cannot suppress a trait, and a code patch on `TraitSet.HasTrait` can only mute the effects -
which is why the nullifying is a gene. The hediff and the gene are the two halves of "is she
wearing one", both added and removed by an idempotent `Sync` hooked to dressing, undressing and
spawning - the last of which covers loading a save.

She wears the collar invisibly. The def has no `wornGraphicPath`, which is not an oversight:
given an empty one, `ApparelGraphicRecordGetter.TryGetGraphicApparel` returns false and
`DynamicPawnRenderNodeSetup_Apparel` gives the apparel no render node, so nothing is drawn on
her (verified in the assembly, not assumed - `Apparel.WornGraphicPath` never falls back to the
item icon, and no config validator asks for one). The item icon is unaffected, so the collar
still reads as a collar on the ground, in a stockpile and in the gear list. The bookkeeping
hediff is hidden from the Health tab the same way a def-level hide is possible at all, by its
comp refusing to be visible; the gene row is the one visible trace, because `GeneDef` offers no
field to hide it.

## 4. Locked decisions

Names, rosters, composition:
1. **Dragonia** (neutral) and **Scalebound Broods** (lewdist). Both medieval. (2026-09-03)
2. Lizardman + normal Dragon are Dragonia only. Malef Dragon, Dragonewt, Salamander
   are Broods only. Everything else is shared. Bunyip is wild only. (2026-09-03)
3. Broods force the Monster Extremists meme and nothing else. (2026-09-03)
4. One member pawnkind per faction; the species mix lives in `FactionDef.xenotypeSet`. (2026-09-05)

Genes and bodies:
5. One egg per birth, built from VEF comps, no custom birth code. (2026-09-03)
6. Only two forced-trait genes: Dragon's Pride and Wurm's Mind. Lizardman, Malef,
   Dragonewt and Bunyip get none. (2026-09-05)
7. Dragon fire-breath is flavour text, not yet an ability. (2026-09-05)
8. Tail and flight genes keep `thingDefSwap`. It cannot fire on a pawn who has her
   own race, and it is a harmless fallback otherwise. (2026-09-20)
9. The body lives on the race, not in a gene, so the tail or wings are there at
   birth. (2026-09-20)

Races and corpses:
10. **One Health row per mamono.** The tailed five and winged three use Big and
    Small's own tracker; the three plain species get a tracker of ours labelled
    with the species. (2026-09-20)
11. Plain-body species get a race def too: it is the only way they get their own
    corpse and hide. (2026-09-20)
12. No C# race assignment at generation. Big and Small applies `setRace` while
    genes are generated, before health is built. (2026-09-20)
13. `LeatherAmount` is 30 for every species. Raise it per species if a hide should
    be worth more. (2026-09-20)
14. No per-species size and no per-species armour rows yet. Save migration is out
    of policy. (2026-09-20)

Trade and events:
15. Only the neutral realm trades. (2026-09-20)
16. Vanilla's visitor-group incident covers Dragonia's guests; the custom lamia
    guest incident was built and then removed. (2026-09-20)
17. The medusa ambush happens in a real ruin, dressed at map setup. (2026-09-20
    build, 2026-09-20 fix)
18. Dark Dragon's Blood works in two steps, one vial each: an ordinary woman rises
    as a Dragonewt, and a second vial makes her a Malef Dragon. A normal Dragon
    skips to the second step. A Malef Dragon is unchanged. (2026-09-22)
19. Dragon's Lifeblood copies vanilla beer's alcohol exactly, and adds two effects:
    a mana or essence top-up, and a hidden two-day hediff that halves the wait
    between lovin' through vanilla's own lovin'-MTB comp. (2026-09-22)
20. The Scalebound Broods stay `permanentEnemy` and never trade. Their share of
    Dragon's Lifeblood reaches the player as raid and settlement loot instead of
    through a trader (user ruling 2026-09-22).
21. Dragonium is plasteel with seven numbers moved, and Dragonia is its only seller. It
    lives in the vanilla `ResourcesRaw` category and is excluded by name from the three
    vanilla raw-resource stock generators, rather than sitting in a category of its own.
    It is not deep-drillable. (2026-09-22, category ruling revised the same day)
22. Worn dragonium wards the wearer: +10% essence recovery a piece, as an offset so a set
    adds up, and mana drain x0.95 a piece, as a factor so a set compounds. The stats live
    in core so any material can use them. (2026-09-22)
23. Dragon orbs are trade goods, not buildings: no Architect entry, one calling orb per
    dragonian caravan, plus a glowing variant. A calling orb works at night only, adds its
    species whatever the habitat, ignores extra orbs, and becomes the glowing orb the moment
    a dragon it called arrives - silently, with no message. Dragons only, no wyverns. Sold
    by the caravan trader alone. (2026-09-22)
24. Dragons are arrogant. `PMM_Arrogant` is a core trait with `commonality` 0, so no random
    pawn ever rolls it and only a gene can hand it out; `PMM_GeneTrait_DragonPride` forces it
    beside Greedy. Jealous was dropped from that gene the same day (user ruling): vanilla lists
    Greedy and Jealous as each other's `conflictingTrait`, so forcing both asked for two traits
    the game itself says cannot coexist. The Dragon xenotype is the only carrier, so malef dragons
    and wyverns are unchanged. No soft dependency on any trait mod is needed, because the trait is
    ours - the idea of gating it on Vanilla Traits Expanded was dropped once that mod turned out
    to have no such trait at all. (2026-09-23)
    Its teeth are code, not XML (`Project Mamono/Source/ProjectMamono/ArrogantTraitPatch.cs`),
    because a TraitDef cannot read another pawn's state: her opinion of anyone below her ISEKAI
    level is docked 2 a level up to 12, and her chance to pick an insult over small talk is
    multiplied by 1.5. The penalty also has to be written onto the screen by hand: vanilla builds
    the Social tab's opinion breakdown from the contributions it knows about, so a patched total
    moves the number and lists no reason for it. A second postfix appends the line in the
    engine's own shape (" - Arrogant (they are 6 levels lower): -6") and only while the penalty
    applies, so a collar takes it off the screen with the modifier. The line says how far below
    her the other pawn stands, because the amount alone does not answer the question a player is
    asking; its wording is a keyed translation in core (`PMM_ArrogantOpinionLine`), with the
    trait label taken from the def. The market value offset it started with was dropped the
    same day. It is mutually exclusive with vanilla's `Kind`, declared on both sides
    (`Project Mamono/Defs/TraitDefs_Mamono.xml` plus
    `Project Mamono/Patches/Trait_ArrogantConflicts.xml`).
25. The wedding collar is Dragonia's jewelry for married dragons: worn on the neck like
    vanilla's slave collar, bought from the caravan trader, not makeable and not stuffable,
    and invisible on the pawn (no `wornGraphicPath`, so the render tree gives it no node; the
    item icon still shows on the ground and in a pawn's gear). While it is on it nullifies
    greedy and arrogant through its own gene's `suppressedTraits` - the only mechanism that
    takes those rows out of her list rather than muting them - gives kind and masochistic for
    real, and lifts mood by 3.
    `Masochist` has no `TraitDefOf` entry in this game build, so it is looked up by defName.
    The bookkeeping hediff is hidden from the Health tab (`CompDisallowVisible`), and the gene
    is the visible trace - `GeneDef` has no field to hide one, so a runtime gene always shows.
    (2026-09-23; the nullifying moved from a `TraitSet.HasTrait` patch to the gene the same day,
    because a patch mutes the effects but cannot take the traits out of her list)
## 5. Facts that bite (do not relearn these)

- A gene `thingDefSwap` never fires on a pawn who has her own race def. Big and
  Small only discards the `Human` def, so a reptile with her own race keeps it.
- `forceRace` is priority 9001 in the gene-generation swap pass. Without it a gene
  body swap can win the tie.
- B&S's trackers cannot be used as `ParentName`. A node is registered for XML
  inheritance only when it carries a `<Name>` attribute, and B&S uses `<defName>`
  alone. Trying it fills the log with "Could not find parent node".
- Child `<li>` list nodes append to the parent's. Use `Inherit="False"` to replace.
- `PatchOperationFindMod` matches a mod's **display name**, not its package id.
  Guard on the def you need instead.
- A `caravanTraderKind` alone does not make caravans work: the faction also needs a
  `Trader` pawn group maker with `traders`, `carriers` and `guards`.
- Vanilla only turns a visitor into a trader for a faction that lists
  `visitorTraderKinds`.
- FactionDefs need both `factionIconPath` and `settlementTexturePath`.
- `XenotypeSet.BaselinerChance` is one minus the sum of the listed chances, so a
  0.9 roster leaves 10% baseliner.
- PawnKindDefs need `initialWillRange` and `initialResistanceRange`.
- Never write files through a terminal heredoc; it mangles them. Use the file tool.
- `FurnitureBase` hard-codes `designationCategory Furniture`, so it cannot be the parent of a
  def that must stay out of the Architect menu. Copy `minifiedDef MinifiedThing` out of it by
  hand instead.
- Vanilla rolls `BuildingsFurniture` in three trader kinds (`Base_Outlander_Standard`,
  `Caravan_Outlander_BulkGoods`, `Orbital_Misc`), and none of the three has an
  `excludedThingDefs` list to append to, so a furniture def needs the whole element added.
- A minified building is a `MinifiedThing`, never its own def, so
  `listerThings.ThingsOfDef(thatDef)` only ever returns installed ones - which is what makes
  "an orb in a stockpile does nothing" free.
- A dragon is a fiery-gene carrier, so core's fire ward lands on whoever she bonds. The Dragon and
  Salamander xenotypes list core's `ProjectMamono_MamonoFiery`, and core's `FireWardComponent` wards
  any humanlike pawn whose living tsugai partner carries that gene - checked hourly, so it lands
  within the hour of the bond. Nothing in either mod is wrong; the two features meet.
- VEF's hatcher writes the conception-time gene mix into the baby's **xenogenes**, not its
  endogenes. An egg-born mamono baby therefore has to have that set stripped, or she carries her
  mother's genes twice - the duplicate row under "Xenogenes" in the gene inspector. `EggCompat.cs`
  drops it and adds the mother's genes back as endogenes.
- A keyed string's `{PAWN_labelShort}`-style tokens **are** valid and are what vanilla uses (113 uses
  of `{0_labelShort}` alone in Core), but they only fill in when the caller passes the matching named
  argument: `"Key".Translate(pawn.Named("PAWN"))`. A call with **no** arguments prints the token at
  the player verbatim, which is how the harvest's "too little blood" warning read as
  `{PAWN_labelShort}` (2026-09-23). Nothing was wrong with the key text.
- A `Recipe_Surgery` subclass must call **`OnSurgerySuccess(...)` itself** from `ApplyOnPawn`, the way
  vanilla's `Recipe_ExtractHemogen` does. Neither obvious alternative works: `base.ApplyOnPawn` does not
  reach the hook, and `base.OnSurgerySuccess` goes to the empty base and skips the override - both were
  tried in game and produced no vial (2026-09-23). And `<anesthetize>` defaults to **true**, so a
  surgery that takes no medicine still put the patient under until it was set false.
- An operation with **no body part to apply to is withheld silently**: `PMM_HarvestDarkBlood`
  targeted a fixed `Torso` part, which resolved to zero parts on a malef (a winged Big and Small
  body), so it never appeared even though `AvailableOnNow` returned true and the recipe was on her
  def's own list. Whole-body operations set `targetsBodyPart false` and drop
  `appliedOnFixedBodyParts`, like vanilla's hemogen extraction. Found 2026-09-23 only by logging the
  live values out of the worker, after three wrong def-side guesses.
- Vanilla's birth popup is `ChoiceLetter_BabyBirth`, and `LetterMaker.MakeLetter(label, text,
  LetterDefOf.BabyBirth, targets)` builds it: the letter class comes from the letter def,
  `Start()` resolves the pawn from the look targets, and it carries Biotech's own naming and
  status choices. A mod gets the same popup by reusing both, not by writing one.
- An ability is invisible to the AI unless **both** halves are present: the `AbilityDef` carries
  `<aiCanUse>true</aiCanUse>` (read by `Ability.AICanTargetNow`) *and* a think-tree node asks for it.
  `hostile` only marks the act as hostile. Vanilla has no generic "cast any hostile ability" node:
  Core's `Abilities_Aggressive` tree is referenced by the raider `DutyDef`s and its single node is
  AnimalWarcall, so each hostile ability needs its own node there - `ThinkNode_ConditionalHasAbility`
  -> `ThinkNode_ConditionalHashIntervalTick` -> a job giver holding the ability's def name.
  That giver cannot be vanilla's `JobGiver_AICastAbility`: it is **abstract**, and its two shipped
  subclasses are hardcoded (one targets the caster, one picks a wild animal), which is why **no XML
  in the game** uses the generic class. Reptiles ships `PMM_Reptiles.JobGiver_AICastHostileAbility`
  (`Source/Reptiles/JobGiver_AICastHostileAbility.cs`) for this: XML sets its public `<abilityDef>`,
  `AttackTargetFinder.BestAttackTarget` finds the caster's best visible enemy, and
  `Ability.AICanTargetNow` has the final say.
  Player pawns are unaffected: the tree sits under a `ThinkNode_ConditionalColonist` with `invert`
  true - and that flag is a **child element**, not an attribute, if you ever go looking for it.
- A pawn spawned straight from dev mode gets **no lord**, so it has no orders: it walks about and
  looks broken. Everything a raider does (walk in, target, assault, kidnap) comes from the lord the
  raid builds, so an AI bug has to be tested with a real raid. A one-session rig did exactly that
  (`PMM_TestRaidMedusa`, a dev-trigger-only incident that fired one-medusa Broods raids); it was
  removed on 2026-09-24 once it had done its job, and the recipe is the bullet below it.
- `IncidentWorker_Raid` picks its group kind with `parms.pawnGroupKind ?? PawnGroupKindDefOf.Combat`,
  read from the incident parms. So a `IncidentWorker_RaidEnemy` subclass that overrides the protected
  virtual `TryExecuteWorker`, sets `parms.faction` + `parms.pawnGroupKind` (+ `raidStrategy`), and
  calls base gets **all** vanilla raid behaviour with pawns of its own choosing. The custom kind needs
  a matching `PawnGroupMaker` on the faction (even if its worker ignores the options) or the game
  refuses to generate pawns for that kind at all. `PawnGroupKindWorker` needs three overrides:
  `GeneratePawns` (protected), `MinPointsToGenerateAnything` and `GeneratePawnKindsExample` (public).
  So are wild mamonos - a pawn only gets that tree through a duty, which means raiders (2026-09-23).
- `uninstallWork` lives under `<building>`, not at the top level of the ThingDef.
- VEF's human egg-layer comp keeps egg progress in its own field and never touches the hediff's
  severity, so `maxSeverity` on that hediff is only a cap. The Health tab's bracketed
  "(egg-bearer)" is vanilla's `Hediff.LabelInBrackets`, which reads the current stage label, so
  it needs no severity at all.
- **A KCSG ruin's garrison belongs to the map's faction, not the pawnkind's, and Medieval
  Overhaul's ruin quests pick that faction at random** (found 2026-09-24). MO's scripts run
  `QuestNode_GetSitePartDefsByTagsAndFaction` before the node that sets `enemyFaction`, and that
  first node reads `slate.Get("enemyFaction")` as its preferred faction, so it is null and
  `SiteMakerHelper` takes a random hostile faction for `siteFaction`; vanilla's `Util_GenerateSite`
  then builds the site with `$siteFaction`. A `KCSG.SymbolDef` with `spawnPartOfFaction` true hands
  its pawn to that faction and ignores its own `<faction>`, and `PawnGenerator.XenotypesAvailableFor`
  puts the faction's `xenotypeSet` on top of whatever pawnkind is generated - so MO's brigand kinds
  spawned as insect mamonos of an insect faction, with only our xenotype-pinned matriarch looking
  right. The whole Medieval Overhaul coupling was removed over this (2026-09-24). Fixing it properly
  means reordering MO's quest nodes; offered and declined (user ruling).
- **Do not leave vestigial code or config behind (user ruling 2026-09-24).** When a feature loses
  its last consumer, delete it in the same pass - the def, the kind, the patch, and the patch file
  when that was the only thing left in it. A self-gating patch that can never fire, or a roster
  nothing reads, is not "kept in case": it reads as live support to the next agent. The Medieval
  Overhaul cave patch went that way: symbol swap first, then the roster and member kind, then the
  file.

## 6. What is left to do

### 6.1 Test debt (one list; run it in game with dev mode on)
Confirmed in game **2026-09-24**: items 1-13 and 15-20, and 14 is closed. Confirmed **2026-10-04**:
items 22-26, the features built after that pass. What is actually left is **21**, the lamia's coil
rework, which has not been tried yet; the items below carry inline marks rather than being deleted.
1. **Confirmed in game 2026-09-24.** Boot with zero red errors; check the log for def-load warnings.
2. **Confirmed in game 2026-09-24.** Worldgen: one Dragonia and one Broods faction, mountains only, icons visible,
   Broods ideoligion has Monster Extremists, leaders generated.
3. **Confirmed in game 2026-09-24.** Every species comes out on her own race: faction pawns, wild wander-ins, raid
   pawns, and a woman corrupted into each species.
4. **Confirmed in game 2026-09-24.** A tailed or winged mamono draws her tail or wings once, from the B&S tracker.
5. **Confirmed in game 2026-09-24.** Health tab shows one row per mamono: the species name on a plain-body one, B&S's
   "snake-person" / "winged humanoid" on a tailed or winged one.
6. **Confirmed in game 2026-09-24.** Butchering a reptile gives her own scales, and her corpse sits under the "mamono
   corpses" line in the butcher menu and the item filters.
7. **Confirmed in game 2026-09-24.** Egg inheritance: a reptile pregnancy lays exactly one fertilized egg, no birth
   bed, and the egg hatches the right baby - her gene list showing her mother's genes
   once, with no xenogene copy, and a colony hatchling raising the vanilla birth popup
   so she can be named. **Highest risk seam** (`EggCompat.cs`).
8. **Confirmed in game 2026-09-24.** Tail grapple: a lamia drags and roots a colonist; it ends on the timer and when
   the caster goes down. A raider lamia uses it on her own - which needs `aiCanUse` and a
   think-tree node (added 2026-09-23, §5); re-test the AI half.
9. **Confirmed in game 2026-09-24.** Petrify: cast on a colonist, she is downed, her needs freeze for two days, she
   thaws standing up. A raider medusa uses it - which needs `aiCanUse` and a think-tree node
   (added 2026-09-23, §5); re-test the AI half with a real raid, because a dev-spawned pawn has no
   lord and cannot show raider behaviour at all (the raid-rig recipe is in §5).
10. **Confirmed in game 2026-09-24.** Dark Dragon's Blood: an ordinary woman's first vial makes her a dragonewt, her
    second makes her a malef dragon, and a normal dragon reaches malef on her first.
    A malef and a man get nothing. Harvest surgery on a living malef gives a vial
    and costs half her blood, and it is refused when it would kill her. The operation
    has to be listed under Operations on her, which it was not until the malef race was
    named in its `recipeUsers` (2026-09-23).
11. Shedding: scales drop once a year and the fresh-scales moodlet appears. **Confirmed in game
    2026-09-23** (tested at a 1-day interval, since reverted to 60).
12. **Confirmed in game 2026-09-24.** Trade: a caravan arrives with scales, jade and medieval goods and
    buys art; a dragonia visitor group arrives and one of them may trade.
13. **Confirmed in game 2026-09-24.** Ruin ambush: a caravan near a ruin is ambushed on a dressed map
    with walls, stone figures and buried loot.
14. **Closed 2026-09-24: nothing left to test.** The Medieval Overhaul coupling is gone - the ruin's
    garrison belongs to whatever faction the quest rolled, so none of it was reachable. See §5 for
    the mechanism and why the patch was not worth keeping.
15. **Confirmed in game 2026-09-24.** Dragon's Lifeblood: a dragonian caravan and a dragonian visitor both arrive with
    flasks for sale; drinking one builds the vanilla alcohol stages over several
    flasks, satisfies chemical recreation, builds alcohol tolerance, tops up mana on
    a mamono and essence on a human, and brings lovin' round sooner for a day or two.
    The hediff itself must not appear on the Health tab.
16. **Confirmed in game 2026-09-24.** Dragonium: a dragonian caravan arrives with 50-150 of it and no other vanilla trader
    ever stocks it (check an outlander base, a neolithic base and an orbital bulk trader);
    armour, weapons and furniture can be made of it from the Metallic stuff list; a full
    outfit is better against heat and worse in the cold than plasteel; a deep drill cannot
    find it; and in the storage filters it sits under raw resources beside plasteel.
17. **Confirmed in game 2026-09-24.** The dragonium ward: a human in a full dragonium set recovers essence faster and the stat
    shows on her card; a mamono in the same set drains mana at 86%; a dragonium sword's own card
    shows the ward figures but carrying it changes nothing.
18. **Confirmed in game 2026-09-24.** Dragon orbs: neither orb appears in the Architect menu; a dragonian caravan sells one
    calling orb and one or two glowing orbs and no other trader ever has either; an installed
    calling orb at night makes a dragon arrive (the dev log says "called by a dragon orb"),
    and the orb becomes the glowing orb in place with no message. A minified orb still in a
    stockpile calls nothing, and the dev log says why by day.
19. **Confirmed in game 2026-09-24.** Arrogant: a freshly generated dragon - wild, faction or player -
    shows the arrogant trait on her Social tab; her opinion of a pawn below her level is docked, and the reasons under
    "Opinion of" list it as " - Arrogant (they are 6 levels lower): -6"; she is also quicker to
    insult them and to fall out with them. A malef dragon and a wyvern do not have it, and no
    random pawn, raider or trader ever spawns with it. Put a wedding collar on her and the
    penalty, and the line explaining it, both go.
20. **Confirmed in game 2026-09-24.** Wedding collar: a dragonian caravan sells one or two. On a dragon, putting one on adds kind
    and masochistic to her trait list, takes greedy and arrogant out of it altogether, raises her
    mood by 3, leaves no collar row on her Health tab, and shows nothing on her body - the collar
    is invisible while worn. Taking it off hands back exactly what it took, so a woman who was
    already masochistic keeps that trait, and greedy and arrogant come back. Her gene list shows
    the collar's gene; her Needs tab shows the mood thought. Save and reload while wearing one,
    and nothing changes.
21. **Added 2026-09-24, not covered by item 8's pass.** The lamia's coil rework: a coiled man wears down
    in about half an hour instead of nearly three; he rolls to break out every eighth of an hour, 10%
    plus his strength advantage over her; the coils hold while she carries him off, and still end if
    she goes down.
22. **Confirmed in game 2026-10-04.** Cold torpor: a reptile in the cold slows down, then falls asleep,
    and wakes when she is warm again. She never takes hypothermia and never freezes to death, and the
    torpor hediff clears once she is back inside her own safe range.
23. **Confirmed in game 2026-10-04.** The Mamono rename: every label, defName and C# type reads Mamono,
    and the mod loads clean with it.
24. **Confirmed in game 2026-10-04.** Effect boxes: no gene prints a prose line in its effect list any
    more, only the engine's own stat and condition lines; the mechanics whose only line was prose
    (tease, venom, petrify) read in the description instead.
25. **Confirmed in game 2026-10-04.** Family mamono meat: butchering any of the eleven species gives
    `PMM_MamonoMeat` rather than that species' own meat, and the corpse still gives her own hide.
26. **Confirmed in game 2026-10-04.** One family per woman: the reptile, insect and slime-gel identity
    genes conflict through `PMM_MamonoFamily`, so a woman loses the other family's gene when she takes
    one, and the gene editors refuse the mix.

### 6.2 Repo chores
Cleared 2026-09-23: the blood harvest surgery has the changelog line it was missing, for the
day it landed; `README.md` points at this file;
the `Thoughts_Reptile.xml` wording is committed; and `PMM_Gene_MalefCorruption` now describes
the blood instead of the scrapped teasing, since its old text sent a player looking for a power
that does not exist. Its lore wording stays the user's to change.

Nothing else is recorded as open here.

### 6.3 Later, if wanted
- Per-species size, armour rows and richer hides.
- Real art to replace the `mktex.py` placeholders.
- A custom species-labelled Health row for the eight whose row is labelled by B&S.
  This needs our tracker to declare its own copy of B&S's render-node XML. It was
  offered and declined once; the copy is the cost.

## 7. Corrections this file makes to the older documents

| Older claim | Truth |
|---|---|
| `PLAN.md` §5 lists `Gene_Reptile.cs` and `GeneInjection.cs` | `GeneInjection.cs` was deleted; `class Gene_Reptile` lives in `Shedding.cs`. Twelve files exist in `Source/Reptiles/` |
| `PLAN.md` §1 lists `Defs/BackstoryDefs/` | No such folder exists |
| `PLAN.md` §3 gives the shedding work to `Gene_Reptile.cs` | It is in `Shedding.cs` |
| `XENOTYPES.md` §0 says the tail gene serves Lamia, Medusa, Bunyip | Five species carry it: basilisk, lamia, medusa, wurm, bunyip |
| `XENOTYPES.md` trait column gives Lizardman "Aggressive", Malef/Dragonewt "Lustful", Bunyip "poor talker" | Those genes were scrapped. Only Dragon's Pride and Wurm's Mind shipped. The Wurm's is SlowLearner degree 0, not TooSmart |
| `XENOTYPES.md` §6 lists `Genes_Petrify.xml` and `Genes_MalefCorruption.xml` | Both genes are in `Genes_ReptileMechanics.xml`. The namer pack is `RulePacks_Namers_Reptile.xml` |
| `RACES-PLAN.md` §2 says the tracker goes in `<raceHediffList>` | It is `<raceHediff>`, which is what §7 decision 1 ruled |
| `RACES-PLAN.md` §4 says races carry comfort range, `SM_BodySizeMultiplier`, `MeatAmount` and melee tools | They carry only `LeatherAmount`. Size, temperature, armour and tools stay on the genes on purpose |
| `RACES-PLAN.md` §4 example name `PMM_Race_ReptileDragon` | The real names are `PMM_Race_<Species>`, e.g. `PMM_Race_Dragon` |
| `HANDOFF.md` §3 "no other optional mods" and §7 "`Patches/` is empty" | `Patches/` holds two patches, neither tied to an optional mod. A Medieval Overhaul patch existed and was removed 2026-09-24 (§5) |
| `HANDOFF.md` §10 names memory files `pmm-reptiles.md` and `pmm-reptiles-phase2.md` | The live repo notes are `/memories/repo/reptile-races.md`, `reptile-features.md` and `reptile-doc-drift.md` |
| All four documents are silent about the blood harvest surgery | It is in this file (§3) |

## 8. Where the facts live

- `MASTER-PLAN.md` - this file. State of the mod, decisions, test list.
- `XENOTYPES.md` - species detail and the §3–§5 design of petrify and the malef item.
  Read it for flavour, not for file paths.
- `PLAN.md` - phase 1 design history (factions, genes, eggs, grapple).
- `RACES-PLAN.md` - the reasoning behind the eleven races, and the B&S findings.
- `HANDOFF.md` - the long-form lessons and the per-item work log.
- `CHANGELOG.md` - what a player sees, newest first.
