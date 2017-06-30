using System;
using System.IO;
using System.Collections.Generic;

namespace CollabEdit.IO
{
    public static class FilePath
    {
        public static string Combine(string path1, string path2)
        {

            if (path2.StartsWith("./"))
                return Path.Combine(path1, path2.Substring(2));

            if (path2.IndexOf("..") >= 0)
            {
                var parts2 = new List<string>(path2.Split(Path.DirectorySeparatorChar));
                var resultParts = new List<string>(path1.Split(Path.DirectorySeparatorChar));

                for (int i = 0; i < parts2.Count; i++)
                {
                    var segment = parts2[i];
                    if (segment != "..")
                        resultParts.Add(segment);
                    else
                    {
                        if (resultParts.Count > 1)
                            resultParts.RemoveAt(resultParts.Count - 1);
                        else
                            throw new ArgumentException("Path argument contains too many backtrace commands, resulting path can't be calculated", "path2");
                    }
                }

                return string.Join(Path.DirectorySeparatorChar.ToString(), resultParts);
            }

            return Path.Combine(path1, path2);
        }
    }
}