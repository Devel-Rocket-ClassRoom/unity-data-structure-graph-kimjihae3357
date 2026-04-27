using System;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
    private List<(TElement Element, TPriority Priority)> _heap;
    private readonly IComparer<TPriority> _comparer;

    public int Count => _heap.Count;
    public List<(TElement Element, TPriority Priority)> Elements => _heap;

    public PriorityQueue() : this(Comparer<TPriority>.Default) => _heap = new();
    public PriorityQueue(IComparer<TPriority> comparer) => _comparer = comparer;

    public void Enqueue(TElement element, TPriority priority)
    {
        // 1. 맨 뒤에 추가
        _heap.Add((element, priority));

        // 2. 부모와 비교
        int currIdx = Count - 1;
        while (currIdx > 0)
        {
            var parentIdx = (currIdx - 1) / 2;
            var parentE = _heap[parentIdx];

            // 3. 비교해서 작을 시 스왑
            var compare = (_heap[currIdx].Priority).CompareTo(parentE.Priority);
            if (compare < 0)
            {
                _heap[parentIdx] = _heap[currIdx];
                _heap[currIdx] = parentE;
                currIdx = parentIdx;
            }
            else
            {
                break;
            }
        } // 4. 큰놈 나올 때 까지 반복
    }
    public TElement Dequeue()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        // 1. 루트(배열 [0]) 값을 결과로 보관.
        var result = _heap[0].Element;

        // 2. 배열의 마지막 원소를 루트로 이동.
        int currIdx = 0;
        _heap[currIdx] = _heap[Count - 1];
        _heap.RemoveAt(Count - 1);

        // 3. 자식 중 더 작은 쪽과 비교하며 아래로 내려간다 (swap).
        while (currIdx * 2 + 1 < Count)
        {
            int leftChildIdx = currIdx * 2 + 1;
            int rightChildIdx = currIdx * 2 + 2;

            var targetIdx = leftChildIdx;
            if (rightChildIdx < Count)
            {
                var childCompare = _heap[targetIdx].Priority.CompareTo(_heap[rightChildIdx].Priority);
                if (childCompare > 0)
                {
                    targetIdx = rightChildIdx;
                }
            }

            var target = _heap[targetIdx];
            var compare = _heap[currIdx].Priority.CompareTo(target.Priority);
            if (compare > 0)
            {
                _heap[targetIdx] = _heap[currIdx];
                _heap[currIdx] = target;
                currIdx = targetIdx;
            }
            else
            {
                break;
            }
        }

        return result;
    }

    public TElement Peek() => _heap[0].Element;

    public void Clear() => _heap.Clear();

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (_comparer.Compare(_heap[index].Priority, _heap[parent].Priority) >= 0)
                break;
            Swap(index, parent);
            index = parent;
        }
    }

    private void HeapifyDown(int index)
    {
        int count = _heap.Count;
        while (true)
        {
            int smallest = index;
            int left = 2 * index + 1;
            int right = 2 * index + 2;

            if (left < count && _comparer.Compare(_heap[left].Priority, _heap[smallest].Priority) < 0)
                smallest = left;
            if (right < count && _comparer.Compare(_heap[right].Priority, _heap[smallest].Priority) < 0)
                smallest = right;

            if (smallest == index)
                break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int a, int b)
    {
        var temp = _heap[a];
        _heap[a] = _heap[b];
        _heap[b] = temp;
    }
}