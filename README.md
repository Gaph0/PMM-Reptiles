# Project Mamono Reptiles

Reptile mamonos for Project Mamono, for RimWorld 1.6. It adds eleven species,
two mountain factions, a reptilian gene, and a set of dragonian trade goods.

- **Eleven reptile species.** Basilisk, lamia, medusa, wurm, dragon, lizardman,
  wyvern, malef dragon, dragonewt, salamander and bunyip, each with her own body,
  her own hide and her own corpse.
- **Two mountain factions.** Dragonia is neutral and trades; the Scalebound Broods
  are hostile, raid your colony, and never trade.
- **A reptilian gene.** Sharp scales, a body that wants heat, a single egg in
  place of a live birth, a hide shed once a year, and a cold that puts her to
  sleep instead of killing her.
- **Dragonian trade goods, and dark blood.** A violet metal, a dragon orb that
  calls a wild dragon at night, a wedding collar, a strong wine, and a drug that
  turns an ordinary woman into a dragonewt and a dragonewt into a malef dragon.

**Requires:** Harmony, Biotech, Project Mamono, Big and Small - Framework, Vanilla Expanded Framework

**Optional:** Ideology (the Broods' war meme, and its ruined archotech sites), Odyssey (volcanic hunting grounds for the salamander, and Odyssey's own ruins)

## Content

Eleven reptile mamonos live in the world's mountains, caves, deserts, wetlands,
rivers and lakes, and in the settlements of two factions. You meet them as
raiders, as wild women you can tame, as traders, and as women you take in. Each
one butchers into mamono meat, and her corpse gives her own hide.

### Xenotypes

| Mamono | Lore | Purpose in game | In plain words |
|---|---|---|---|
| **Basilisk** | A venomous hunter of caves and deserts. Her strikes carry a venom that slows a victim down. | A cave hunter and a fighter. She stops a man running, then closes in. | As tough as a human, slower because of the tail, and her venom ends a chase. |
| **Lamia** | A serpent-tailed woman of the high caves and mountains, and a persuasive talker. | A negotiator, recruiter and trader, and a grappler in a fight. | As tough as a human, no quicker, and good with words. |
| **Medusa** | A rarer lamia-kin who lives among cold stones and old ruins. | The one who turns people to stone. She waits in ruins for passing caravans. | As tough as a human and slower, but her gaze freezes you for two days. |
| **Wurm** | A colossal tunneller of deep caves and wet places. Very strong, very slow to learn. | The heaviest melee fighter of the faction. | Bigger than a human and much harder to kill, and slow to learn anything. |
| **Dragon** | A winged queen of the mountain keeps. Fire does not burn her. She is proud and greedy. | A flyer and a heavy fighter, and the only carrier of the dragon's pride gene. | Harder to kill than a human, flies, and no flame can hurt her, but she takes every slight badly. |
| **Lizardman** | A cave swordmaster, quick to anger and quicker with a sword. | A soldier of the faction, and a strong melee fighter. | A little tougher than a human, heals fast, and deadly with a blade. |
| **Wyvern** | A lean, swift cousin of the dragon, without her strength or her pride. | A flyer that carries caravans over mountains and dives on raiders. | About as tough as a human, quicker, and flies. |
| **Malef Dragon** | A corrupted dragon. Her blood carries the dark dragon's blood. | The strongest of the Broods: a flyer with claws and tease, and the source of the drug. | As hard to kill as a dragon, flies, and the most persuasive woman here. |
| **Dragonewt** | A woman remade by dark dragon's blood: scaled, teasing, and a strong talker. | A fast talker of the Broods, and the rank below a malef dragon. | About as tough as a human, with claws and a fine way with words. |
| **Salamander** | A fire-kissed lizardman breed of volcanic caves and cooling lava. | A fireproof fighter of the Broods. | About as tough as a human, and fire cannot touch her. |
| **Bunyip** | A strong, clawed lamia-kin of rivers and lakes, and a poor talker. | Wild and tameable, and one of the strongest fighters here. She belongs to no faction. | Tougher than a human and strong in a close fight, and hopeless at talking. |

Bunyips are wild only. Malef dragons and dragonewts are never wild: they come
from the Broods, or from a vial of dark dragon's blood.

