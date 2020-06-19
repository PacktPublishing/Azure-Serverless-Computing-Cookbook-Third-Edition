using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
public static void Run(Stream myBlob, string name,Stream imageSmall,Stream imageMedium, ILogger log)
{
           try
            {
                IImageFormat format;

                using (Image<Rgba32> input = Image.Load<Rgba32>(myBlob, out format))
                {
                    ResizeImageAndSave(input, imageSmall, ImageSize.Small, format);
                }

                myBlob.Position = 0;
                using (Image<Rgba32> input = Image.Load<Rgba32>(myBlob, out format))
                {
                    ResizeImageAndSave(input, imageMedium, ImageSize.Medium, format);
                }

            }
            catch (Exception e)
            {
                log.LogError(e, $"unable to process the blob");
            }

}
        public static void ResizeImageAndSave(Image<Rgba32> input, Stream output, ImageSize size, IImageFormat format)
        {
            var dimensions = imageDimensionsTable[size];

            input.Mutate(x => x.Resize(width: dimensions.Item1, height: dimensions.Item2));
            input.Save(output, format);
        }

        public enum ImageSize { ExtraSmall, Small, Medium }

        private static Dictionary<ImageSize, (int, int)> imageDimensionsTable = new Dictionary<ImageSize, (int, int)>() 
        {
          
            { ImageSize.Small,      (100, 100) },
            { ImageSize.Medium,     (200, 200) }
        };
