using System.Collections.Generic;
using UnityEngine;

namespace Wandering.Utils;

public static class PriorityQueues {
    public class HotMinHeap<TValue> {
        public MinHeap<TValue> hotHeap = new MinHeap<TValue>();
        public MinHeap<TValue> coldHeap = new MinHeap<TValue>();
        public int Count;

        private int hotThreshold = int.MinValue;
        private int coldThreshold = int.MinValue;

        public void Enqueue(int priority, TValue value) {
            if (priority < hotThreshold) {
                hotHeap.Enqueue(priority, value);
            } else {
                coldHeap.Enqueue(priority, value);
                coldThreshold = Mathf.Max(priority, coldThreshold);
            }

            Count++;
        }

        public KeyValuePair<int, TValue> Dequeue() {
            if (hotHeap.Count <= 0) {
                (hotHeap, coldHeap) = (coldHeap, hotHeap);
                hotThreshold = coldThreshold;
                coldThreshold = int.MinValue;
            }

            Count--;
            return hotHeap.Dequeue();
        }

        public void Clear() {
            Count = 0;
            hotHeap.Clear();
            hotThreshold = int.MinValue;
            coldHeap.Clear();
            coldThreshold = int.MinValue;

        }
    }

    public class MinHeap<TValue> {
        private List<KeyValuePair<int, TValue>> heap = new();
        public int Count => heap.Count;

        public void Enqueue(int priority, TValue value) {
            heap.Add(new KeyValuePair<int, TValue>(priority, value));
            HeapifyFromEndToBeginning();
        }

        private void HeapifyFromEndToBeginning() {
            int parentIndex;
            for (int currentIndex = heap.Count - 1; currentIndex > 0; currentIndex = parentIndex) {
                parentIndex = (currentIndex - 1) / 2;
                int parentPriority = heap[parentIndex].Key;
                int currentPriority = heap[currentIndex].Key;
                if (parentPriority > currentPriority) {
                    ExchangeElements(parentIndex, currentIndex);
                } else {
                    break;
                }
            }
        }

        public KeyValuePair<int, TValue> Dequeue() {
            KeyValuePair<int, TValue> item = heap[0];
            DeleteRoot();
            HeapifyFromBeginningToEnd();
            return item;
        }

        private void DeleteRoot() {
            if (heap.Count <= 1) {
                heap.Clear();
            } else {
                heap[0] = heap[^1];
                heap.RemoveAt(heap.Count - 1);
            }
        }

        private void HeapifyFromBeginningToEnd() {
            int currentIndex = 0;
            while (true) {
                int smallestIndex = currentIndex;
                int leftIndex = smallestIndex * 2 + 1;
                int rightIndex = smallestIndex * 2 + 2;

                if (leftIndex < heap.Count && heap[leftIndex].Key < heap[smallestIndex].Key) {
                    smallestIndex = leftIndex;
                }

                if (rightIndex < heap.Count && heap[rightIndex].Key < heap[smallestIndex].Key) {
                    smallestIndex = rightIndex;
                }

                if (smallestIndex != currentIndex) {
                    ExchangeElements(smallestIndex, currentIndex);
                    currentIndex = smallestIndex;
                } else {
                    break;
                }
            }
        }

        private void ExchangeElements(int pairOne, int pairTwo) {
            (heap[pairOne], heap[pairTwo]) = (heap[pairTwo], heap[pairOne]);
        }

        public void Clear() {
            heap.Clear();
        }
    }
}
