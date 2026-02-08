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
