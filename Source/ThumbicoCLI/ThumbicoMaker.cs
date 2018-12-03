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
    using ThumbicoCLI.Properties;

    /// <summary>
    /// The thumbnail generator class.
    /// </summary>
    public class ThumbicoMaker
    {
        private string outputExtension;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbicoMaker"/> class.
        /// </summary>
        public ThumbicoMaker()
        {
            this.Width = 256;
            this.Height = 256;
            this.OutputName = "{n}{e}_{t}";
            this.ImageFormat = ImageFormat.Png;
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
        /// Gets or sets the output directory where the thumbicons will be saved.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the output thumbicon file name template.
        /// </summary>
        public string OutputName { get; set; }

        /// <summary>
        /// Gets or sets the options flags for generating the thumbnails.
        /// </summary>
        public ThumbnailFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the image format of the generated thumbnails (PNG, JPEG, etc.).
        /// </summary>
        public ImageFormat ImageFormat { get; set; }

        /// <summary>
        /// Prepares the thumbnail generator to generate thumbnails.
        /// </summary>
        public void Prepare()
        {
            this.outputExtension = this.ImageFormat.ToString().ToLower();

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
        /// <param name="sourcePath">The source file path and name.</param>
        public void Make(string sourcePath)
        {
            // Get the absolute path for the specified path string
            try
            {
                sourcePath = Path.GetFullPath(sourcePath);
            }
            catch (Exception e)
            {
                // Output any exceptions and return
                Console.WriteLine(Resources.Processing, sourcePath);
                Console.WriteLine(e.Message);
                Console.WriteLine();
                return;
            }

            // Tell the console user we are starting to process the current source file
            Console.WriteLine(Resources.Processing, sourcePath);

            // MAIN FUNCTIONALITY! Try to generate the thumbail or icon
            Bitmap bitmap = ShellThumbnail.GetThumbnail(sourcePath, this.Width, this.Height, this.Flags, out bool isIcon);

            // There was an error generating the thumnail
            if (bitmap == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to generate the thumbnail/icon!");
                Console.ResetColor();
                Console.WriteLine();
                return;
            }

            // Where are we saving the thumbnail: in the global output directory, or in the directory of the source file?
            string thumbiconDirectory = string.IsNullOrEmpty(this.OutputDirectory) ? Path.GetDirectoryName(sourcePath) : this.OutputDirectory;

            // Get the full path and name where to save the generated thumbnail
            string thumbiconName = this.OutputName
                .Replace("{n}", Path.GetFileNameWithoutExtension(sourcePath))
                .Replace("{e}", Path.GetExtension(sourcePath))
                .Replace("{t}", isIcon ? "icon" : "thumbnail")
                .Replace("{w}", bitmap.Width.ToString())
                .Replace("{h}", bitmap.Height.ToString());

            string thumbiconPath = Path.ChangeExtension(Path.Combine(thumbiconDirectory, thumbiconName), this.outputExtension);

            // Save the thumbnail in the correct image file format
            try
            {
                bitmap.Save(thumbiconPath, this.ImageFormat);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to save the thumbnail/icon!");
                Console.ResetColor();
                Console.WriteLine(e.Message);
                Console.WriteLine();
                return;
            }

            // Inform the user that the thumbnail was saved
            Console.WriteLine(isIcon ? Resources.IconSavedAs : Resources.ThumbnailSavedAs, thumbiconPath);
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
