using LogiX.Content;
using static LogiX.OpenGL.GL;
using Symphony;
using System.Numerics;

namespace LogiX.Graphics;

public struct ShaderVariable
{
    public string Name { get; set; }
    public int Location { get; set; }
    public int Type { get; set; }
    public string TypeName { get => this.GetTypeAsString(); }

    private string GetTypeAsString()
    {
        switch (Type)
        {
            case GL_FLOAT:
                return "float";
            case GL_FLOAT_VEC2:
                return "vec2";
            case GL_FLOAT_VEC3:
                return "vec3";
            case GL_FLOAT_VEC4:
                return "vec4";
            case GL_INT:
                return "int";
            case GL_INT_VEC2:
                return "ivec2";
            case GL_INT_VEC3:
                return "ivec3";
            case GL_INT_VEC4:
                return "ivec4";
            case GL_BOOL:
                return "bool";
            case GL_BOOL_VEC2:
                return "bvec2";
            case GL_BOOL_VEC3:
                return "bvec3";
            case GL_BOOL_VEC4:
                return "bvec4";
            case GL_FLOAT_MAT2:
                return "mat2";
            case GL_FLOAT_MAT3:
                return "mat3";
            case GL_FLOAT_MAT4:
                return "mat4";
            case GL_SAMPLER_2D:
                return "sampler2D";
            case GL_SAMPLER_CUBE:
                return "samplerCube";
            default:
                return "unknown";
        }
    }
}

public class ShaderProgramDescription
{
    public string VertexShader { get; set; }
    public string FragmentShader { get; set; }
}

public class ShaderProgram : GLContentItem<ShaderProgramDescription>
{
    public uint ProgramID { get; private set; }
    private static Stack<uint> _programStack = new Stack<uint>();

    public ShaderProgram(string identifier, IContentSource source, ShaderProgramDescription content) : base(identifier, source, content)
    {
    }

    private ShaderProgram(VertexShader vs, FragmentShader fs) : base("", null, new ShaderProgramDescription())
    {
        this.InitGL(vs, fs);
    }

    public static ShaderProgram Create(VertexShader vs, FragmentShader fs)
    {
        return new ShaderProgram(vs, fs);
    }

    public unsafe void InitGL(VertexShader vs, FragmentShader fs)
    {
        // Create program
        uint programID = glCreateProgram();
        glAttachShader(programID, vs.ShaderID);
        glAttachShader(programID, fs.ShaderID);

        // Link program
        glLinkProgram(programID);
        int* status = stackalloc int[1];
        glGetProgramiv(programID, GL_LINK_STATUS, status);

        if (*status == GL_FALSE)
        {
            int* length = stackalloc int[1];
            glGetProgramiv(programID, GL_INFO_LOG_LENGTH, length);
            string info = glGetProgramInfoLog(programID, *length);

            throw new Exception($"Failed to link shader program: {info}");
        }

        this.ProgramID = programID;
    }

    public unsafe override void InitGL(ShaderProgramDescription newContent)
    {
        var vertexShader = LogiX.ContentManager.GetContentItem<VertexShader>(newContent.VertexShader);
        var fragmentShader = LogiX.ContentManager.GetContentItem<FragmentShader>(newContent.FragmentShader);

        vertexShader.ContentUpdated += (sender, e) => { this.OnContentUpdated(newContent); };
        fragmentShader.ContentUpdated += (sender, e) => { this.OnContentUpdated(newContent); };

        // Create program
        uint programID = glCreateProgram();
        glAttachShader(programID, vertexShader.ShaderID);
        glAttachShader(programID, fragmentShader.ShaderID);

        // Link program
        glLinkProgram(programID);
        int* status = stackalloc int[1];
        glGetProgramiv(programID, GL_LINK_STATUS, status);

        if (*status == GL_FALSE)
        {
            int* length = stackalloc int[1];
            glGetProgramiv(programID, GL_INFO_LOG_LENGTH, length);
            string info = glGetProgramInfoLog(programID, *length);

            throw new Exception($"Failed to link shader program: {info}");
        }

        this.ProgramID = programID;
    }

    public override void DestroyGL()
    {
        if (this.ProgramID != 0)
        {
            glDeleteProgram(this.ProgramID);
            this.ProgramID = 0;
        }
    }

    public void Use(Action action)
    {
        ShaderProgram.Push(this);
        action();
        ShaderProgram.Pop();
    }

