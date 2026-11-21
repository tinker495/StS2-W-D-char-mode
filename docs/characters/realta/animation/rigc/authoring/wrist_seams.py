"""Seat the two bronze cuff openings under the clenched gloves during the thrust."""

import math


# Source-space centres of the two oval openings, painted into the forearm crops.
CUFFS = {"r": (424, 835.5), "l": (913, 989)}


def register_cuffs(canvas):
    for side, pivot in CUFFS.items():
        arm, cuff = "forearm_" + side, "cuff_" + side
        canvas.bones[cuff] = {"name": cuff, "parent": arm, "pivot": pivot}
        canvas.rotation[cuff] = canvas.rotation[arm]


def cuff_attachment(canvas, item, side):
    """The elbow and fur retain their painted seat. Only the distal bracer follows the glove."""
    arm, cuff = "forearm_" + side, "cuff_" + side
    elbow = canvas.bones[arm]["pivot"]
    end = CUFFS[side]
    dx, dy = end[0] - elbow[0], end[1] - elbow[1]
    length_squared = dx * dx + dy * dy
    x, y, w, h = item["rect"]
    cols, rows = math.ceil(w / 18), math.ceil(h / 18)
    rim = ([(0,c) for c in range(cols + 1)] + [(r,cols) for r in range(1,rows + 1)]
           + [(rows,c) for c in reversed(range(cols))] + [(r,0) for r in reversed(range(1,rows))])
    grid = rim + [(r,c) for r in range(1,rows) for c in range(1,cols)]
    lookup = {rc: i for i,rc in enumerate(grid)}
    uvs, weights, triangles = [], [], []
    angle = math.radians(canvas.rotation[arm])
    cs, sn = math.cos(angle), math.sin(angle)
    for r,c in grid:
        u,v = c / cols,r / rows
        px,py = x+w*u,y+h*v
        along = ((px-elbow[0])*dx+(py-elbow[1])*dy) / length_squared
        blend = min(1, max(0, (along-.23)/.80))
        blend = blend * blend * (3-2*blend)
        vertex = []
        for bone, origin, mass in ((arm,elbow,1-blend),(cuff,end,blend)):
            if mass:
                sx,sy = px-origin[0],origin[1]-py
                vertex.append({"bone":bone,"x":sx*cs+sy*sn,"y":-sx*sn+sy*cs,"weight":mass})
        uvs.extend((u,v))
        weights.append(vertex)
    for r in range(rows):
        for c in range(cols):
            a,b,d,e = (lookup[k] for k in ((r,c),(r,c+1),(r+1,c),(r+1,c+1)))
            triangles.extend((a,b,e,a,e,d))
    return {"image":item["path"],"type":"mesh","uvs":uvs,"weights":weights,"triangles":triangles,"hull":len(rim)}


def seat_cuffs(pose, contact):
    if not contact:
        return
    # These points lie in the solid palmar leather, just behind the fingers wrapping the wood.
    # The far grip is the same drawing reflected and reduced around its registered wood contact.
    for side, palm in (("r",(390,836)), ("l",(919,1020.2))):
        arm, cuff = "forearm_"+side, "cuff_"+side
        start = pose.transform(arm, CUFFS[side])
        end = pose.transform("hand_"+side, palm)
        pose.place(cuff, tuple(a+contact*(b-a) for a,b in zip(start,end)))
