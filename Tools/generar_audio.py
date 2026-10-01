"""
Genera los efectos de sonido y la musica (estilo chiptune) del Snake como WAV mono 16 bits,
sintetizando ondas cuadradas, triangulares, senoidales y ruido con la libreria estandar de Python.

Uso (desde la raiz del repositorio):
    python Tools/generar_audio.py [carpeta_salida]

Por defecto escribe en Assets/Audio. La semilla aleatoria es fija, asi que
siempre genera exactamente los mismos archivos.
"""
import math
import os
import random
import struct
import sys
import wave

OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join("Assets", "Audio")
SR = 22050
random.seed(7)


def midi(n):
    return 440.0 * 2 ** ((n - 69) / 12)


def tone(dur, f0, f1=None, kind="square", duty=0.5, vol=0.5, attack=0.004, release=0.02, decay=0.0, vibrato=0.0):
    """Nota con barrido de frecuencia opcional, envolvente y vibrato."""
    f1 = f0 if f1 is None else f1
    n = int(dur * SR)
    out = []
    phase = 0.0
    for i in range(n):
        t = i / SR
        k = i / max(n - 1, 1)
        f = f0 * (f1 / f0) ** k if f0 > 0 and f1 > 0 else f0
        if vibrato:
            f *= 1 + 0.012 * math.sin(2 * math.pi * vibrato * t)
        phase = (phase + f / SR) % 1.0
        if kind == "square":
            s = 1.0 if phase < duty else -1.0
        elif kind == "tri":
            s = 4 * abs(phase - 0.5) - 1
        elif kind == "sine":
            s = math.sin(2 * math.pi * phase)
        else:  # ruido
            s = random.uniform(-1, 1)
        env = 1.0
        if t < attack:
            env = t / attack
        if dur - t < release:
            env *= max(dur - t, 0) / release
        if decay:
            env *= math.exp(-decay * t)
        out.append(s * env * vol)
    return out


def noise(dur, vol=0.5, decay=20.0):
    return tone(dur, 1, kind="noise", vol=vol, decay=decay, release=0.005)


def concat(*parts):
    out = []
    for p in parts:
        out.extend(p)
    return out


def mix_into(buf, samples, start):
    i0 = int(start * SR)
    for i, s in enumerate(samples):
        if i0 + i < len(buf):
            buf[i0 + i] += s


def save(name, samples, peak=0.85):
    m = max(abs(s) for s in samples) or 1.0
    scale = peak / m
    path = os.path.join(OUT, name + ".wav")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, s * scale)) * 32767)) for s in samples))
    print(f"{name}.wav  {len(samples) / SR:.2f}s")


os.makedirs(OUT, exist_ok=True)

# ---------- Efectos ----------
save("sfx_eat", concat(
    tone(0.05, midi(88), kind="square", duty=0.25, vol=0.5),
    tone(0.10, midi(93), kind="square", duty=0.25, vol=0.5, decay=12),
))

crash = [0.0] * int(0.6 * SR)
mix_into(crash, noise(0.35, vol=0.7, decay=9), 0)
mix_into(crash, tone(0.5, 320, 55, kind="square", duty=0.5, vol=0.45, decay=4), 0)
save("sfx_crash", crash)

save("sfx_gameover", concat(
    tone(0.16, midi(67), kind="square", duty=0.5, vol=0.4),
    tone(0.16, midi(64), kind="square", duty=0.5, vol=0.4),
    tone(0.16, midi(60), kind="square", duty=0.5, vol=0.4),
    tone(0.70, midi(48), kind="square", duty=0.5, vol=0.45, vibrato=6, release=0.25),
), peak=0.75)

save("sfx_countdown", tone(0.14, midi(69), kind="square", duty=0.25, vol=0.5, release=0.04))
save("sfx_go", tone(0.40, midi(81), kind="square", duty=0.25, vol=0.5, decay=4, release=0.08))

click = [0.0] * int(0.05 * SR)
mix_into(click, tone(0.035, 1400, 900, kind="square", duty=0.5, vol=0.4, decay=60), 0)
mix_into(click, noise(0.02, vol=0.2, decay=150), 0)
save("sfx_click", click, peak=0.6)

# ---------- Musica en bucle: 8 compases a 128 BPM (La menor) ----------
BPM = 128
BEAT = 60 / BPM
EIGHTH = BEAT / 2
BARS = 8
music = [0.0] * int(BARS * 4 * BEAT * SR)

A2, F2, C3, G2, E2 = 45, 41, 48, 43, 40
chords = [  # (bajo, notas del acorde para el arpegio)
    (A2, [57, 60, 64, 69]),  # Am
    (F2, [53, 57, 60, 65]),  # F
    (C3, [55, 60, 64, 67]),  # C
    (G2, [55, 59, 62, 67]),  # G
    (A2, [57, 60, 64, 69]),  # Am
    (F2, [53, 57, 60, 65]),  # F
    (G2, [55, 59, 62, 67]),  # G
    (E2, [56, 59, 64, 68]),  # E
]
arp_pattern = [0, 1, 2, 3, 2, 1, 0, 1]
melody = [  # compases 5-8: (compas, inicio en corcheas, duracion en corcheas, nota)
    (4, 0, 2, 69), (4, 2, 1, 72), (4, 3, 1, 76), (4, 4, 2, 74), (4, 6, 2, 72),
    (5, 0, 2, 69), (5, 2, 1, 65), (5, 3, 1, 69), (5, 4, 4, 72),
    (6, 0, 2, 71), (6, 2, 1, 74), (6, 3, 1, 79), (6, 4, 2, 77), (6, 6, 2, 74),
    (7, 0, 2, 76), (7, 2, 1, 74), (7, 3, 1, 71), (7, 4, 4, 68),
]

for bar, (bass, notes) in enumerate(chords):
    t0 = bar * 4 * BEAT
    # Bajo (triangular): raiz - raiz - quinta - octava
    for beat, offset in enumerate([0, 0, 7, 12]):
        mix_into(music, tone(BEAT * 0.9, midi(bass + offset), kind="tri", vol=0.35, release=0.03), t0 + beat * BEAT)
    # Arpegio (cuadrada fina)
    arp_vol = 0.12 if bar < 4 else 0.07
    for i, idx in enumerate(arp_pattern):
        mix_into(music, tone(EIGHTH * 0.8, midi(notes[idx] + 12), kind="square", duty=0.25, vol=arp_vol,
                             decay=6, release=0.02), t0 + i * EIGHTH)
    # Bateria: bombo en 1 y 3, caja en 2 y 4, platillo en cada contratiempo
    for beat in range(4):
        tb = t0 + beat * BEAT
        if beat in (0, 2):
            mix_into(music, tone(0.12, 150, 45, kind="sine", vol=0.55, decay=18, release=0.02), tb)
        else:
            mix_into(music, noise(0.10, vol=0.18, decay=30), tb)
        mix_into(music, noise(0.03, vol=0.06, decay=120), tb + EIGHTH)

for bar, start, length, note in melody:
    mix_into(music, tone(EIGHTH * length * 0.92, midi(note), kind="square", duty=0.5, vol=0.16,
                         vibrato=5, release=0.03), bar * 4 * BEAT + start * EIGHTH)

save("music_loop", music, peak=0.8)
