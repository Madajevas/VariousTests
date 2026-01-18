using System.Text;

using Various.Streams;

namespace VariousTests.Streams
{
    internal class Base64StreamTests
    {
        [TestCase(1024)]
        [TestCase(3 * 1024)]
        [TestCase(64 * 1024 * 1024 + 1)]
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

            Assert.That(streamResult, Is.EqualTo(regularResult));
        }

        [Test]
        public void Encoding_UsingWriteMethodWithOffsetAndLength_ProducesCorrectResult([Random(1024, 4096, 7)] int bytesToWrite)
        {
            var bytes = new byte[64 * 1024 + 1];
            Random.Shared.NextBytes(bytes);

            var regularResult = Convert.ToBase64String(bytes);

            using var output = new MemoryStream();
            var base64Stream = Base64Stream.CreateForEncoding(output);
            var offset = 0;
            while (offset < bytes.Length)
            {
                var bytesCount = Math.Min(bytesToWrite, bytes.Length - offset);
                var buff = bytes[offset..(offset + bytesCount)];
                base64Stream.Write(buff, 0, bytesCount);
                offset += bytesToWrite;
            }
            base64Stream.Flush();
            var streamResult = Encoding.ASCII.GetString(output.ToArray());

            Assert.That(streamResult, Is.EqualTo(regularResult));
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
        [TestCase(3 * 1024)]
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

            using var base64Stream = Base64Stream.CreateForDecoding(source);
            using var target = new MemoryStream();
            base64Stream.CopyTo(target, 1024);

            Assert.That(target.ToArray(), Is.EqualTo(bytes));
        }

        [Test]
        public void Decoding_UsingReadMethodWithOffsetAndLength_ProducesCorrectResult([Random(1024, 4096, 7)] int bytesToRead)
        {
            var bytes = new byte[64 * 1024 + 1];
            Random.Shared.NextBytes(bytes);
            var regularResult = Convert.ToBase64String(bytes);
            var base64 = Convert.ToBase64String(bytes);
            using var source = new MemoryStream();
            using (var writer = new StreamWriter(source, leaveOpen: true))
            {
                writer.Write(base64);
            }
            source.Position = 0;

            var decoded = new List<byte>();
            using var base64Stream = Base64Stream.CreateForDecoding(source);
            var buffer = new byte[bytesToRead];
            int read;
            while ((read = base64Stream.Read(buffer, 0, bytesToRead)) > 0)
            {
                decoded.AddRange(buffer.Take(read));
            }

            Assert.That(decoded, Is.EqualTo(bytes));
        }
    }
}
