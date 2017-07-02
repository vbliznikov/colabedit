
namespace CollabEdit.Services
{
    public interface IPathMapService
    {
        string PhysicalRoot { get; }
        string VirtualRoot { get; }
        string ToLocalPath(string virtualPath);
        string ToVirtulPath(string loalPath);
    }
}