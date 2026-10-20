"""Source-canvas access to the trimmed runtime parts (single owner: Sts2WD/animations/realta/combat/parts)."""
from pathlib import Path
import json
from PIL import Image

HERE = Path(__file__).resolve().parent
PARTS_DIR = HERE.parents[3] / 'Sts2WD/animations/realta/combat/parts'
MANIFEST = json.loads((HERE / 'parts.json').read_text())

def canvas(name):
    """The part placed back on its original canvas (VFX parts are upscaled back to canvas units)."""
    m = MANIFEST[name]; im = Image.open(PARTS_DIR / f'{name}.png').convert('RGBA')
    if m['scale'] != 1.0:
        im = im.resize((round(im.width / m['scale']), round(im.height / m['scale'])), Image.Resampling.LANCZOS)
    out = Image.new('RGBA', tuple(m['canvas'])); out.paste(im, tuple(m['offset'])); return out
