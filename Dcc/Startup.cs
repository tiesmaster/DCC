using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Tiesmaster.Dcc
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if(env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.RunDcc(new DccOptions { Host = "localhost", Port = "3000"});
        }
    }
}