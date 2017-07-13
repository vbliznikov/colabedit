namespace VersionControl
{
    public interface IObjectStorage<T>
    {
        int Add(T value);
        void Remove(int hash);
        T Get(int hash);
        bool Contains(int hash);
        int Count { get; }
    }
}
