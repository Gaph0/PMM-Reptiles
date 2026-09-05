#!/usr/bin/env python3
"""Generates the mod's placeholder textures (pure stdlib: zlib + struct).
Each texture is drawn by a small function on an RGBA pixel grid and written
as a PNG. Run from the mod root:  python3 Tools/mktex.py
"""
import math, struct, zlib, os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

def write_png(path, w, h, px):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    raw = b"".join(b"\x00" + bytes(px[y * w * 4:(y + 1) * w * 4]) for y in range(h))
    def chunk(tag, data):
        c = tag + data
        return struct.pack(">I", len(data)) + c + struct.pack(">I", zlib.crc32(c) & 0xffffffff)
    png = (b"\x89PNG\r\n\x1a\n"
           + chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, 6, 0, 0, 0))
           + chunk(b"IDAT", zlib.compress(raw, 9))
           + chunk(b"IEND", b""))
    with open(path, "wb") as f:
        f.write(png)

class Canvas:
    def __init__(self, w, h):
        self.w, self.h = w, h
        self.px = bytearray(w * h * 4)
    def blend(self, x, y, r, g, b, a):
        a = max(0, min(255, int(a)))
        if 0 <= x < self.w and 0 <= y < self.h and a > 0:
            i = (y * self.w + x) * 4
            na = a + self.px[i + 3] * (255 - a) // 255
            if na == 0:
                return
            for c, v in ((0, r), (1, g), (2, b)):
                self.px[i + c] = max(0, min(255, (v * a + self.px[i + c] * self.px[i + 3] * (255 - a) // 255) // na))
            self.px[i + 3] = na
    def disc(self, cx, cy, rad, col, soft=1.0):
        r, g, b, a = col
        for y in range(int(cy - rad - 2), int(cy + rad + 2)):
            for x in range(int(cx - rad - 2), int(cx + rad + 2)):
                d = math.hypot(x - cx + .5, y - cy + .5)
                if d <= rad:
                    self.blend(x, y, r, g, b, a)
                elif d <= rad + soft:
                    self.blend(x, y, r, g, b, int(a * (rad + soft - d) / soft))
    def ellipse(self, cx, cy, rx, ry, col, soft=1.0):
        r, g, b, a = col
        for y in range(int(cy - ry - 2), int(cy + ry + 2)):
            for x in range(int(cx - rx - 2), int(cx + rx + 2)):
                d = math.hypot((x - cx + .5) / max(rx, .1), (y - cy + .5) / max(ry, .1))
                if d <= 1:
                    self.blend(x, y, r, g, b, a)
                elif d <= 1 + soft / max(rx, ry) * 2:
                    self.blend(x, y, r, g, b, int(a * max(0, (1 + soft / max(rx, ry) * 2 - d) / (soft / max(rx, ry) * 2))))
    def line(self, x0, y0, x1, y1, wdt, col):
        r, g, b, a = col
        steps = int(max(abs(x1 - x0), abs(y1 - y0), 1)) * 2
        for i in range(steps + 1):
            t = i / steps
            self.disc(x0 + (x1 - x0) * t, y0 + (y1 - y0) * t, wdt / 2, col, 0.8)
    def circle_line(self, cx, cy, rad, wdt, col, a0=0, a1=2 * math.pi):
        r, g, b, a = col
        steps = int(abs(a1 - a0) * rad * 2) + 1
        for i in range(steps + 1):
            t = a0 + (a1 - a0) * i / steps
            self.disc(cx + math.cos(t) * rad, cy + math.sin(t) * rad, wdt / 2, col, 0.6)

# ---------------------------------------------------------------- scales
def tex_scales(size=64):
    c = Canvas(size, size)
    rows = [(3, 1.0), (4, 1.0), (5, 1.0), (4, 1.0), (3, 1.0)]
    y = 8
    dark = (74, 94, 48, 255)
    mid = (118, 142, 76, 255)
    lite = (152, 176, 105, 255)
    for ri, (n, _) in enumerate(rows):
        spacing = size / (n + 0.4)
        x = spacing * 0.7
        for si in range(n):
            rad = size * 0.115
            # scute: overlapping disc, lit top-left
            c.disc(x, y, rad, dark, 1.2)
            c.disc(x, y - rad * 0.14, rad * 0.86, mid, 1.2)
            c.disc(x - rad * 0.25, y - rad * 0.32, rad * 0.42, lite, 1.2)
            x += spacing
        y += size * 0.135
    return c

def tex_egg(size=64):
    c = Canvas(size, size)
    cx, cy = size / 2, size / 2 + 1
    # speckled egg: cream shell with scale-green speckles
    c.ellipse(cx, cy, size * 0.30, size * 0.37, (212, 204, 168, 255), 1.4)
    c.ellipse(cx, cy, size * 0.27, size * 0.345, (226, 218, 184, 255), 1.2)
    import random
    rnd = random.Random(7)
    for _ in range(46):
        a = rnd.uniform(0, 2 * math.pi)
        d = math.sqrt(rnd.uniform(0, 1))
        x = cx + math.cos(a) * d * size * 0.24
        y = cy + math.sin(a) * d * size * 0.31
        if ((x - cx) / (size * 0.27)) ** 2 + ((y - cy) / (size * 0.345)) ** 2 < 0.92:
            r = rnd.uniform(size * 0.018, size * 0.038)
            c.disc(x, y, r, (118, 142, 76, 200), 1.0)
    c.ellipse(cx - size * 0.07, cy - size * 0.12, size * 0.07, size * 0.11, (245, 240, 214, 210), 1.4)
    return c

def tex_grapple(size=64):
    # white coil-stack glyph on transparent, like vanilla ability icons:
    # three open rings (widening downward) + a tail rising to the right
    c = Canvas(size, size)
    col = (235, 235, 235, 255)
    cx, s = size / 2, size
    wd = s * 0.065
    # stacked coil arcs (open at the top-right where the tail leaves)
    rings = [(s * .30, s * .115), (s * .385, s * .15), (s * .47, s * .185)]
    for i, (cy, rad) in enumerate(rings):
        a0, a1 = -math.pi * 0.32, math.pi * 1.30
        steps = 160
        for k in range(steps + 1):
            t = a0 + (a1 - a0) * k / steps
            x = cx + math.cos(t) * rad
            y = cy + math.sin(t) * rad * 0.55
            c.disc(x, y, wd / 2, col, 0.7)
    # tail rising from the top ring's end, curving right
    pts = [(cx + math.cos(-math.pi * 0.32) * rings[0][1], rings[0][0] + math.sin(-math.pi * 0.32) * rings[0][1] * 0.55),
           (cx + s * .22, s * .20), (cx + s * .30, s * .12), (cx + s * .38, s * .10)]
    for i in range(len(pts) - 1):
        c.line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], wd * 0.9, col)
    c.disc(pts[-1][0], pts[-1][1], wd * 0.62, col, 0.6)
    return c

def tex_petrify(size=64):
    # white petrifying-gaze glyph on transparent, vanilla ability-icon style:
    # a flattened eye with a slit pupil and three short radiating rays above
    c = Canvas(size, size)
    col = (235, 235, 235, 255)
    cx, cy, s = size / 2, size * 0.58, size
    wd = s * 0.06
    # eye outline (flattened ellipse stroke, full loop)
    steps = 200
    for k in range(steps + 1):
        t = 2 * math.pi * k / steps
        c.disc(cx + math.cos(t) * s * 0.30, cy + math.sin(t) * s * 0.16, wd / 2, col, 0.7)
    # slit pupil
    c.ellipse(cx, cy, s * 0.035, s * 0.10, col, 0.8)
    # rays fanning out above the eye
    for dx in (-0.16, 0.0, 0.16):
        x0 = cx + dx * s
        c.line(x0, cy - s * 0.24, x0 + dx * s * 0.30, cy - s * 0.37, wd * 0.8, col)
    return c

def tex_dark_blood(size=64):
    # a stoppered vial of blackened blood: dark glass body, near-black fill,
    # a cork, and a faint crimson glint
    c = Canvas(size, size)
    cx, s = size / 2, size
    glass = (40, 30, 46, 235)
    blood = (58, 12, 22, 255)
    glint = (150, 40, 54, 210)
    cork = (122, 92, 58, 255)
    # glass vial body (rounded rectangle)
    for y in range(int(s * 0.30), int(s * 0.88)):
        for x in range(int(s * 0.34), int(s * 0.66)):
            c.blend(x, y, *glass)
    # neck
    for y in range(int(s * 0.22), int(s * 0.32)):
        for x in range(int(s * 0.42), int(s * 0.58)):
            c.blend(x, y, *glass)
    # blood fill (lower two-thirds of the body)
    for y in range(int(s * 0.52), int(s * 0.86)):
        for x in range(int(s * 0.36), int(s * 0.64)):
            c.blend(x, y, *blood)
    # crimson glint on the blood surface
    c.ellipse(cx - s * 0.06, s * 0.58, s * 0.05, s * 0.10, glint, 1.0)
    # cork
    for y in range(int(s * 0.16), int(s * 0.26)):
        for x in range(int(s * 0.40), int(s * 0.60)):
            c.blend(x, y, *cork)
    # glass highlight
    c.line(cx - s * 0.12, s * 0.34, cx - s * 0.12, s * 0.80, s * 0.02, (200, 200, 220, 120))
    return c

def icon_dragonia(size=64):
    # white castle/tower on transparent, vanilla world-icon style
    c = Canvas(size, size)
    col = (232, 232, 232, 255)
    def rect(x0, y0, x1, y1, cc=col):
        for y in range(int(y0), int(y1)):
            for x in range(int(x0), int(x1)):
                c.blend(x, y, *cc)
    s = size
    # keep
    rect(s * .30, s * .38, s * .70, s * .86)
    # battlements
    for i in range(4):
        rect(s * (.30 + i * .115), s * .30, s * (.36 + i * .115), s * .40)
    # side towers
    rect(s * .14, s * .52, s * .28, s * .86)
    rect(s * .72, s * .52, s * .86, s * .86)
    for x in (0, 1):
        tx = s * (.14 + x * .58)
        for i in range(2):
            rect(tx + i * s * .075, s * .44, tx + i * s * .075 + s * .05, s * .54)
    # gate
    dark = (0, 0, 0, 0)
    for y in range(int(s * .66), int(s * .86)):
        for x in range(int(s * .44), int(s * .56)):
            c.px[(y * c.w + x) * 4:(y * c.w + x) * 4 + 4] = b"\x00\x00\x00\x00"
    return c

def icon_broods(size=64):
    # coiled snake strike glyph on transparent
    c = Canvas(size, size)
    col = (232, 232, 232, 255)
    cx, cy, s = size / 2, size * 0.58, size
    # tight lower coil
    c.circle_line(cx, cy, s * 0.20, s * 0.075, col, 0, 2 * math.pi * 0.92)
    # rising neck curve to the strike head
    pts = [(cx + s * .19, cy - s * .04), (cx + s * .30, cy - s * .18), (cx + s * .28, cy - s * .34), (cx + s * .12, cy - s * .40)]
    for i in range(len(pts) - 1):
        c.line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], s * 0.07, col)
    # head wedge
    c.disc(pts[-1][0], pts[-1][1], s * 0.065, col, 0.8)
    c.line(pts[-1][0], pts[-1][1], pts[-1][0] - s * .12, pts[-1][1] - s * .02, s * 0.035, col)
    return c

