using System;
using System.Diagnostics;
using System.IO;

namespace ByondPatcher
{
    public static class Program
    {
        private const string _fileName = "dreamseeker.exe";

        public static void Main(string[] args)
        {
            string path;

            if (args.Length == 1)
            {
                path = args[0];
            }
            else
            {
                Console.WriteLine($"Enter the path file {_fileName} or drag the file and drop them in the console window:");
                path = Console.ReadLine();
            }

            FileVersionInfo fileInfo;
            while (!IsValidFile(path, out fileInfo))
            {
                Console.Write($"[Error] File {_fileName} not found.\nEnter the path again: ");
                path = Console.ReadLine();
            }

            if (fileInfo.ProductBuildPart <= 512)
                PatchLE512(path);
            else
                PatchGT512(path);
        }

        private static bool IsValidFile(string path, out FileVersionInfo fileInfo)
        {
            fileInfo = null;
            if (!File.Exists(path))
                return false;

            fileInfo = FileVersionInfo.GetVersionInfo(path);
            return fileInfo.OriginalFilename is _fileName;
        }

        /// <summary>
        ///     Patch a version less than or equal to 512
        /// </summary>
        /// <param name="path">File path</param>
        private static void PatchLE512(string path)
        {
            (byte[] pattern, byte[] patch)[] pathPair =
            {
                new(new byte[] { 15, 69, 249 }, new byte[] { 137, 207, 144 }),
                new(new byte[] { 116, 72 }, new byte[] { 144, 144 }),
                new(new byte[] { 15, 132, 68, 2, 0 }, new byte[] { 144, 144, 144, 144, 144 }),
                new(new byte[] { 116, 74 }, new byte[] { 144, 144 }),
                new(new byte[] { 116, 63 }, new byte[] { 144, 144 }),
                new(new byte[] { 116, 14 }, new byte[] { 144, 144 }),
                new(new byte[] { 116, 14 }, new byte[] { 144, 144 }),
                new(new byte[] { 116, 79 }, new byte[] { 144, 144 }),
            };

            Helper.PathBytes(path, pathPair);
        }

        /// <summary>
        ///     Patch a version greater than 512
        /// </summary>
        /// <param name="path">File path</param>
        private static void PatchGT512(string path)
        {
            (byte[] pattern, byte[] patch)[] pathPair =
            {
                new (new byte[] { 191, 30, 0, 0, 0 }, new byte[] { 191, 0, 0, 0, 0 })
            };

            Helper.PathBytes(path, pathPair);
        }
    }
}
