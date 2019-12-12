using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ServerChunkedUploadStream
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            
            app.Run(async (context) => {
                var cancellation = context.RequestAborted;

                MemoryStream output = new MemoryStream();

                int bufferSize = 512; // buffer 512 bytes
                
                // Rent a shared buffer to write the request body into.
                byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

                Console.WriteLine($"Start read: {DateTime.Now}");
                while (true)
                {
                    var bytesRemaining = await context.Request.Body.ReadAsync(buffer, offset: 0, buffer.Length);
                    if (bytesRemaining == 0)
                    {
                        break;
                    }
                    Console.WriteLine($"Body: {bytesRemaining} readed.");

                    await output.WriteAsync(buffer, 0, bytesRemaining);
                }

                await output.FlushAsync(cancellation);

                Console.WriteLine($"End read: {DateTime.Now}");

                ArrayPool<byte>.Shared.Return(buffer);

                Console.WriteLine($"Done. Size of output: {output.Length}");

                output.Dispose();

                Console.WriteLine("Request Finished");
            });
        }
    }
}
