using System.Diagnostics;

namespace BlazorSlice.Dialog.Services;

public class DialogReference : IDialogReference
{
    private readonly TaskCompletionSource<DialogResult> _resultCompletion = new();

    private readonly IDialogService _dialogService;

    public DialogReference(Guid dialogInstanceId, IDialogService dialogService)
    {
        Id = dialogInstanceId;
        _dialogService = dialogService;
    }
    
    public void Close()
    {
        _dialogService.Close(this);
    }

    public void Close(DialogResult result)
    {
        _dialogService.Close(this, result);
    }

    public bool Dismiss(DialogResult result)
    {
        return _resultCompletion.TrySetResult(result);
    }
    public Guid Id { get; }
    public RenderFragment RenderFragment { get; set; }
    public Task<DialogResult> Result => _resultCompletion.Task;

    public object Dialog { get; private set; }

    public void InjectDialog(object inst)
    {
        Dialog = inst;
    }
    
    public void InjectRenderFragment(RenderFragment rf)
    {
        RenderFragment = rf;
    }

    public async Task<T> GetReturnValueAsync<T>()
    {
        var result = await Result;
        try
        {
            return (T)result.Data;
        }
        catch (InvalidCastException)
        {
            Debug.WriteLine($"Could not cast return value to {typeof(T)}, returning default.");
            return default;
        }
    }
}