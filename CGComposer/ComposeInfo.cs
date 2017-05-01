using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CGComposer
{
    static class ComposeInfo
    {
        public static readonly Dictionary<string, string[]> FlagGroup;
        public static readonly Dictionary<string, string> CutInTable;
        public static string CGDirectory { get; set; }
        public static string PictureMode { get; set; }
        public static char JokerChar { get; set; }
        public static int PiercingBitMaskSize { get; set; }
        public static int PiercingBitMaskDefaultValue { get; set; }

        static ComposeInfo()
        {
            FlagGroup = new Dictionary<string, string[]>();
            CutInTable = new Dictionary<string, string>();
        }

        public static void ReadComposeInfo()
        {
            string tempLineValue;
            string key;
            Regex flagGroupRegex = new Regex(@"flagGroup\['(.+?)'\]");
            Regex cutInTableRegex = new Regex(@"CutInTable\['(.+?)'\].+""(d.+?)""");

            using (StreamReader inputReader = new StreamReader(PictureMode, Encoding.GetEncoding(932)))
            {
                while (null != (tempLineValue = inputReader.ReadLine()))
                {
                    // If the line contains flagGroup information
                    if (flagGroupRegex.Match(tempLineValue).Success)
                    {
                        key = GetRegexMatchGroup(tempLineValue, flagGroupRegex, 1);

                        // Parse flagGroup values from next line
                        if (null != (tempLineValue = inputReader.ReadLine()))
                        {
                            FlagGroup[key] = ParseFlagGroupValues(tempLineValue).ToArray();
                        }
                    }
                    // Else if the line contains CutInTable information
                    else if (cutInTableRegex.Match(tempLineValue).Success)
                    {
                        key = GetRegexMatchGroup(tempLineValue, cutInTableRegex, 1);
                        CutInTable[key] = GetRegexMatchGroup(tempLineValue, cutInTableRegex, 2);
                    }
                }
            }
        }

        private static List<string> ParseFlagGroupValues(string line)
        {
            List<string> flagGroupValues = new List<string>();

            foreach (Match m in Regex.Matches(line, @"'(.+?)'"))
            {
                flagGroupValues.Add(m.Groups[1].Value);
            }

            return flagGroupValues;
        }

        public static bool IsValidDirectory(string CGDirectory)
        {
            return TryParsePiercingInfo(CGDirectory) || TryParseWithoutPiercingInfo(CGDirectory);
        }

        private static bool TryParsePiercingInfo(string CGDirectory)
        {
            Regex reg = new Regex(@"^.+Z(.+?)p(.+?)$");

            var files = Directory.GetFiles(
                CGDirectory, @"*.png",
                SearchOption.TopDirectoryOnly
            ).Where(path => reg.IsMatch(Path.GetFileNameWithoutExtension(path)))
            .ToArray();

            if (files.Count() != 0)
            {
                SetJokerCharacter(files[0], reg);
                SetPiercingBitMaskSize(files[0], reg);
            }

            return (files.Count() != 0);
        }

        private static bool TryParseWithoutPiercingInfo(string CGDirectory)
        {
            Regex reg = new Regex(@"^.+Z([^a-zA-Z0-9]+?)$");

            var files = Directory.GetFiles(
                CGDirectory, @"*.png",
                SearchOption.TopDirectoryOnly
            ).Where(path => reg.IsMatch(Path.GetFileNameWithoutExtension(path)))
            .ToArray();

            if (files.Count() != 0)
            {
                SetJokerCharacter(files[0], reg);
                PiercingBitMaskSize = 0;
                PiercingBitMaskDefaultValue = 1;
            }

            return (files.Count() != 0);
        }

        private static void SetJokerCharacter(string file, Regex reg)
        {
            JokerChar = GetRegexMatchGroup(Path.GetFileNameWithoutExtension(file), reg, 1)[0];
        }

        private static void SetPiercingBitMaskSize(string file, Regex reg)
        {
            PiercingBitMaskSize = GetRegexMatchGroup(Path.GetFileNameWithoutExtension(file), reg, 2).Length;
            PiercingBitMaskDefaultValue = (int)Math.Pow(2, PiercingBitMaskSize);
        }

        private static string GetRegexMatchGroup(string str, Regex reg, int groupNumber)
        {
            Match match = reg.Match(str);

            if (match.Success)
            {
                Group group = match.Groups[groupNumber];
                return group.Value;
            }

            return null;
        }
    }

    static class ComposeHelper
    {
        public static List<string> GetBasicLayers(string compositionName)
        {
            List<string> layers = new List<string>();

            // Check if composition matches the format of ".*Z.*"
            int tailIndex = compositionName.IndexOf('Z');

            if (tailIndex == -1)
            {
                // If the format does not match, the composition itself is the only element (the base element)
                layers.Add(Path.Combine(ComposeInfo.CGDirectory, compositionName + @".png"));
            }
            else
            {
                char[] baseElement = compositionName.ToCharArray();
                string tail = compositionName.Substring(tailIndex + 1);

                // Replace the characters behind the 'Z' with joker characters
                // This is the base element
                ModifyIntoBaseElement(baseElement, tailIndex);
                layers.Add(Path.Combine(ComposeInfo.CGDirectory, new string(baseElement) + @".png"));

                // Get the rest of the basic layers from the tail and add into layers
                GenerateAndAddLayersFromTail(tail, tailIndex, baseElement, layers);
            }

            return layers;
        }

        private static void ModifyIntoBaseElement(char[] composition, int tailIndex)
        {
            for (int idx = tailIndex + 1; idx < composition.Length; ++idx)
            {
                composition[idx] = ComposeInfo.JokerChar;
            }
        }

        private static void ModifyIntoBaseMask(char[] baseElement, int tailIndex)
        {
            for (int idx = tailIndex - 2; idx < tailIndex; ++idx)
            {
                baseElement[idx] = ComposeInfo.JokerChar;
            }
            baseElement[tailIndex] = 'Y';
        }

        private static void GenerateAndAddLayersFromTail(string tail, int tailIndex, char[] baseElement, List<string> layers)
        {
            // Replace the 2 characters in front of the 'Z' with joker characters
            ModifyIntoBaseMask(baseElement, tailIndex);

            for (int idx = 0; idx < tail.Length; ++idx)
            {
                if (tail[idx] != ComposeInfo.JokerChar)
                {
                    baseElement[tailIndex + 1 + idx] = tail[idx];
                    layers.Add(Path.Combine(ComposeInfo.CGDirectory, new string(baseElement) + @".png"));
                    baseElement[tailIndex + 1 + idx] = ComposeInfo.JokerChar;
                }
            }
        }

        public static List<string> GetPubeLayers(string baseLayer, string directoryPath = null)
        {
            if (directoryPath == null) directoryPath = ComposeInfo.CGDirectory;

            var files = Directory.GetFiles(
                directoryPath, Path.GetFileNameWithoutExtension(baseLayer) + @"i?.png",
                SearchOption.TopDirectoryOnly
            );

            return files.ToList();
        }

        public static List<string> GetPiercingLayers(string baseLayer, string directoryPath = null)
        {
            if (directoryPath == null) directoryPath = ComposeInfo.CGDirectory;

            var files = Directory.GetFiles(
                directoryPath, Path.GetFileNameWithoutExtension(baseLayer) + "p" + new string('?', ComposeInfo.PiercingBitMaskSize) + ".png",
                SearchOption.TopDirectoryOnly
            );

            return files.ToList();
        }

        public static string CombinePiercingLayersBitMask(List<string> piercingLayers)
        {
            int combinedMask = ComposeInfo.PiercingBitMaskDefaultValue;

            foreach (var layer in piercingLayers)
            {
                int currentMask = Convert.ToInt32(
                    layer.Substring(layer.Length - 4 - ComposeInfo.PiercingBitMaskSize, ComposeInfo.PiercingBitMaskSize),
                    2
                );
                combinedMask |= currentMask;
            }

            string result = Convert.ToString(combinedMask, 2);

            return result.Substring(result.Length - ComposeInfo.PiercingBitMaskSize);
        }

        public static List<string> GetCutInLayers(string cutInBaseName)
        {
            Regex reg = new Regex(@"^" + cutInBaseName + @"[^ip]+?\.png$");

            var files = Directory.GetFiles(
                ComposeInfo.CGDirectory, cutInBaseName + @"*.png",
                SearchOption.TopDirectoryOnly
            ).Where(path => reg.IsMatch(Path.GetFileName(path)));

            return files.ToList();
        }
    }
}
