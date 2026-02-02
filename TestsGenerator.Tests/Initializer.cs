using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TestsGenerator.Tests
{
    [SetUpFixture]
    internal class Initializer
    {
        [OneTimeSetUp]
        public static void SetUp()
        {
            VerifySourceGenerators.Initialize();
        }
    }
}
