using System;

namespace SKBKontur.Catalogue.Objects
{
    public static class ShardingHelpers
    {
        public static int GetShard(int hashCode, int shardsCount)
        {
            if(shardsCount <= 0)
                throw new InvalidProgramStateException(string.Format("ShardsCount should be positive: {0}", shardsCount));
            var longAbsHashCode = Math.Abs((long)hashCode);
            return (int)(longAbsHashCode % shardsCount);
        }
    }
}