"""Extract the worksheet artwork from the source PDF as transparent PNGs.

The worksheet is pure vector art, so each item is rendered at high DPI and the
surrounding card background is flood-filled away from the crop border.  Areas of
light colour that are enclosed by the drawing (toast crumb, biscuit highlights)
stay opaque because they are not connected to the border.

Usage:  python tools/extract_artwork.py
"""

from __future__ import annotations

import io
import pathlib

import pymupdf
from PIL import Image

ROOT = pathlib.Path(__file__).resolve().parent.parent
PDF = ROOT / "eyfs_halving_and_sharing_worksheet.pdf"
OUT = ROOT / "src" / "Resources" / "Images"

# name -> (page index, x0, y0, x1, y1) in PDF points, taken from the drawing bounds
ITEMS = {
    "pizza": (0, 106.1, 267.8, 169.1, 330.8),
    "toast": (0, 269.1, 270.8, 326.1, 327.8),
    "breadroll": (0, 427.7, 284.3, 487.7, 314.3),
    "apple": (0, 105.7, 528.3, 125.2, 552.0),
    "biscuit": (0, 101.2, 660.0, 122.2, 681.0),
    "sweet": (1, 99.7, 231.8, 129.0, 245.3),
    "berry": (1, 117.7, 373.5, 132.7, 388.5),
}

PAD = 3.0  # points of breathing room around the tight drawing bounds
MAX_EDGE = 640
MAX_DPI = 2400
BG_TOLERANCE = 26


def _is_background(pixel: tuple[int, int, int]) -> bool:
    return all(channel >= 255 - BG_TOLERANCE for channel in pixel)


def _make_transparent(img: Image.Image) -> Image.Image:
    """Flood fill the background from the border so enclosed light areas survive."""
    img = img.convert("RGBA")
    width, height = img.size
    pixels = img.load()

    seen = bytearray(width * height)
    stack: list[tuple[int, int]] = []

    for x in range(width):
        stack.append((x, 0))
        stack.append((x, height - 1))
    for y in range(height):
        stack.append((0, y))
        stack.append((width - 1, y))

    while stack:
        x, y = stack.pop()
        if x < 0 or y < 0 or x >= width or y >= height:
            continue
        index = y * width + x
        if seen[index]:
            continue
        seen[index] = 1
        if not _is_background(pixels[x, y][:3]):
            continue
        pixels[x, y] = (0, 0, 0, 0)
        stack.extend(((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)))

    return img


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    doc = pymupdf.open(PDF)

    for name, (page_index, x0, y0, x1, y1) in ITEMS.items():
        page = doc[page_index]
        clip = pymupdf.Rect(x0 - PAD, y0 - PAD, x1 + PAD, y1 + PAD)
        dpi = min(MAX_DPI, max(600, round(MAX_EDGE / (max(clip.width, clip.height) / 72))))
        pixmap = page.get_pixmap(clip=clip, dpi=dpi, alpha=True)
        image = Image.open(io.BytesIO(pixmap.tobytes("png")))
        image = _make_transparent(image)

        bounds = image.getbbox()
        if bounds:
            image = image.crop(bounds)

        scale = MAX_EDGE / max(image.size)
        if scale < 1:
            image = image.resize(
                (max(1, round(image.width * scale)), max(1, round(image.height * scale))),
                Image.LANCZOS,
            )

        target = OUT / f"{name}.png"
        image.save(target, optimize=True)
        print(f"{target.relative_to(ROOT)}  {image.size[0]}x{image.size[1]}")


if __name__ == "__main__":
    main()
