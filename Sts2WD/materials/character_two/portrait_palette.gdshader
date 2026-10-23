shader_type canvas_item;

uniform vec4 shadow_color : source_color = vec4(0.02, 0.02, 0.02, 1.0);
uniform vec4 middle_color : source_color = vec4(0.5, 0.5, 0.5, 1.0);
uniform vec4 light_color : source_color = vec4(0.98, 0.98, 0.98, 1.0);
uniform float shadow_hold = 0.02;
uniform float middle_point = 0.55;
varying vec4 vertex_tint;

void vertex() {
    vertex_tint = COLOR;
}

void fragment() {
    vec4 source = texture(TEXTURE, UV);
    float value = pow(clamp(dot(source.rgb, vec3(0.2126, 0.7152, 0.0722)), 0.0, 1.0), 0.85);
    vec3 palette = mix(shadow_color.rgb, middle_color.rgb, smoothstep(shadow_hold, middle_point, value));
    palette = mix(palette, light_color.rgb, smoothstep(middle_point, 0.95, value));
    COLOR = vec4(palette, source.a) * vertex_tint;
}
