#version 330 core
layout (location = 0) in vec2 position;
layout (location = 1) in vec2 uvCoords;

uniform mat4 projection;

out vec2 uv;

void main()
{
    gl_Position = projection * vec4(position, 0.0, 1.0);
    uv = uvCoords;
}