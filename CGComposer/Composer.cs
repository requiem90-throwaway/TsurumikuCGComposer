using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace CGComposer
{
    static class Composer
    {
        public static void Compose(TextBox LogTextBox = null)
        {
            MakeOutputFolders();

            foreach (var CGBase in ComposeInfo.FlagGroup)
            {
                foreach (var CG in CGBase.Value)
                {
                    ComposeCG(CG, LogTextBox);
                }
            }
        }

        private static void MakeOutputFolders()
        {
            if (!Directory.Exists(ComposeInfo.CGDirectory + @"\composite"))
            {
                Directory.CreateDirectory(ComposeInfo.CGDirectory + @"\composite");
            }
            if (!Directory.Exists(ComposeInfo.CGDirectory + @"\composite with piercings"))
            {
                Directory.CreateDirectory(ComposeInfo.CGDirectory + @"\composite with piercings");
            }
            if (!Directory.Exists(ComposeInfo.CGDirectory + @"\cut in"))
            {
                Directory.CreateDirectory(ComposeInfo.CGDirectory + @"\cut in");
            }
            if (!Directory.Exists(ComposeInfo.CGDirectory + @"\cut in with piercings"))
            {
                Directory.CreateDirectory(ComposeInfo.CGDirectory + @"\cut in with piercings");
            }
        }

        private static void OverlayImages(Bitmap baseImage, Bitmap overlayImage)
        {
            using (Graphics g = Graphics.FromImage(baseImage))
            {
                g.DrawImage(overlayImage, 0, 0, baseImage.Width, baseImage.Height);
            }
        }

        private static void FadeImageToBlack(Bitmap baseImage)
        {
            using (Graphics g = Graphics.FromImage(baseImage))
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(127, Color.Black)))
                {
                    g.FillRectangle(brush, 0, 0, baseImage.Width, baseImage.Height);
                }
            }
        }

        private static void ComposeCG(string compositionName, TextBox LogTextBox = null)
        {
            List<string> basicLayers = ComposeHelper.GetBasicLayers(compositionName);
            if (basicLayers.Count == 0) return;

            using (Bitmap baseImage = (Bitmap)System.Drawing.Image.FromFile(basicLayers[0]))
            {
                // Composite base
                ComposeLayers(baseImage, basicLayers);

                // Composite pubes
                List<string> pubeLayers = ComposeHelper.GetPubeLayers(basicLayers[0]);
                PrintResult(ComposePubeLayers(baseImage, pubeLayers, compositionName), LogTextBox);

                List<string> cutInLayers = null;
                if (ComposeInfo.CutInTable.ContainsKey(compositionName))
                {
                    // Composite cut ins
                    cutInLayers = ComposeHelper.GetCutInLayers(ComposeInfo.CutInTable[compositionName]);
                    PrintResult(ComposeCutInLayers(baseImage, cutInLayers, compositionName), LogTextBox);
                }

                List<string> piercingLayers = ComposeHelper.GetPiercingLayers(basicLayers[0]);
                if (piercingLayers.Count > 0)
                {
                    // Composite piercings
                    string piercingLayersBitMask = ComposeHelper.CombinePiercingLayersBitMask(piercingLayers);
                    PrintResult(ComposePiercingLayers(baseImage, piercingLayers, compositionName, piercingLayersBitMask), LogTextBox);

                    // Composite cut ins + piercings
                    if (cutInLayers != null)
                    {
                        PrintResult(ComposeCutInLayers(baseImage, cutInLayers, compositionName, piercingLayersBitMask), LogTextBox);
                    }
                }
            }
        }

        private static void ComposeLayers(Bitmap baseImage, List<string> layers)
        {
            foreach (var layer in layers)
            {
                using (Bitmap overLayImage = (Bitmap)System.Drawing.Image.FromFile(layer))
                {
                    OverlayImages(baseImage, overLayImage);
                }
            }
        }

        private static string ComposePubeLayers(Bitmap baseImage, List<string> layers, string compositionName)
        {
            ComposeLayers(baseImage, layers);
            baseImage.Save(Path.Combine(ComposeInfo.CGDirectory, "composite", compositionName + @".png"));

            return compositionName + @".png";
        }

        private static string ComposePiercingLayers(Bitmap baseImage, List<string> layers, string compositionName, string piercingLayersBitMask)
        {
            ComposeLayers(baseImage, layers);
            string piercingfileName = compositionName + "p" + piercingLayersBitMask + @".png";
            baseImage.Save(Path.Combine(ComposeInfo.CGDirectory, "composite with piercings", piercingfileName));

            return piercingfileName;
        }

        private static List<string> ComposeCutInLayers(Bitmap baseImage, List<string> layers, string compositionName, string piercingLayersBitMask = null)
        {
            List<string> results = new List<string>();

            foreach (var layer in layers)
            {
                using (Bitmap overLayImage = (Bitmap)System.Drawing.Image.FromFile(layer))
                {
                    using (Bitmap baseImageCopy = new Bitmap(baseImage))
                    {
                        if (!Path.GetFileNameWithoutExtension(layer).Contains("TP"))
                        {
                            FadeImageToBlack(baseImageCopy);
                        }

                        OverlayImages(baseImageCopy, overLayImage);

                        List<string> pubicLayers = ComposeHelper.GetPubeLayers(layer);
                        ComposeLayers(baseImageCopy, pubicLayers);

                        if (piercingLayersBitMask == null)
                        {
                            var filename = compositionName + "_" + Path.GetFileNameWithoutExtension(layer) + @".png";
                            baseImageCopy.Save(Path.Combine(ComposeInfo.CGDirectory, "cut in", filename));

                            results.Add(filename);
                        }
                        else
                        {
                            List<string> piercingLayers = ComposeHelper.GetPiercingLayers(layer);
                            ComposeLayers(baseImageCopy, piercingLayers);

                            // Perform check to see if this composition is obsolete (no difference with/without piercings)
                            var filename = compositionName + "_" + Path.GetFileNameWithoutExtension(layer) + @".png";
                            if (!IsDuplicate("cut in", filename, baseImageCopy))
                            {
                                filename = compositionName + "p" + piercingLayersBitMask + "_" + Path.GetFileNameWithoutExtension(layer) + @".png";
                                baseImageCopy.Save(Path.Combine(ComposeInfo.CGDirectory, "cut in with piercings", filename));

                                results.Add(filename);
                            }
                        }
                    }
                }
            }

            return results;
        }

        private static bool IsDuplicate(string directory, string referenceImage, Bitmap image)
        {
            bool isDuplicate;
            using (Bitmap withoutPiercing = (Bitmap)System.Drawing.Image.FromFile(Path.Combine(ComposeInfo.CGDirectory, directory, referenceImage)))
            {
                isDuplicate = CompareBitmaps(image, withoutPiercing);
            }

            return isDuplicate;
        }

        public delegate void UpdateTextCallback(string message);
        private static void PrintResult(string result, TextBox LogTextBox)
        {
            if (LogTextBox != null)
            {
                LogTextBox.Dispatcher.Invoke(
                    new UpdateTextCallback(LogTextBox.AppendText),
                    new object[] { result + Environment.NewLine }
                );
            }
        }

        private static void PrintResult(List<string> results, TextBox LogTextBox)
        {
            foreach (var result in results)
            {
                PrintResult(result, LogTextBox);
            }
        }

        private static bool CompareBitmaps(Bitmap leftBitmap, Bitmap rightBitmap)
        {
            if (Equals(leftBitmap, rightBitmap))
                return true;
            if (leftBitmap == null || rightBitmap == null)
                return false;
            if (!leftBitmap.Size.Equals(rightBitmap.Size) || !leftBitmap.PixelFormat.Equals(rightBitmap.PixelFormat))
                return false;

            if (leftBitmap == null || rightBitmap == null)
                return true;

            #region Optimized code for performance

            int bytes = leftBitmap.Width * leftBitmap.Height * (System.Drawing.Image.GetPixelFormatSize(leftBitmap.PixelFormat) / 8);

            bool result = true;
            byte[] b1bytes = new byte[bytes];
            byte[] b2bytes = new byte[bytes];

            BitmapData bmd1 = leftBitmap.LockBits(new Rectangle(0, 0, leftBitmap.Width - 1, leftBitmap.Height - 1), ImageLockMode.ReadOnly, leftBitmap.PixelFormat);
            BitmapData bmd2 = rightBitmap.LockBits(new Rectangle(0, 0, rightBitmap.Width - 1, rightBitmap.Height - 1), ImageLockMode.ReadOnly, rightBitmap.PixelFormat);

            Marshal.Copy(bmd1.Scan0, b1bytes, 0, bytes);
            Marshal.Copy(bmd2.Scan0, b2bytes, 0, bytes);

            for (int n = 0; n <= bytes - 1; n++)
            {
                if (b1bytes[n] != b2bytes[n])
                {
                    result = false;
                    break;
                }
            }

            leftBitmap.UnlockBits(bmd1);
            rightBitmap.UnlockBits(bmd2);

            #endregion

            return result;
        }
    }
}
