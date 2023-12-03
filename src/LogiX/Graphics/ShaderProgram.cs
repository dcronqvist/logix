using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Symphony;
using static DotGL.GL;

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

public class ShaderProgram : Content<ShaderProgram>
{
    private uint _programId;
    public uint ProgramID => _programId;

    private unsafe static bool TryCompileShader(int shaderType, string source, out uint shaderId, out string error)
    {
        uint shader = glCreateShader(shaderType);
        glShaderSource(shader, source);

        glCompileShader(shader);

        int* status = stackalloc int[1];
        glGetShaderiv(shader, GL_COMPILE_STATUS, status);

        if (*status == 0)
        {
            // An error occured
            int* length = stackalloc int[1];
            glGetShaderiv(shader, GL_INFO_LOG_LENGTH, length);
            error = glGetShaderInfoLog(shader, *length);
            shaderId = 0;
            return false;
        }

        error = null;
        shaderId = shader;
        return true;
    }

    private unsafe static bool TryLinkProgram(uint shaderProgram, out string error)
    {
        glLinkProgram(shaderProgram);

        int* status = stackalloc int[1];
        glGetProgramiv(shaderProgram, GL_LINK_STATUS, status);

        if (*status == 0)
        {
            // An error occured
            int* length = stackalloc int[1];
            glGetProgramiv(shaderProgram, GL_INFO_LOG_LENGTH, length);
            error = glGetProgramInfoLog(shaderProgram, *length);
            return false;
        }

        error = null;
        return true;
    }

    public bool HasAttribs(out string[] missing, params (string, string, int)[] attributes)
    {
        var miss = new List<string>();
        var existing = this.GetAttributes();

        foreach ((var attrib, var type, var loc) in attributes)
        {
            if (!existing.Any(a => a.Name == attrib && a.TypeName == type && a.Location == loc))
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
            string name = glGetActiveUniform(this.ProgramID, i, 16, out int size, out int type);
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
            string name = glGetActiveAttrib(this.ProgramID, i, 16, out int size, out int type);
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

    public static bool TryCreateShader(string vertexShaderSource, string fragmentShaderSource, out ShaderProgram program, out string[] errors)
    {
        program = null;
        List<string> errorList = new List<string>();

        if (!TryCompileShader(GL_VERTEX_SHADER, vertexShaderSource, out uint vertexShader, out string vertexError))
        {
            errorList.Add($"Vertex Shader error: {vertexError.Trim()}");
        }
        if (!TryCompileShader(GL_FRAGMENT_SHADER, fragmentShaderSource, out uint fragmentShader, out string fragmentError))
        {
            errorList.Add($"Fragment Shader error: {fragmentError.Trim()}");
        }

        if (errorList.Count > 0)
        {
            errors = errorList.ToArray();
            return false;
        }

        uint programId = glCreateProgram();
        glAttachShader(programId, vertexShader);
        glAttachShader(programId, fragmentShader);

        if (!TryLinkProgram(programId, out string linkError))
        {
            errorList.Add(linkError);
        }

        errors = errorList.ToArray();
        program = new ShaderProgram()
        {
            _programId = programId
        };

        return true;
    }

    private void Use() => glUseProgram(_programId);

    private static Stack<uint> _boundPrograms = new Stack<uint>();

    private static void PushUse(ShaderProgram program)
    {
        _boundPrograms.Push(program._programId);
        program.Use();
    }

    private static void PopUse()
    {
        _boundPrograms.Pop();
        glUseProgram(_boundPrograms.TryPeek(out uint program) ? program : 0); // 0 is the default program (no program)
    }

    public void UseWith(Action action)
    {
        PushUse(this);
        action();
        PopUse();
    }

    private static float[] GetMatrix4x4Values(Matrix4x4 matrix)
    {
        return new float[]
        {
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        };
    }

    // Uniform API
    public void SetUniform1f(string name, float value) => glUniform1f(glGetUniformLocation(_programId, name), value);
    public void SetUniform2f(string name, float x, float y) => glUniform2f(glGetUniformLocation(_programId, name), x, y);
    public void SetUniform2f(string name, Vector2 vector) => SetUniform2f(name, vector.X, vector.Y);
    public void SetUniform3f(string name, float x, float y, float z) => glUniform3f(glGetUniformLocation(_programId, name), x, y, z);
    public void SetUniform3f(string name, Vector3 vector) => SetUniform3f(name, vector.X, vector.Y, vector.Z);
    public void SetUniform4f(string name, float x, float y, float z, float w) => glUniform4f(glGetUniformLocation(_programId, name), x, y, z, w);
    public void SetUniform4f(string name, Vector4 vector) => SetUniform4f(name, vector.X, vector.Y, vector.Z, vector.W);
    public void SetUniformfv(string name, float[] values) => glUniform1fv(glGetUniformLocation(_programId, name), values);
    public void SetUniform1i(string name, int value) => glUniform1i(glGetUniformLocation(_programId, name), value);
    public void SetUniform2i(string name, int x, int y) => glUniform2i(glGetUniformLocation(_programId, name), x, y);
    public void SetUniform3i(string name, int x, int y, int z) => glUniform3i(glGetUniformLocation(_programId, name), x, y, z);
    public void SetUniform4i(string name, int x, int y, int z, int w) => glUniform4i(glGetUniformLocation(_programId, name), x, y, z, w);
    public void SetUniformiv(string name, int[] values) => glUniform1iv(glGetUniformLocation(_programId, name), values);
    public unsafe void SetUniformMatrix2fv(string name, int amount, bool transpose, float[] values) { fixed (float* f = &values[0]) glUniformMatrix2fv(glGetUniformLocation(_programId, name), amount, transpose, f); }
    public unsafe void SetUniformMatrix3fv(string name, int amount, bool transpose, float[] values) { fixed (float* f = &values[0]) glUniformMatrix3fv(glGetUniformLocation(_programId, name), amount, transpose, f); }
    public unsafe void SetUniformMatrix4fv(string name, int amount, bool transpose, float[] values) { fixed (float* f = &values[0]) glUniformMatrix4fv(glGetUniformLocation(_programId, name), amount, transpose, f); }
    public unsafe void SetUniformMatrix4f(string name, bool transpose, Matrix4x4 matrix) { float[] vals = GetMatrix4x4Values(matrix); fixed (float* f = &vals[0]) glUniformMatrix4fv(glGetUniformLocation(_programId, name), 1, transpose, f); }

    protected override void OnContentUpdated(ShaderProgram newContent)
    {
        _programId = newContent._programId;
    }

    public override void Unload()
    {
        glDeleteProgram(_programId);
    }
}
