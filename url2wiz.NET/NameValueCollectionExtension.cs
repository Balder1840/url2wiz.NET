using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace url2wiz.NET
{
    public static class NameValueCollectionExtension
    {
        public static IEnumerable<string> GetAllValues(this NameValueCollection nv)
        {
            return nv.AllKeys.SelectMany(key => nv.GetValues(key));
        }
    }
}
