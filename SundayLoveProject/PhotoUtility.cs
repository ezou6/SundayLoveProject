using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using System.IO;

namespace SundayLoveProject {
    public class PhotoUtility {
        public static int photoWidth, photoHeight;
        public async static Task<string> ResizePhotoAsync(FileResult photo) {
            var newFilePath = "";
            using (Stream sourceStream = await photo.OpenReadAsync()) {
                // Load the image into a SKBitmap
                SKBitmap originalBitmap = SKBitmap.Decode(sourceStream);
                var newWidth = photoWidth;
                var newHeight = photoHeight;
                if(originalBitmap.Width > originalBitmap.Height) {
                    newWidth = photoHeight;
                    newHeight = photoWidth;
                }
     
                // Resize the image
                SKBitmap resizedBitmap = new SKBitmap(new SKImageInfo(newWidth, newHeight));
                using (SKCanvas canvas = new SKCanvas(resizedBitmap)) {
                    canvas.DrawBitmap(originalBitmap, new SKRect(0, 0, newWidth, newHeight));
                }

                // Save the resized image to a temporary file
                string newFileName = "resized_" + photo.FileName;
                newFilePath = Path.Combine(FileSystem.CacheDirectory, newFileName); // Use CacheDirectory for temporary files
                using (FileStream fs = new FileStream(newFilePath, FileMode.Create)) {
                    resizedBitmap.Encode(SKEncodedImageFormat.Jpeg, 80).SaveTo(fs);
                }

                // Dispose of the SKBitmap objects
                originalBitmap.Dispose();
                resizedBitmap.Dispose();

            }
            // Create a FileResult for the temporary file
            //return new FileResult(newFilePath);
            return newFilePath;
        }

    }

}
