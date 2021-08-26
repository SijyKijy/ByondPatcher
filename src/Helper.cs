using System;
using System.Diagnostics;
using System.IO;

namespace ByondPatcher
{
    public static class Helper
    {
        public static void PathBytes(string path, (byte[] pattern, byte[] patch)[] pathPair)
        {
            byte[] source = File.ReadAllBytes(path);

            int changedBytes = 0;
            int position = 0;

            for (int i = 0; i < pathPair.Length; i++)
                changedBytes += PathBytes(source, pathPair[i].pattern, pathPair[i].patch, ref position);

            Console.WriteLine("Bytes changed = " + changedBytes);
            File.WriteAllBytes(path, source);

            Console.WriteLine("Patched. Press any key to close this window...");
            Console.ReadLine();
        }

        public static int PathBytes(Span<byte> source, ReadOnlySpan<byte> pattern, ReadOnlySpan<byte> patch, ref int position)
        {
            int changedBytes = 0;

            while (position < source.Length)
            {
                if (IsPatternMatch(source, pattern, position))
                {
                    Debug.WriteLine($"[INFO] Pattern [{string.Join(';', pattern.ToArray())}] Found at {position}, patching with [{string.Join(';', patch.ToArray())}]");

                    for (int i = 0; i < patch.Length; i++)
                        source[position + i] = patch[i];

                    changedBytes++;
                    break;
                }
                position++;
            }

            return changedBytes;
        }

        private static bool IsPatternMatch(ReadOnlySpan<byte> source, ReadOnlySpan<byte> pattern, int position)
        {
            for (int i = 0; i < pattern.Length; i++)
                if (source[position + i] != pattern[i])
                    return false;
            return true;
        }
    }
}
