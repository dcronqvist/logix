#version 330 core
layout (location = 0) in vec4 v1;
layout (location = 1) in vec4 v2;
layout (location = 2) in vec4 v3;
layout (location = 3) in vec4 color;
layout (location = 4) in mat4 model;

out vec2 TexCoords;
out vec4 Color;

uniform mat4 projection;

void main()
{
    vec4 arr[3];
    arr[0] = v1;
    arr[1] = v2;
    arr[2] = v3;

    vec4 vertex = arr[gl_VertexID];

	TexCoords = vertex.zw;
    Color = color;
    gl_Position = projection * model * vec4(vertex.xy, 0.0, 1.0);
}