    public static void Push(ShaderProgram program)
    {
        _programStack.Push(program.ProgramID);
        glUseProgram(program.ProgramID);
    }

    public static void Pop()
    {
        var removed = _programStack.Pop();
        if (_programStack.TryPeek(out uint next))
        {
            glUseProgram(next);
        }
        else
        {
            glUseProgram(0);
        }
    }

    public bool HasAttribs(out string[] missing, params (string, string, int)[] attributes)
    {
        var miss = new List<string>();
        var existing = this.GetAttributes();

        foreach ((var attrib, var type, var loc) in attributes)
        {
            if (!existing.Any(a => a.Name == attrib && a.TypeName == type))
            {
                miss.Add(attrib);
            }
        }

        missing = miss.ToArray();
        return miss.Count == 0;
    }

    public bool HasUniforms(out string[] missing, params (string, string)[] uniforms)
    {
        var miss = new List<string>();
        var existing = this.GetUniforms();

        foreach ((var uniform, var type) in uniforms)
        {
            if (!existing.Any(a => a.Name == uniform && a.TypeName == type))
            {
                miss.Add(uniform);
            }
        }

        missing = miss.ToArray();
        return miss.Count == 0;
    }

    public unsafe ShaderVariable[] GetUniforms()
    {
        int* uniformAmount = stackalloc int[1];
        glGetProgramiv(this.ProgramID, GL_ACTIVE_UNIFORMS, uniformAmount);

        ShaderVariable[] uniforms = new ShaderVariable[*uniformAmount];

        for (uint i = 0; i < *uniformAmount; i++)
        {
            glGetActiveUniform(this.ProgramID, i, 16, out int length, out int size, out int type, out string name);
            var variable = new ShaderVariable()
            {
                Name = name,
                Type = type,
                Location = glGetUniformLocation(this.ProgramID, name)
            };
            uniforms[i] = variable;
        }

        return uniforms;
    }

    public unsafe ShaderVariable[] GetAttributes()
    {
        int* attribAmount = stackalloc int[1];
        glGetProgramiv(this.ProgramID, GL_ACTIVE_ATTRIBUTES, attribAmount);

        ShaderVariable[] attribs = new ShaderVariable[*attribAmount];

        for (uint i = 0; i < *attribAmount; i++)
        {
            glGetActiveAttrib(this.ProgramID, i, 16, out int length, out int size, out int type, out string name);
            var variable = new ShaderVariable()
            {
                Name = name,
                Type = type,
                Location = glGetAttribLocation(this.ProgramID, name)
            };
            attribs[i] = variable;
        }

        return attribs;
    }

    public void SetInt(string name, int value)
    {
        glUniform1i(glGetUniformLocation(this.ProgramID, name), value);
    }

    public void SetFloat(string name, float value)
    {
        glUniform1f(glGetUniformLocation(this.ProgramID, name), value);
    }

    public void SetVec2(string name, float x, float y)
    {
        glUniform2f(glGetUniformLocation(this.ProgramID, name), x, y);
    }

    public void SetVec3(string name, float x, float y, float z)
    {
        glUniform3f(glGetUniformLocation(this.ProgramID, name), x, y, z);
    }

    public void SetVec4(string name, float x, float y, float z, float w)
    {
        glUniform4f(glGetUniformLocation(this.ProgramID, name), x, y, z, w);
    }

    public void SetMatrix4x4(string name, Matrix4x4 matrix)
    {
        glUniformMatrix4fv(glGetUniformLocation(ProgramID, name), 1, false, Utilities.GetMatrix4x4Values(matrix));
    }

    public void SetFloatArray(string name, float[] values)
    {
        glUniform1fv(glGetUniformLocation(ProgramID, name), values.Length, values);
    }

    public void SetTexture2D(int activeTexture, string name, Texture2D texture)
    {
        glActiveTexture(GL_TEXTURE0 + activeTexture);
        glBindTexture(GL_TEXTURE_2D, texture.GLID);
        glUniform1i(glGetUniformLocation(ProgramID, name), activeTexture);
    }

    public void SetBool(string name, bool value)
    {
        glUniform1i(glGetUniformLocation(this.ProgramID, name), value ? 1 : 0);
    }

    public override bool IsGLInitialized()
    {
        return this.ProgramID != 0;
    }
}