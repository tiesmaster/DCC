using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Tiesmaster.Dcc
{
    internal class Helpers
    {
        internal static bool CanRequestContainBody(string requestMethod)
        {
            return !HttpMethods.IsGet(requestMethod) &&
                   !HttpMethods.IsHead(requestMethod) &&
                   !HttpMethods.IsDelete(requestMethod) &&
                   !HttpMethods.IsTrace(requestMethod);
        }
    }
}