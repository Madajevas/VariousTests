using Various.Parsing;

namespace VariousTests.Parsing
{
    internal class FormDataParserTests
    {
        private const string FormData = "Name=John&age=30&city=New%20York";

        [Test]
        public void Parse_WhenUsingStringToStringDictionaryBackplaneStrategy_ParsesPersonCorrectly()
        {
            var person = FormDataParser.Parse<IPerson, StringToStringDictionaryBackplaneStrategy>(FormData);

            Assert.That(person.Name, Is.EqualTo("John"));
            Assert.That(person.Age, Is.EqualTo(30));
            Assert.That(person.City, Is.EqualTo("New York"));
        }

        [Test]
        public void Parse_WhenUsingMemoryOfCharToStringDictionaryBackplaneStrategy_ParsesPersonCorrectly()
        {
            var person = FormDataParser.Parse<IPerson, MemoryOfCharToStringDictionaryBackplaneStrategy>(FormData);

            Assert.That(person.Name, Is.EqualTo("John"));
            Assert.That(person.Age, Is.EqualTo(30));
            Assert.That(person.City, Is.EqualTo("New York"));
        }

        [Test]
        public void Parse_WhenUsingMemoryOfCharToMemoryOfCharDictionaryBackplaneStrategy_ParsesPersonCorrectly()
        {
            var person = FormDataParser.Parse<IPerson, MemoryOfCharToMemoryOfCharDictionaryBackplaneStrategy>(FormData);

            Assert.That(person.Name, Is.EqualTo("John"));
            Assert.That(person.Age, Is.EqualTo(30));
            Assert.That(person.City, Is.EqualTo("New York"));
        }
    }

    interface IPerson
    {
        string Name { get; }
        int Age { get; }
        string City { get; }
    }
}