### Genes

The seven genes this mod adds. Every species also carries the core Mamono gene,
and several carry Project Mamono's venom, fiery body or claws.

- `reptilian` - sharp scales, a body that wants heat, a single egg in place of a
  live birth, and a hide shed once a year. Sharp armour +15%, comfortable minimum
  temperature +10°C, and cold cannot kill her: it slows her, then puts her to
  sleep until she is warm again. Metabolic efficiency -1, complexity 2.
- `lamia tail` - a serpent's lower body, and the tail grapple. Metabolic
  efficiency -1, complexity 1.
- `petrifying gaze` - the gaze that freezes a victim in place. Metabolic
  efficiency -1, complexity 2.
- `malef corruption` - her blood carries the corruption, which the surgery and
  the drug look for. Metabolic efficiency -1, complexity 2.
- `dragon's pride` - arrogant and greedy from birth. Metabolic efficiency -2.
- `wurm's mind` - slow to learn, and half a body larger than her sisters. Body
  size +0.5.
- `wedding collar` - while a collar is worn she is neither greedy nor arrogant.
  No metabolic cost.

### Events

- **A wild mamono wanders in** - a wild reptile walks onto your map and can be
  tamed. Each species arrives in its own land: the mountains and caves bring
  lamias, basilisks, wurms, wyverns, dragons, medusas, lizardmen and salamanders,
  the desert brings a basilisk, the wetlands a wurm, and rivers and lakes a
  bunyip. Each kind is rare, and only the right land produces it.
- **A dragon answers the orb** - a dragon orb that is installed and lit at night
  can draw a wild dragon to your colony, whatever the land around you is like.
  When a dragon arrives, the orb becomes a glowing dragon orb.
- **A medusa in the ruins** - a caravan that passes close to a ruin can be
  ambushed. One medusa waits among broken walls and stone figures. The ambushers
  carry anyone they down away with them.
- **Dragonia comes to trade** - a dragonian caravan arrives with scales,
  dragonium, dragon orbs, wedding collars and wine for sale, and buys art. A
  visiting group from the faction can include a trader of its own.

### Abilities

- `tail grapple` - "Lash out with her tail and drag the victim into her coils.
  Constricted victims cannot get away while her teasing embrace wears their will
  down." He is pulled next to her and held for an hour, and he can try to wrench
  free every few minutes - a small chance, better if he is stronger than she is.
  The coils hold while she carries him off, and let go if she goes down.
  Cooldown 6 hours, range 5 cells, no cost.
- `petrifying gaze` - "Fix a victim with the stone-cold gaze of the medusa. The
  target is frozen where she stands - downed, hunger and weariness held in
  perfect stasis - for two days." She wakes standing when the stone lets go. The
  gaze needs a working, uncovered eye: a blind woman, or one wearing a war mask
  or a veil, cannot be turned. Cooldown 7 days, range 10 cells, no cost.

### Factions

- **Dragonia** - a medieval mountain faction of dragons and humans. Neutral: it
  starts at peace, sends caravans and visitors that trade with you, and can be
  befriended or offended. It fights only if you make it an enemy.
- **Scalebound Broods** - lewdist raiders of the same mountains, out to claim men
  and remake women. Permanent enemies: they raid, and they never trade or make
  peace. Their raiders and captured camps carry vials of dark dragon's blood.

## Found a bug?

Report it on the issue tracker: https://github.com/Gaph0/PMM-Reptiles/issues

Please do not leave bug reports in the comments section. Post them on the
tracker so they can be tracked and fixed.

## License and attributions

Licensed under the Unlicense. See the
[licence](https://github.com/Gaph0/PMM-Reptiles/blob/main/LICENSE.txt).

Thanks to:

- Gapho - the mod.
- Project Mamono (PMM.Core) - the mamonos, the bonds and the tease, and the shared art.
- Big and Small - Framework - the serpent and winged bodies, and the race pattern.
- Vanilla Expanded Framework - the framework under the mod.
- Harmony - the patches under everything.
- Biotech - genes and xenotypes at all.
- Ideology - the Broods' war meme, and the ruined archotech sites.
- Odyssey - the volcanic hunting grounds, and its own kinds of ruin.
