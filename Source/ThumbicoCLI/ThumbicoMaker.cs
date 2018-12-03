//----------------------------------------------------------------------------------------------------
//
// <copyright file="ThumbicoMaker.cs" company="Aurelitec">
// Copyright (c) Aurelitec (https://www.aurelitec.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
//
// Description: The thumbnail generator class.
//
//----------------------------------------------------------------------------------------------------

namespace ThumbicoCLI
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    /// <summary>
    /// The thumbnail generator class.
    /// </summary>
    public class ThumbicoMaker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbicoMaker"/> class.
        /// </summary>
        public ThumbicoMaker()
        {
            this.Width = 256;
            this.Height = 256;
            this.Extension = ".thumbico.png";
            this.ThumbImageFormat = ImageFormat.Png;
            this.Flags = 0;
        }

        /// <summary>
        /// Gets or sets the width of the generated thumbnails.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the generated thumbnails.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the output directory where the thumbnails will be saved.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the file extension of the generated thumbnails.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets the options flags for generating the thumbnails.
        /// </summary>
        public ThumbnailFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the image format of the generated thumbnails (PNG, JPEG, etc.).
        /// </summary>
        private ImageFormat ThumbImageFormat { get; set; }

        /// <summary>
        /// Prepares the thumbnail generator to generate thumbnails.
        /// </summary>
        public void Prepare()
        {
            // Get the image format from the thumbanil extension (ImageFormat.Png if extension is .PNG, etc.)
            Dictionary<string, ImageFormat> imageFormats = new Dictionary<string, ImageFormat>()
            {
                { ".bmp", ImageFormat.Bmp },
                { ".gif", ImageFormat.Gif },
                { ".jpg", ImageFormat.Jpeg },
                { ".jpeg", ImageFormat.Jpeg },
                { ".png", ImageFormat.Png },
                { ".tiff", ImageFormat.Tiff },
            };
            string extension = Path.GetExtension(this.Extension);
            ImageFormat format;
            if (imageFormats.TryGetValue(extension, out format))
            {
                this.ThumbImageFormat = format;
            }

            // TO DO: We should output to the user the width, height, image format, output directory,
            // and options flags that will be used for generating the thumbnails.
            Console.WriteLine("Width: " + this.Width.ToString());
            Console.WriteLine("Height: " + this.Height.ToString());
            Console.WriteLine("Flags: " + this.Flags.ToString());
            Console.WriteLine();
        }

        /// <summary>
        /// Generates and saves a thumbnail for a specific source file.
        /// </summary>
        /// <param name="path">The source file path and name.</param>
        public void Make(string path)
        {
            // Get the absolute path for the specified path string
            try
            {
                path = Path.GetFullPath(path);
            }
            catch (Exception e)
            {
                // Output any exceptions and return
                Console.WriteLine(Properties.Resources.Processing, path);
                Console.WriteLine(e.Message);
                Console.WriteLine();
                return;
            }

            // Tell the console user we are starting to process the current source file
            Console.WriteLine(Properties.Resources.Processing, path);

            // MAIN FUNCTIONALITY! Try to generate the thumbail or icon
            bool isIcon;
            Bitmap bitmap = ShellThumbnail.GetThumbnail(path, this.Width, this.Height, this.Flags, out isIcon);

            // There was an error generating the thumnail
            if (bitmap == null)
            {
                Console.WriteLine("Unable to generate the thumbail!");
                return;
            }

            // Where are we saving the thumbnail: in the global output directory, or in the directory of the source file?
            string directory = string.IsNullOrEmpty(this.OutputDirectory) ? Path.GetDirectoryName(path) : this.OutputDirectory;

            // Get the full path and name where to save the generated thumbnail
            string thumbFileName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(thumbFileName)) thumbFileName = "thumbicon";

            string thumbPath = Path.Combine(directory, Path.ChangeExtension(thumbFileName, this.Extension));
            Console.WriteLine("PATH" + Path.GetFileName(path));

            // Save the thumbnail in the correct image file format
            bitmap.Save(thumbPath, this.ThumbImageFormat);

            // Inform the user that the thumbnail was saved
            Console.WriteLine((isIcon ? "Icon" : "Thumbnail") + " saved to " + thumbPath);
            Console.WriteLine();
        }

        /// <summary>
        /// Generates and saves thumbnails for a list of specific source files.
        /// </summary>
        /// <param name="paths">The list of path and names of the source files.</param>
        public void Make(List<string> paths)
        {
            if (paths != null)
            {
                paths.ForEach(path => { this.Make(path); });
            }
        }
    }
}
