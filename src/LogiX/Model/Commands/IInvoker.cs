using System;
using System.Collections.Generic;

namespace LogiX.Model.Commands;

public interface IInvoker
{
    void ExecuteCommand(ICommand command);
    void Undo(int steps = 1);
    void Redo(int steps = 1);
    bool CanUndo();
    bool CanRedo();

    IReadOnlyCollection<ICommand> UndoStack { get; }
    IReadOnlyCollection<ICommand> RedoStack { get; }
}

public class Invoker : IInvoker
{
    private readonly Stack<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();

    public IReadOnlyCollection<ICommand> UndoStack => _undoStack;
    public IReadOnlyCollection<ICommand> RedoStack => _redoStack;

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
    }

    public void Undo(int steps = 1)
    {
        for (int i = 0; i < steps; i++)
        {
            if (_undoStack.Count == 0)
                return;

            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
        }
    }

    public void Redo(int steps = 1)
    {
        for (int i = 0; i < steps; i++)
        {
            if (_redoStack.Count == 0)
                return;

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }
    }

    public bool CanUndo() => _undoStack.Count > 0;
    public bool CanRedo() => _redoStack.Count > 0;
}
