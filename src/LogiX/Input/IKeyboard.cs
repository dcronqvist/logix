using System;
using LogiX.Eventing;

namespace LogiX.Input;

public interface IKeyboard<TChar, TKeys, TModifierKeys>
{
    IEventProvider<TChar> CharacterTypedEventProvider { get; }
    IEventProvider<TKeys> KeyPressedEventProvider { get; }

    event EventHandler<TChar> CharacterTyped;
    event EventHandler<(TKeys, TModifierKeys)> KeyPressed;
    event EventHandler<(TKeys, TModifierKeys)> KeyPressedOrRepeated;
    event EventHandler<(TKeys, TModifierKeys)> KeyReleased;

    void Begin();
    void End();

    bool IsKeyDown(TKeys key);
    bool IsKeyPressed(TKeys key);
    bool IsKeyReleased(TKeys key);
    bool IsKeyCombinationPressed(params TKeys[] keys);
    bool TryGetNextKeyPressed(out TKeys key);
}
