using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Tiesmaster.Dcc
{
    public static class DccExtension
    {
        /// <summary>
        /// Sends request to remote server as specified in options
        /// </summary>
        /// <param name="app"></param>
        public static void RunProxy(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseMiddleware<DccMiddleware>();
        }

        /// <summary>
        /// Sends request to remote server as specified in options
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options">Options for setting port, host, and scheme</param>
        public static void RunProxy(this IApplicationBuilder app, Tiesmaster.Dcc.DccOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            app.UseMiddleware<DccMiddleware>(Options.Create(options));
        }
    }
}
