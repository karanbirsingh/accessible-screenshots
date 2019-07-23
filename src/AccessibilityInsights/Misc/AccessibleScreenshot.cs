// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Axe.Windows.Actions;
using Axe.Windows.Core.Bases;
using Axe.Windows.Core.Types;
using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AccessibilityInsights.Misc
{
    /// <summary>
    /// Action for saving snapshot data
    /// </summary>
    public static class AccessibleScreenshot
    {
        const string AccessibleKeyword = "a11ytree";

        /// <summary>
        /// Save snapshot zip
        /// </summary>
        /// <param name="ecId">ElementContext Id</param>
        /// <param name="path">The output file</param>
        public static void SaveScreenshot(string path, Guid ecId)
        {
            var a11yElement = DataManager.GetDefaultInstance().GetA11yElement(ecId, 0);
            var bm = DataManager.GetDefaultInstance().GetScreenshot(ecId);
            var screenshotElement = DataManager.GetDefaultInstance().GetScreenshotElement(ecId);
            var trimmedElement = FromA11yElement(a11yElement);

            var trimmedScreenshot = TrimScreenshot(bm, screenshotElement.BoundingRectangle, a11yElement.BoundingRectangle);
            var encoded = AddMetadata(trimmedScreenshot, trimmedElement);
            using (FileStream str = File.Open(path, FileMode.Create))
            {
                encoded.Save(str);
            }
        }

        private static Bitmap TrimScreenshot(Bitmap bm, Rectangle source, Rectangle portion)
        {
            var relativeX = portion.X - source.X;
            var relativeY = portion.Y - source.Y;

            Bitmap target = new Bitmap(portion.Width, portion.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(bm, new Rectangle(0, 0, target.Width, target.Height),
                                 new Rectangle(relativeX, relativeY, target.Width, target.Height),
                                 GraphicsUnit.Pixel);
            }
            return target;
        }

        static ElementNode FromA11yElement(IA11yElement element)
        {
            var controlType = string.IsNullOrEmpty(element.LocalizedControlType) ?
                ControlType.GetInstance().GetNameById(element.ControlTypeId) : element.LocalizedControlType;
            return new ElementNode()
            {
                Name = element.Name,
                ControlType = controlType,
                Children = element.Children.Select(child => FromA11yElement(child))
            };
        }

        public static BitmapSource Convert(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        public static PngBitmapEncoder AddMetadata(Bitmap bitmap, ElementNode element)
        {
            var tree = AccessibleXmp.ToXmpString(element);
            var pngMetadata = new BitmapMetadata("png");

            pngMetadata.SetQuery("/iTXt/Keyword", AccessibleKeyword.ToCharArray());
            pngMetadata.SetQuery("/iTXt/TextEntry", tree);

            //pngMetadata.SetQuery("/[1]iTXt/Keyword", "keyword0".ToCharArray());
            //pngMetadata.SetQuery("/[1]iTXt/TextEntry", "textentry0");

            var bmSource = Convert(bitmap);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmSource, null, pngMetadata, null));
            return encoder;
        }

        public static ElementNode LoadMetadata(string path)
        {
            var bitmap = new Bitmap(path).Clone();
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(path);
            var a11yTag = directories.Where(dir => dir.Name == "PNG-iTXt").SelectMany(dir => dir.Tags)
                .Where(tag => tag.Description.StartsWith(AccessibleKeyword, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (a11yTag == null) return null;

            return AccessibleXmp.FromXmpElement(a11yTag.Description.Substring(AccessibleKeyword.Length + 1));
        }
    }
}