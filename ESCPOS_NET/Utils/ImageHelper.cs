using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Mime;

namespace ESCPOS_NET.Utils;

public class ImageHelper
{
    private const int x = 560;
    
    public static byte[] CenterAlign(Bitmap bitmap)
    {
        ImageConverter converter = new ImageConverter();
        var rectangle = AdjustmentRectangle(bitmap.Width, bitmap.Height);
        if(rectangle.Width == 0) return (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
        var mearge = Meagre(rectangle, bitmap);
        return (byte[])converter.ConvertTo(mearge, typeof(byte[]));
    }
    
    private static Bitmap AdjustmentRectangle(int width, int height)
    {
        if (width >= x) return new Bitmap(0,0);
        var rectWidth = (x - width) / 2;
        return new Bitmap(rectWidth, height);
    }

    private static Bitmap Meagre(Image image1, Image image2)
    {
        Bitmap bitmap = new Bitmap(image1.Width + image2.Width, Math.Max(image1.Height, image2.Height));
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.DrawImage(image1, 0, 0);
            g.DrawImage(image2, image1.Width, 0);
        }
        return bitmap;
    }
}