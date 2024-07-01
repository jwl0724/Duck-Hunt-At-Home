shader_type canvas_item;

void fragment() {
    vec2 center = vec2(0.5, 0.5); // Center of the circle (relative to the UV coordinates)
    float radius = 0.5; // Radius of the circle (relative to the UV coordinates)
    
    // Calculate the distance from the current pixel to the center
    float dist = distance(UV, center);
    
    if (dist > radius) {
        discard; // Discard pixels outside the circle
    }
    
    COLOR = texture(TEXTURE, UV); // Sample the texture color
}
