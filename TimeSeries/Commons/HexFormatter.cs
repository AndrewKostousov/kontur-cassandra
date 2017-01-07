using JetBrains.Annotations;

namespace Commons
{
    public static class HexFormatter
    {
        [CanBeNull]
        public static string ToHexString([CanBeNull] this byte[] bytes)
        {
            if (bytes == null)
                return null;
            if (bytes.Length == 0)
                return string.Empty;
            var lookup32 = lookup;
            var result = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (var i = 0; i < 256; i++)
            {
                var s = i.ToString("X2");
                result[i] = s[0] + ((uint)s[1] << 16);
            }
            return result;
        }

        private static readonly uint[] lookup = CreateLookup32();
    }
}