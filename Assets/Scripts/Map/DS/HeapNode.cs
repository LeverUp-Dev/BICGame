namespace Hypocrites.Map.DS
{
    public class HeapNode<T>
    {
        public float Key { get; set; }
        public T Data { get; set; }

        public HeapNode(float key, T data)
        {
            Key = key;
            Data = data;
        }
    }
}
