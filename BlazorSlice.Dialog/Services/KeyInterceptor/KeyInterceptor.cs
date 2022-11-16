using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorSlice.Dialog.Services.KeyInterceptor;

public class KeyInterceptor : IKeyInterceptor
{
    private bool _isDisposed = false;

    private readonly DotNetObjectReference<KeyInterceptor> _dotNetRef;
    private readonly IJSRuntime _jsRuntime;
    private bool _isObserving;
    private string _elementId;
    
    [DynamicDependency(nameof(OnKeyDown))]
    [DynamicDependency(nameof(OnKeyUp))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(KeyboardEvent))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(KeyboardEventArgs))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(KeyOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(KeyInterceptorOptions))]
    public KeyInterceptor(IJSRuntime jsRuntime)
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        _jsRuntime = jsRuntime;
    }
    
    public async Task Connect(string elementId, KeyInterceptorOptions options)
    {
        if (_isObserving || _isDisposed)
            return;
        _elementId = elementId;
        _isObserving = await _jsRuntime.InvokeVoidAsyncWithErrorHandling("keyInterceptor.connect", _dotNetRef, elementId, options);
    }

    public async Task UpdateKey(KeyOptions option)
    {
        await _jsRuntime.InvokeVoidAsync($"keyInterceptor.updatekey", _elementId, option);
    }

    public async Task Disconnect()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync($"keyInterceptor.disconnect", _elementId);
        }
        catch (Exception) {  /*ignore*/ }
        _isObserving = false;
    }
    
    [JSInvokable]
    public void OnKeyDown(KeyboardEventArgs args)
    {
        KeyDown?.Invoke(args);
    }

    [JSInvokable]
    public void OnKeyUp(KeyboardEventArgs args)
    {
        KeyUp?.Invoke(args);
    }

    public event KeyboardEvent KeyDown;
    public event KeyboardEvent KeyUp;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || _isDisposed)
            return;
        _isDisposed = true;
        KeyDown = null;
        KeyUp = null;
        Disconnect().AndForget();
        _dotNetRef.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}