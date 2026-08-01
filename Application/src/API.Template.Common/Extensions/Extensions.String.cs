using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Common.Extensions
{
    public static partial class Extensions
    {
        public static string Capitalize(this string str)
        {
            return str.First().ToString().ToUpper() + str.Substring(1);
        }
    }
}
