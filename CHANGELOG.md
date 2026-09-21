# Changelog

## Player-facing

- 2026-09-22: Added a cave matriarch to the Medieval Overhaul snake ruin. The ruin's strongest defender is now a lamia.
- 2026-09-22: Added basilisks, medusas, lamias and wurms to the Medieval Overhaul cave snake faction. A tenth of the faction are ordinary humans.

- 2026-09-20: Removed the vanilla speed genes from lamias and basilisks. Her tail already slows her, so the two only cancelled each other out.
- 2026-09-20: Changed the notes about a refused wander-in to appear only in development mode.
- 2026-09-20: Fixed Dragonia trade caravans failing to arrive. The realm now sends a trader with pack animals and guards.
- 2026-09-20: Added dragonian visiting traders. Some of the visitors Dragonia sends now come to trade.
- 2026-09-20: Added trader caravans to Dragonia. They bring scales, jade, medieval goods and pack animals, and buy art.
- 2026-09-20: Changed the medusa ambush to happen in a proper ruin, with broken walls, stone figures and buried loot.
- 2026-09-20: Added a small mood boost to a reptile momo after she sheds her scales.
- 2026-09-20: Changed a tailed or winged reptile momo to show one Health tab row instead of two.
- 2026-09-20: Added a "momo corpses" line in the butcher menu, holding all eleven reptile momos.
- 2026-09-20: Changed butchering a reptile momo to give her own scales instead of human leather.
- 2026-09-20: Added a species row to the Health tab for every reptile momo.
- 2026-09-19: Added Big and Small - Framework as a required mod.
- 2026-09-19: Added a serpent tail to wyrms and basilisks as well. A wyrm's tail crushes anyone she coils around.
- 2026-09-19: Rebalanced the wurm: +0.5 body size, so she is visibly bigger and eats more.
- 2026-09-19: Changed lamias and medusas to move on a serpent tail instead of legs.
- 2026-09-06: Changed reptile scale yields and prices to match the leather chart.
- 2026-09-05: Added 11 reptile xenotypes, with trait genes, wild wander-ins and habitat gates.
- 2026-09-05: Added the medusa petrifying gaze. It downs the target and holds every need in stasis for 2 days, with a 7-day cooldown. Nothing is immune.
- 2026-09-05: Added the medusa ruins ambush. A caravan that passes near a ruin can be ambushed by Broods medusas lying in wait among the stones.
- 2026-09-05: Changed Ideology to a soft dependency. Without the DLC both factions still spawn, and the Broods simply lose their forced Monster Extremists meme.
- 2026-09-05: Added Dark Dragon's Blood: an ingestible drug that remakes the drinker as a malef dragon. Human baseliners and normal dragons both change. Dragonewts, malefs, men, children and beasts do not.
- 2026-09-05: Changed each faction to one member pawnkind, with the faction's xenotype roster deciding the species mix.
- 2026-09-05: Fixed a faction member tooltip that showed "Baseliner 100%".
- 2026-09-05: Changed faction member pawnkinds to be faction leaders, so factions can generate leaders.
- 2026-09-05: Added the momo claws gene to every clawed species.
- 2026-09-05: Rebalanced dragon's pride: +2 metabolic efficiency.
- 2026-09-05: Fixed wild reptile momos walking straight off the map after they spawned.
- 2026-09-05: Fixed an invalid trait degree on wyrms spamming the log.
- 2026-09-04: Changed both factions to settle in mountains only.
- 2026-09-04: Changed each faction to start with one settlement.
- 2026-09-04: Fixed a crash when a faction settlement was drawn on the world map.
- 2026-09-04: Changed the egg-layer hediff to show egg progress in the Health tab.
- 2026-09-04: Fixed errors that appeared when the mod loaded.
- 2026-09-03: Added the two mountain factions: neutral Dragonia and the hostile Scalebound Broods.
- 2026-09-03: Added the reptilian gene: sharp resistance, warmth-loving, and a yearly shed of usable scales.
- 2026-09-03: Added the lamia tail gene. She grapples with her tail, drags the victim into her coils and constricts them while the tease wears their will down.
- 2026-09-03: Added egg-laying. A reptile momo lays one fertilized egg instead of giving birth.

## Internal

- 2026-09-22: Added `PMM_Reptile_CaveSnakeMatriarch` and pointed MO's `DankPyon_BrigandLeader` KCSG symbol at her.
- 2026-09-22: Added a snake cave faction patch: one reptile member kind, a 0.225 x 4 xenotypeSet roster and a weight of 70 in the Combat group.

- 2026-09-20: Removed `MoveSpeed_VeryQuick` from the lamia and `MoveSpeed_Quick` from the basilisk. Big and Small's tail tracker applies `MoveSpeed` -0.5 to both, so the genes only cancelled the tail's cost. Wyverns keep `MoveSpeed_Quick` (no tail).
- 2026-09-20: Changed the wander-in, execute-failure and spawn messages to go through core's `PMMLog`, so they only appear in development mode.
- 2026-09-20: Added the `PMM_DragoniaVisitor` trader kind and the faction's `visitorTraderKinds`, so a visitor group from Dragonia can include a trader.
- 2026-09-20: Changed the lamia guest's lord job to vanilla's own visit duration (no `durationTicks`), matching `IncidentWorker_VisitorGroup`.
- 2026-09-20: Added the Dragonia trader pawnkind and the faction's Trader pawn group maker. Listing only `caravanTraderKinds` left the faction unable to build a trader caravan, which failed with "has no usable PawnGroupMakers for ... groupKind=Trader".
- 2026-09-20: Removed the lamia guest's local mana handling. Core now fills a visiting momo's mana on spawn and floors her drain.
- 2026-09-20: Added a Dragonia trader kind and wired it to the faction's caravan traders.
- 2026-09-20: Added the lamia guest incident, which uses a long vanilla colony-visit lord job.
- 2026-09-20: Added a map hook that dresses the medusa ambush map as a ruin.
- 2026-09-20: Added a fresh-scales thought that the shedding hediff grants.
- 2026-09-20: Changed the tailed and winged races to use Big and Small's body tracker as their only tracker.
- 2026-09-20: Added a race def and a race tracker for each of the eleven reptile species, on the Big and Small pattern.
- 2026-09-20: Changed the reptile pawnkind and xenotype names to sort together in the dev spawner.
- 2026-09-20: Added `reports/` to `.gitignore`. It holds generated validation output.
- 2026-09-20: Changed the README to match the code.
- 2026-09-20: Removed the custom lamia tail render hediff. The Big & Small naga tracker draws the tail on its own.
- 2026-09-19: Added gene icons for the reptile genes.
- 2026-09-19: Changed the build to use MSBuild.
- 2026-09-05: Added release.sh.
- 2026-09-05: Removed the Alpha Genes and Cyanobot's Genes cosmetic-gene soft dependency. Xenotypes carry only their built-in genes.
- 2026-09-05: Removed the LizardmanFury, Lustful and BunyipTongue trait genes and their xenotype references.
- 2026-09-05: Added HANDOFF.md.
- 2026-09-03: Added the initial mod skeleton: About, build scripts and the assembly entry point.
