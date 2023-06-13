using System;

namespace Hypocrites.DB.Save
{
    [Serializable]
    public class JsonSave<T>
    {
        public T[] items;
    }
}
