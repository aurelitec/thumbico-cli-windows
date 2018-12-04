//----------------------------------------------------------------------------------------------------
//
// <copyright file="Utils.cs" company="Aurelitec">
// Copyright (c) Aurelitec (https://www.aurelitec.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
//
// Description: Static utility methods.
//
//----------------------------------------------------------------------------------------------------

namespace ThumbicoCLI
{
    using System;
    using ThumbicoCLI.Properties;

    /// <summary>
    /// Static utility methods.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Writes an error message to the standard output stream.
        /// </summary>
        /// <param name="summary">The error summary.</param>
        /// <param name="errorMessage">The error message.</param>
        public static void WriteError(string summary, string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write(Resources.Error);
            Console.ResetColor();
            Console.WriteLine($" {summary}: {errorMessage}{Environment.NewLine}");
        }
    }
}
