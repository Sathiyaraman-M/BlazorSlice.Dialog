using BlazorSlice.Dialog.Services.KeyInterceptor;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorSlice.Dialog;

public partial class DialogInstance : IDisposable
{
    private DialogOptions _options = new();
    private readonly string _elementId = "dialog_" + Guid.NewGuid().ToString().Substring(0, 8);
    private IKeyInterceptor _keyInterceptor;

    [Inject] private IKeyInterceptorFactory KeyInterceptorFactory { get; set; }

    [CascadingParameter] private DialogProvider Parent { get; set; }
    [CascadingParameter] private DialogOptions GlobalDialogOptions { get; set; } = new DialogOptions();
    
    [Parameter]
    public DialogOptions Options
    {
        get => _options ??= new DialogOptions();
        set => _options = value;
    }
    
    [Parameter] 
    public string Class { get; set; }
    
    [Parameter] 
    public string Style { get; set; }
    
    [Parameter(CaptureUnmatchedValues = true)] 
    public Dictionary<string, object> UserAttributes { get; set; } = new();

    [Parameter]
    public string Title { get; set; }
    
    [Parameter]
    public RenderFragment TitleContent { get; set; }
    
    [Parameter]
    public RenderFragment Content { get; set; }
    
    [Parameter]
    public Guid Id { get; set; }

    [Parameter] 
    public string CloseIcon { get; set; } = TablerIcons.X;
    
    private string Position { get; set; }
    private string DialogSize { get; set; }
    private bool DisableBackdropClick { get; set; }
    private bool CloseOnEscapeKey { get; set; }
    private bool NoHeader { get; set; }
    private bool CloseButton { get; set; }
    private bool Scrollable { get; set; }
    private bool FullWidth { get; set; }
    
    protected override void OnInitialized()
    {
        ConfigureInstance();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //Since CloseOnEscapeKey is the only thing to be handled, turn interceptor off
            if (CloseOnEscapeKey)
            {
                _keyInterceptor = KeyInterceptorFactory.Create();

                await _keyInterceptor.Connect(_elementId, new KeyInterceptorOptions()
                {
                    TargetClass = "mud-dialog",
                    Keys = {
                        new KeyOptions { Key="Escape", SubscribeDown = true },
                    },
                });
                _keyInterceptor.KeyDown += HandleKeyDown;
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    internal void HandleKeyDown(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "Escape":
                if (CloseOnEscapeKey)
                {
                    Cancel();
                }
                break;
        }
    }

    public void SetOptions(DialogOptions options)
    {
        Options = options;
        ConfigureInstance();
        StateHasChanged();
    }

    public void SetTitle(string title)
    {
        Title = title;
        StateHasChanged();
    }
    
    public void Close()
    {
        Close(DialogResult.Ok<object>(null));
    }
    
    public void Close(DialogResult dialogResult)
    {
        Parent.DismissInstance(Id, dialogResult);
    }
    
    public void Close<T>(T returnValue)
    {
        var dialogResult = DialogResult.Ok<T>(returnValue);
        Parent.DismissInstance(Id, dialogResult);
    }
    
    public void Cancel()
    {
        Close(DialogResult.Cancel());
    }
    
    private void ConfigureInstance()
    {
        Position = SetPosition();
        DialogSize = SetSize();
        NoHeader = SetHideHeader();
        CloseButton = SetCloseButton();
        FullWidth = SetFullWidth();
        Scrollable = SetScrollable();
        DisableBackdropClick = SetDisableBackdropClick();
        CloseOnEscapeKey = SetCloseOnEscapeKey();
        //Class = ClassName;
    }
    
    private string SetPosition()
    {
        DialogPosition position;

        if (Options.Position.HasValue)
        {
            position = Options.Position.Value;
        }
        else if (GlobalDialogOptions.Position.HasValue)
        {
            position = GlobalDialogOptions.Position.Value;
        }
        else
        {
            position = DialogPosition.Default;
        }
        return position == DialogPosition.Centered ? "modal-dialog-centered" : string.Empty;
    }
    
    private string SetSize()
    {
        Size size;

        if (Options.Size.HasValue)
        {
            size = Options.Size.Value;
        }
        else if (GlobalDialogOptions.Size.HasValue)
        {
            size = GlobalDialogOptions.Size.Value;
        }
        else
        {
            size = Size.Default;
        }
        return size != Size.Default ? (size == Size.Large ? "modal-lg" : "modal-sm") : string.Empty;
    }
    
    private bool SetFullWidth()
    {
        if (Options.FullWidth.HasValue)
            return Options.FullWidth.Value;

        if (GlobalDialogOptions.FullWidth.HasValue)
            return GlobalDialogOptions.FullWidth.Value;

        return false;
    }
    
    private bool SetScrollable()
    {
        if (Options.Scrollable.HasValue)
            return Options.Scrollable.Value;

        if (GlobalDialogOptions.Scrollable.HasValue)
            return GlobalDialogOptions.Scrollable.Value;

        return false;
    }

    private string ClassName => new CssBuilder()
        .AddClass("modal-dialog")
        .AddClass(Position)
        .AddClass(DialogSize)
        .AddClass("modal-dialog-fullwidth", FullWidth)
        .AddClass("modal-dialog-scrollable", Scrollable)
        .AddClass(_dialog?.Class)
        .Build();
    
    private bool SetHideHeader()
    {
        if (Options.NoHeader.HasValue)
            return Options.NoHeader.Value;

        if (GlobalDialogOptions.NoHeader.HasValue)
            return GlobalDialogOptions.NoHeader.Value;

        return false;
    }

    private bool SetCloseButton()
    {
        if (Options.CloseButton.HasValue)
            return Options.CloseButton.Value;

        if (GlobalDialogOptions.CloseButton.HasValue)
            return GlobalDialogOptions.CloseButton.Value;

        return false;
    }

    private bool SetDisableBackdropClick()
    {
        if (Options.DisableBackdropClick.HasValue)
            return Options.DisableBackdropClick.Value;

        if (GlobalDialogOptions.DisableBackdropClick.HasValue)
            return GlobalDialogOptions.DisableBackdropClick.Value;

        return false;
    }

    private bool SetCloseOnEscapeKey()
    {
        if (Options.CloseOnEscapeKey.HasValue)
            return Options.CloseOnEscapeKey.Value;

        if (GlobalDialogOptions.CloseOnEscapeKey.HasValue)
            return GlobalDialogOptions.CloseOnEscapeKey.Value;

        return false;
    }

    private void HandleBackgroundClick()
    {
        if (DisableBackdropClick)
            return;

        if (_dialog?.OnBackdropClick == null)
        {
            Cancel();
            return;
        }

        _dialog?.OnBackdropClick.Invoke();
    }

    private Dialog _dialog;
    private bool _disposedValue;

    public void Register(Dialog dialog)
    {
        if (dialog == null)
            return;
        _dialog = dialog;
        Class = _dialog.Class;
        Style = _dialog.Style;
        TitleContent = _dialog.TitleContent;
        StateHasChanged();
    }
    
    public void ForceRender()
    {
        StateHasChanged();
    }

    public void CancelAll()
    {
        Parent?.DismissAll();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_keyInterceptor != null)
                {
                    _keyInterceptor.KeyDown -= HandleKeyDown;
                    _keyInterceptor.Dispose();
                }
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}