def preview(w=1200, h=600):
    c = Canvas(w, h)
    # layered mountain bands
    bands = [((36, 48, 34, 255), 0.55), ((52, 68, 44, 255), 0.68), ((74, 94, 58, 255), 0.80), ((100, 122, 76, 255), 0.90)]
    for bi, (col, base) in enumerate(bands):
        for x in range(w):
            ridge = base + 0.05 * math.sin(x * 0.004 + bi * 2.2) + 0.03 * math.sin(x * 0.013 + bi * 5.1)
            for y in range(int(ridge * h), h):
                c.blend(x, y, *col)
    # sky glow
    for y in range(h):
        for x in range(w):
            i = (y * w + x) * 4
            if c.px[i + 3] < 255:
                glow = max(0, 1 - math.hypot(x - w * .5, y - h * .34) / (w * .55))
                c.blend(x, y, int(34 + 90 * glow), int(42 + 70 * glow), int(30 + 36 * glow), 255 - c.px[i + 3])
    # sun behind the peaks
    sun = Canvas(w, h)
    sun.disc(w * .5, h * .30, 92, (226, 196, 110, 255), 3)
    for y in range(h):
        for x in range(w):
            i = (y * w + x) * 4
            if sun.px[i + 3] and c.px[i + 3] < 255:
                c.blend(x, y, 226, 196, 110, sun.px[i + 3])
    return c

