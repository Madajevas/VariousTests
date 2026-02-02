using TestsGenerator.Abstractions;

namespace TestsToTest
{
    [Multistep]
    public partial class MyOwnSpecialTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [MultistepParticipant]
        public int Test1()
        {
            return 42;
        }

        [MultistepParticipant]
        public void Test2(int outputOfTestBefore)
        {
            Assert.That(outputOfTestBefore, Is.EqualTo(42));
        }

        [MultistepParticipant]
        public double Test3(int outputOfTestBefore)
        {
            Assert.That(outputOfTestBefore, Is.GreaterThan(0));

            return (double)outputOfTestBefore;
        }

        [MultistepParticipant]
        public void Test4(int argOne, double argTwo)
        {
            Assert.That(argOne, Is.EqualTo(argTwo));
        }

        //[MultistepParticipant]
        //public void Test5(int argOne, Guid argTwo)
        //{
        //    Assert.That(argOne.ToString(), Is.EqualTo(argTwo.ToString()));
        //}
    }
}
