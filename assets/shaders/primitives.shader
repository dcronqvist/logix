#VERTEXBEGIN
#version 330 core
layout(location = 0) in vec2 v1;
layout(location = 1) in vec2 v2;
layout(location = 2) in vec2 v3;
layout(location = 3) in vec4 color;
layout(location = 4) in mat4 model;

uniform mat4 projection;

out vec4 ourColor;

void main() {
    ourColor = color;
    vec2 arr[3];
    arr[0] = v1;
    arr[1] = v2;
    arr[2] = v3;
    gl_Position = projection * model * vec4(arr[gl_VertexID], 0.0, 1.0);
}
#VERTEXEND

#FRAGMENTBEGIN
#version 330 core
out vec4 FragColor;

in vec4 ourColor;

void main() {
    FragColor = ourColor;
}
#FRAGMENTEND
