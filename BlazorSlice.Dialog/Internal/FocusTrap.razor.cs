using Microsoft.AspNetCore.Components.Web;

namespace BlazorSlice.Dialog.Internal;

public partial class FocusTrap
{
    private ElementReference _firstBumper;
    private ElementReference _lastBumper;
    private ElementReference _fallback;
    private ElementReference _root;

    private bool _shiftDown;
    private bool _disabled;
    private bool _initialized;
    
    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public string Style { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> UserAttributes { get; set; } = new();
    
    [Parameter] 
    public DefaultFocus DefaultFocus { get; set; } = DefaultFocus.FirstChild;
    
    [Parameter]
    public RenderFragment ChildContent { get; set; }
    
    [Parameter]
    public bool Disabled
    {
        get => _disabled;
        set
        {
            if (_disabled == value) return;
            _disabled = value;
            _initialized = false;
        }
    }

    private string TrapTabIndex => (Disabled? "-1" : "0");
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
            await SaveFocusAsync();

        if (!_initialized)
            await InitializeFocusAsync();
    }

    private Task OnBottomFocusAsync(FocusEventArgs args)
    {
        return FocusLastAsync();
    }

    private Task OnBumperFocusAsync(FocusEventArgs args)
    {
        return _shiftDown ? FocusLastAsync() : FocusFirstAsync();
    }

    private Task OnRootFocusAsync(FocusEventArgs args)
    {
        return FocusFallbackAsync();
    }

    private void OnRootKeyDown(KeyboardEventArgs args)
    {
        HandleKeyEvent(args);
    }

    private void OnRootKeyUp(KeyboardEventArgs args)
    {
        HandleKeyEvent(args);
    }

    private Task OnTopFocusAsync(FocusEventArgs args)
    {
        return FocusFirstAsync();
    }

    private Task InitializeFocusAsync()
    {
        _initialized = true;

        if (!_disabled)
        {
            switch (DefaultFocus)
            {
                case DefaultFocus.Element: return FocusFallbackAsync();
                case DefaultFocus.FirstChild: return FocusFirstAsync();
                case DefaultFocus.LastChild: return FocusLastAsync();
            }
        }
        return Task.CompletedTask;
    }

    private Task FocusFallbackAsync()
    {
        return _fallback.FocusAsync().AsTask();
    }

    private Task FocusFirstAsync()
    {
        return _root.FocusFirstAsync(2, 4).AsTask();
    }

    private Task FocusLastAsync()
    {
        return _root.FocusLastAsync(2, 4).AsTask();
    }

    private void HandleKeyEvent(KeyboardEventArgs args)
    {
        _shouldRender = false;
        if (args.Key == "Tab")
            _shiftDown = args.ShiftKey;
    }

    private Task RestoreFocusAsync()
    {
        return _root.RestoreFocusAsync().AsTask();
    }

    private Task SaveFocusAsync()
    {
        return _root.SaveFocusAsync().AsTask();
    }

    bool _shouldRender = true;

    protected override bool ShouldRender()
    {
        if (_shouldRender)
            return true;
        _shouldRender = true; // auto-reset _shouldRender to true
        return false;
    }

    public void Dispose()
    {
        if (!_disabled)
            RestoreFocusAsync().AndForget(TaskOption.Safe);
    }
}