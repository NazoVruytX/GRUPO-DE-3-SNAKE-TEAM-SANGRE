"""
Genera los sprites pixel art del Snake (PNG RGBA) usando solo la libreria estandar de Python.

Uso (desde la raiz del repositorio):
    python Tools/generar_sprites.py [carpeta_salida] [carpeta_vista_previa]

Por defecto escribe en Assets/Sprites. Si se indica una carpeta de vista previa,
tambien guarda cada sprite ampliado y una maqueta del tablero para revisarlos.
"""
import os
import struct
import sys
import zlib

OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join("Assets", "Sprites")
PREVIEW = sys.argv[2] if len(sys.argv) > 2 else None


def hexc(h, a=255):
    h = h.lstrip("#")
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


T = (0, 0, 0, 0)


class Img:
    def __init__(self, w, h):
        self.w, self.h = w, h
        self.px = [[T for _ in range(w)] for _ in range(h)]  # px[y][x], y=0 abajo

    def set(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.px[y][x] = c

    def get(self, x, y):
        return self.px[y][x]

    def rect(self, x0, y0, x1, y1, c):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.set(x, y, c)

    def save(self, path, scale=1):
        w, h = self.w * scale, self.h * scale
        raw = bytearray()
        for row in range(h):
            y = self.h - 1 - row // scale  # PNG va de arriba hacia abajo
            raw.append(0)
            for col in range(w):
                raw.extend(self.px[y][col // scale])

        def chunk(tag, data):
            c = struct.pack(">I", len(data)) + tag + data
            return c + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)

        png = b"\x89PNG\r\n\x1a\n"
        png += chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, 6, 0, 0, 0))
        png += chunk(b"IDAT", zlib.compress(bytes(raw), 9))
        png += chunk(b"IEND", b"")
        with open(path, "wb") as f:
            f.write(png)


def rounded_mask(x0, y0, x1, y1, r):
    """Conjunto de pixeles de un rectangulo con esquinas redondeadas (radio en pixeles)."""
    cells = set()
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            dx = max(x0 + r - x, 0, x - (x1 - r))
            dy = max(y0 + r - y, 0, y - (y1 - r))
            if r == 0 or (dx - 0.5) ** 2 + (dy - 0.5) ** 2 <= r * r * 0.9 or dx == 0 or dy == 0:
                cells.add((x, y))
    return cells


def shaded_block(img, mask, base, light, dark, outline):
    """Rellena una forma con contorno, brillo arriba-izquierda y sombra abajo-derecha."""
    for (x, y) in mask:
        img.set(x, y, base)
    for (x, y) in mask:
        neighbours = [(x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)]
        if any(n not in mask for n in neighbours):
            img.set(x, y, outline)
    for (x, y) in mask:
        if img.get(x, y) != base:
            continue
        if (x, y + 1) in mask and img.get(x, y + 1) == outline or (x - 1, y) in mask and img.get(x - 1, y) == outline:
            img.set(x, y, light)
        elif (x, y - 1) in mask and img.get(x, y - 1) == outline or (x + 1, y) in mask and img.get(x + 1, y) == outline:
            img.set(x, y, dark)


# Paleta
G_OUT = hexc("#1B4D2E")
G_BASE = hexc("#4CC764")
G_LIGHT = hexc("#8CEB8F")
G_DARK = hexc("#2F9A4C")
WHITE = hexc("#FFFFFF")
BLACK = hexc("#111111")
RED = hexc("#E5484D")
RED_DARK = hexc("#A8283A")
RED_OUT = hexc("#5E1020")
RED_LIGHT = hexc("#FF9AA0")
BROWN = hexc("#6B4226")
LEAF = hexc("#5BD16F")
LEAF_DARK = hexc("#2F9A4C")


def snake_head():
    img = Img(16, 16)
    mask = rounded_mask(1, 1, 14, 14, 3)
    shaded_block(img, mask, G_BASE, G_LIGHT, G_DARK, G_OUT)
    # Ojos mirando a la derecha
    for ey in (9, 4):
        img.rect(9, ey, 11, ey + 2, WHITE)
        img.rect(10, ey + 1, 11, ey + 2, BLACK)
    # Fosas nasales
    img.set(13, 8, G_OUT)
    img.set(13, 7, G_OUT)
    return img


def snake_body():
    img = Img(16, 16)
    mask = rounded_mask(1, 1, 14, 14, 2)
    shaded_block(img, mask, G_BASE, G_LIGHT, G_DARK, G_OUT)
    # Escama central
    for (x, y) in [(7, 8), (8, 8), (6, 7), (9, 7), (7, 6), (8, 6)]:
        img.set(x, y, G_DARK)
    img.set(7, 7, G_LIGHT)
    img.set(8, 7, G_LIGHT)
    return img


