using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomMap.DS
{
    public class MinHeap<T>
    {
        public int HeapSize { get; }
        public int Count { get; private set; }
        HeapNode<T>[] heap;

        #region Search Methods
        public MinHeap(int heapSize)
        {
            HeapSize = heapSize;
            Count = 0;
            heap = new HeapNode<T>[HeapSize + 1]; // 0번 인덱스는 사용하지 않으므로 1을 추가해 할당
        }

        public HeapNode<T> Find(int index)
        {
            return heap[index];
        }

        public HeapNode<T> FindParent(int index)
        {
            return heap[index / 2];
        }

        public HeapNode<T> FindLeftChild(int index)
        {
            return heap[index * 2];
        }

        public HeapNode<T> FindRightChild(int index)
        {
            return heap[index * 2 + 1];
        }
        #endregion

        #region Manipulation Methods
        public void Clear()
        {
            Count = 0;
            System.Array.Clear(heap, 0, heap.Length);
        }

        public void Insert(float key, T data)
        {
            int index = ++Count;
            
            // 트리의 마지막 다음 노드(count + 1)에서부터 부모와 Key를 비교하며 새 노드를 삽입할 위치 탐색
            while (index != 1 && heap[index / 2].Key > key)
            {
                heap[index] = FindParent(index);
                index /= 2;
            }

            // 탐색된 위치에 새 노드 삽입
            heap[index] = new HeapNode<T>(key, data);
        }

        public T Pop()
        {
            if (Count == 0)
                return default;

            int index = 1;
            HeapNode<T> root = heap[index];
            HeapNode<T> last = heap[Count--];

            while (index <= Count)
            {
                // 자식 노드가 없는 경우
                if (index * 2 > Count)
                {
                    break;
                }
                // 자식 노드가 하나인 경우
                else if (index * 2 == Count)
                {
                    HeapNode<T> left = FindLeftChild(index);
                    if (last.Key > left.Key)
                    {
                        heap[index] = left;
                        index *= 2;
                    }
                    else
                    {
                        break;
                    }
                }
                // 자식 노드가 둘인 경우
                else
                {
                    HeapNode<T> left = FindLeftChild(index);
                    HeapNode<T> right = FindRightChild(index);

                    if (last.Key > left.Key || last.Key > right.Key)
                    {
                        bool isLeft = left.Key <= right.Key;
                        heap[index] = isLeft ? left : right;
                        index = index * 2 + (isLeft ? 0 : 1);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            heap[index] = last;

            return root.Data;
        }
        #endregion

        #region Util Methods
        public bool isEmpty()
        {
            return Count == 0;
        }

        public bool isFull()
        {
            return Count == HeapSize - 1;
        }

        public void Print()
        {
            int level = 1;
            for (int i = 1; i <= Count; ++i)
            {
                if (i == level)
                {
                    Debug.Log("");
                    level *= 2;
                }

                Debug.Log($"{heap[i].Data} ({heap[i].Key})");
            }
        }
        #endregion
    }
}
