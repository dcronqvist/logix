using System;
using System.Collections.Generic;
using DotGLFW;
using LogiX.Model.Circuits;
using LogiX.Model.Commands;

namespace LogiX.UserInterface.Actions;

public interface IEditorAction
{
    bool CanExecute(IInvoker editorInvoker, CircuitDefinitionViewModel viewModel);
    bool IsSelected(IInvoker editorInvoker, CircuitDefinitionViewModel viewModel);
    void Execute(IInvoker editorInvoker, CircuitDefinitionViewModel viewModel);
    IEnumerable<Keys> GetShortcut();
}

public class EditorAction(
    Func<IInvoker, CircuitDefinitionViewModel, bool> canExecute,
    Func<IInvoker, CircuitDefinitionViewModel, bool> isSelected,
    Action<IInvoker, CircuitDefinitionViewModel> execute,
    params Keys[] shortcut) : IEditorAction
{
    public bool CanExecute(IInvoker editorInvoker, CircuitDefinitionViewModel viewModel) => canExecute(editorInvoker, viewModel);
    public bool IsSelected(IInvoker editorInvoker, CircuitDefinitionViewModel viewModel) => isSelected(editorInvoker, viewModel);
    public void Execute(IInvoker editorInvoker, CircuitDefinitionViewModel viewModel) => execute(editorInvoker, viewModel);
    public IEnumerable<Keys> GetShortcut() => shortcut;

    public static EditorAction Empty => new(
        (editorInvoker, viewModel) => true,
        (editorInvoker, viewModel) => false,
        (editorInvoker, viewModel) => { });
}
