#VERTEXBEGIN
#version 330 core
layout (location = 0) in vec2 position;

out vec2 TexCoords;

uniform float uvCoords[12];
uniform mat4 projection;
uniform mat4 model;

void main()
{
	TexCoords = vec2(uvCoords[0 + gl_VertexID * 2], uvCoords[1 + gl_VertexID * 2]);
    gl_Position = projection * model * vec4(position.xy, 0.0, 1.0);
}
#VERTEXEND

#FRAGMENTBEGIN
#version 330 core
out vec4 FragColor;
in vec2 TexCoords;

uniform sampler2D image;
uniform vec4 textureColor;

void main()
{
    FragColor = textureColor * texture(image, TexCoords);
} 
#FRAGMENTEND