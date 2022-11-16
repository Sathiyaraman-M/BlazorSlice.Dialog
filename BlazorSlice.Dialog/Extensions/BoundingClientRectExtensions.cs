using BlazorSlice.Dialog.Interop;

namespace BlazorSlice.Dialog.Extensions;

public static class BoundingClientRectExtensions
{
    public static bool IsEqualTo(this BoundingClientRect sourceRect, BoundingClientRect targetRect)
    {
        if (sourceRect is null || targetRect is null) return false;
        return sourceRect.Top == targetRect.Top
               && sourceRect.Left == targetRect.Left
               && sourceRect.Width == targetRect.Width
               && sourceRect.Height == targetRect.Height
               && sourceRect.WindowHeight == targetRect.WindowHeight
               && sourceRect.WindowWidth == targetRect.WindowWidth
               && sourceRect.ScrollX == targetRect.ScrollX
               && sourceRect.ScrollY == targetRect.ScrollY;
    }
}