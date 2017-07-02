using System;
using System.IO;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace CollabEdit.Services
{
    public class PathMapService : IPathMapService
    {
        public const string HomePathDefault = "home";
        public string PhysicalRoot { get; }
        public string VirtualRoot { get; }

        public PathMapService(IOptions<ExplorerOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            var config = options.Value ?? new ExplorerOptions();

            if (string.IsNullOrEmpty(config.EditorRoot))
                throw new ArgumentException("EditorRoot configuration value is incorrect. " +
                "Should be set to full or relative path where editor content will recide.", nameof(options));
            if (string.IsNullOrEmpty(config.VirtualRoot))
                throw new ArgumentException("VirtualRoot configuration value is incorrect.", nameof(options));

            VirtualRoot = config.VirtualRoot;
            PhysicalRoot = Path.IsPathRooted(config.EditorRoot)
                ? config.EditorRoot
                : Path.GetFullPath(config.EditorRoot);

            if (config.CreateIfNotExists && !Directory.Exists(PhysicalRoot))
                Directory.CreateDirectory(PhysicalRoot);

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