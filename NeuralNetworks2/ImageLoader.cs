using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Net;

namespace NeuralNetworks2
{
    public static class ImageLoader
    {
        const string ImageFormatRegex = "\\.png$";

        public static Bitmap LoadImage(string path)
        {
            return new Bitmap(path);
        }

        public static IDictionary<string,Bitmap> LoadImagesFromDirectory(string dirPath)
        {
            dirPath = Path.GetFullPath(dirPath);
            IDictionary<string,Bitmap> imagesWithNames = new Dictionary<string,Bitmap>();
            var fileNames = Directory.GetFiles(dirPath).Where(name => Regex.IsMatch(name, ImageFormatRegex));

            foreach (var fileName in fileNames)
            {
                imagesWithNames.Add(fileName.Substring(dirPath.Length).TrimStart('\\'), LoadImage(fileName));
            }

            return imagesWithNames;
        }

        public static IList<TrainingElement> LoadTrainingElementsFromDirectory(string dirPath, int numberOfResultCategories)
        {
            var images = LoadImagesFromDirectory(dirPath).Select(imageWithName => new TrainingElement
            {
                Input = ParseImageToVector(imageWithName.Value),
                ExpectedOutput = ParseNameToExpectedResult(imageWithName.Key, numberOfResultCategories)
            });

            return images.ToList();
        }

        public static IList<TrainingElement> LoadTrainingElementsFromDirectoryWithoutLabels(string dirPath)
        {
            var images = LoadImagesFromDirectory(dirPath).Select(imageWithName => new TrainingElement
            {
                Input = ParseImageToVector(imageWithName.Value),
                ExpectedOutput = ParseImageToVector(imageWithName.Value)
            });

            return images.ToList();
        }

        private static IList<byte> ParseNameToExpectedResult(string name, int numberOfResultCategories)
        {
            var value = name[0].ToInt();
            var result = new List<byte>();
            for (int i = 0; i < numberOfResultCategories; i++)
            {
                result.Add(value == i ? (byte) 1 : (byte) 0);
            }
            return result;
        }
        
        public static IList<byte> ParseImageToVector(Bitmap bmp)
        {
            return BitmapToByteArray(bmp).ToList();
        }

        public static byte[] BitmapToByteArray(Bitmap bmp)
        {
            var result = new byte[bmp.Width * bmp.Height];

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    result[bmp.Width * y + x] = (byte) (bmp.GetPixel(x, y).R < 196 ? 0 : 1);
                }
            }
            return result;
        }

        public static int ToInt(this char val)
        {
            return val - 48;
        }

        public static Bitmap ParseVectorToImage(byte[] imageData)
        {
            var bmp = new Bitmap(7,10, PixelFormat.Format24bppRgb);
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData =
                bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;
            // Declare an array to hold the bytes of the bitmap.
            int bytes  = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            int j = -1;
            for (int counter = 0; counter < imageData.Length; counter++)
            {
                if (counter % bmp.Width == 0) j++;
                rgbValues[3*(counter + j)] = imageData[counter];
                rgbValues[3*(counter + j) + 1] = imageData[counter];
                rgbValues[3*(counter + j) + 2] = imageData[counter];
            }

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public static Bitmap GetHiddenLayerImage(int numX, int numY, int sizeX, int sizeY, IEnumerator<byte[]> neurons, int border = 2)
        {
            var result = new Bitmap(numX*(sizeX+border) + 3*border, numY*(sizeY+border) + 3 * border, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(result);

            for (int i = 0; i < numY; i++)
            {
                for (int j = 0; j < numX && neurons.MoveNext(); j++)
                {
                    g.DrawImageUnscaled(ParseVectorToImage(neurons.Current), j * (sizeX + border) + 2 * border, i * (sizeY + border) + 2 * border);
                }
            }
            g.Dispose();
            return result;

        }
    }
}
