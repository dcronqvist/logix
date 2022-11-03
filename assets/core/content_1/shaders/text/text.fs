#version 330 core
out vec4 FragColor;
in vec2 TexCoords;
in vec4 Color;

uniform sampler2D text;

void main()
{
	vec4 sampled = vec4(1.0, 1.0, 1.0, texture(text, TexCoords).r);
    FragColor = Color * sampled;
} 