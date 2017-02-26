using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Tiesmaster.Dcc
{
    public class Startup
    {
        public static void Main()
        {
            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseKestrel()
                .Build();

            host.Run();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            app.RunDcc(new DccOptions { Host = "localhost", Port = "3000"});
        }
    }
}