def food_apple():
    img = Img(16, 16)
    cx, cy, r = 7.5, 6.5, 5.6
    mask = set()
    for y in range(16):
        for x in range(16):
            if (x - cx) ** 2 + ((y - cy) * 1.05) ** 2 <= r * r:
                mask.add((x, y))
    shaded_block(img, mask, RED, RED_LIGHT, RED_DARK, RED_OUT)
    img.rect(4, 8, 5, 9, RED_LIGHT)
    img.set(4, 10, RED_LIGHT)
    # Tallo y hoja
    img.rect(7, 12, 7, 14, BROWN)
    img.rect(8, 13, 10, 14, LEAF)
    img.set(11, 14, LEAF)
    img.set(9, 13, LEAF_DARK)
    img.set(10, 13, LEAF_DARK)
    return img


def board_checker():
    img = Img(32, 32)
    a, b = hexc("#1A2133"), hexc("#1E263A")
    for y in range(32):
        for x in range(32):
            img.set(x, y, a if ((x // 16) + (y // 16)) % 2 == 0 else b)
    return img


def wall_block():
    img = Img(16, 16)
    base, light, dark, mortar = hexc("#46506E"), hexc("#5E6A8E"), hexc("#323A52"), hexc("#232A3D")
    img.rect(0, 0, 15, 15, base)
    # Ladrillos: dos filas desplazadas
    for y in (0, 7, 8, 15):
        img.rect(0, y, 15, y, mortar)
    for x in (0, 15):
        img.rect(x, 8, x, 15, mortar)
    img.rect(7, 0, 8, 7, mortar)
    for (x0, y0, x1, y1) in [(1, 9, 14, 14), (0, 1, 6, 6), (9, 1, 15, 6)]:
        img.rect(x0, y1, x1, y1, light)
        img.rect(x0, y0, x1, y0, dark)
    return img


def ui_button():
    """Boton estilo pixel en escala de grises (se tinta con Image.color). Borde 9-slice de 4 px."""
    img = Img(16, 16)
    mask = rounded_mask(0, 0, 15, 15, 2)
    for (x, y) in mask:
        img.set(x, y, WHITE)
    for (x, y) in mask:
        if any(n not in mask for n in [(x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)]):
            img.set(x, y, hexc("#000000"))
    shade = hexc("#B4B4B4")
    for x in range(1, 15):
        for y in (1, 2):
            if (x, y) in mask and img.get(x, y) == WHITE:
                img.set(x, y, shade)
    return img


def ui_panel():
    img = Img(16, 16)
    mask = rounded_mask(0, 0, 15, 15, 2)
    for (x, y) in mask:
        img.set(x, y, WHITE)
    return img


def particle():
    img = Img(4, 4)
    img.rect(0, 0, 3, 3, WHITE)
    return img


def app_icon(head):
    """Icono del ejecutable (256x256): la cabeza de la serpiente ampliada 12 veces."""
    img = Img(256, 256)
    mask = rounded_mask(0, 0, 255, 255, 40)
    for (x, y) in mask:
        img.set(x, y, hexc("#141A28"))
    for y in range(16):
        for x in range(16):
            c = head.get(x, y)
            if c[3]:
                img.rect(32 + x * 12, 32 + y * 12, 43 + x * 12, 43 + y * 12, c)
    return img


os.makedirs(OUT, exist_ok=True)
head = snake_head()
sprites = {
    "snake_head": head,
    "snake_body": snake_body(),
    "food_apple": food_apple(),
    "board_checker": board_checker(),
    "wall_block": wall_block(),
    "ui_button": ui_button(),
    "ui_panel": ui_panel(),
    "particle": particle(),
    "app_icon": app_icon(head),
}
for name, img in sprites.items():
    img.save(os.path.join(OUT, name + ".png"))
    if PREVIEW:
        os.makedirs(PREVIEW, exist_ok=True)
        img.save(os.path.join(PREVIEW, name + "_x8.png"), scale=8 if img.w <= 32 else 4)
print("sprites:", ", ".join(sprites))

if PREVIEW:
    # Maqueta: tablero 12x8 con paredes, serpiente y manzana (para revisar el conjunto)
    W, H = 14, 10
    mock = Img(W * 16, H * 16)
    board, wall = sprites["board_checker"], sprites["wall_block"]

    def blit(src, cx, cy, rot=0):
        for y in range(16):
            for x in range(16):
                sx, sy = x, y
                for _ in range(rot):  # rotacion 90 grados antihoraria
                    sx, sy = sy, 15 - sx
                c = src.get(sx, sy)
                if c[3]:
                    mock.set(cx * 16 + x, cy * 16 + y, c)

    for cy in range(H):
        for cx in range(W):
            if cx in (0, W - 1) or cy in (0, H - 1):
                blit(wall, cx, cy)
            else:
                for y in range(16):
                    for x in range(16):
                        mock.set(cx * 16 + x, cy * 16 + y, board.get((cx * 16 + x) % 32, (cy * 16 + y) % 32))
    for (cx, cy) in [(3, 4), (4, 4), (5, 4), (6, 4), (6, 5)]:
        blit(sprites["snake_body"], cx, cy)
    blit(head, 6, 6, rot=1)
    blit(sprites["food_apple"], 10, 6)
    mock.save(os.path.join(PREVIEW, "mock_x3.png"), scale=3)
