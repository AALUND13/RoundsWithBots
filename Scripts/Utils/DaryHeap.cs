using System;

namespace RoundsWithBots.Utils {
    public class DaryHeap<T> where T : IHeapItem<T> {
        const float GROWTH_FACTOR = 2; // 2x growth factor, 4, 8, 16, 32, 64, 128, etc.

        private T[] items;
        private readonly int d;
        private int currentItemCount;

        public int Capacity => items.Length;
        public int Count => currentItemCount;

        public DaryHeap(int d, int initialCapacity = 4) {
            if(d < 2) throw new ArgumentOutOfRangeException(nameof(d), "Arity must be at least 2");
            this.d = d;
            items = new T[initialCapacity];
            currentItemCount = 0;
        }

        public void Clear() {
            for(int i = 0; i < currentItemCount; i++) {
                items[i].HeapIndex = -1;
                items[i] = default(T);
            }
            currentItemCount = 0;
        }

        public void Add(T item) {
            if(item == null) throw new ArgumentNullException(nameof(item));
            if(item.HeapIndex >= 0 && item.HeapIndex < currentItemCount && Contains(item)) {
                UpdateItem(item);
                return;
            }
            EnsureCapacity();
            item.HeapIndex = currentItemCount;
            items[currentItemCount] = item;
            SortUp(item);
            currentItemCount++;
        }

        public T RemoveFirst() {
            if (currentItemCount == 0)
                throw new InvalidOperationException("Heap is empty");

            T firstItem = items[0];

            currentItemCount--;
            if (currentItemCount > 0) {
                items[0] = items[currentItemCount];
                items[0].HeapIndex = 0;
                SortDown(items[0]);
            }

            firstItem.HeapIndex = -1;
            return firstItem;
        }

        public void UpdateItem(T item) {
            SortUp(item);
            SortDown(item);
        }

        public bool Contains(T item) {
            int idx = item.HeapIndex;
            return idx >= 0 && idx < currentItemCount && ReferenceEquals(items[idx], item);
        }

        private void EnsureCapacity() {
            if(currentItemCount < items.Length) return;
            int newCapacity = (int)(items.Length * GROWTH_FACTOR);
            if(newCapacity < 4) newCapacity = 4;
            Array.Resize(ref items, newCapacity);
        }

        private void SortDown(T item) {
            int index = item.HeapIndex;
            while(true) {
                int firstChild = d * index + 1;
                if(firstChild >= currentItemCount) break;
                int bestChild = firstChild;
                for(int i = 1; i < d; i++) {
                    int childIndex = firstChild + i;
                    if(childIndex < currentItemCount && items[childIndex].CompareTo(items[bestChild]) < 0) {
                        bestChild = childIndex;
                    }
                }
                if(items[bestChild].CompareTo(item) < 0) {
                    Swap(item, items[bestChild]);
                    index = bestChild;
                } else break;
            }
        }

        private void SortUp(T item) {
            int index = item.HeapIndex;
            int parentIndex = (index - 1) / d;
            while(index > 0 && item.CompareTo(items[parentIndex]) < 0) {
                Swap(item, items[parentIndex]);
                index = parentIndex;
                parentIndex = (index - 1) / d;
            }
        }

        private void Swap(T a, T b) {
            items[a.HeapIndex] = b;
            items[b.HeapIndex] = a;
            int tempIndex = a.HeapIndex;
            a.HeapIndex = b.HeapIndex;
            b.HeapIndex = tempIndex;
        }

    }

    public interface IHeapItem<T> : IComparable<T> {
        int HeapIndex { get; set; }
    }
}
