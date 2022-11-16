using System.Diagnostics.CodeAnalysis;

namespace BlazorSlice.Dialog.Interfaces;

public interface IDialogReference
{
    Guid Id { get; }
    RenderFragment RenderFragment { get; set; }

    Task<DialogResult> Result { get; }

    void Close();
    void Close(DialogResult result);

    bool Dismiss(DialogResult result);

    object Dialog { get; }

    void InjectRenderFragment(RenderFragment rf);

    void InjectDialog(object inst);

    Task<T> GetReturnValueAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>();
}