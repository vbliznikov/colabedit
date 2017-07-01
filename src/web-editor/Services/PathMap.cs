using System;
using System.IO;
using System.Collections.Generic;

namespace CollabEdit.Controllers
{
    public class PathMap
    {
        public const string HomePathDefault = "home";
        public string PhysicalRoot { get; }
        public string VirtualRoot { get; }

        public PathMap(string physicalRoot) : this(physicalRoot, null)
        {
        }

        public PathMap(string physicalRoot, string virtualRoot)
        {
            if (string.IsNullOrEmpty(physicalRoot)) throw new ArgumentException("Argument may not be null or empty string", "physicalRoot");

            PhysicalRoot = Path.IsPathRooted(physicalRoot) ? physicalRoot : Path.GetFullPath(physicalRoot);
            if (!string.IsNullOrEmpty(virtualRoot))
                VirtualRoot = virtualRoot;
            else
                VirtualRoot = HomePathDefault;
        }

        public string ToLocalPath(string virtualPath)
        {
            var parts = new List<string>(virtualPath.Split('/', '\\'));
            if (parts[0].Equals(VirtualRoot, StringComparison.OrdinalIgnoreCase))
            {
                if (parts.Count == 1) return PhysicalRoot;

                parts.RemoveAt(0);
                return Path.Combine(PhysicalRoot, string.Join(Path.DirectorySeparatorChar.ToString(), parts));
            }
            else
                return Path.Combine(PhysicalRoot, virtualPath);
        }

        public string ToVirtulPath(string loalPath)
        {
            return Path.Combine(VirtualRoot, loalPath.Replace(PhysicalRoot, "")
                .TrimStart(Path.DirectorySeparatorChar));
        }
    }
}