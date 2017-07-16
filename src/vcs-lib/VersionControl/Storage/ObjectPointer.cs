namespace CollabEdit.VersionControl.Storage
{
    public class ObjectPointer<T>
    {
        public ObjectPointer(int pointer, IObjectStorage<T> storage)
        {
            Pointer = pointer;
            Storage = storage;
        }
        protected IObjectStorage<T> Storage { get; set; }
        public int Pointer { get; }
        public bool HasValue => Storage.Contains(Pointer);
        public T Value => Storage.Get(Pointer);
    }
}
