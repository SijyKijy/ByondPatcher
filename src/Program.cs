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
            byte[] incArray = File.ReadAllBytes(path);
            byte[] resArray = new byte[incArray.Length];

            incArray.CopyTo(resArray, 0);

            int i = 0;
            Helper.PatchTripletBytes(resArray, 15, 69, 249, 137, 207, 144, ref i);
            Helper.PatchPairBytes(resArray, 116, 72, 144, 144, ref i);
            Helper.PatchFiveBytes(resArray, 15, 132, 68, 2, 0, 144, 144, 144, 144, 144, ref i);
            Helper.PatchPairBytes(resArray, 116, 74, 144, 144, ref i);
            Helper.PatchPairBytes(resArray, 116, 63, 144, 144, ref i);
            Helper.PatchPairBytes(resArray, 116, 14, 144, 144, ref i);
            Helper.PatchPairBytes(resArray, 116, 14, 144, 144, ref i);
            Helper.PatchPairBytes(resArray, 116, 79, 144, 144, ref i);

            int num = 0;
            for (int j = 0; j < incArray.Length; j++)
            {
                if (incArray[j] != resArray[j])
                    num++;
            }

            Console.WriteLine("Bytes changed = " + num);
            File.WriteAllBytes(path, resArray);

            Console.WriteLine("Patched. Press any key to close this window...");
            Console.ReadLine();
        }

        /// <summary>
        ///     Patch a version greater than 512
        /// </summary>
        /// <param name="path">File path</param>
        private static void PatchGT512(string path)
        {
            byte[] incArray = File.ReadAllBytes(path);
            byte[] resArray = new byte[incArray.Length];

            incArray.CopyTo(resArray, 0);

            int i = 0;
            Helper.PatchFiveBytes(resArray, 191, 30, 0, 0, 0, 191, 0, 0, 0, 0, ref i);

            int num = 0;
            for (int j = 0; j < incArray.Length; j++)
            {
                if (incArray[j] != resArray[j])
                    num++;
            }

            Console.WriteLine("Bytes changed = " + num);
            File.WriteAllBytes(path, resArray);

            Console.WriteLine("Patched. Press any key to close this window...");
            Console.ReadLine();
        }
    }
}
