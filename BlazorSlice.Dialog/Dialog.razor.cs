namespace BlazorSlice.Dialog;

public partial class Dialog
{
    protected string ContentClass => new CssBuilder("modal-body")
        .AddClass(ClassContent)
        .Build();

    protected string ActionClass => new CssBuilder("modal-footer")
        .AddClass(ClassActions)
        .Build();
    
    [CascadingParameter] private DialogInstance DialogInstance { get; set; }

    [Inject] public IDialogService DialogService { get; set; }
    
    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public string Style { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> UserAttributes { get; set; } = new();

    [Parameter]
    public RenderFragment TitleContent { get; set; }

    [Parameter]
    public RenderFragment DialogContent { get; set; }

    [Parameter]
    public RenderFragment DialogActions { get; set; }

    [Parameter]
    public DialogOptions Options { get; set; }

    [Parameter]
    public Action OnBackdropClick { get; set; }
    
    [Parameter]
    public bool DisableSidePadding { get; set; }

    [Parameter]
    public string ClassContent { get; set; }

    [Parameter]
    public string ClassActions { get; set; }

    [Parameter]
    public string ContentStyle { get; set; }

    [Parameter]
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value)
                return;
            _isVisible = value;
            IsVisibleChanged.InvokeAsync(value);
        }
    }
    private bool _isVisible;

    [Parameter] 
    public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter]
    public DefaultFocus DefaultFocus { get; set; }

    private bool IsInline => DialogInstance == null;

    private IDialogReference _reference;
    
    public IDialogReference Show(string title = null, DialogOptions options = null)
    {
        if (!IsInline)
            throw new InvalidOperationException("You can only show an inlined dialog.");
        if (_reference != null)
            Close();
        var parameters = new DialogParameters()
        {
            [nameof(Class)] = Class,
            [nameof(Style)] = Style,
            [nameof(TitleContent)] = TitleContent,
            [nameof(DialogContent)] = DialogContent,
            [nameof(DialogActions)] = DialogActions,
            [nameof(DisableSidePadding)] = DisableSidePadding,
            [nameof(ClassContent)] = ClassContent,
            [nameof(ClassActions)] = ClassActions,
            [nameof(ContentStyle)] = ContentStyle,
        };
        _reference = DialogService.Show<Dialog>(title, parameters, options ?? Options);
        _reference.Result.ContinueWith(t =>
        {
            _isVisible = false;
            InvokeAsync(() => IsVisibleChanged.InvokeAsync(false));
        });
        return _reference;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (IsInline)
        {
            if (_isVisible && _reference == null)
            {
                Show(); // if isVisible and we don't have any reference we need to call Show
            }
            else if (_reference != null)
            {
                if (IsVisible)
                    (_reference.Dialog as Dialog)?.ForceUpdate(); // forward render update to instance
                else
                    Close(); // if we still have reference but it's not visible call Close
            }
        }
        base.OnAfterRender(firstRender);
    }

    /// <summary>
    /// Used for forwarding state changes from inlined dialog to its instance
    /// </summary>
    internal void ForceUpdate()
    {
        StateHasChanged();
    }

    /// <summary>
    /// Close the currently open inlined dialog
    /// </summary>
    /// <param name="result"></param>
    public void Close(DialogResult result = null)
    {
        if (!IsInline || _reference == null)
            return;
        _reference.Close(result);
        _reference = null;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        DialogInstance?.Register(this);
    }
}