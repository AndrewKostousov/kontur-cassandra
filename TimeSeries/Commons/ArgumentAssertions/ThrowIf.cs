using System;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.ArgumentAssertions
{
    public static class ThrowIf
    {
        public static class Argument
        {
            [AssertionMethod]
            [ContractAnnotation("argument: null => halt")]
            public static void IsNull([CanBeNull] object argument, [InvokerParameterName] string argumentName)
            {
                if(argument == null)
                    throw new ArgumentNullException(argumentName);
            }

            [AssertionMethod]
            public static void IsNullOrEmpty([CanBeNull] string argument, [InvokerParameterName] string argumentName)
            {
                if(string.IsNullOrEmpty(argument))
                    throw new ArgumentException(argumentName);
            }

            [AssertionMethod]
            public static void LessThanOrEqualTo(int value, int argument, [InvokerParameterName] string argumentName)
            {
                if(argument < value)
                    throw new ArgumentOutOfRangeException(argumentName);
            }

            [AssertionMethod]
            public static void IsEmpty([CanBeNull] string value, [InvokerParameterName] string argumentName)
            {
                if(value == string.Empty)
                    throw new ArgumentException(argumentName);
            }
        }
    }
}