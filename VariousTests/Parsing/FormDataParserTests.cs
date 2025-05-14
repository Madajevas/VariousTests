using System.Text.Json;

using Various.Parsing;

namespace VariousTests.Parsing
{
    internal class FormDataParserTests
    {
        private const string FormData = "Name=John&age=30&city=New%20York";

        [Test]
        public void Parse_ParsesPersonCorrectly()
        {
            var person = FormDataParser.Parse<IPerson>(FormData);

            Assert.That(person.Name, Is.EqualTo("John"));
            Assert.That(person.Age, Is.EqualTo(30));
            Assert.That(person.City, Is.EqualTo("New York"));
        }

        [Test]
        public void Parse_ParsesPersonWithAdditionalFields()
        {
            var json = """{"name":"John","age":"30","city":"New York","address":"Some Street and Some Apartment","FavoriteNumber":"404"}""";
            var person = JsonSerializer.Deserialize<Person>(json, JsonSerializerOptions.Web);
        }
    }

    interface IPerson
    {
        string Name { get; }
        int Age { get; }
        string City { get; }
    }

    class Person
    {
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public string City { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int? FavoriteNumber { get; set; }
    }
}
