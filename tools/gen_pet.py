# -*- coding: utf-8 -*-
"""Ten-pose build: expand 10 character poses into a full desktop-companion animation set.

Usage:
    python gen_pet.py --src poses --out path/to/mod/pet/aigirl

The ten source PNGs are named by pose key (as produced by gen_character.py):
    stand, blink, mouth, think, sleep, raised, happy, shy, typing, wave
"""
import argparse
import math
import os
import shutil
from collections import deque
from PIL import Image, ImageDraw, ImageFont

_ap = argparse.ArgumentParser(description="Expand 10 poses into a companion animation set.")
_ap.add_argument("--src", default="poses", help="folder holding the 10 pose PNGs")
_ap.add_argument("--out", default="pet_out", help="output folder for the animation set")
_args = _ap.parse_args()

SRC_DIR = _args.src
OUT = _args.out
CANVAS = 500
BOTTOM_Y = 480
MODES = ["Nomal", "Happy", "PoorCondition", "Ill"]

POSES = {
    "stand":  ("stand.png", 400),
    "blink":  ("blink.png", 400),
    "mouth":  ("mouth.png", 400),
    "think":  ("think.png", 400),
    "sleep":  ("sleep.png", None),   # horizontal composition: fit by width
    "raised": ("raised.png", 400),
    "happy":  ("happy.png", 400),
    "shy":    ("shy.png", 400),
    "typing": ("typing.png", 380),
    "wave":   ("wave.png", 400),
}

def remove_bg(img, tol=18):
    img = img.convert("RGBA")
    w, h = img.size
    px = img.load()
    def near_white(p):
        r, g, b = p[0], p[1], p[2]
        return r > 255 - tol * 3 and g > 255 - tol * 3 and b > 255 - tol * 3 and \
               abs(r - g) < tol and abs(g - b) < tol and abs(r - b) < tol
    visited = bytearray(w * h)
    q = deque()
    for x in range(w):
        q.append((x, 0)); q.append((x, h - 1))
    for y in range(h):
        q.append((0, y)); q.append((w - 1, y))
    while q:
        x, y = q.popleft()
        if x < 0 or y < 0 or x >= w or y >= h:
            continue
        i = y * w + x
        if visited[i]:
            continue
        visited[i] = 1
        if not near_white(px[x, y]):
            continue
        px[x, y] = (255, 255, 255, 0)
        q.extend(((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)))
    return img

def load_pose(fname, target_h):
    img = Image.open(os.path.join(SRC_DIR, fname))
    # 大图先缩小加速泛洪
    if img.width > 1100:
        s = 1100 / img.width
        img = img.resize((1100, int(img.height * s)), Image.LANCZOS)
    img = remove_bg(img)
    img = img.crop(img.getbbox())
    if target_h is None:  # 横构图按宽度适配
        scale = 400 / img.width
    else:
        scale = target_h / img.height
    img = img.resize((max(1, int(img.width * scale)), max(1, int(img.height * scale))), Image.LANCZOS)
    return img

print("loading poses...")
P = {k: load_pose(f, h) for k, (f, h) in POSES.items()}
for k, v in P.items():
    print(f"  {k}: {v.size}")

def compose(key="stand", dx=0, dy=0, sx=1.0, sy=1.0, rot=0.0, alpha=1.0, draw_fn=None):
    c = P[key]
    if sx != 1.0 or sy != 1.0:
        c = c.resize((max(1, int(c.width * sx)), max(1, int(c.height * sy))), Image.LANCZOS)
    if rot != 0.0:
        c = c.rotate(rot, expand=True, resample=Image.BICUBIC)
    canvas = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
    x = (CANVAS - c.width) // 2 + int(dx)
    y = BOTTOM_Y - c.height + int(dy)
    if alpha < 1.0:
        a = c.getchannel("A").point(lambda v: int(v * alpha))
        c = c.copy()
        c.putalpha(a)
    canvas.alpha_composite(c, (x, y))
    if draw_fn:
        draw_fn(ImageDraw.Draw(canvas))
    return canvas

def save_seq(rel_dir, frames):
    d = os.path.join(OUT, rel_dir)
    os.makedirs(d, exist_ok=True)
    for i, (img, dur) in enumerate(frames):
        img.save(os.path.join(d, f"_{i:03d}_{dur}.png"))

try:
    FONT = ImageFont.truetype("msyhbd.ttc", 44)
    FONT_S = ImageFont.truetype("msyhbd.ttc", 30)
except Exception:
    FONT = FONT_S = None

def zzz(draw, phase):
    draw.text((350, 200 + phase * 6), "Z", font=FONT_S, fill=(90, 70, 130, 200))
    draw.text((385, 165 + phase * 4), "Z", font=FONT, fill=(90, 70, 130, 230))

def dots(draw, n):
    for i in range(n):
        draw.ellipse((345 + i * 26, 95, 345 + i * 26 + 14, 109), fill=(90, 70, 130, 220))

