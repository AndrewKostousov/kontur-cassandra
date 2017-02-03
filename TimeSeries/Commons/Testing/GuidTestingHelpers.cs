using System;

namespace Commons.Testing
{
    public static class GuidTestingHelpers
    {
        public static byte GetLastByte(Guid guid)
        {
            return guid.ToByteArray()[15];
        }

        public static Guid Guid(byte lastByte)
        {
            var bytes = new byte[16];
            bytes[15] = lastByte;
            return new Guid(bytes);
        }
    }
}