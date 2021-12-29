namespace LogiX.Components;

public interface IUISubmitter<TReturn, TArg>
{
    TReturn SubmitUI(TArg arg);
}