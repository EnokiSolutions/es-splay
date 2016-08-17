using System;
using System.Collections.Generic;

namespace Es.Splay
{
    public interface ISplayTree<T> : ICollection<T> where T : IComparable<T>
    {
        IEnumerable<T> NearBy(T data, int before, int after);
        T Best();
        T Worst();
        void ForwardOrderTraverse(T data, Func<T, bool> f);
        void ReverseOrderTraverse(T data, Func<T, bool> f);
        bool TryGetRank(T data, out int rank);
        int Balance();
        int Prune(int newDesiredCount, Func<T, bool> locked = null);
    }
}