#version 330 core
out vec4 FragColor;

in vec2 uv;
uniform bool inner;

void main()
{
    float x = uv.x * uv.x - uv.y;

    if (inner)
    {
        if (x > 0)
            discard;
    }
    else
    {
        if (x < 0)
            discard;
    }


    FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}