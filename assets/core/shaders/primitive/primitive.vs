#version 330 core
layout (location = 0) in vec2 v1;
layout (location = 1) in vec2 v2;
layout (location = 2) in vec2 v3;
layout (location = 3) in vec4 color;
layout (location = 4) in mat4 model;

uniform mat4 projection;

out vec4 ourColor;

void main()
{
    ourColor = color;
    
    if (gl_VertexID == 0) {
        gl_Position = projection * model * vec4(v1, 0.0, 1.0);
    } else if (gl_VertexID == 1) {
        gl_Position = projection * model * vec4(v2, 0.0, 1.0);
    } else if (gl_VertexID == 2) {
        gl_Position = projection * model * vec4(v3, 0.0, 1.0);
    }
}