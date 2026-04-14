#!/usr/bin/env python3
"""
Sprite sheet processor for CrunchTime2D
- Removes gray background via BFS flood fill from edges (handles gray hoodie safely)
- Auto-detects and crops individual sprites per row band
- Saves transparent PNGs to ART/Edits/ without touching originals
"""

import os
import numpy as np
from PIL import Image
from collections import deque

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
EDITS_DIR = os.path.join(BASE_DIR, "Edits")

BG_TOLERANCE    = 38   # color-distance threshold for background flood fill
MIN_SPRITE_PX   = 150  # min non-transparent pixels to count as a valid sprite
MIN_SPRITE_DIM  = 15   # min width/height in pixels
GAP_THRESHOLD   = 0.03 # fraction of non-transparent pixels to count a row/col as "gap"

# source file -> (output subfolder, optional list of frame names in order)
SOURCES = {
    "Posiciones 1.png": ("male_char/poses_unlabeled", None),
    "3.jpg": ("male_char/poses", [
        "front", "back", "right_profile", "throwing_side",
        "sneaking_side", "waving_front", "right_profile_2",
        "left_profile", "3quarter_front", "looking_back",
        "using_device", "standing_idle",
    ]),
    "4.jpg":  ("male_char/run",    None),
    "5.jpg":  ("female_char/poses", None),
    "6.jpg":  ("female_char/run",  None),
    "7.jpg":  ("enemy_alien",      None),
    "8.jpg":  ("enemy_hydra",      None),
    "9.jpg":  ("enemy_mech",       None),
    "10.jpg": ("enemy_mage",       None),
    "11.jpg": ("enemy_drone",      None),
}


# ---------------------------------------------------------------------------
# Background removal
# ---------------------------------------------------------------------------

def remove_background(img: Image.Image, tolerance: int = BG_TOLERANCE) -> Image.Image:
    """
    BFS flood fill from every edge pixel.  Only pixels reachable from the
    border AND within `tolerance` color distance of the sampled background
    are made transparent.  This keeps interior gray pixels (e.g. gray hoodie)
    intact.
    """
    img_rgba = img.convert("RGBA")
    arr = np.array(img_rgba, dtype=np.uint8)
    h, w = arr.shape[:2]

    # Sample bg from four corners and average
    corners = np.array([
        arr[0,    0,    :3],
        arr[0,    w-1,  :3],
        arr[h-1,  0,    :3],
        arr[h-1,  w-1,  :3],
    ], dtype=float)
    bg = corners.mean(axis=0)

    rgb = arr[:, :, :3].astype(float)
    dist_map = np.sqrt(np.sum((rgb - bg) ** 2, axis=2))  # shape (h, w)

    visited  = np.zeros((h, w), dtype=bool)
    to_erase = np.zeros((h, w), dtype=bool)
    queue = deque()

    # Seed: every edge pixel
    for x in range(w):
        queue.append((0,     x))
        queue.append((h - 1, x))
    for y in range(1, h - 1):
        queue.append((y, 0))
        queue.append((y, w - 1))

    while queue:
        y, x = queue.popleft()
        if visited[y, x]:
            continue
        visited[y, x] = True

        if dist_map[y, x] < tolerance:
            to_erase[y, x] = True
            for dy, dx in ((-1, 0), (1, 0), (0, -1), (0, 1)):
                ny, nx = y + dy, x + dx
                if 0 <= ny < h and 0 <= nx < w and not visited[ny, nx]:
                    queue.append((ny, nx))

    arr[to_erase, 3] = 0
    return Image.fromarray(arr)


# ---------------------------------------------------------------------------
# Sprite detection helpers
# ---------------------------------------------------------------------------

def find_row_bands(alpha: np.ndarray) -> list:
    """Return list of (start, end) row ranges that contain sprite content."""
    h, w = alpha.shape
    row_density = np.sum(alpha > 0, axis=1) / w
    is_gap = row_density < GAP_THRESHOLD

    bands, in_band, start = [], False, 0
    for i in range(h):
        if not is_gap[i] and not in_band:
            in_band, start = True, i
        elif is_gap[i] and in_band:
            in_band = False
            if i - start >= MIN_SPRITE_DIM:
                bands.append((start, i))
    if in_band and h - start >= MIN_SPRITE_DIM:
        bands.append((start, h))
    return bands


