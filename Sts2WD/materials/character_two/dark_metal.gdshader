shader_type canvas_item;
void fragment() {
    vec4 surface = texture(TEXTURE, UV);
    float value = dot(surface.rgb, vec3(0.2126, 0.7152, 0.0722));
    COLOR = vec4(vec3(0.065, 0.08, 0.13) * mix(0.6, 1.6, value), surface.a);
}
