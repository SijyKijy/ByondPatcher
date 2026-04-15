using System;
using System.IO;
using System.Text;

namespace ByondPatcher;

public static class Helper
{
    public static void Backup(string path)
    {
        var bak = path + ".bak";
        if (!File.Exists(bak))
        {
            File.Copy(path, bak);
            Console.WriteLine($"[Backup] {Path.GetFileName(bak)}");
        }
    }

    public static void PatchSequential(string path, (byte[] pattern, byte[] patch)[] pairs)
    {
        byte[] source = File.ReadAllBytes(path);

        int changed = 0;
        int position = 0;

        foreach (var (pattern, patch) in pairs)
        {
            int idx = FindPattern(source, pattern, position);
            if (idx >= 0)
            {
                Array.Copy(patch, 0, source, idx, patch.Length);
                position = idx + patch.Length;
                changed++;
            }
        }

        Console.WriteLine($"Bytes changed = {changed}");
        File.WriteAllBytes(path, source);
    }

    public static int PatchExport(byte[] source, string exportSubstring, byte[] expectedBytes, byte[] patchBytes)
    {
        int offset = FindExportOffset(source, exportSubstring);
        if (offset < 0)
            offset = FindPattern(source, expectedBytes);

        if (offset < 0)
        {
            if (FindPattern(source, patchBytes) >= 0)
            {
                Console.WriteLine($"  [=] {exportSubstring}: already patched");
                return 0;
            }
            Console.WriteLine($"  [!] {exportSubstring}: not found");
            return 0;
        }

        if (MatchesAt(source, offset, patchBytes))
        {
            Console.WriteLine($"  [=] {exportSubstring}: already patched");
            return 0;
        }

        if (!MatchesAt(source, offset, expectedBytes))
        {
            Console.WriteLine($"  [!] {exportSubstring}: unexpected bytes at 0x{offset:X}");
            return 0;
        }

        Array.Copy(patchBytes, 0, source, offset, patchBytes.Length);
        Console.WriteLine($"  [+] {exportSubstring}: patched at 0x{offset:X}");
        return 1;
    }

    public static int FindPattern(byte[] source, byte[] pattern, int startPos = 0)
    {
        for (int i = startPos; i <= source.Length - pattern.Length; i++)
            if (MatchesAt(source, i, pattern))
                return i;
        return -1;
    }

    private static bool MatchesAt(byte[] source, int offset, byte[] pattern)
    {
        if (offset + pattern.Length > source.Length)
            return false;
        for (int i = 0; i < pattern.Length; i++)
            if (source[offset + i] != pattern[i])
                return false;
        return true;
    }

    public static int FindExportOffset(byte[] pe, string nameSubstring)
    {
        if (pe.Length < 0x40 || pe[0] != 'M' || pe[1] != 'Z')
            return -1;

        int peHeaderOffset = BitConverter.ToInt32(pe, 0x3C);
        if (peHeaderOffset + 4 + 20 + 100 >= pe.Length)
            return -1;

        int coffHeader = peHeaderOffset + 4;
        int optionalHeader = coffHeader + 20;
        int sectionCount = BitConverter.ToInt16(pe, coffHeader + 2);
        int optionalHeaderSize = BitConverter.ToInt16(pe, coffHeader + 16);
        int exportDirRva = BitConverter.ToInt32(pe, optionalHeader + 96);
        if (exportDirRva == 0)
            return -1;

        int sectionsStart = optionalHeader + optionalHeaderSize;

        int ToFileOffset(int rva)
        {
            for (int i = 0; i < sectionCount; i++)
            {
                int sec = sectionsStart + i * 40;
                int sectionRva = BitConverter.ToInt32(pe, sec + 12);
                int rawSize = BitConverter.ToInt32(pe, sec + 16);
                int rawOffset = BitConverter.ToInt32(pe, sec + 20);
                if (rva >= sectionRva && rva < sectionRva + rawSize)
                    return rawOffset + (rva - sectionRva);
            }
            return -1;
        }

        int exportDir = ToFileOffset(exportDirRva);
        if (exportDir < 0) return -1;

        int nameCount = BitConverter.ToInt32(pe, exportDir + 24);
        int funcTable = ToFileOffset(BitConverter.ToInt32(pe, exportDir + 28));
        int nameTable = ToFileOffset(BitConverter.ToInt32(pe, exportDir + 32));
        int ordinalTable = ToFileOffset(BitConverter.ToInt32(pe, exportDir + 36));
        if (funcTable < 0 || nameTable < 0 || ordinalTable < 0) return -1;

        for (int i = 0; i < nameCount; i++)
        {
            int nameOffset = ToFileOffset(BitConverter.ToInt32(pe, nameTable + i * 4));
            if (nameOffset < 0) continue;

            int end = nameOffset;
            while (end < pe.Length && pe[end] != 0) end++;
            string exportName = Encoding.ASCII.GetString(pe, nameOffset, end - nameOffset);

            if (exportName.Contains(nameSubstring))
            {
                int ordinal = BitConverter.ToInt16(pe, ordinalTable + i * 2);
                int funcRva = BitConverter.ToInt32(pe, funcTable + ordinal * 4);
                return ToFileOffset(funcRva);
            }
        }

        return -1;
    }
}
