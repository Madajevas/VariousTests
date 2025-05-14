using BenchmarkDotNet.Engines;

using Microsoft.AspNetCore.WebUtilities;

using System.Text.Json;

using Various.Parsing;

namespace VariousBenchmarks.Parsing
{
    [MemoryDiagnoser]
    [SimpleJob(iterationCount: 10)]
    public class FormDataToType
    {
        [Params(
            "name=John&age=30&city=New%20York",
            "name=John&age=30&city=New%20York&address=Some%20Street%20and%20Some%20Apartment&FavoriteNumber=404"
        )]
        public string FormData;

        private Consumer consumer = new();

        [Benchmark(Baseline = true)]
        public IPerson SerializeToJsonAndDeserialize()
        {
            using var formReader = new FormReader(FormData);
            var formCollection = formReader.ReadForm().ToDictionary(k => k.Key, v => v.Value.ToString());
            return JsonSerializer.Deserialize<Person>(JsonSerializer.Serialize(formCollection), JsonSerializerOptions.Web);
        }

        [Benchmark]
        public IPerson ParseUsingFormDataParser()
        {
            return FormDataParser.Parse<IPerson>(FormData);
        }

        [Benchmark]
        public IPerson ParseUsingFormDataParserAndAccessMembers()
        {
            var person = FormDataParser.Parse<IPerson>(FormData);

            consumer.Consume(person.Name);
            consumer.Consume(person.Age);
            consumer.Consume(person.City);

            return person;
        }
    }

    public interface IPerson
    {
        string Name { get; }
        int Age { get; }
        string City { get; }
        string Address { get; }
        int FavoriteNumber { get; }
    }

    class Person : IPerson
    {
        public string Name { get; init ; }
        public int Age { get; init; }
        public string City { get; init; }
        public string Address { get; init; }
        public int FavoriteNumber { get; init; }
    }
}
