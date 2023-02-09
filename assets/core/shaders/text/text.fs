#version 330 core
out vec4 FragColor;
in vec2 TexCoords;

in float Thickness;
in float Softness;
in vec4 Color;
in vec4 OutlineColor;
in float OutlineThickness;
in float OutlineSoftness;

uniform sampler2D text;

void main()
{
    float a = texture(text, TexCoords).a;
    float outline = smoothstep(OutlineThickness - OutlineSoftness, OutlineThickness + OutlineSoftness, a);
    a = smoothstep(1.0 - Thickness - Softness, 1.0 - Thickness + Softness, a);

    FragColor = vec4(mix(OutlineColor.rgb, Color.rgb, outline), a);
} 