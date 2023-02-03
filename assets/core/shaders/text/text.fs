#version 330 core
out vec4 FragColor;
in vec2 TexCoords;
in vec4 Color;
in float Edge;
in float Width;

uniform sampler2D text;

void main()
{
    float dist = texture(text, TexCoords).r;
    float alpha = smoothstep(Edge, Edge + Width, dist);

    FragColor = vec4(Color.rgb, alpha * Color.a);
} 