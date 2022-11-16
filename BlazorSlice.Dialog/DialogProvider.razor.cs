using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorSlice.Dialog;

public partial class DialogProvider
{
    [Inject] private IDialogService DialogService { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }

    [Parameter] 
    public bool? NoHeader { get; set; }
    
    [Parameter]
    public bool? CloseButton { get; set; }
    
    [Parameter]
    public bool? DisableBackdropClick { get; set; }
    
    [Parameter]
    public bool? CloseOnEscapeKey { get; set; }
    
    [Parameter]
    public bool? FullWidth { get; set; }
    
    [Parameter]
    public DialogPosition? Position { get; set; }
    
    [Parameter] 
    public Size? Size { get; set; }

    private readonly Collection<IDialogReference> _dialogs = new();
    private readonly DialogOptions _globalDialogOptions = new();

    protected override void OnInitialized()
    {
        DialogService.OnDialogInstanceAdded += AddInstance;
        DialogService.OnDialogCloseRequested += DismissInstance;
        NavigationManager.LocationChanged += LocationChanged;

        _globalDialogOptions.DisableBackdropClick = DisableBackdropClick;
        _globalDialogOptions.CloseOnEscapeKey = CloseOnEscapeKey;
        _globalDialogOptions.CloseButton = CloseButton;
        _globalDialogOptions.NoHeader = NoHeader;
        _globalDialogOptions.Position = Position;
        _globalDialogOptions.FullWidth = FullWidth;
        _globalDialogOptions.Size = Size;
    }

    internal void DismissInstance(Guid id, DialogResult result)
    {
        var reference = GetDialogReference(id);
        if (reference != null)
            DismissInstance(reference, result);
    }

    private void AddInstance(IDialogReference dialog)
    {
        _dialogs.Add(dialog);
        StateHasChanged();
    }

    public void DismissAll()
    {
        _dialogs.ToList().ForEach(r => DismissInstance(r, DialogResult.Cancel()));
        StateHasChanged();
    }

    private void DismissInstance(IDialogReference dialog, DialogResult result)
    {
        if (!dialog.Dismiss(result)) return;

        _dialogs.Remove(dialog);
        StateHasChanged();
    }

    private IDialogReference GetDialogReference(Guid id)
    {
        return _dialogs.SingleOrDefault(x => x.Id == id);
    }

    private void LocationChanged(object sender, LocationChangedEventArgs args)
    {
        DismissAll();
    }

    public void Dispose()
    {
        if (NavigationManager != null)
            NavigationManager.LocationChanged -= LocationChanged;

        if (DialogService != null)
        {
            DialogService.OnDialogInstanceAdded -= AddInstance;
            DialogService.OnDialogCloseRequested -= DismissInstance;
        }
    }
}