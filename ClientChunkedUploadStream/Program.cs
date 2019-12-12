using System;
using System.IO;
using System.Net.Http;

namespace ClientChunkedUploadStream
{
    class Program
    {
        static void Main(string[] args)
        {
            const int bufferSize = 512;

            var handler = new HttpClientHandler();
            handler.MaxRequestContentBufferSize = 0;
            handler.ServerCertificateCustomValidationCallback += (m, c, ch, s) => true;

            var client = new HttpClient();
            client.DefaultRequestHeaders.TransferEncodingChunked = true;

            var request = new HttpRequestMessage(HttpMethod.Post, args[0]);

            if (File.Exists(args[1]))
            {
                var fileStream = File.Open(args[1], FileMode.Open, FileAccess.Read);
                request.Content = new StreamContent(fileStream, bufferSize);
                request.Headers.TransferEncodingChunked = true;

                Console.WriteLine($"Start upload: {DateTime.Now}");
                
                client.SendAsync(request)
                    .GetAwaiter()
                    .GetResult();
                
                Console.WriteLine($"File uploaded: {DateTime.Now}");
            }
        }
    }
}
