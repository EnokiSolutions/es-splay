using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Es.Splay.Test
{
    // Example of using Intrusive, and usefull for testing Intrusive using the same tests as the <T> style tree.
    [ExcludeFromCodeCoverage]
    internal sealed class SplayTreeViaIntrusive<T> : ISplayTree<T> where T:IComparable<T>
    {
        private readonly SplayTreeIntrusive<Node, T> _t = new SplayTreeIntrusive<Node, T>();

        internal sealed class Node : SplayTreeIntrusive<Node, T>.Node
        {
            internal Node(T value)
            {
                Value = value;
            }
            protected internal override T Value { get; }
            protected internal override SplayTreeIntrusive<Node, T>.Node Copy()
            {
                return new Node(Value);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _t.Select(x=>x.Value).GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _t.Select(x => x.Value).GetEnumerator();
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

        public int Count
        {
            get { return _t.Count; }
            set { _t.Count = value; }
        }

        bool ICollection<T>.IsReadOnly => false;

        internal Node Root
        {
            get { return (Node) _t.Root; }
            set { _t.Root = value; }
        }

        internal void Traverse(Node node, Func<Node, bool> func)
        {
            _t.Traverse(node, n=>func((Node)n));
        }

        public override string ToString()
        {
            return _t.ToString();
        }
    }
}