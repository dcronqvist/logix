using System.Numerics;
using ImGuiNET;
using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;
using LogiX.Rendering;

namespace LogiX.Graphics.UI;

public class CircuitAppearanceDialog : Modal
{
    public Circuit Circuit { get; set; }

    private Camera2D _camera;
    private Framebuffer _previewFramebuffer;

    int previewSize = 300;

    public CircuitAppearanceDialog(Circuit circuit) : base($"Circuit Appearance Editor", ImGuiWindowFlags.AlwaysAutoResize, ImGuiPopupFlags.None)
    {
        this.Circuit = circuit;
        this._previewFramebuffer = new Framebuffer(previewSize, previewSize, true);
        this._camera = new Camera2D(new Vector2(previewSize / 2, previewSize / 2), 1f, new Vector2(previewSize, previewSize));
    }

    public override void SubmitUI(Editor editor)
    {
        var circ = this.Circuit;
        var integrated = Integrated.CreateDescriptionFromCircuit("NOTHIN", circ);
        var node = integrated.CreateNode() as Integrated;
        var scheduler = new Scheduler();
        scheduler.AddNode(node);
        scheduler.Prepare();
        node.Position = (new Vector2(previewSize / 2, previewSize / 2) - node.GetMiddleOffset()).ToVector2i(Constants.GRIDSIZE);

        // Here we must show a preview of the appearance.

        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
        var fShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.fb_default");
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.text");
        var image = Utilities.ContentManager.GetContentItem<Texture2D>("core.texture.icon");

        this._previewFramebuffer.Bind(() =>
        {
            Framebuffer.Clear(ColorF.Transparent);

            node.Render(scheduler.GetPinCollectionForNode(node), this._camera);
            PrimitiveRenderer.FinalizeRender(pShader, this._camera);
            TextRenderer.FinalizeRender(tShader, this._camera);
        });

        ImGui.BeginChild("left", new Vector2(previewSize, previewSize), true);


        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.Image((nint)_previewFramebuffer.GetTexture(), new Vector2(previewSize, previewSize), new Vector2(0, 1), new Vector2(1, 0));

        var currZoom = this._camera.Zoom;
        ImGui.PushItemWidth(previewSize);
        if (ImGui.SliderFloat("Zoom", ref currZoom, 0.5f, 5f))
        {
            this._camera.Zoom = currZoom;
        }

        if (ImGui.Button("Save"))
        {
            editor.Project.UpdateCircuit(circ);
            ImGui.CloseCurrentPopup();
        }
    }
}