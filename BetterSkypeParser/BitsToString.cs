namespace BetterSkypeParser
{
    public static class BitsToString
    {
        public static string Convert(double bits)
        {
            string[] units = [Lang.Resources.Bits, Lang.Resources.Bytes, Lang.Resources.Kilobytes,
                              Lang.Resources.Megabytes, Lang.Resources.Gigabytes, Lang.Resources.Terabytes];
            int unitIndex = 0;

            if (bits >= 8)
            {
                bits /= 8;
                unitIndex++;

                while (bits >= 1024 && unitIndex < units.Length-1)
                {
                    bits /= 1024;
                    unitIndex++;
                }
            }

            return $"{bits:0.##} {units[unitIndex]}";
        }
    }
}
