# PMM Reptiles — own race defs for the 11 species

**DONE 2026-09-20.** All 11 species have their own race def, and every one of them is
on the family's shared "momo corpses" line. What follows is the reasoning and the
record of what shipped; where the two disagree, the XML and the code are the truth.

Before this, a reptile pawn was a vanilla Human pawn wearing a reptile xenotype: her
body came from a gene body swap and her corpse was an ordinary `Corpse_Human` shared
with every human in the game, so she could never join a shared corpse line.

---

## 1. Why

| Gap today | What a race def fixes |
|---|---|
| A dead dragonian leaves an ordinary `Corpse_Human`, so reptiles cannot join the core "momo corpses" line. | Her own corpse def, registered with `ProjectMomo.MomoCorpses`. |
| Temperature range, melee tools, body size, meat, leather and flesh live in genes. | They move to the race def, the same home the other three mods use. |
| No Big and Small race tracker, so no per-species armour row or custom graphics. | A tracker, like the slime and elemental races have. |
| Her body comes from a gene body swap, which only works while she is Human. | The body sits on the race def, where it always works. |

## 2. The one rule that shapes the whole plan

**A gene `thingDefSwap` never fires on a pawn who has her own race def.** Big and
Small discards only the `Human` def and a couple of others
(`RaceMorpher.IsDiscardable`); for any other race the swap candidate list ends up
holding her own def, so nothing changes.

That is exactly why the lamia, medusa and bunyip tails and the dragon, wyvern and
malef wings work today: a reptile pawn is literally Human, so the swap wins. Give
her a race def and every one of those swaps stops working.

So the body must move onto the race def, and the gene must stop swapping:

- **Tail species:** `<race><body>BS_Naga_Body</body></race>` plus the Naga tracker in
  the race's `BigAndSmall.RaceExtension` (`<raceHediffList>`). The tracker draws the
  tail. Recipe proven on the insect races, 2026-09-20.
- **Winged species:** body `BS_HumanoidWithWings_Body` plus the winged tracker. The
  strong flight gene raises the `ShowBaseWingsRight` / `ShowBaseWingsLeft` flags so
  the tracker draws its full wings; the weak gene raises no flags and draws its own
  smaller nodes instead.
- **One renderer only.** The Big and Small tracker draws the swapped body. Never ship
  a second render hediff for the same art, or it is drawn twice.

## 3. The 11 species

| Species | Race def | Body | Her Health row (the tracker) |
|---|---|---|---|
| Basilisk | `PMM_Race_Basilisk` | `BS_SnakeHuman` | `BS_Naga_Race` |
| Lamia | `PMM_Race_Lamia` | `BS_SnakeHuman` | `BS_Naga_Race` |
| Medusa | `PMM_Race_Medusa` | `BS_SnakeHuman` | `BS_Naga_Race` |
| Wurm | `PMM_Race_Wurm` | `BS_SnakeHuman` | `BS_Naga_Race` |
| Bunyip | `PMM_Race_Bunyip` | `BS_SnakeHuman` | `BS_Naga_Race` |
| Dragon | `PMM_Race_Dragon` | `BS_HumanoidWithWings_Body` | `BS_HumanoidWithWings_Race` |
| Wyvern | `PMM_Race_Wyvern` | `BS_HumanoidWithWings_Body` | `BS_HumanoidWithWings_Race` |
| Malef Dragon | `PMM_Race_MalefDragon` | `BS_HumanoidWithWings_Body` | `BS_HumanoidWithWings_Race` |
| Lizardman | `PMM_Race_Lizardman` | Human (inherited) | `PMM_RaceTracker_Lizardman` |
| Dragonewt | `PMM_Race_Dragonewt` | Human (inherited) | `PMM_RaceTracker_Dragonewt` |
| Salamander | `PMM_Race_Salamander` | Human (inherited) | `PMM_RaceTracker_Salamander` |

Five serpent-tailed, three winged, three plain. The Basilisk and the Wurm are tailed
too - both carry `PMM_Gene_LamiaTail`, which the first draft of this plan missed by
reading the summary table in `XENOTYPES.md` instead of the xenotype XML.

## 4. What each race def carries

Follow the slime and elemental pattern (`Race_SlimeMomo.xml`,
`Race_ElementalMomo.xml`):

- `<ThingDef ParentName="Human">` with a defName such as `PMM_Race_ReptileDragon`,
  and a label and description players read.
- A `BigAndSmall.RaceExtension` with the body's race tracker. Point the tracker at
  one per species only if the species ever needs its own armour, size or romance
  tags; one shared tracker is what the slimes do.
- `statBases` for everything a gene cannot change: comfort temperature range,
  `SM_BodySizeMultiplier`, `MeatAmount`.
- `leatherDef` pointing at that species' scale item
  (`Defs/ThingDefs/Items_ReptileScales_Species.xml`) plus a `LeatherAmount`. This is
  what makes butchering yield the right scales.
- Melee tools, where the species has claws or teeth.
- A race tracker hediff (`ParentName="BS_DefaultRaceTracker"`) carrying
  `BigAndSmall.CompProperties_Race` with `canSwapAwayFrom`, so a transformed pawn can
  swap away cleanly.

