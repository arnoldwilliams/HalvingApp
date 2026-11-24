"""Synthesise the app's sound effects as small mono 44.1 kHz WAV files.

Everything is generated from sine partials with short attack/release envelopes so
the files are tiny, licence-free and never clip.

Usage:  python tools/generate_sounds.py
"""

from __future__ import annotations

import math
import pathlib
import struct
import wave

RATE = 44_100
OUT = pathlib.Path(__file__).resolve().parent.parent / "src" / "Resources" / "Raw"


def _bell(freq: float, duration: float, amplitude: float, decay: float = 6.0) -> list[float]:
    samples = []
    count = int(RATE * duration)
    for i in range(count):
        t = i / RATE
        envelope = math.exp(-decay * t) * min(1.0, t / 0.004)
        wave_value = (
            math.sin(2 * math.pi * freq * t)
            + 0.32 * math.sin(2 * math.pi * freq * 2 * t)
            + 0.12 * math.sin(2 * math.pi * freq * 3 * t)
        )
        samples.append(amplitude * envelope * wave_value / 1.44)
    return samples


def _tone(freq_start: float, freq_end: float, duration: float, amplitude: float) -> list[float]:
    samples = []
    count = int(RATE * duration)
    phase = 0.0
    for i in range(count):
        progress = i / count
        freq = freq_start + (freq_end - freq_start) * progress
        phase += 2 * math.pi * freq / RATE
        envelope = math.sin(math.pi * progress) ** 0.8
        samples.append(amplitude * envelope * (math.sin(phase) + 0.25 * math.sin(2 * phase)))
    return samples


def _mix(*layers: tuple[float, list[float]]) -> list[float]:
    length = max(len(samples) for _, samples in layers)
    out = [0.0] * length
    for offset, samples in layers:
        start = int(offset * RATE)
        for i, value in enumerate(samples):
            if start + i < length:
                out[start + i] += value
    return out


def _silence(duration: float) -> list[float]:
    return [0.0] * int(RATE * duration)


def _write(name: str, samples: list[float], peak: float = 0.85) -> None:
    highest = max(1e-9, max(abs(sample) for sample in samples))
    scale = peak / highest
    frames = bytearray()
    for sample in samples:
        value = int(max(-1.0, min(1.0, sample * scale)) * 32767)
        frames += struct.pack("<h", value)

    OUT.mkdir(parents=True, exist_ok=True)
    path = OUT / f"{name}.wav"
    with wave.open(str(path), "wb") as handle:
        handle.setnchannels(1)
        handle.setsampwidth(2)
        handle.setframerate(RATE)
        handle.writeframes(bytes(frames))
    print(f"{path.relative_to(OUT.parent.parent.parent)}  {len(samples) / RATE:.2f}s")


def correct() -> None:
    samples = _mix(
        (0.00, _bell(1046.50, 0.55, 0.55)),
        (0.09, _bell(1318.51, 0.55, 0.55)),
        (0.18, _bell(1567.98, 0.70, 0.60)),
        (0.26, _bell(2093.00, 0.55, 0.22, decay=9.0)),
    )
    _write("correct", samples, peak=0.75)


def wrong() -> None:
    samples = _mix(
        (0.00, _tone(392.0, 330.0, 0.16, 0.5)),
        (0.16, _tone(330.0, 262.0, 0.24, 0.5)),
    )
    _write("wrong", samples, peak=0.45)


def tap() -> None:
    samples = _mix((0.0, _bell(1400.0, 0.09, 0.4, decay=30.0)))
    _write("tap", samples, peak=0.30)


def star() -> None:
    samples = _mix(
        (0.00, _bell(1567.98, 0.20, 0.35, decay=14.0)),
        (0.05, _bell(2093.00, 0.22, 0.35, decay=14.0)),
        (0.10, _bell(2637.02, 0.26, 0.35, decay=12.0)),
    )
    _write("star", samples, peak=0.40)


def fanfare() -> None:
    melody = [523.25, 659.25, 783.99, 1046.50]
    layers = [(i * 0.13, _bell(freq, 0.75, 0.45)) for i, freq in enumerate(melody)]
    layers.append((0.52, _bell(1046.50, 0.9, 0.35, decay=4.0)))
    layers.append((0.52, _bell(1318.51, 0.9, 0.25, decay=4.0)))
    layers.append((0.52, _bell(1567.98, 0.9, 0.20, decay=4.0)))
    _write("fanfare", _mix(*layers), peak=0.65)


if __name__ == "__main__":
    correct()
    wrong()
    tap()
    star()
    fanfare()
