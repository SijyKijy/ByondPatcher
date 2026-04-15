using System;
using System.Diagnostics;
using System.IO;

namespace ByondPatcher;

public static class Program
{
    private const string DreamSeeker = "dreamseeker.exe";
    private const string ByondCore = "byondcore.dll";

    public static void Main(string[] args)
    {
        string path;

        if (args.Length == 1)
        {
            path = args[0];
        }
        else
        {
            Console.WriteLine($"Enter the path to {DreamSeeker} or drag and drop it into this window:");
            path = Console.ReadLine()?.Trim('"') ?? "";
        }

        FileVersionInfo fileInfo;
        while (!IsValidFile(path, out fileInfo))
        {
            Console.Write($"[Error] {DreamSeeker} not found.\nEnter the path again: ");
            path = Console.ReadLine()?.Trim('"') ?? "";
        }

        int version = fileInfo.ProductBuildPart;
        Console.WriteLine($"[Info] BYOND version: {fileInfo.ProductMajorPart}.{version}");

        if (version <= 512)
            Patch512(path);
        else if (version <= 515)
            Patch513(path);
        else
            Patch516(path);

        Console.WriteLine("Press any key to close...");
        Console.ReadKey();
    }

    private static bool IsValidFile(string path, out FileVersionInfo fileInfo)
    {
        fileInfo = null;
        if (!File.Exists(path))
            return false;

        fileInfo = FileVersionInfo.GetVersionInfo(path);
        return fileInfo.OriginalFilename is DreamSeeker;
    }

    /// <summary>
    ///     BYOND 512 and earlier - Jcc -> NOP
    /// </summary>
    private static void Patch512(string path)
    {
        Helper.Backup(path);
        (byte[] pattern, byte[] patch)[] pairs =
        [
            ([ 0x0F, 0x45, 0xF9 ], [ 0x89, 0xCF, 0x90 ]),
            ([ 0x74, 0x48 ], [ 0x90, 0x90 ]),
            ([ 0x0F, 0x84, 0x44, 0x02, 0x00 ], [ 0x90, 0x90, 0x90, 0x90, 0x90 ]),
            ([ 0x74, 0x4A ], [ 0x90, 0x90 ]),
            ([ 0x74, 0x3F ], [ 0x90, 0x90 ]),
            ([ 0x74, 0x0E ], [ 0x90, 0x90 ]),
            ([ 0x74, 0x0E ], [ 0x90, 0x90 ]),
            ([ 0x74, 0x4F ], [ 0x90, 0x90 ]),
        ];

        Helper.PatchSequential(path, pairs);
        Console.WriteLine($"[OK] {DreamSeeker} patched");
    }

    /// <summary>
    ///     BYOND 513-515 - MOV EDI, 30 -> MOV EDI, 0
    /// </summary>
    private static void Patch513(string path)
    {
        Helper.Backup(path);
        (byte[] pattern, byte[] patch)[] pairs =
        [
            ([ 0xBF, 0x1E, 0x00, 0x00, 0x00 ], [ 0xBF, 0x00, 0x00, 0x00, 0x00 ]),
        ];

        Helper.PatchSequential(path, pairs);
        Console.WriteLine($"[OK] {DreamSeeker} patched");
    }

    /// <summary>
    ///     BYOND 516+ - patch IsByondMember in byondcore.dll to return true
    /// </summary>
    private static void Patch516(string path)
    {
        var corePath = Path.Combine(Path.GetDirectoryName(path)!, ByondCore);
        if (!File.Exists(corePath))
        {
            Console.WriteLine($"[Error] {ByondCore} not found next to {DreamSeeker}");
            return;
        }

        Helper.Backup(corePath);

        // mov eax, 1; ret 4  (thiscall, always return true)
        byte[] patch = [ 0xB8, 0x01, 0x00, 0x00, 0x00, 0xC2, 0x04, 0x00 ];

        byte[] source = File.ReadAllBytes(corePath);
        int count = 0;

        // DungClient::IsByondMember - push ebp; mov ebp,esp; push [ebp+8]; call ...
        count += Helper.PatchExport(source, "IsByondMember@DungClient",
            [ 0x55, 0x8B, 0xEC, 0xFF, 0x75, 0x08, 0xE8 ], patch);

        // DungPager::IsByondMember - push ebp; mov ebp,esp; mov ecx,[ecx+C]; pop ebp; jmp ...
        count += Helper.PatchExport(source, "IsByondMember@DungPager",
            [ 0x55, 0x8B, 0xEC, 0x8B, 0x49, 0x0C, 0x5D, 0xE9 ], patch);

        if (count > 0)
        {
            File.WriteAllBytes(corePath, source);
            Console.WriteLine($"[OK] {ByondCore}: {count} function(s) patched");
        }
        else
        {
            Console.WriteLine($"[Info] {ByondCore}: already patched or patterns not found");
        }
    }
}
