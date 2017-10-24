using System.Collections.Generic;

namespace Beursspel.Utilities
{
    public static class ListUtils
    {
        public static string Join(this List<string> ls)
        {
            return string.Join(", ", ls);
        }
    }
}