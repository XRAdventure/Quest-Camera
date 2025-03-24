
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using ZXing;
using ZXing.QrCode;
using ZXing.Rendering;
using System.Linq;

namespace QRCodeVS
{
    public class QRCodeGenerator 
    {
        private string projectPath = string.Empty;
        public QRCodeGenerator() 
        {
            projectPath = TryGetSolutionDirectoryInfo().FullName;
        }

        public DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
        {
            var directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory;
        }
        public void GenerateQRCode(string content, string fileName = "")
        {
            string QrcodeContent = content;
            int width = 1200; // width of the Qr Code
            int height = 1200; // height of the Qr Code

            BarcodeWriterPixelData qrCodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                    QrVersion = 3,
                    Margin = 4,
                }
            };

            PixelData pixelData = qrCodeWriter.Write(QrcodeContent);
            Bitmap bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb);

            using (var ms = new MemoryStream())
            {

                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0,
                       pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                // save to stream as PNG
                bitmap.Save(ms, ImageFormat.Png);
                string filename = GetFilePath(fileName);
                bitmap.Save(filename, ImageFormat.Png);
            }
        }
        string GetFilePath(string fileName)
        {
            string folderPath = Path.Combine(projectPath, "QRCodeGenerated");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return Path.Combine(folderPath, fileName + ".png");
        }


    }
}
