//----------------------------------------------------------------------------------------------------
//
// <copyright file="Program.cs" company="Aurelitec">
// Copyright (c) Aurelitec (https://www.aurelitec.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
//
// Description: The main program class.
//
//----------------------------------------------------------------------------------------------------

namespace ThumbicoCLI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using Mono.Options;

    /// <summary>
    /// The main program class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The thumbnail generator object.
        /// </summary>
        private static ThumbicoMaker maker;

        /// <summary>
        /// Parses the command line, sets the options, and returns the list of input files.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The list of input files.</returns>
        private static List<string> ParseCommandLine(string[] args)
        {
            // Create the options set
            var p = new OptionSet()
            {
                { "w|width:", "the width of the thumbnail.", (int v) => maker.Width = v },
                { "h|height:", "the height of the thumbnail.", (int v) => maker.Height = v },
                { "d|dir|directory:", "the output directory.", v => maker.OutputDirectory = v },
                { "e|ext|extension:", "the extension.", v => maker.Extension = v },

                // flags
                { "rf|ResizeToFit", "ResizeToFit",                   v => maker.Flags |= ThumbnailFlags.ResizeToFit },
                { "bs|BiggerSizeOk", "BiggerSizeOk",                 v => maker.Flags |= ThumbnailFlags.BiggerSizeOk },
                { "mo|MemoryOnly", "MemoryOnly",                     v => maker.Flags |= ThumbnailFlags.MemoryOnly },
                { "io|IconOnly", "IconOnly",                         v => maker.Flags |= ThumbnailFlags.IconOnly },
                { "to|ThumbnailOnly", "ThumbnailOnly",               v => maker.Flags |= ThumbnailFlags.ThumbnailOnly },
                { "co|InCacheOnly", "InCacheOnly",                   v => maker.Flags |= ThumbnailFlags.InCacheOnly },
                { "cs|CropToSquare", "CropToSquare",                 v => maker.Flags |= ThumbnailFlags.CropToSquare },
                { "wt|WideThumbnails", "WideThumbnails",             v => maker.Flags |= ThumbnailFlags.WideThumbnails },
                { "ib|IconBackground", "IconBackground",             v => maker.Flags |= ThumbnailFlags.IconBackground },
                { "su|ScaleUp", "ScaleUp",                           v => maker.Flags |= ThumbnailFlags.ScaleUp },
            };

            // Parse the command line and return the list of input files
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("Command line argument error: ");
                Console.WriteLine(e.Message);

                // TO DO: We should implement a HELP command and screen.
                //// Console.WriteLine("Try `greet --help' for more information.");
                return null;
            }

            return extra;
        }

        /// <summary>
        /// The Main function: starts and runs the program.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        private static void Main(string[] args)
        {
            // Write the program header with the program name, version, copyright, license
            Console.WriteLine(@"
  _____ _                     _     _              ____ _     ___ 
 |_   _| |__  _   _ _ __ ___ | |__ (_) ___ ___    / ___| |   |_ _|
   | | | '_ \| | | | '_ ` _ \| '_ \| |/ __/ _ \  | |   | |    | | 
   | | | | | | |_| | | | | | | |_) | | (_| (_) | | |___| |___ | | 
   |_| |_| |_|\__,_|_| |_| |_|_.__/|_|\___\___/   \____|_____|___|
                                                                  
            ");

            string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            Console.WriteLine($"Thumbico CLI Version {version}");
            Console.WriteLine("Copyright (c) 2014-2018 Aurelitec. MIT License. https://www.aurelitec.com\n");

            // Create the thumbico generator object
            Program.maker = new ThumbicoMaker();

            // Parse the command line
            List<string> files = Program.ParseCommandLine(args);

            // If we have input files in the command line, prepare, generate and save thumbnails for them
            if (files != null)
            {
                Program.maker.Prepare();
                Program.maker.Make(files);
            }
        }
    }
}
