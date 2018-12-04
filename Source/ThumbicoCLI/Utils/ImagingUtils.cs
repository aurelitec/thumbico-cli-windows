//----------------------------------------------------------------------------------------------------
//
// <copyright file="ImagingUtils.cs" company="Aurelitec">
// Copyright (c) Aurelitec (https://www.aurelitec.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
//
// Description: Imaging types and utility methods.
//
//----------------------------------------------------------------------------------------------------

namespace ThumbicoCLI
{
    using System.IO;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Specifies the file format of the image.
    /// </summary>
    public enum ImageFormat
    {
        /// <summary>
        /// Bitmap (BMP)
        /// </summary>
        Bmp,

        /// <summary>
        /// Graphics Interchange Format (GIF)
        /// </summary>
        Gif,

        /// <summary>
        /// Joint Photographics Experts Group (JPEG)
        /// </summary>
        Jpg,

        /// <summary>
        /// Portable Network Graphics (PNG)
        /// </summary>
        Png,

        /// <summary>
        /// Tagged Image File Format (TIFF)
        /// </summary>
        Tiff
    }

    /// <summary>
    /// Imaging static utility methods.
    /// </summary>
    internal static class ImagingUtils
    {
        /// <summary>
        /// Saves a bitmap image to the specified file.
        /// </summary>
        /// <param name="bitmap">The bitmap image to save.</param>
        /// <param name="filePath">The name of the file to which to save the image.</param>
        /// <param name="imageFormat">An ImageFormat that specifies the format of the saved image.</param>
        public static void SaveBitmap(BitmapSource bitmap, string filePath, ImageFormat imageFormat)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder =
                    imageFormat.Equals(ImageFormat.Bmp) ? new BmpBitmapEncoder() :
                    imageFormat.Equals(ImageFormat.Gif) ? new GifBitmapEncoder() :
                    imageFormat.Equals(ImageFormat.Jpg) ? new JpegBitmapEncoder() :
                    imageFormat.Equals(ImageFormat.Png) ? new PngBitmapEncoder() :
                    imageFormat.Equals(ImageFormat.Tiff) ? new TiffBitmapEncoder() :
                    (BitmapEncoder)new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(fileStream);
            }
        }
    }
}
