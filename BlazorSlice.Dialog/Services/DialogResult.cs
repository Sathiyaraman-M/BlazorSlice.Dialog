namespace BlazorSlice.Dialog.Services;

public class DialogResult
{
    public object Data { get; }
    public Type DataType { get; }
    public bool Cancelled { get; }

    protected internal DialogResult(object data, Type resultType, bool cancelled)
    {
        Data = data;
        DataType = resultType;
        Cancelled = cancelled;
    }

    public static DialogResult Ok<T>(T result) => Ok(result, default);

    public static DialogResult Ok<T>(T result, Type dialogType) => new(result, dialogType, false);

    public static DialogResult Cancel() => new(default, typeof(object), true);
}