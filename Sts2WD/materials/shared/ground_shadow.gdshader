shader_type canvas_item;
// Procedural elliptical contact shadow shared by standing sprites and skeletal rigs
// without extra texture imports; inherits parent Visuals modulation during fades and shop dimming.

varying vec4 modulate_color;

void vertex() {
	modulate_color = COLOR;
}

void fragment() {
	vec2 p = (UV - vec2(0.5)) * 2.0;
	float r2 = dot(p, p);
	float core = exp(-r2 * 3.4);
	float rim = 1.0 - smoothstep(0.35, 1.0, sqrt(r2));
	float mask = clamp(core * 0.72 + rim * 0.28, 0.0, 1.0) * step(r2, 1.0);
	COLOR = vec4(modulate_color.rgb, modulate_color.a * mask);
}
