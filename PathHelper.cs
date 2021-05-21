using System.Diagnostics;

namespace ByondPather
{
    public static class PathHelper
    {
        public static void PatchPairBytes(byte[] b, byte A0, byte A1, byte B0, byte B1, ref int i)
        {
            while (i < b.Length)
            {
                if (b[i] == A0 && b[i + 1] == A1)
                {
                    Debug.WriteLine($"[INFO] Pattern {A0}{A1} Found at {i}, patching with {B0}{B1}");

                    b[i] = B0;
                    b[i + 1] = B1;
                    break;
                }
                i++;
            }
        }

        public static void PatchTripletBytes(byte[] b, byte A0, byte A1, byte A2, byte B0, byte B1, byte B2, ref int i)
        {
            while (i < b.Length)
            {
                if (b[i] == A0 && b[i + 1] == A1 && b[i + 2] == A2)
                {
                    Debug.WriteLine($"[INFO] Pattern {A0}{A1}{A2} Found at {i}, patching with {B0}{B1}{B2}");

                    b[i] = B0;
                    b[i + 1] = B1;
                    b[i + 2] = B2;
                    break;
                }
                i++;
            }
        }

        public static void PatchFiveBytes(byte[] b, byte A0, byte A1, byte A2, byte A3, byte A4, byte B0, byte B1, byte B2, byte B3, byte B4, ref int i)
        {
            while (i < b.Length)
            {
                if (b[i] == A0 && b[i + 1] == A1 && b[i + 2] == A2 && b[i + 3] == A3 && b[i + 4] == A4)
                {
                    Debug.WriteLine($"[INFO] Pattern {A0}{A1}{A2}{A4} Found at {i}, patching with {B0}{B1}{B2}{B4}");

                    b[i] = B0;
                    b[i + 1] = B1;
                    b[i + 2] = B2;
                    b[i + 3] = B3;
                    b[i + 4] = B4;
                    break;
                }
                i++;
            }
        }
    }
}
