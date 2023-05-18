using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Versatile.Common;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Versatile.CommonUI.Services;

public class WindowsService
{
    public static async void CaptureElement(UIElement content)
    {
        var dataPackage = new DataPackage();
        var stream = await RenderContentToRasterStreamAsync(content);
        dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));

        Clipboard.SetContent(dataPackage);
    }

    private static async Task<InMemoryRandomAccessStream> RenderContentToRasterStreamAsync(UIElement content)
    {
        var scale = VersatileApp.GetService<Window>().Content.XamlRoot.RasterizationScale;

        double actualHeight = content.RenderSize.Height;
        double actualWidth = content.RenderSize.Width;
        double renderHeight = actualHeight * scale;
        double renderWidth = actualWidth * scale;

        var renderTargetBitmap = new RenderTargetBitmap();
        await renderTargetBitmap.RenderAsync(content);






        var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
        var pixels = pixelBuffer.ToArray();
        var stream = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
        encoder.SetPixelData(BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            (uint)renderTargetBitmap.PixelWidth,
            (uint)renderTargetBitmap.PixelHeight,
            scale,
            scale,
            pixels);
        await encoder.FlushAsync();

        return stream;
    }

}
