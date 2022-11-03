#version 330 core
out vec4 FragColor;
in vec2 TexCoords;

uniform sampler2D image;
uniform vec4 textureColor;

void main()
{
    FragColor = textureColor * texture(image, TexCoords);
} 