# Simple species silhouettes on a tinted disc — placeholder xenotype icons.
def xeno_icon(r, g, b, glyph):
    c = Canvas(64, 64)
    # tinted background disc
    c.disc(32, 32, 30, (r // 3, g // 3, b // 3, 255), 1.5)
    c.circle_line(32, 32, 28, 3, (r, g, b, 255))
    col = (235, 235, 235, 255)
    glyph(c, col)
    return c

def _serpent(c, col):  # lamia/medusa/bunyip
    c.circle_line(32, 38, 12, 5, col, 0, 2 * math.pi * 0.85)
    c.line(42, 34, 46, 18, 4.5, col)
    c.disc(47, 16, 5, col, 0.7)

def _fang(c, col):  # basilisk
    c.line(24, 18, 24, 40, 5, col)
    c.line(40, 18, 40, 40, 5, col)
    c.line(24, 40, 32, 50, 4, col)
    c.line(40, 40, 32, 50, 4, col)

def _wing(c, col):  # dragon/wyvern/malef/dragonewt
    pts = [(16, 40), (30, 18), (48, 16), (40, 30), (52, 34), (36, 40), (24, 46)]
    for i in range(len(pts) - 1):
        c.line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], 4, col)

def _blade(c, col):  # lizardman/salamander
    c.line(22, 46, 42, 16, 4, col)
    c.line(20, 34, 32, 48, 3.5, col)
    c.disc(43, 15, 4.5, col, 0.7)

