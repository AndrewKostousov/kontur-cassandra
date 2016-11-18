using System;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Objects
{
    public class InvalidProgramStateException : Exception
    {
        public InvalidProgramStateException([NotNull] string message, [CanBeNull] Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}