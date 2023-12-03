namespace LogiX.UserInterface.Views;

public interface IView
{
    void Update(float deltaTime, float totalTime);
    void Render(float deltaTime, float totalTime);
    void SubmitGUI(float deltaTime, float totalTime);
}
