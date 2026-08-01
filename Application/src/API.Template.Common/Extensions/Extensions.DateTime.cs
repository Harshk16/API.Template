using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Template.Common.Extensions
{
    public static partial class Extension
    {
        public static DateTime? ToDateTime(this string str, string format)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            return DateTime.ParseExact(str, format, CultureInfo.InvariantCulture);
        }   
    }
}
