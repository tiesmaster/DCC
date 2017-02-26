using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

// TODO: consider moving this to Tiesmaster.Dcc.Builder

namespace Tiesmaster.Dcc
{
    public static class DccExtension
    {
        /// <summary>
        /// Run DCC record and playback reverse proxy server.
        /// </summary>
        /// <param name="app"></param>
        public static void RunDcc(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseMiddleware<DccMiddleware>();
        }

        /// <summary>
        /// Run DCC record and playback reverse proxy server.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options">Options for overriding the defaults of DCC</param>
        public static void RunDcc(this IApplicationBuilder app, DccOptions options)
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