def gen():
    if os.path.exists(OUT):
        shutil.rmtree(OUT)

    # ---- Default 呼吸 (变体1) + 呼吸带眨眼 (变体2/3) ----
    breath = []
    for i in range(8):
        t = math.sin(math.pi * 2 * i / 8)
        breath.append((compose("stand", sy=1.0 + 0.010 * t), 180))
    blinkv = []
    for i in range(6):
        t = math.sin(math.pi * 2 * i / 6)
        blinkv.append((compose("stand", sy=1.0 + 0.010 * t), 170))
    blinkv.append((compose("blink"), 140))
    blinkv.append((compose("stand"), 60))
    for m in MODES:
        save_seq(fr"Default\{m}\1", breath)
        save_seq(fr"Default\{m}\2", blinkv)
    # ---- StartUP: 挥手渐入 ----
    fr = []
    for i in range(5):
        p = (i + 1) / 5
        fr.append((compose("wave", alpha=p, dy=-(1 - p) * 30), 90))
    fr.append((compose("wave"), 500))
    fr.append((compose("stand"), 150))
    for m in MODES:
        save_seq(fr"StartUP\{m}", fr)
    # ---- Shutdown: 挥手渐出 ----
    fr = [(compose("wave"), 400)]
    for i in range(5):
        p = 1 - (i + 1) / 5
        fr.append((compose("wave", alpha=max(p, 0.01), dy=-(1 - p) * 20), 90))
    for m in MODES:
        save_seq(fr"Shutdown\{m}_1", fr)
    # ---- Say 说话: 嘴型交替 + 微弹 ----
    a = [(compose("stand"), 80), (compose("mouth", dy=-2), 100)]
    b = []
    for i in range(6):
        key = "mouth" if i % 2 == 0 else "stand"
        t = math.sin(math.pi * 2 * i / 6)
        b.append((compose(key, dy=-abs(t) * 4, rot=t * 1.2), 150))
    c = [(compose("mouth"), 90), (compose("stand"), 90)]
    for m in MODES:
        save_seq(fr"Say\Default\A_{m}", a)
        save_seq(fr"Say\Default\B_{m}", b)
        save_seq(fr"Say\Default\C_{m}", c)
    # ---- Think 思考: 托腮 + 点点点 ----
    a = [(compose("stand"), 80), (compose("think"), 120)]
    b = []
    for i in range(4):
        nd = (i % 3) + 1
        b.append((compose("think", draw_fn=lambda d, nd=nd: dots(d, nd)), 280))
    c = [(compose("think"), 100), (compose("stand"), 90)]
    for m in MODES:
        save_seq(fr"Think\{m}\A", a)
        save_seq(fr"Think\{m}\B", b)
        save_seq(fr"Think\{m}\C", c)
    # ---- Sleep: 站立→闭眼→躺下 + Zzz ----
    a = [(compose("stand"), 150), (compose("blink"), 300), (compose("sleep", alpha=0.9), 200)]
    b = []
    for i in range(4):
        t = math.sin(math.pi * 2 * i / 4)
        b.append((compose("sleep", sy=1.0 + 0.008 * t, draw_fn=lambda d, i=i: zzz(d, i % 2)), 450))
    c = [(compose("sleep"), 150), (compose("blink"), 250), (compose("stand"), 120)]
    for m in MODES:
        save_seq(fr"Sleep\A_{m}", a)
        save_seq(fr"Sleep\B_{m}", b)
        save_seq(fr"Sleep\C_{m}", c)
    # ---- Raise 提起: 悬空图摆动 ----
    dyn = []
    for i in range(6):
        t = math.sin(math.pi * 2 * i / 6)
        dyn.append((compose("raised", rot=t * 6, dy=-6), 110))
    for m in MODES:
        save_seq(fr"Raise\Raised_Dynamic\{m}\1", dyn)
    a = [(compose("raised", rot=-3, dy=-4), 100)]
    b = []
    for i in range(6):
        t = math.sin(math.pi * 2 * i / 6)
        b.append((compose("raised", rot=t * 4, dy=-5), 140))
    c = [(compose("raised", rot=-1), 90), (compose("stand"), 100)]
    for m in MODES:
        save_seq(fr"Raise\Raised_Static\A_{m}", a)
        save_seq(fr"Raise\Raised_Static\B_{m}", b)
        save_seq(fr"Raise\Raised_Static\C_{m}", c)
    # ---- Touch_Head 摸头: 眯眼笑 + 轻弹 ----
    a = [(compose("stand", sy=0.985), 80), (compose("happy", dy=-3), 100)]
    b = []
    for i in range(4):
        t = math.sin(math.pi * 2 * i / 4)
        b.append((compose("happy", dy=-abs(t) * 7, sy=1.0 + 0.008 * t), 150))
    c = [(compose("happy"), 100), (compose("stand"), 90)]
    for m in MODES:
        save_seq(fr"Touch_Head\A_{m}", a)
        save_seq(fr"Touch_Head\B_{m}", b)
        save_seq(fr"Touch_Head\C_{m}", c)
    # ---- Touch_Body 摸身体: 害羞扭动 ----
    a = [(compose("stand", rot=1), 80), (compose("shy", dy=-1), 100)]
    b = []
    for i in range(4):
        t = math.sin(math.pi * 2 * i / 4)
        b.append((compose("shy", rot=t * 2.5, dx=t * 3), 150))
    c = [(compose("shy"), 100), (compose("stand"), 90)]
    for m in MODES:
        save_seq(fr"Touch_Body\A_{m}", a)
        save_seq(fr"Touch_Body\B_{m}", b)
        save_seq(fr"Touch_Body\C_{m}", c)
    # ---- IDEL 空闲: 打字工作 (随机播放, 很符合AI工具人) ----
    a = [(compose("stand"), 120), (compose("typing", alpha=0.95), 150)]
    b = []
    for i in range(6):
        t = math.sin(math.pi * 2 * i / 6)
        b.append((compose("typing", dy=-abs(t) * 2), 260))
    c = [(compose("typing"), 120), (compose("stand"), 110)]
    for m in MODES:
        save_seq(fr"IDEL\typing\A_{m}", a)
        save_seq(fr"IDEL\typing\B_{m}", b)
        save_seq(fr"IDEL\typing\C_{m}", c)

    total = 0
    for root, _, files in os.walk(OUT):
        total += len([f for f in files if f.endswith(".png")])
    print("generated frames:", total)

if __name__ == "__main__":
    gen()
