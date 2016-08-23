using System;
using System.Collections.Generic;

namespace Es.Splay
{
    public interface ISplayTreeIntrusive<TN, in TV> : IEnumerable<TN> where TV : IComparable<TV>
    {
        void Add(TN n);
        void Remove(TN n);
        TN Find(TV v);
        TN FindNear(TV v);
        IEnumerable<TN> NearBy(TN n, int before, int after);
        TN Best();
        TN Worst();
        void ForwardOrderTraverse(TN n, Func<TN, bool> f);
        void ReverseOrderTraverse(TN n, Func<TN, bool> f);
        int Rank(TN n);
        int Balance();
        int Prune(int newDesiredCount, Func<TN, bool> locked = null);
        void Clear();
        int Count { get; }
        bool Contains(TV item);
        void CopyTo(TV[] array, int arrayIndex);
        void CopyTo(TN[] array, int arrayIndex);
        void Validate(); // for debugging
    }
}