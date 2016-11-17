using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Net;

namespace NeuralNetworks2
{
    public static class ImageLoader
    {
        const string ImageFormatRegex = "\\.png$";
        static ImageConverter converter = new ImageConverter();

        public static Bitmap LoadImage(string path)
        {
            return new Bitmap(path);
        }

        public static IDictionary<string,Bitmap> LoadImagesFromDirectory(string dirPath)
        {
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

        private static IList<int> ParseNameToExpectedResult(string name, int numberOfResultCategories)
        {
            var value = name[0].ToInt();
            var result = new List<int>();
            for (int i = 0; i < numberOfResultCategories; i++)
            {
                result.Add(value == i ? 1 : 0);
            }
            return result;
        }
        
        public static IList<double> ParseImageToVector(Bitmap bmp)
        {
            return BitmapToByteArray(bmp).Select(value => (double) value).ToList();
        }

        public static byte[] BitmapToByteArray(Bitmap bmp)
        {
            var result = new byte[bmp.Width * bmp.Height];

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Console.WriteLine(bmp.Width * y + x);
                    result[bmp.Width * y + x] = bmp.GetPixel(x, y).R;
                }
            }
            return result;
        }

        public static int ToInt(this char val)
        {
            return val - 48;
        }
    }
}
