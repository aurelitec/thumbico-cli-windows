//----------------------------------------------------------------------------------------------------
//
// <copyright file="ShellThumbnail.cs" company="Aurelitec">
// Copyright (c) Aurelitec (https://www.aurelitec.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
//
// Description: Provides a method to return an icon or thumbnail for a Shell item.
//
//----------------------------------------------------------------------------------------------------

namespace ThumbicoCLI
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Flag options for generating the thumbnail.
    /// </summary>
    [Flags]
    public enum ThumbnailFlags
    {
        /// <summary>
        /// Shrink the bitmap as necessary to fit, preserving its aspect ratio.
        /// </summary>
        ResizeToFit = 0x00,

        /// <summary>
        /// Passed by callers if they want to stretch the returned image themselves.
        /// </summary>
        BiggerSizeOk = 0x01,

        /// <summary>
        /// Return the item only if it is already in memory.
        /// </summary>
        MemoryOnly = 0x02,

        /// <summary>
        /// Return only the icon, never the thumbnail.
        /// </summary>
        IconOnly = 0x04,

        /// <summary>
        /// Return only the thumbnail, never the icon. Note that not all items have thumbnails,
        /// so this flag will cause the method to fail in these cases.
        /// </summary>
        ThumbnailOnly = 0x08,

        /// <summary>
        /// Allows access to the disk, but only to retrieve a cached item.
        /// </summary>
        InCacheOnly = 0x10,

        /// <summary>
        /// (Introduced in Windows 8) If necessary, crop the bitmap to a square.
        /// </summary>
        CropToSquare = 0x20,

        /// <summary>
        /// (Introduced in Windows 8) Stretch and crop the bitmap to a 0.7 aspect ratio.
        /// </summary>
        WideThumbnails = 0x40,

        /// <summary>
        /// (Introduced in Windows 8) If returning an icon, paint a background using the
        /// associated app's registered background color.
        /// </summary>
        IconBackground = 0x80,

        /// <summary>
        /// (Introduced in Windows 8) If necessary, stretch the bitmap so that the height
        /// and width fit the given size.
        /// </summary>
        ScaleUp = 0x100,
    }

    /// <summary>
    /// Provides a method to return an icon or thumbnail for a Shell item.
    /// </summary>
    public static class ShellThumbnail
    {
        /// <summary>
        /// Gets either an icon or a thumbnail for a Shell item specified as a path string.
        /// </summary>
        /// <param name="path">The path of the shell item.</param>
        /// <param name="width">The width of the thumbnail to return.</param>
        /// <param name="height">The height of the thumbnail to return.</param>
        /// <param name="flags">The options flags for generating the thumbnail.</param>
        /// <param name="isIcon">Returns true if the generated thumbnail is actually an icon.</param>
        /// <returns>A managed bitmap containing the requested thumbnail.</returns>
        public static Bitmap GetThumbnail(string path, int width, int height, ThumbnailFlags flags, out bool isIcon)
        {
            Bitmap bitmap = null;
            isIcon = false;

            // Try to get the thumbnail or icon from the Windows Shell
            IntPtr nativeBitmap = ShellThumbnail.GetHBitmap(
                path,
                width,
                height,
                ThumbnailFlags.ThumbnailOnly | flags);
            if (nativeBitmap == IntPtr.Zero)
            {
                nativeBitmap = ShellThumbnail.GetHBitmap(path, width, height, ThumbnailFlags.IconOnly | flags);
                isIcon = true;
            }

            // Convert the received native bitmap to a managed bitmap
            if (nativeBitmap != IntPtr.Zero)
            {
                bitmap = HBitmapToManagedBitmap(nativeBitmap, PixelFormat.Format32bppArgb);
                if (isIcon)
                {
                    // Because of a strange behaviour, icons are returned up side down, so we must flip them vertically
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }
            }

            // Return the managed bitmap
            return bitmap;
        }

        /// <summary>
        /// Gets the best possible image (thumbnail or icon) for a shell item (a thumbnail or an icon).
        /// </summary>
        /// <param name="path">The path of the shell item.</param>
        /// <param name="width">The width of the generated image.</param>
        /// <param name="height">The height of the generated image.</param>
        /// <param name="flags">The option flags for generating the image.</param>
        /// <returns>A native bitmap containing the image.</returns>
        private static IntPtr GetHBitmap(string path, int width, int height, ThumbnailFlags flags)
        {
            ////Console.WriteLine(flags.ToString());

            IntPtr nativeBitmap = IntPtr.Zero;

            // Creates and initializes a IShellItemImageFactoryGuid object from the item's parsing name
            NativeMethods.IShellItemImageFactory shellItem;
            if (NativeMethods.SHCreateItemFromParsingName(
                path,
                IntPtr.Zero,
                NativeMethods.IShellItemImageFactoryGuid,
                out shellItem) == NativeMethods.S_OK)
            {
                NativeMethods.SIZE nativeSIZE = new NativeMethods.SIZE() { cx = width, cy = height };

                // Get the best possible image for the shell item, or throw an exception on error
                shellItem.GetImage(nativeSIZE, flags, out nativeBitmap);
            }

            // Return the native GDI bitmap handle, or IntPtr.Zero on error
            return nativeBitmap;
        }

        /// <summary>
        /// Converts a native HBitmap to a managed bitmap and frees the original HBitmap.
        /// </summary>
        /// <param name="nativeHBitmap">The native HBitmap object.</param>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>The converted managed bitmap.</returns>
        private static Bitmap HBitmapToManagedBitmap(IntPtr nativeHBitmap, PixelFormat pixelFormat)
        {
            // Get width, height and the address of the pixel data for the native HBitmap
            NativeMethods.BITMAP bitmapStruct = default(NativeMethods.BITMAP);
            NativeMethods.GetObjectBitmap(nativeHBitmap, Marshal.SizeOf(bitmapStruct), ref bitmapStruct);

            // Create a managed bitmap that has its pixel data pointing to the pixel data of the native HBitmap
            // No memory is allocated for its pixel data
            Bitmap managedBitmapPointer = new Bitmap(
                bitmapStruct.bmWidth, bitmapStruct.bmHeight, bitmapStruct.bmWidthBytes, pixelFormat, bitmapStruct.bmBits);

            // Create a managed bitmap and allocate memory for pixel data
            Bitmap managedBitmapReal = new Bitmap(bitmapStruct.bmWidth, bitmapStruct.bmHeight, pixelFormat);

            // Copy the pixels of the native HBitmap into the canvas of the managed bitmap
            Graphics graphics = Graphics.FromImage(managedBitmapReal);
            graphics.DrawImage(managedBitmapPointer, 0, 0);

            // Important! Call the Dispose method to release the Graphics and related resources
            graphics.Dispose();

            // Important! Delete the native HBitmap object and free memory
            NativeMethods.DeleteObject(nativeHBitmap);

            // Return the managed bitmap, clone of the native HBitmap, with correct transparency
            return managedBitmapReal;
        }
    }

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    internal static class NativeMethods
    {
        public const int S_OK = 0;

        public static readonly Guid IShellItemImageFactoryGuid = new Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b");

        [ComImportAttribute]
        [GuidAttribute("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellItemImageFactory
        {
            [PreserveSig]
            void GetImage(
            [In, MarshalAs(UnmanagedType.Struct)] SIZE size,
            [In] ThumbnailFlags flags,
            [Out] out IntPtr phbm);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern int SHCreateItemFromParsingName(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            [In] IntPtr pbc,
            [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out][MarshalAs(UnmanagedType.Interface, IidParameterIndex = 2)] out IShellItemImageFactory ppv);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32", CharSet = CharSet.Auto, EntryPoint = "GetObject")]
        public static extern int GetObjectBitmap(IntPtr hObject, int nCount, ref BITMAP lpObject);

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public IntPtr bmBits;
        }
    }
}
