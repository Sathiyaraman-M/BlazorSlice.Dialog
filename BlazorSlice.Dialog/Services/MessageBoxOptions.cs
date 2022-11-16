namespace BlazorSlice.Dialog.Services;

public class MessageBoxOptions
{
    public string Title { get; set; }
    public string Message { get; set; }
    public MarkupString MarkupMessage { get; set; }
    public string YesText { get; set; } = "OK";
    public string NoText { get; set; }
    public string CancelText { get; set; }
}