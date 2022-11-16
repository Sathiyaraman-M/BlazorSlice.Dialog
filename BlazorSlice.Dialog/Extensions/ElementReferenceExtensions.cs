using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BlazorSlice.Dialog.Interop;
using Microsoft.JSInterop;

namespace BlazorSlice.Dialog.Extensions;

[ExcludeFromCodeCoverage]
public static class ElementReferenceExtensions
{
    private static readonly PropertyInfo jsRuntimeProperty =
        typeof(WebElementReferenceContext).GetProperty("JSRuntime", BindingFlags.Instance | BindingFlags.NonPublic);

    internal static IJSRuntime GetJSRuntime(this ElementReference elementReference)
    {
        if (elementReference.Context is not WebElementReferenceContext context)
        {
            return null;
        }

        return (IJSRuntime)jsRuntimeProperty.GetValue(context);
    }
    
    public static ValueTask FocusFirstAsync(this ElementReference elementReference, int skip = 0, int min = 0) =>
        elementReference.GetJSRuntime()?.InvokeVoidAsync("elementRef.focusFirst", elementReference, skip, min) ?? ValueTask.CompletedTask;
    
    public static ValueTask FocusLastAsync(this ElementReference elementReference, int skip = 0, int min = 0) =>
        elementReference.GetJSRuntime()?.InvokeVoidAsync("elementRef.focusLast", elementReference, skip, min) ?? ValueTask.CompletedTask;

    public static ValueTask SaveFocusAsync(this ElementReference elementReference) =>
        elementReference.GetJSRuntime()?.InvokeVoidAsync("elementRef.saveFocus", elementReference) ?? ValueTask.CompletedTask;

    public static ValueTask RestoreFocusAsync(this ElementReference elementReference) =>
        elementReference.GetJSRuntime()?.InvokeVoidAsync("elementRef.restoreFocus", elementReference) ?? ValueTask.CompletedTask;

    public static ValueTask BlurAsync(this ElementReference elementReference) =>
        elementReference.GetJSRuntime()?.InvokeVoidAsync("elementRef.blur", elementReference) ?? ValueTask.CompletedTask;

    public static ValueTask SelectAsync(this ElementReference elementReference) =>
        elementReference.GetJSRuntime()?.InvokeVoidAsync("elementRef.select", elementReference) ?? ValueTask.CompletedTask;

    public static ValueTask SelectRangeAsync(this ElementReference elementReference, int pos1, int pos2) =>
        elementReference.GetJSRuntime()?.InvokeVoidAsync("elementRef.selectRange", elementReference, pos1, pos2) ?? ValueTask.CompletedTask;

    public static ValueTask ChangeCssAsync(this ElementReference elementReference, string css) =>
        elementReference.GetJSRuntime()?.InvokeVoidAsync("elementRef.changeCss", elementReference, css) ?? ValueTask.CompletedTask;

    public static ValueTask<BoundingClientRect> GetBoundingClientRectAsync(this ElementReference elementReference) =>
        elementReference.GetJSRuntime()?.InvokeAsync<BoundingClientRect>("elementRef.getBoundingClientRect", elementReference) ?? ValueTask.FromResult(new BoundingClientRect());
    
    public static ValueTask<BoundingClientRect> GetClientRectFromParentAsync(this ElementReference elementReference) =>
        elementReference.GetJSRuntime()?.InvokeAsync<BoundingClientRect>("elementRef.getClientRectFromParent", elementReference) ?? ValueTask.FromResult(new BoundingClientRect());
    
    public static ValueTask<BoundingClientRect> GetClientRectFromFirstChildAsync(this ElementReference elementReference) =>
        elementReference.GetJSRuntime()?.InvokeAsync<BoundingClientRect>("elementRef.getClientRectFromFirstChild", elementReference) ?? ValueTask.FromResult(new BoundingClientRect());

    public static ValueTask<bool> HasFixedAncestorsAsync(this ElementReference elementReference) =>
        elementReference.GetJSRuntime()?
            .InvokeAsync<bool>("elementRef.hasFixedAncestors", elementReference) ?? ValueTask.FromResult(false);

    public static ValueTask ChangeCssVariableAsync(this ElementReference elementReference, string variableName, int value) =>
        elementReference.GetJSRuntime()?.InvokeVoidAsync("elementRef.changeCssVariable", elementReference, variableName, value) ?? ValueTask.CompletedTask;

    public static ValueTask<int> AddEventListenerAsync<T>(this ElementReference elementReference, DotNetObjectReference<T> dotnet, string @event, string callback, bool stopPropagation = false) where T : class
    {
        var parameters = dotnet?.Value.GetType().GetMethods().First(m => m.Name == callback).GetParameters().Select(p => p.ParameterType);
        if (parameters != null)
        {
            var parameterSpecs = new object[parameters.Count()];
            for (var i = 0; i < parameters.Count(); ++i)
            {
                parameterSpecs[i] = GetSerializationSpec(parameters.ElementAt(i));
            }
            return elementReference.GetJSRuntime()?.InvokeAsync<int>("elementRef.addEventListener", elementReference, dotnet, @event, callback, parameterSpecs, stopPropagation) ?? ValueTask.FromResult(0);
        }
        else
        {
            return new ValueTask<int>(0);
        }
    }

    public static ValueTask RemoveEventListenerAsync(this ElementReference elementReference, string @event, int eventId) =>
        elementReference.GetJSRuntime()?.InvokeVoidAsync("elementRef.removeEventListener", elementReference, eventId) ?? ValueTask.CompletedTask;

    private static object GetSerializationSpec(Type type)
    {
        var props = type.GetProperties();
        var propsSpec = new Dictionary<string, object>();
        foreach (var prop in props)
        {
            if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
            {
                propsSpec.Add(prop.Name.ToJsString(), "*");
            }
            else if (prop.PropertyType.IsArray)
            {
                propsSpec.Add(prop.Name.ToJsString(), GetSerializationSpec(prop.PropertyType.GetElementType()));
            }
            else if (prop.PropertyType.IsClass)
            {
                propsSpec.Add(prop.Name.ToJsString(), GetSerializationSpec(prop.PropertyType));
            }
        }

        return propsSpec;
    }

    public static ValueTask<int> AddDefaultPreventingHandler(this ElementReference elementReference, string eventName) =>
        elementReference.GetJSRuntime()?.InvokeAsync<int>("elementRef.addDefaultPreventingHandler", elementReference, eventName) ?? new ValueTask<int>(0);

    public static ValueTask RemoveDefaultPreventingHandler(this ElementReference elementReference, string eventName, int listenerId) =>
        elementReference.GetJSRuntime()?.InvokeVoidAsync("elementRef.removeDefaultPreventingHandler", elementReference, eventName, listenerId) ?? ValueTask.CompletedTask;

    public static ValueTask<int[]> AddDefaultPreventingHandlers(this ElementReference elementReference, string[] eventNames) =>
        elementReference.GetJSRuntime()?.InvokeAsync<int[]>("elementRef.addDefaultPreventingHandlers", elementReference, eventNames) ?? new ValueTask<int[]>(Array.Empty<int>());

    public static ValueTask RemoveDefaultPreventingHandlers(this ElementReference elementReference, string[] eventNames, int[] listenerIds)
    {
        if (eventNames.Length != listenerIds.Length)
        {
            throw new ArgumentException($"Number of elements in {nameof(eventNames)} and {nameof(listenerIds)} has to match.");
        }

        return elementReference.GetJSRuntime()?.InvokeVoidAsync("elementRef.removeDefaultPreventingHandlers", elementReference, eventNames, listenerIds) ?? ValueTask.CompletedTask;
    }
}