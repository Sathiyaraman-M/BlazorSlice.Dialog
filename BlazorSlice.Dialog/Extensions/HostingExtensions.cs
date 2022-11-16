using BlazorSlice.Dialog.Services.KeyInterceptor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlazorSlice.Dialog.Extensions;

public static class HostingExtensions
{
    public static IServiceCollection AddBlazorSliceDialog(this IServiceCollection services)
    {
        services.TryAddScoped<IDialogService, DialogService>();
        services.AddBlazorSliceKeyInterceptor();
        return services;
    }
    
    public static IServiceCollection AddBlazorSliceKeyInterceptor(this IServiceCollection services)
    {
        services.TryAddTransient<IKeyInterceptor, KeyInterceptor>();
        services.TryAddScoped<IKeyInterceptorFactory, KeyInterceptorFactory>();

        return services;
    }
}