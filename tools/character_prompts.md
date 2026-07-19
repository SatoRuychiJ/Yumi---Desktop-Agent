# Character prompt spec

The rule for turning **one reference image** into the **ten poses** a companion needs.

Every pose uses the same consistency block, then swaps in one action. The goal is ten images of the
**same character** — same face, hair, outfit, colours, art style, line weight and proportions — on a
**pure white background**, so they drop straight into `gen_pet.py`.

## Consistency block (prepended to every prompt)

> A full-body chibi anime character, the **same character** as the attached reference image:
> identical face, hairstyle and hair colour, outfit, colour palette, art style, line weight and body
> proportions. Keep the character 100% consistent with the reference. Front-facing, centered in
> frame, full body from head to toe. **Pure solid white (#FFFFFF) background**, even lighting, no
> shadow, no ground line, no border, no text, no extra props unless the pose calls for one.

## The ten poses

| Key | Action |
|-----|--------|
| `stand`  | standing upright and calm, arms relaxed, neutral expression, looking ahead |
| `wave`   | raising one hand and waving hello, cheerful friendly smile |
| `happy`  | big happy smile with both eyes closed (^_^), delighted |
| `mouth`  | mouth open mid-sentence as if talking, friendly expression |
| `think`  | one hand on the cheek, eyes looking up, thoughtful |
| `shy`    | shy and bashful, hands raised near the face, light blush, glancing away |
| `blink`  | standing upright, both eyes gently closed (a blink), calm |
| `typing` | sitting and typing on a laptop, leaning slightly forward, focused |
| `sleep`  | lying on one side asleep, eyes closed, peaceful (horizontal composition) |
| `raised` | both arms lifted overhead as if being picked up, slightly surprised happy face |

Full prompt for a pose = *consistency block* + `"\nPose for this image: " + action + "."`

## Notes

- `sleep` is the only horizontal (lying-down) composition; the others are upright full-body.
- Keep the background pure white — `gen_pet.py` removes it later by edge flood-fill.
- These ten keys are exactly what `gen_pet.py` consumes, so the generated files feed straight in.
