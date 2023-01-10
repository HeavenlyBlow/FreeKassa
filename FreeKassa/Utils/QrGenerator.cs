using System;
using System.Drawing;
using ESCPOS_NET.Utils;
using QRCoder;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;
using Size = System.Drawing.Size;

namespace FreeKassa.Utils
{
    public static class QrGenerator
    {
        public static byte[] Generated(string encode)
        {
            // var qrCode = QrCode(encode);

            return ImageHelper.CenterAlign(QrCode(encode));

            // var mearge = Meagre(AdjustmentRectangle(), qrCode);
            // mearge.Save("qrcode.png");
            // ImageConverter converter = new ImageConverter();
            // return (byte[])converter.ConvertTo(mearge, typeof(byte[]));
        }
        
        private static Bitmap QrCode(string encode)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(encode, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return new Bitmap(qrCodeImage, new Size(200, 200));
        }
        
        // private static Bitmap AdjustmentRectangle()
        // {
        //     Bitmap rectangle = new Bitmap(180,200);
        //     return rectangle;
        // }
        //
        // private static Bitmap Meagre(Image image1, Image image2)
        // {
        //     Bitmap bitmap = new Bitmap(image1.Width + image2.Width, Math.Max(image1.Height, image2.Height));
        //     using (Graphics g = Graphics.FromImage(bitmap))
        //     {
        //         g.DrawImage(image1, 0, 0);
        //         g.DrawImage(image2, image1.Width, 0);
        //     }
        //
        //     return bitmap;
        // }
    }
}