def find_col_segs(band_alpha: np.ndarray) -> list:
    """Return list of (start, end) column ranges with content, within a band."""
    h, w = band_alpha.shape
    col_density = np.sum(band_alpha > 0, axis=0) / h
    is_gap = col_density < GAP_THRESHOLD

    segs, in_seg, start = [], False, 0
    for i in range(w):
        if not is_gap[i] and not in_seg:
            in_seg, start = True, i
        elif is_gap[i] and in_seg:
            in_seg = False
            if i - start >= MIN_SPRITE_DIM:
                segs.append((start, i))
    if in_seg and w - start >= MIN_SPRITE_DIM:
        segs.append((start, w))
    return segs


# ---------------------------------------------------------------------------
# Crop and save
# ---------------------------------------------------------------------------

def crop_sprites(img_rgba: Image.Image, output_dir: str, names: list = None) -> int:
    """Detect sprites row-by-row and save each as a numbered/named PNG."""
    arr   = np.array(img_rgba)
    alpha = arr[:, :, 3]

    os.makedirs(output_dir, exist_ok=True)
    count = 0

    for r_start, r_end in find_row_bands(alpha):
        band_alpha = alpha[r_start:r_end, :]

        for c_start, c_end in find_col_segs(band_alpha):
            cell       = arr[r_start:r_end, c_start:c_end]
            cell_alpha = cell[:, :, 3]

            if np.sum(cell_alpha > 0) < MIN_SPRITE_PX:
                continue

            # Tight-crop to actual pixel bounds + small padding
            rows_on = np.any(cell_alpha > 0, axis=1)
            cols_on = np.any(cell_alpha > 0, axis=0)
            r1 = int(np.argmax(rows_on));           r2 = int(len(rows_on) - np.argmax(rows_on[::-1]))
            c1 = int(np.argmax(cols_on));           c2 = int(len(cols_on) - np.argmax(cols_on[::-1]))

            pad = 3
            r1 = max(0, r1 - pad);  r2 = min(cell.shape[0], r2 + pad)
            c1 = max(0, c1 - pad);  c2 = min(cell.shape[1], c2 + pad)

            sprite = cell[r1:r2, c1:c2]
            if sprite.shape[0] < MIN_SPRITE_DIM or sprite.shape[1] < MIN_SPRITE_DIM:
                continue

            count += 1
            fname = f"{names[count-1]}.png" if (names and count - 1 < len(names)) else f"sprite_{count:02d}.png"
            Image.fromarray(sprite).save(os.path.join(output_dir, fname))
            print(f"    [{count:02d}] {fname}  ({sprite.shape[1]}x{sprite.shape[0]}px)")

    print(f"  -> {count} sprites saved")
    return count


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    os.makedirs(EDITS_DIR, exist_ok=True)
    print(f"Output root: {EDITS_DIR}\n{'='*60}")

    for source_file, (subfolder, names) in SOURCES.items():
        src = os.path.join(BASE_DIR, source_file)
        if not os.path.exists(src):
            print(f"[SKIP] {source_file} — not found\n")
            continue

        print(f"\n[{source_file}]  ->  {subfolder}/")
        try:
            img = Image.open(src)
            print(f"  Size: {img.size}  Mode: {img.mode}")

            img_rgba = remove_background(img)

            # Save full sheet (bg removed) in the top-level character/enemy folder
            top = subfolder.split("/")[0]
            sheet_dir = os.path.join(EDITS_DIR, top)
            os.makedirs(sheet_dir, exist_ok=True)
            base = os.path.splitext(source_file)[0]
            sheet_path = os.path.join(sheet_dir, f"{base}_sheet.png")
            img_rgba.save(sheet_path)
            print(f"  Sheet (no bg): {os.path.relpath(sheet_path, EDITS_DIR)}")

            out_dir = os.path.join(EDITS_DIR, subfolder)
            crop_sprites(img_rgba, out_dir, names)

        except Exception as e:
            print(f"  [ERROR] {e}")

    print(f"\n{'='*60}\nDone.  Check: {EDITS_DIR}")


if __name__ == "__main__":
    main()
