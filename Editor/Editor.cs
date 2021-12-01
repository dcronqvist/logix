namespace LogiX.Editor;

public class Editor : Application
{
    float x;
    float speed;
    bool isSaga;

    public override void Initialize()
    {
        speed = 20f;
    }

    public override void LoadContent()
    {

    }

    public override void SubmitUI()
    {
        ImGui.BeginMainMenuBar();
        if (ImGui.BeginMenu("File"))
        {
            ImGui.Text("Hejsan");
            ImGui.Separator();
            ImGui.Text("Svejsan");
            ImGui.EndMenu();
        }
        ImGui.EndMainMenuBar();

        ImGui.SetNextWindowPos(new Vector2(0, 21), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(120f, 720f - 21f), ImGuiCond.Always);
        ImGui.Begin("Sidebar", ImGuiWindowFlags.AlwaysAutoResize);
        ImGui.End();


        ImGui.Begin("Hej", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
        ImGui.Text("Hej");
        ImGui.End();

        ImGui.Begin("Hej2", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
        ImGui.Text("Hej2");
        ImGui.End();
    }

    public override void Update()
    {
        x += Raylib.GetFrameTime() * speed;

        if (x > 1280 || x < 0)
        {
            speed *= -1f;
        }
    }

    public override void Render()
    {
        Raylib.ClearBackground(Color.BLUE);

        Raylib.DrawText(isSaga ? "Is Saga" : "Is Daniel", (int)x, 10, 100, Color.RED);
    }
}