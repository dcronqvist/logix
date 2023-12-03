using System;
using System.Collections;
using System.Drawing;
using ImGuiNET;
using LogiX.Extensions;
using LogiX.Graphics;

namespace LogiX.UserInterface.Coroutines;

public static class CoroutineHelpers
{
    public static IEnumerator Wait()
    {
        yield return null;
    }

    public static IEnumerator WaitFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
            yield return null;
    }

    public static IEnumerator WaitUntil(Func<bool> predicate)
    {
        while (!predicate())
            yield return null;
    }

    public static IEnumerator WaitUntil(Func<bool> predicate, Action whileWaiting)
    {
        while (!predicate())
        {
            whileWaiting();
            yield return null;
        }
    }

    public static IEnumerator WaitUntilCancelable(Func<bool> predicate, Func<bool> shouldCancel, Action whileWaiting, Action<bool> finishedSuccessfully)
    {
        while (!predicate())
        {
            if (shouldCancel())
            {
                finishedSuccessfully(false);
                yield break;
            }

            whileWaiting();
            yield return null;
        }

        finishedSuccessfully(true);
    }

    public static IEnumerator WaitIndefinitely()
    {
        while (true)
            yield return null;
    }

    public static ICoroutineRenderer Render(Action<IRenderer> render) => new LambdaCoroutineRenderer((renderer, _, _) => render(renderer));
    public static ICoroutineRenderer Render(Action<IRenderer, float> render) => new LambdaCoroutineRenderer((renderer, deltaTime, _) => render(renderer, deltaTime));
    public static ICoroutineRenderer Render(Action<IRenderer, float, float> render) => new LambdaCoroutineRenderer((renderer, deltaTime, totalTime) => render(renderer, deltaTime, totalTime));

    public static IEnumerator ShowContextMenu(Action menuItems)
    {
        bool finished = false;
        yield return WaitUntil(
            predicate: () => finished,
            whileWaiting: () =>
            {
                ImGui.OpenPopup("MAINCONTEXTMENU");
                if (ImGui.BeginPopup("MAINCONTEXTMENU"))
                {
                    menuItems();

                    var popupSize = ImGui.GetWindowSize();
                    var popupPos = ImGui.GetWindowPos();

                    var windowRect = new RectangleF(
                        popupPos.X,
                        popupPos.Y,
                        popupSize.X,
                        popupSize.Y
                    );

                    var mousePosInWindow = ImGui.GetMousePos();

                    if (!windowRect.InflateRect(20, 20).Contains(mousePosInWindow))
                        finished = true;

                    ImGui.EndPopup();
                }

                if (!ImGui.IsPopupOpen("MAINCONTEXTMENU"))
                    finished = true;
            });
    }
}
