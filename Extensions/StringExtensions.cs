using System.Net;

namespace Beursspel.Extensions
{
    public static class StringExtensions
    {
        public static string ClearHtml(this string s)
        {
            return WebUtility.HtmlEncode(s);
        }
    }
}