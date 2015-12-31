using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace url2wiz.NET
{
    public static class DateTimeExtension
    {
        private static readonly long UtcStartInJS = 621355968000000000L;//UTC start in JS  new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        public static long ToUtcTicksLikeJS(this DateTime dt)
        {
            if(dt.Kind != DateTimeKind.Utc)
            {
                return (dt.ToUniversalTime().Ticks - UtcStartInJS) / 10000;
            }
            return (dt.Ticks - UtcStartInJS) / 10000; ;
        }
    }
}
