using Microsoft.AspNetCore.Components.Web;

namespace BlazorSlice.Dialog.Services.KeyInterceptor;

public delegate void KeyboardEvent(KeyboardEventArgs args);

public interface IKeyInterceptor : IDisposable
{
    Task Connect(string elementId, KeyInterceptorOptions options);
    Task Disconnect();
    Task UpdateKey(KeyOptions option);

    event KeyboardEvent KeyDown;
    event KeyboardEvent KeyUp;
}