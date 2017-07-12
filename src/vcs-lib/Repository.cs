using System;

namespace VersionControl
{
    public class Commit<T>
    {
        public Commit<T> Parent {get; internal set;}

        public T Value {get; internal set;}
    }
}
