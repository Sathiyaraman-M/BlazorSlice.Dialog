namespace BlazorSlice.Dialog.Services;

public class DialogOptions
{
    public DialogPosition? Position { get; set; }

    public Size? Size { get; set; }

    public bool? DisableBackdropClick { get; set; }
    public bool? CloseOnEscapeKey { get; set; }
    public bool? NoHeader { get; set; }
    public bool? CloseButton { get; set; }
    public bool? Scrollable { get; set; }
    public bool? FullWidth { get; set; }
}