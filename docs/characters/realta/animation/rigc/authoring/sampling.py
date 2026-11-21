"""Keep authored curves and contacts when baking solved joint poses into Spine keys."""

import math

import numpy as np
from scipy.interpolate import CubicHermiteSpline, PchipInterpolator


class Phrasing:
    """Shape-preserving motion between unequal beats, with a stationary start and finish."""

    def __init__(self, times, **channels):
        self.times = times
        self.splines = {}
        for name, values in channels.items():
            slopes = PchipInterpolator(times, values).derivative()(times)
            slopes[0] = 0
            slopes[-1] = 0
            self.splines[name] = CubicHermiteSpline(times, values, slopes)

    def at(self, time):
        time = min(self.times[-1], max(self.times[0], time))
        return {name: float(curve(time)) for name, curve in self.splines.items()}


def sample_times(duration, fps, beats=()):
    uniform = [index / fps for index in range(math.floor(duration * fps) + 1)]
    return sorted({round(time, 9) for time in [*uniform, *beats, duration]})


def kept_indices(times, values, tolerance, protected=()):
    """Keep the points a linear Spine track needs to reproduce this solved curve."""
    included = {0, len(values) - 1, *protected}
    anchors = sorted(included)
    pending = list(zip(anchors, anchors[1:]))
    while pending:
        first, last = pending.pop()
        if last - first < 2:
            continue
        weights = (times[first + 1:last] - times[first]) / (times[last] - times[first])
        predicted = values[first] + weights[:, None] * (values[last] - values[first])
        error = np.max(np.abs(values[first + 1:last] - predicted), axis=1)
        worst = int(np.argmax(error))
        if error[worst] > tolerance:
            keep = first + 1 + worst
            included.add(keep)
            pending.extend([(first, keep), (keep, last)])
    return sorted(included)


def bake(times, poses, beats=()):
    times_array = np.asarray(times)
    canvas = poses[0].canvas
    protected = [i for i, t in enumerate(times) if any(abs(t - beat) < 1e-8 for beat in beats)]
    tracks = []
    for name in canvas.bones:
        if any(name in pose.angles for pose in poses):
            angles = [pose.angles.get(name, 0.0) for pose in poses]
            continuous = np.degrees(np.unwrap(np.radians(angles)))
            numeric_track(tracks, times_array, name, "rotate", continuous[:, None], 0.003, protected)
        if any(name in pose.moves for pose in poses):
            moves = np.array([pose.track_translation(name) for pose in poses])
            numeric_track(tracks, times_array, name, "translate", moves, 0.008, protected)
        if any(name in pose.scales for pose in poses):
            scales = np.array([pose.scales.get(name, (1.0, 1.0)) for pose in poses])
            numeric_track(tracks, times_array, name, "scale", scales, 0.00015, protected)
    return tracks


def numeric_track(tracks, times, name, prop, values, tolerance, protected):
    if np.max(np.abs(values)) < 0.00001:
        return
    keys = [
        {"t": float(times[i]), "v": [round(float(value), 5) for value in values[i]]}
        for i in kept_indices(times, values, tolerance, protected)
    ]
    tracks.append({"bone": name, "property": prop, "keys": keys})


def reorder(setup, moving, before=None):
    """Spine offsets are counted from setup independently for every ordered slot."""
    moving = [name for name in setup if name in moving]
    rest = [name for name in setup if name not in moving]
    at = rest.index(before) if before else 0
    desired = rest[:at] + moving + rest[at:]
    return [{"slot": name, "offset": desired.index(name) - setup.index(name)}
            for name in moving if desired.index(name) != setup.index(name)]