def _gaze(c, col):  # medusa
    c.ellipse(32, 32, 13, 8, col, 1.0)
    c.disc(32, 32, 5, (0, 0, 0, 0), 0.5)
    c.disc(32, 32, 3, col, 0.5)

def _burrow(c, col):  # wurm
    c.disc(32, 36, 11, col, 1.0)
    c.ellipse(32, 22, 13, 7, col, 1.0)

def _paw(c, col):  # bunyip fallback (unused)
    c.disc(32, 32, 12, col, 1.0)

SPECIES = {
    "PMM_Basilisk": ((110, 150, 90), _fang),
    "PMM_Dragon": ((168, 80, 50), _wing),
    "PMM_Lamia": ((150, 140, 90), _serpent),
    "PMM_Lizardman": ((100, 130, 70), _blade),
    "PMM_Medusa": ((140, 145, 140), _gaze),
    "PMM_Wurm": ((120, 100, 70), _burrow),
    "PMM_Wyvern": ((110, 120, 140), _wing),
    "PMM_MalefDragon": ((80, 60, 90), _wing),
    "PMM_Dragonewt": ((160, 110, 80), _wing),
    "PMM_Salamander": ((190, 90, 40), _blade),
    "PMM_Bunyip": ((90, 120, 90), _serpent),
}

def main():
    t = os.path.join(ROOT, "Textures")
    write_png(os.path.join(t, "Things/Item/Resource/PMM_ReptileScale.png"), 64, 64, tex_scales().px)
    write_png(os.path.join(t, "Things/Item/Resource/PMM_ReptileScale/PMM_ReptileScale_a.png"), 64, 64, tex_scales().px)
    write_png(os.path.join(t, "Things/Item/Resource/PMM_ReptileScale/PMM_ReptileScale_b.png"), 64, 64, tex_scales().px)
    write_png(os.path.join(t, "Things/Item/Resource/PMM_ReptileEgg.png"), 64, 64, tex_egg().px)
    write_png(os.path.join(t, "UI/Abilities/PMM_TailGrapple.png"), 64, 64, tex_grapple().px)
    write_png(os.path.join(t, "UI/Abilities/PMM_Petrify.png"), 64, 64, tex_petrify().px)
    write_png(os.path.join(t, "Things/Item/Resource/PMM_DarkDragonsBlood.png"), 64, 64, tex_dark_blood().px)
    write_png(os.path.join(t, "World/WorldObjects/Expanding/PMM_DragoniaHold.png"), 64, 64, icon_dragonia().px)
    write_png(os.path.join(t, "World/WorldObjects/Expanding/PMM_BroodNest.png"), 64, 64, icon_broods().px)
    write_png(os.path.join(ROOT, "About/preview.png"), 1200, 600, preview().px)
    for name, (rgb, glyph) in SPECIES.items():
        write_png(os.path.join(t, "UI/Icons/Xenotypes", name + ".png"), 64, 64, xeno_icon(*rgb, glyph).px)
    print("textures written")

if __name__ == "__main__":
    main()
