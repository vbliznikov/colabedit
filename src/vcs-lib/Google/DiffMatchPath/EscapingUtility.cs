namespace DiffMatchPatch
{
    public static class EscapingUtility
    {
        /// <summary>
        ///  Unescape selected chars for compatability with JavaScript's encodeURI.
        /// In speed critical applications this could be dropped since the
        /// receiving application will certainly decode these fine.
        /// </summary>
        /// <remarks>
        /// Note that this function is case-sensitive.  Thus "%3F" would not be
        /// unescaped.  But this is ok because it is only called with the output of
        /// Web​Utility.UrlEncode which returns lowercase hex.
        /// </remarks>
        /// <example>"%3f" -> "?", "%24" -> "$", etc.</example>
        /// <param name="str">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string UnescapeForEncodeUsiCcompatability(string str) {
            return str.Replace("%21", "!").Replace("%7e", "~")
                .Replace("%27", "'").Replace("%28", "(").Replace("%29", ")")
                .Replace("%3b", ";").Replace("%2f", "/").Replace("%3f", "?")
                .Replace("%3a", ":").Replace("%40", "@").Replace("%26", "&")
                .Replace("%3d", "=").Replace("%2b", "+").Replace("%24", "$")
                .Replace("%2c", ",").Replace("%23", "#");
        }
    }
}