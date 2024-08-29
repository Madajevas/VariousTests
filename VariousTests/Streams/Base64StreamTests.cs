using System.Text;

using Various.Streams;

namespace VariousTests.Streams
{
    internal class Base64StreamTests
    {
        [TestCase(1024)]
        [TestCase(2 * 1024)]
        [TestCase(6 * 1024)]
        public void EncodingBytesToBase64_AndStreamingEncoding_ShouldProduceSameResult(int length)
        {
            var bytes = new byte[length];
            Random.Shared.NextBytes(bytes);

            var regularResult = Convert.ToBase64String(bytes);

            using var output = new MemoryStream();
            var base64Stream = Base64Stream.CreateForEncoding(output);
            base64Stream.Write(bytes);
            base64Stream.Flush();
            var streamResult = Encoding.ASCII.GetString(output.ToArray());

            Assert.That(regularResult, Is.EqualTo(streamResult));
        }

        [Test]
        public void Dispose_ShouldDisposeOutputStream()
        {
            var output = Substitute.For<Stream>();
            output.CanWrite.Returns(true);

            Base64Stream.CreateForEncoding(output).Dispose();

            output.Received().Dispose();
        }

        [TestCase(1024)]
        [TestCase(2 * 1024)]
        [TestCase(6 * 1024)]
        public void DecodingBytesFromBase64_ProducesCorrectResult(int length)
        {
            var bytes = new byte[length];
            Random.Shared.NextBytes(bytes);
            var base64 = Convert.ToBase64String(bytes);
            using var source = new MemoryStream();
            using (var writer = new StreamWriter(source, leaveOpen: true))
            {
                writer.Write(base64);
            }
            source.Position = 0;

            var base64Stream = Base64Stream.CreateForDecoding(source);
            using var target = new MemoryStream();
            base64Stream.CopyTo(target);

            Assert.That(target.ToArray(), Is.EqualTo(bytes));
        }
    }
}
