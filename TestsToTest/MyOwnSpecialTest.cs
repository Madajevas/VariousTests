namespace TestsToTest
{
    [Multistep]
    public partial class MyOwnSpecialTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [MyTest]
        public int Test1()
        {
            return 42;
        }

        [MyTest]
        public void Test2(int outputOfTestBefore)
        {
            Assert.That(outputOfTestBefore, Is.EqualTo(42));
        }

        [MyTest]
        public double Test3(int outputOfTestBefore)
        {
            Assert.That(outputOfTestBefore, Is.GreaterThan(0));

            return (double)outputOfTestBefore;
        }

        [MyTest]
        public void Test4(int argOne, double argTwo)
        {
            Assert.That(argOne, Is.EqualTo(argTwo));
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MultistepAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MyTestAttribute : Attribute
    {

    }
}
