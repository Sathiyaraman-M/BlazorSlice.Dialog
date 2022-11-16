using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BlazorSlice.Dialog.Services.KeyInterceptor;

public interface IKeyInterceptorFactory
{
    public IKeyInterceptor Create();
}

public class KeyInterceptorFactory : IKeyInterceptorFactory
{
    private readonly IServiceProvider _provider;

    public KeyInterceptorFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IKeyInterceptor Create() =>
        new KeyInterceptor(_provider.GetRequiredService<IJSRuntime>());
}