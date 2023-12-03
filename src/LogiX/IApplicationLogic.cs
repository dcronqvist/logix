namespace LogiX;

public interface IApplicationLogic
{
    void Initialize();
    void Frame(float deltaTime, float totalTime);
    void Unload();
}