## 5. Wiring that has to change with the races

- **Xenotypes.** Each of the 11 `XenotypeDef`s needs
  `BigAndSmall.XenotypeExtension` with `setRace` (the new race def) and
  `forceRace: true`. This is the same table the slime and elemental mods got on
  2026-09-20; `forceRace` is required, not cosmetic, or the gene-level swap
  candidates win the tie.
- **Generation and transformation.** The mod's own generation-time postfixes and the
  core `MomoTransformation.ApplyXenotype` path already call Big and Small's
  `TrySwapToXenotypeThingDef`, so a corrupted woman lands on the species race once
  `setRace` is filled in.
- **Wild kinds.** `Defs/PawnKindDefs/PawnKinds_ReptileWild.xml` pins one species per
  kind through the kind's `xenotypeSet`. Check every one of the nine still lands on
  the right race.
- **Faction rosters.** Nothing changes: the member species still comes from each
  faction's `xenotypeSet`.
- **Code that assumes Human.** Grep for the race before starting: `ThingDefOf.Human`,
  `race == Human`, and every place a Human-only gene or body part is added or
  removed.
- **The corpse line.** Done: `Source/Reptiles/ReptileMomoCorpses.cs` registers all
  eleven races through `ReptileDefOf` from its own `[StaticConstructorOnStartup]`.
  The category def and the mover live in the core mod
  (`Source/ProjectMomo/MomoCorpses.cs`), so nothing is written twice. That
  registration was the whole point of this project.

## 6. What shipped

- `Defs/ThingDefs/Races_ReptileMomo.xml` - the eleven race defs, plus our own trackers
  for the three plain species (the tailed and winged ones use B&S's).
- `Defs/XenotypeDefs/Xenotypes_Reptile.xml` - `setRace` + `forceRace` on all eleven
  xenotypes.
- `Source/Reptiles/ReptileMod.cs` - eleven `ReptileDefOf` race fields.
- `Source/Reptiles/ReptileMomoCorpses.cs` - the corpse line registration.

The pawn kinds still say `<race>Human</race>` on purpose: one faction kind and one
wild kind per species all lean on the xenotype to pick the race.

Still to test in game:

1. Every species comes out on her own race - faction pawns, wild wander-ins, raid
   pawns, and a woman corrupted into each species.
2. A tailed or winged momo draws her tail or wings once, from the B&S tracker.
3. Butchering a reptile gives her own scales, and her corpse sits under the "momo
   corpses" line in the butcher menu and the item filters.
4. The Health tab shows ONE row per momo: the species name on a plain-body one
   (lizardman, dragonewt, salamander), and B&S's own "snake-person" / "winged
   humanoid" row on a tailed or winged one.

## 7. Decisions taken

1. **One Health row per momo (user ruling 2026-09-20: option 1).** The tailed five and
   the winged three use Big & Small's own tracker as their only tracker -
   `BS_Naga_Race` / `BS_HumanoidWithWings_Race`, set straight in `raceHediff` - so the
   pawn shows one row, drawn and labelled by B&S ("snake-person", "winged humanoid"),
   which also brings the body's stats and comps. The three plain species keep a
   tracker of ours, labelled with the species. The accepted trade for the eight: the
   row does not carry the species name, which the Bio tab's xenotype still gives.
   A custom, species-labelled row would need our tracker to declare its own copy of
   B&S's render-node XML; that was offered and declined.
   Merging B&S's tracker into a custom one by inheritance is impossible and was tried
   on 2026-09-20: a node is registered for XML inheritance only when it carries a
   `<Name>` attribute (`XmlInheritance.TryRegister` never reads `defName`), and B&S
   declares its trackers with `<defName>` alone. Player.log filled with
   `Could not find parent node named BS_Naga_Race`, and the trackers loaded with no
   art, no stats and no comps.
2. **Plain-body species get a race def too.** It is the only way to give them their own
   corpse and their own hide.
3. **No C# race assignment at generation.** Big & Small applies `setRace` in its own
   `GenerateGenes` postfix, before health is built, which is the right moment for a
   body with different parts. Assigning `pawn.def` in a `PawnGenerator` postfix would
   be too late for a serpent tail.
4. **The tail and flight genes keep their `thingDefSwap`.** It can no longer fire on a
   pawn who has her own race def, and it is a harmless fallback for any Human-race
   pawn. Removing it would gain nothing.
5. **`LeatherAmount` is 30**, the same count the insect momos give in chitin. Raise it
   per species if her hide should be worth more per corpse.

## 8. Where the facts live

- `XENOTYPES.md` — the 11 xenotypes, their genes and their body swaps.
- `HANDOFF.md` §7 and §9 — the gene body-swap findings and the hard-won lessons.
- `Project Momo Insects/HANDOFF.md` §12.9 — how a corpse category is moved, and the
  four attempts it took.
- `Source/ProjectMomo/MomoCorpses.cs` in core — the registration call the reptile mod
  now uses.
