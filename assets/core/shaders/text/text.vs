#version 330 core
layout (location = 0) in vec4 v1;
layout (location = 1) in vec4 v2;
layout (location = 2) in vec4 v3;
layout (location = 3) in mat4 model;
layout (location = 7) in float thickness;
layout (location = 8) in float softness;
layout (location = 9) in vec4 color;
layout (location = 10) in vec4 outlineColor;
layout (location = 11) in float outlineThickness;
layout (location = 12) in float outlineSoftness;

out vec2 TexCoords;

out float Thickness;
out float Softness;
out vec4 Color;
out vec4 OutlineColor;
out float OutlineThickness;
out float OutlineSoftness;

uniform mat4 projection;

void main()
{
    vec4 arr[3];
    arr[0] = v1;
    arr[1] = v2;
    arr[2] = v3;

    vec4 vertex = arr[gl_VertexID];

    Thickness = thickness;
    Softness = softness;
    Color = color;
    OutlineColor = outlineColor;
    OutlineThickness = outlineThickness;
    OutlineSoftness = outlineSoftness;

	TexCoords = vertex.zw;
    gl_Position = projection * model * vec4(vertex.xy, 0.0, 1.0);
}