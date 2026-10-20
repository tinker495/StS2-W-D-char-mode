shader_type canvas_item;

uniform vec4 cold_color : source_color = vec4(0.10, 0.08, 0.08, 1.0);
uniform vec4 hot_color : source_color = vec4(0.24, 0.12, 0.10, 1.0);

varying vec4 canvas_tint;

void vertex() {
    canvas_tint = COLOR;
}

void fragment() {
    vec4 surface = texture(TEXTURE, UV);
    float luminance = dot(surface.rgb, vec3(0.2126, 0.7152, 0.0722));
    vec3 metal = mix(cold_color.rgb, hot_color.rgb, smoothstep(0.0, 1.0, UV.y));
    COLOR = vec4(metal * mix(0.65, 1.5, luminance), surface.a) * canvas_tint;
}
