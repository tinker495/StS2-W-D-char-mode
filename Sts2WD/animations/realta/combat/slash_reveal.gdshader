shader_type canvas_item;
// Reparameterize the original painting over the blade's baked 240 Hz sweep.
uniform vec2 paint_offset;
uniform float paint_scale;
uniform vec2 circle_center;
uniform float angle_min;
uniform float angle_max;
uniform float radial_inner[128];
uniform float radial_outer[128];
uniform float sweep_length;
uniform float reveal = 0.0;
varying float opacity;
void vertex() {
    opacity = COLOR.a;
    COLOR.rgb = vec3(1.0);
}
void fragment() {
    float progress = clamp(UV.x / max(reveal, 0.00001), 0.0, 1.0);
    float paint_progress = progress * 0.96;
    float a = mix(angle_min, angle_max, paint_progress);
    float index = paint_progress * 127.0;
    int lo = min(int(index),126);
    float ri = mix(radial_inner[lo],radial_inner[lo+1],fract(index));
    float ro = mix(radial_outer[lo],radial_outer[lo+1],fract(index));
    vec2 paint = circle_center + vec2(cos(a),sin(a)) * mix(ro,ri,UV.y);
    vec2 paint_uv = (paint-paint_offset)*paint_scale*TEXTURE_PIXEL_SIZE;
    float leading = smoothstep(0.0, 9.0, (reveal-UV.x)*sweep_length);
    vec4 ink = texture(TEXTURE,paint_uv);
    // Feather the mesh boundary as well as the painted alpha; no straight cut edge.
    float sides = smoothstep(0.0, 0.07, UV.y) * smoothstep(0.0, 0.12, 1.0-UV.y);
    COLOR = vec4(ink.rgb, ink.a*opacity*leading*sides);
}
