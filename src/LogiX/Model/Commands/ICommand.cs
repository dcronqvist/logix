using System;

namespace LogiX.Model.Commands;

public interface ICommand
{
    string GetTitle();

    void Execute();
    void Undo();
}

public class LambdaCommand<T>(string title, Func<T> execute, Action<T> undo) : ICommand
{
    private T _executeResult;

    public string GetTitle() => title;
    public void Execute() => _executeResult = execute();
    public void Undo() => undo(_executeResult);
}

public class LambdaCommand(string title, Action execute, Action undo) : ICommand
{
    public string GetTitle() => title;
    public void Execute() => execute();
    public void Undo() => undo();
}

public class CompositeCommand(string title, params ICommand[] commands) : ICommand
{
    public string GetTitle() => title;

    public void Execute()
    {
        foreach (var command in commands)
            command.Execute();
    }

    public void Undo()
    {
        for (int i = commands.Length - 1; i >= 0; i--)
            commands[i].Undo();
    }
}
