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
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Flag options for generating the thumbnail.
    /// </summary>
    [Flags]
    public enum ThumbnailFlags
    {
        /// <summary>
        /// Shrink the bitmap as necessary to fit, preserving its aspect ratio.
        /// </summary>
        ResizeToFit = 0x00000000,

        /// <summary>
        /// Passed by callers if they want to stretch the returned image themselves.
        /// </summary>
        BiggerSizeOk = 0x00000001,

        /// <summary>
        /// Return the item only if it is already in memory.
        /// </summary>
        MemoryOnly = 0x00000002,

        /// <summary>
        /// Return only the icon, never the thumbnail.
        /// </summary>
        IconOnly = 0x00000004,

        /// <summary>
        /// Return only the thumbnail, never the icon. Note that not all items have thumbnails,
        /// so this flag will cause the method to fail in these cases.
        /// </summary>
        ThumbnailOnly = 0x00000008,

        /// <summary>
        /// Allows access to the disk, but only to retrieve a cached item.
        /// </summary>
        InCacheOnly = 0x00000010,

        /// <summary>
        /// (Introduced in Windows 8) If necessary, crop the bitmap to a square.
        /// </summary>
        CropToSquare = 0x00000020,

        /// <summary>
        /// (Introduced in Windows 8) Stretch and crop the bitmap to a 0.7 aspect ratio.
        /// </summary>
        WideThumbnails = 0x00000040,

        /// <summary>
        /// (Introduced in Windows 8) If returning an icon, paint a background using the
        /// associated app's registered background color.
        /// </summary>
        IconBackground = 0x00000080,

        /// <summary>
        /// (Introduced in Windows 8) If necessary, stretch the bitmap so that the height
        /// and width fit the given size.
        /// </summary>
        ScaleUp = 0x00000100,
    }

    /// <summary>
    /// Exposes a method to return either thumbnails or icons for Shell items.
    /// </summary>
    public class ShellThumbnail
    {
        /// <summary>
        /// Gets a thumbnail or icon image that represents a Shell item.
        /// </summary>
        /// <param name="path">The path of the shell item (file/folder).</param>
        /// <param name="width">The width, in pixels, of the requested image.</param>
        /// <param name="height">The height, in pixels, of the requested image.</param>
        /// <param name="flags">Option flags for generating the image.</param>
        /// <returns>A managed <see cref="System.Windows.Media.Imaging.BitmapSource"/> that represents the retrieved thumbnail or icon.</returns>
        public static BitmapSource GetImage(string path, int width, int height, ThumbnailFlags flags)
        {
            NativeMethods.IShellItemImageFactory factory = null;
            IntPtr hBitmap = IntPtr.Zero;
            try
            {
                int hr = NativeMethods.SHCreateItemFromParsingName(path, IntPtr.Zero, NativeMethods.IShellItemImageFactoryGuid, out factory);
                if (hr != 0)
                {
                    throw Marshal.GetExceptionForHR(hr);
                }

                hr = factory.GetImage(new NativeMethods.SIZE(width, height), flags, out hBitmap);
                if (hr != 0)
                {
                    throw Marshal.GetExceptionForHR(hr);
                }

                return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                if (factory != null)
                {
                    Marshal.ReleaseComObject(factory);
                }

                if (hBitmap != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(hBitmap);
                }
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed.")]
        private class NativeMethods
        {
            internal static readonly Guid IShellItemImageFactoryGuid = new Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b");

            [ComImport]
            [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            internal interface IShellItemImageFactory
            {
                [PreserveSig]
                int GetImage([In, MarshalAs(UnmanagedType.Struct)] SIZE size, [In] ThumbnailFlags flags, [Out] out IntPtr phbm);
            }

            [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern int SHCreateItemFromParsingName(
                [MarshalAs(UnmanagedType.LPWStr)] string path,
                IntPtr pbc,
                [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
                [MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory factory);

            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DeleteObject(IntPtr hObject);

            [StructLayout(LayoutKind.Sequential)]
            internal struct SIZE
            {
                public int cx;
                public int cy;

                public SIZE(int cx, int cy)
                {
                    this.cx = cx;
                    this.cy = cy;
                }
            }
        }
    }
}