using Microsoft.AspNetCore.Components.Web;

namespace BlazorSlice.Dialog;

public partial class MessageBox 
{
    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public string Style { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> UserAttributes { get; set; } = new();
    
    [Inject] private IDialogService DialogService { get; set; }

    [CascadingParameter] private DialogInstance DialogInstance { get; set; }

    [Parameter]
    public string Title { get; set; }

    [Parameter]
    public RenderFragment TitleContent { get; set; }

    [Parameter]
    public string Message { get; set; }

    [Parameter]
    public MarkupString MarkupMessage { get; set; }

    [Parameter]
    public RenderFragment MessageContent { get; set; }
    
    [Parameter]
    public string CancelText { get; set; }

    [Parameter]
    public RenderFragment CancelButton { get; set; }

    [Parameter]
    public string NoText { get; set; }

    [Parameter]
    public RenderFragment NoButton { get; set; }

    [Parameter]
    public string YesText { get; set; } = "OK";

    [Parameter]
    public RenderFragment YesButton { get; set; }

    [Parameter]
    public EventCallback<bool> OnYes { get; set; }

    [Parameter]
    public EventCallback<bool> OnNo { get; set; }

    [Parameter]
    public EventCallback<bool> OnCancel { get; set; }

    [Parameter]
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value)
                return;
            _isVisible = value;
            if (IsInline)
            {
                if (_isVisible)
                    _ = Show();
                else
                    Close();
            }

            IsVisibleChanged.InvokeAsync(value);
        }
    }

    private bool _isVisible;
    private bool IsInline => DialogInstance == null;

    private IDialogReference _reference;

    [Parameter]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    public async Task<bool?> Show(DialogOptions options = null)
    {
        if (DialogService == null)
            return null;
        var parameters = new DialogParameters()
        {
            [nameof(Title)] = Title,
            [nameof(TitleContent)] = TitleContent,
            [nameof(Message)] = Message,
            [nameof(MarkupMessage)] = MarkupMessage,
            [nameof(MessageContent)] = MessageContent,
            [nameof(CancelText)] = CancelText,
            [nameof(CancelButton)] = CancelButton,
            [nameof(NoText)] = NoText,
            [nameof(NoButton)] = NoButton,
            [nameof(YesText)] = YesText,
            [nameof(YesButton)] = YesButton,
        };
        _reference = DialogService.Show<MessageBox>(parameters: parameters, options: options, title: Title);
        var result = await _reference.Result;
        if (result.Cancelled || !(result.Data is bool))
            return null;
        return (bool)result.Data;
    }

    public void Close()
    {
        _reference?.Close();
    }

    private EventCallback _yesCallback, _cancelCallback, _noCallback;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        // if (YesButton != null)
        //     _yesCallback = new EventCallback() { EventCallback = OnYesActivated };
        // if (NoButton != null)
        //     _noCallback = new EventCallback() { EventCallback = OnNoActivated };
        // if (CancelButton != null)
        //     _cancelCallback = new EventCallback() { EventCallback = OnCancelActivated };
    }

    private void OnYesActivated(object arg1, MouseEventArgs arg2) => OnYesClicked();

    private void OnNoActivated(object arg1, MouseEventArgs arg2) => OnNoClicked();

    private void OnCancelActivated(object arg1, MouseEventArgs arg2) => OnCancelClicked();

    private void OnYesClicked() => DialogInstance.Close(DialogResult.Ok(true));

    private void OnNoClicked() => DialogInstance.Close(DialogResult.Ok(false));

    private void OnCancelClicked() => DialogInstance.Close(DialogResult.Cancel());

    private void HandleKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            OnCancelClicked();
        }
    }
}