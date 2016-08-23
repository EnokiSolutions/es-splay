using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Es.Splay
{
    // Example of using Intrusive, and usefull for testing Intrusive using the same tests as the <T> style tree.
    public sealed class SplayTreeViaIntrusive<T> : ISplayTreeIntrusive<SplayTreeViaIntrusive<T>.Node, T>, ISplayTree<T> where T:IComparable<T>
    {
        private readonly ISplayTreeIntrusive<Node, T> _t = new SplayTreeIntrusive<Node, T>();

        private sealed class Node : SplayTreeIntrusive<Node, T>.Node
        {
            internal Node(T value)
            {
                Value = value;
            }
            protected internal override T Value { get; set; }
            protected internal override SplayTreeIntrusive<Node, T>.Node Copy()
            {
                return new Node(Value);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _t.Select(x=>x.Value).GetEnumerator();
        }

        IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
        {
            return _t.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _t.Select(x => x.Value).GetEnumerator();
        }

        void ISplayTreeIntrusive<Node, T>.Add(Node n)
        {
            _t.Add(n);
        }

        void ISplayTreeIntrusive<Node, T>.Remove(Node n)
        {
            _t.Remove(n);
        }

        Node ISplayTreeIntrusive<Node, T>.Find(T v)
        {
            return _t.Find(v);
        }

        Node ISplayTreeIntrusive<Node, T>.FindNear(T v)
        {
            return _t.FindNear(v);
        }

        IEnumerable<Node> ISplayTreeIntrusive<Node, T>.NearBy(Node n, int before, int after)
        {
            return _t.NearBy(n, before, after);
        }

        IEnumerable<T> ISplayTree<T>.NearBy(T data, int before, int after)
        {
            var findNear = _t.FindNear(data);

            if (findNear==null)
                return Enumerable.Empty<T>();

            return _t.NearBy(findNear, before, after).Select(x => x.Value);
        }

        T ISplayTree<T>.Best()
        {
            var best = _t.Best();
            if (best==null)
                throw new InvalidOperationException("Tree is empty");
            return best.Value;
        }

        T ISplayTree<T>.Worst()
        {
            var worst = _t.Worst();
            if (worst == null)
                throw new InvalidOperationException("Tree is empty");
            return worst.Value;
        }

        void ISplayTree<T>.ForwardOrderTraverse(T data, Func<T, bool> f)
        {
            var findNear = _t.FindNear(data);
            if (findNear == null)
                return;
            _t.ForwardOrderTraverse(findNear,n=>f(n.Value));
        }

        void ISplayTree<T>.ReverseOrderTraverse(T data, Func<T, bool> f)
        {
            var findNear = _t.FindNear(data);
            if (findNear == null)
                return;
            _t.ReverseOrderTraverse(findNear, n => f(n.Value));
        }

        bool ISplayTree<T>.TryGetRank(T data, out int rank)
        {
            rank = 0;
            var found = _t.Find(data);
            if (found == null)
                return false;
            rank = _t.Rank(found);
            return true;
        }

        int ISplayTree<T>.Balance()
        {
            return _t.Balance();
        }

        int ISplayTree<T>.Prune(int newDesiredCount, Func<T, bool> locked)
        {
            return locked!=null ? _t.Prune(newDesiredCount, n => locked(n.Value)) : _t.Prune(newDesiredCount);
        }

        public void Validate()
        {
            _t.Validate();
        }

        Node ISplayTreeIntrusive<Node, T>.Best()
        {
            return _t.Best();
        }

        Node ISplayTreeIntrusive<Node, T>.Worst()
        {
            return _t.Worst();
        }

        void ISplayTreeIntrusive<Node, T>.ForwardOrderTraverse(Node n, Func<Node, bool> f)
        {
            _t.ForwardOrderTraverse(n, f);
        }

        void ISplayTreeIntrusive<Node, T>.ReverseOrderTraverse(Node n, Func<Node, bool> f)
        {
            _t.ReverseOrderTraverse(n, f);
        }

        int ISplayTreeIntrusive<Node, T>.Rank(Node n)
        {
            return _t.Rank(n);
        }

        int ISplayTreeIntrusive<Node, T>.Balance()
        {
            return _t.Balance();
        }

        int ISplayTreeIntrusive<Node, T>.Prune(int newDesiredCount, Func<Node, bool> locked)
        {
            return _t.Prune(newDesiredCount, locked);
        }

        void ICollection<T>.Add(T item)
        {
            _t.Add(new Node(item));
        }

        void ICollection<T>.Clear()
        {
            _t.Clear();
        }

        bool ICollection<T>.Contains(T item)
        {
            return _t.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            _t.CopyTo(array,arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            var n = _t.Find(item);
            if (n == null)
                return false;
            _t.Remove(n);
            return true;
        }

        int ICollection<T>.Count => _t.Count;

        bool ICollection<T>.IsReadOnly => false;

        void ISplayTreeIntrusive<Node, T>.Clear()
        {
            _t.Clear();
        }

        int ISplayTreeIntrusive<Node, T>.Count => _t.Count;

        bool ISplayTreeIntrusive<Node, T>.Contains(T item)
        {
            return _t.Contains(item);
        }

        void ISplayTreeIntrusive<Node, T>.CopyTo(T[] array, int arrayIndex)
        {
            _t.CopyTo(array, arrayIndex);
        }

        void ISplayTreeIntrusive<Node, T>.CopyTo(Node[] array, int arrayIndex)
        {
            _t.CopyTo(array, arrayIndex);
        }
    }
}