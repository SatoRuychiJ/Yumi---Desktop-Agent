#!/usr/bin/env python3
"""
gen_character.py — one reference image -> the ten pose images a companion needs.

Calls the OpenAI image API (gpt-image-1) once per pose, each guided by the reference
image plus a consistency block, so all ten stay the same character on a white background.
Output file names match the keys that gen_pet.py consumes, so the pipeline connects:

    one image  ->  gen_character.py  ->  10 poses  ->  gen_pet.py  ->  a live companion

Usage
-----
    pip install openai
    setx OPENAI_API_KEY sk-...          (or: export OPENAI_API_KEY=sk-...)
    python gen_character.py reference.png --out poses

Then build the animation set from the ten poses:
    python gen_pet.py --src poses
"""
import argparse
import base64
import os
import sys
from pathlib import Path

# --- Consistency block, prepended to every pose (see character_prompts.md) ---
STYLE = (
    "A full-body chibi anime character, the SAME character as the attached reference image: "
    "identical face, hairstyle and hair color, outfit, color palette, art style, line weight and "
    "body proportions. Keep the character 100% consistent with the reference. Front-facing, centered "
    "in frame, full body from head to toe. Pure solid white (#FFFFFF) background, even lighting, no "
    "shadow, no ground line, no border, no text, no extra props unless the pose calls for one."
)

# --- The ten poses. Keys match gen_pet.py exactly. ---
POSES = [
    ("stand",  "standing upright and calm, arms relaxed at the sides, neutral expression, looking straight ahead"),
    ("wave",   "raising one hand and waving hello, cheerful friendly smile"),
    ("happy",  "a big happy smile with both eyes closed (^_^), delighted"),
    ("mouth",  "mouth open mid-sentence as if talking, friendly expression"),
    ("think",  "one hand resting on the cheek, eyes looking upward, thoughtful"),
    ("shy",    "shy and bashful, both hands raised near the face, light blush, glancing away"),
    ("blink",  "standing upright, both eyes gently closed (a blink), calm expression"),
    ("typing", "sitting and typing on a laptop, leaning slightly forward, focused"),
    ("sleep",  "lying down on one side, asleep, eyes closed, peaceful (horizontal composition)"),
    ("raised", "both arms lifted up overhead as if being picked up from above, slightly surprised happy face"),
]


def build_prompt(action: str) -> str:
    return f"{STYLE}\nPose for this image: {action}."


def main() -> None:
    ap = argparse.ArgumentParser(description="Generate the ten companion poses from one reference image.")
    ap.add_argument("reference", help="path to the reference character image")
    ap.add_argument("--out", default="poses", help="output folder (default: poses)")
    ap.add_argument("--model", default="gpt-image-1", help="OpenAI image model")
    ap.add_argument("--size", default="1024x1024", help="output size, e.g. 1024x1024")
    ap.add_argument("--quality", default="high", help="low | medium | high | auto")
    ap.add_argument("--only", default="", help="comma-separated keys to (re)generate, e.g. stand,wave")
    args = ap.parse_args()

    key = os.environ.get("OPENAI_API_KEY")
    if not key:
        sys.exit("Set OPENAI_API_KEY first (e.g. export OPENAI_API_KEY=sk-...).")

    try:
        from openai import OpenAI
    except ImportError:
        sys.exit("The OpenAI SDK is missing. Run: pip install openai")

    ref = Path(args.reference)
    if not ref.exists():
        sys.exit(f"Reference image not found: {ref}")

    out = Path(args.out)
    out.mkdir(parents=True, exist_ok=True)

    wanted = {k.strip() for k in args.only.split(",") if k.strip()}
    poses = [p for p in POSES if not wanted or p[0] in wanted]

    client = OpenAI(api_key=key)
    print(f"Reference: {ref}")
    print(f"Generating {len(poses)} pose(s) into {out.resolve()} with {args.model} ...\n")

    ok, failed = 0, []
    for name, action in poses:
        print(f"  [{name}] {action}")
        try:
            with open(ref, "rb") as fh:
                resp = client.images.edit(
                    model=args.model,
                    image=fh,
                    prompt=build_prompt(action),
                    size=args.size,
                    quality=args.quality,
                )
            (out / f"{name}.png").write_bytes(base64.b64decode(resp.data[0].b64_json))
            ok += 1
        except Exception as exc:  # noqa: BLE001 - report and continue
            print(f"      ! failed: {exc}")
            failed.append(name)

    print(f"\nDone: {ok}/{len(poses)} generated in {out.resolve()}")
    if failed:
        print(f"Failed: {', '.join(failed)} — rerun with --only {','.join(failed)}")
    print("Next: python gen_pet.py --src", out)


if __name__ == "__main__":
    main()
