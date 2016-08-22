using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Es.Splay
{
    public sealed class SplayTreeIntrusive<TN,TV> : ISplayTreeIntrusive<TN,TV> 
        where TV : IComparable<TV>
        where TN : SplayTreeIntrusive<TN,TV>.Node,new()
    {
        private const int MaxRecursionDepth = 80;
        public Node Root { get; internal set; }

#if DEBUG
        internal Guid TreeGuid = Guid.NewGuid();
#endif

        public abstract class Node
        {
#if DEBUG
            internal Guid TreeGuid = Guid.Empty;
#endif
            internal Node Parent;
            internal Node Left;
            internal Node Right;

            internal int LeftCount;
            internal int RightCount;

            protected Node()
            {
                Left = null;
                LeftCount = 0;
                Right = null;
                RightCount = 0;
                Parent = null;
            }

            protected internal Node Copy()
            {
                return this;
            }

            protected internal TV Value { get; }
        }

        public int Count { get; internal set; }
        
        public void Clear()
        {
            Root = null;
            Count = 0;
        }

        public bool Contains(TV item)
        {
            var found = Find(item);
            if (found == null)
                return false;
            Splay(found);
            return true;
        }

        public void CopyTo(TV[] array, int arrayIndex)
        {
            Traverse(Root, n => {
                array[arrayIndex++] = n.Value;
                return true;
            });
        }
        public void CopyTo(TN[] array, int arrayIndex)
        {
            Traverse(Root, n => {
                array[arrayIndex++] = (TN)n.Copy();
                return true;
            });
        }

        [ExcludeFromCodeCoverage]
        [Conditional("DEBUG")]
        internal void Validate()
        {
            if (Root == null)
                return;

            var seen = new HashSet<TN>();
            Debug.Assert(1 + Root.LeftCount + Root.RightCount == Count);

            Traverse(Root, n =>
            {
                if (seen.Contains(n))
                    return false;

                seen.Add((TN)n);

                if (n.Parent != null)
                    if (n.Parent.Left == n)
                        Debug.Assert(n.Parent.LeftCount == 1 + n.LeftCount + n.RightCount);
                    else
                        Debug.Assert(n.Parent.RightCount == 1 + n.LeftCount + n.RightCount);

                if (n.Left != null)
                    Debug.Assert(n.LeftCount == 1 + n.Left.LeftCount + n.Left.RightCount);

                if (n.Right != null)
                    Debug.Assert(n.RightCount == 1 + n.Right.LeftCount + n.Right.RightCount);

                return true;
            });
        }

        private void Splay(Node x)
        {
            while (x.Parent != null)
            {
                var xIsALeftChild = x.Parent.Left == x;

                if (x.Parent.Parent == null)
                {
                    // no grandparent so we're one away from the root.
                    if (xIsALeftChild)
                        RightRotate(x.Parent); // we're left of root, rotating right will make us root
                    else
                        LeftRotate(x.Parent); // we're right of root, rotating left will make us root
                }
                else
                {
                    var parentIsALeftChild = x.Parent.Parent.Left == x.Parent;

                    if (xIsALeftChild)
                    {
                        if (parentIsALeftChild)
                        {
                            RightRotate(x.Parent.Parent);
                            RightRotate(x.Parent);
                        }
                        else // parentIsARightChild
                        {
                            RightRotate(x.Parent);
                            LeftRotate(x.Parent);
                        }
                    }
                    else // xIsARightChild
                    {
                        if (parentIsALeftChild)
                        {
                            LeftRotate(x.Parent);
                            RightRotate(x.Parent);
                        }
                        else // parentIsARightChild
                        {
                            LeftRotate(x.Parent.Parent);
                            LeftRotate(x.Parent);
                        }
                    }
                }
            }
        }

        public TN Find(TV n)
        {
            var u = Root;

            while (u != null)
            {
                var cmp = u.Value.CompareTo(n);
                if (cmp == 0)
                {
                    return (TN)u;
                }
                u = cmp < 0 ? u.Right : u.Left;
            }
            return null;
        }


        public TN FindNear(TV n)
        {
            var u = Root;
            var p = u;
            while (u != null)
            {
                var cmp = u.Value.CompareTo(n);
                if (cmp == 0)
                {
                    return (TN)u;
                }
                p = u;
                u = cmp < 0 ? u.Right : u.Left;
            }
            return (TN)p;
        }

        private void AddInternal(Node z)
        {
            Debug.Assert(z.Parent == null);
            Debug.Assert(z.Left == null);
            Debug.Assert(z.Right == null);
            Debug.Assert(z.LeftCount == 0);
            Debug.Assert(z.RightCount == 0);

            z.TreeGuid = TreeGuid;

            var u = Root;
            var p = u;
            while (u != null)
            {
                var cmp = u.Value.CompareTo(z.Value);
                p = u;
                if (cmp < 0)
                {
                    ++u.RightCount;
                    u = u.Right;
                }
                else
                {
                    ++u.LeftCount;
                    u = u.Left;
                }
            }

            z.Parent = p;
            if (p == null)
            {
                Root = z;
            }
            else if (p.Value.CompareTo(z.Value) < 0)
            {
                Debug.Assert(p.Right == null);
                p.Right = z;
                p.RightCount = 1;
            }
            else
            {
                Debug.Assert(p.Left == null);
                p.Left = z;
                p.LeftCount = 1;
            }
            ++Count;
            Splay(z);
        }

        public void Add(TN n)
        {
            Debug.Assert(n.TreeGuid == Guid.Empty);

            var z = Find(n.Value);
            if (z == null)
            {
                AddInternal(n);
            }
            else
            {
                throw new InvalidOperationException("Values must be unique");
            }
            if (Root != null)
            {
                Debug.Assert(Count == 1 + Root.LeftCount + Root.RightCount);
            }
        }

        public IEnumerable<TN> NearBy(TN n, int before, int after)
        {
            if (n == null)
            {
                return Enumerable.Empty<TN>();
            }
            Splay(n);

            return Near(before, after);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TN> Near(int before, int after)
        {
            var p = Root;
            var l = new List<TN>(before + after + 1) { (TN)p };

            if (p.Left != null)
            {
                var previous = p.Left;
                while (previous.Right != null)
                {
                    previous = previous.Right;
                }
                ReverseOrderTraverse(
                    (TN)previous,
                    n =>
                    {
                        if (before <= 0)
                            return false;

                        l.Add(n);
                        --before;
                        return true;
                    }
                    );
            }

            if (p.Right != null)
            {
                var next = p.Right;
                while (next.Left != null)
                {
                    next = next.Left;
                }
                ForwardOrderTraverse(
                    (TN)next,
                    n =>
                    {
                        if (after <= 0)
                            return false;

                        l.Add(n);
                        --after;
                        return true;
                    }
                    );
            }

            l.Sort();
            return l;
        }
        
        public TN Best()
        {
            return (TN)LeftMost();
        }

        private Node LeftMost()
        {
            var x = Root;
            while (x?.Left != null)
            {
                x = x.Left;
            }
            return x;
        }

        public TN Worst()
        {
            var x = Root;
            while (x?.Right != null)
            {
                x = x.Right;
            }
            return (TN)x;
        }

        public void Remove(TN z)
        {
            Splay(z);
#if DEBUG
            var totalWeight = 0;
            if (Root != null)
            {
                totalWeight = 1 + Root.LeftCount + Root.RightCount;
            }
#endif

            if (z.Left == null)
            {
                Root = z.Right; //Replace(z, z.Right);
            }
            else if (z.Right == null)
            {
                Root = z.Left; //Replace(z, z.Left);
            }
            else // both z.Left and z.Right are non null
            {
                // take z.Right's leftmost and bring it up to become the new root.
                var zR = z.Right;
                var zL = z.Left;

                var u = zR;
                while (u.Left != null)
                {
                    --u.LeftCount; // we are removing the left most, so updates the counts as we go.
                    u = u.Left;
                }

                var uP = u.Parent;
                if (uP != z)
                {
                    var uR = u.Right;
                    uP.LeftCount = u.RightCount;

                    uP.Left = uR;
                    if (uR != null)
                    {
                        uR.Parent = uP;
                    }

                    u.Right = zR;
                    zR.Parent = u;
                    u.RightCount = z.RightCount - 1; // remember, we're removing moveToRoot from the right side.
                }
                else
                {
                    Debug.Assert(u == zR);
                }

                Root = u;

                u.Left = zL;
                zL.Parent = u;
                u.LeftCount = z.LeftCount;
            }
            if (Root != null)
            {
                Root.Parent = null;
            }
            z.Parent = null;
            z.Left = null;
            z.Right = null;
            z.LeftCount = 0;
            z.RightCount = 0;

#if DEBUG
            z.TreeGuid = Guid.Empty;
            int totalWeightAfter;

            if (Root == null)
                totalWeightAfter = 0;
            else
                totalWeightAfter = 1 + Root.LeftCount + Root.RightCount;

            Debug.Assert(totalWeightAfter == totalWeight - 1);
#endif
            --Count;
        }

        private void LeftRotate(Node x)
        {
#if DEBUG
            var totalWeight =
                x.Parent == null 
                    ? 1 + Root.LeftCount + Root.RightCount
                    : x.Parent.Left == x 
                        ? x.Parent.LeftCount
                        : x.Parent.RightCount;
            Debug.Assert(totalWeight <= Count);
            Debug.Assert(x.Right != null);
#endif

            var right = x.Right;
            var parent = x.Parent;

            var rightLeft = right.Left;
            var rightLeftCount = right.LeftCount;
            x.Right = rightLeft;
            x.RightCount = rightLeftCount;

            if (rightLeft != null)
                rightLeft.Parent = x;

            right.Parent = parent;

            if (parent == null)
                Root = right;
            else if (x == parent.Left)
                parent.Left = right;
            else
                parent.Right = right;

            right.Left = x;
            right.LeftCount = 1 + x.LeftCount + x.RightCount;

            x.Parent = right;

#if DEBUG
            x = right;
            var totalWeightAfter = 
                x.Parent == null 
                    ? 1 + Root.LeftCount + Root.RightCount
                    : x.Parent.Left == x 
                        ? x.Parent.LeftCount
                        : x.Parent.RightCount;
            Debug.Assert(totalWeight == totalWeightAfter);
#endif

        }

        private void RightRotate(Node x)
        {
#if DEBUG
            var totalWeight = x.Parent == null ? 1 + Root.LeftCount + Root.RightCount
                : x.Parent.Left == x ? x.Parent.LeftCount
                    : x.Parent.RightCount;
            Debug.Assert(totalWeight <= Count);
            Debug.Assert(x.Left != null);
#endif

            var left = x.Left;
            var parent = x.Parent;

            var leftRight = left.Right;
            var leftRightCount = left.RightCount;
            x.Left = leftRight;
            x.LeftCount = leftRightCount;

            if (leftRight != null)
                leftRight.Parent = x;

            left.Parent = parent;

            if (parent == null)
                Root = left;
            else if (x == parent.Right)
                parent.Right = left;
            else
                parent.Left = left;

            left.Right = x;
            left.RightCount = 1 + x.RightCount + x.LeftCount;
            x.Parent = left;

#if DEBUG
            x = left;
            var totalWeightAfter = x.Parent == null ? 1 + Root.LeftCount + Root.RightCount
                : x.Parent.Left == x ? x.Parent.LeftCount
                    : x.Parent.RightCount;
            Debug.Assert(totalWeight == totalWeightAfter);
#endif
        }


        internal void Traverse(Node n, Func<Node, bool> f)
        {
            if (n == null)
                return;

            var ln = n.Parent;

            while (n != null)
            {
                if (ln == n.Parent)
                {
                    // we're descending

                    ln = n;
                    if (n.Left != null)
                    {
                        // we can go left, so go left
                        n = n.Left;
                        continue;
                    }

                    if (!f(n)) // going right or up means all values after this are greater.
                        return;

                    // we can't go left
                    if (n.Right != null) // but we can go right
                    {
                        n = n.Right;
                        continue;
                    }
                    // we have to go back up
                    n = n.Parent;
                    continue;
                }

                if (ln == n.Left) // we're ascending from the left side
                {
                    if (!f(n)) // all values after this are greater.
                        return;

                    ln = n;
                    if (n.Right != null) // we can go right
                    {
                        n = n.Right;
                        continue;
                    }
                    // we have to go back up
                    n = n.Parent;
                    continue;
                }

                if (ln == n.Right) // we're ascending from the right side
                {
                    ln = n;
                    n = n.Parent;
                    continue;
                }

                throw new Exception("Invalid Tree");
            }
        }

        public void ForwardOrderTraverse(TN n, Func<TN, bool> f)
        {
            // go forward from n
            var ln = n.Left ?? n.Parent;

            while (n != null)
            {
                if (ln == n.Parent)
                {
                    // we're descending

                    ln = n;
                    if (n.Left != null)
                    {
                        // we can go left, so go left
                        n = (TN)n.Left;
                        continue;
                    }

                    if (!f(n)) // going right or up means all values after this are greater.
                        return;

                    // we can't go left
                    if (n.Right != null) // but we can go right
                    {
                        n = (TN)n.Right;
                        continue;
                    }
                    // we have to go back up
                    n = (TN)n.Parent;
                    continue;
                }

                if (ln == n.Left) // we're ascending from the left side
                {
                    if (!f(n)) // all values after this are greater.
                        return;

                    ln = n;
                    if (n.Right != null) // we can go right
                    {
                        n = (TN)n.Right;
                        continue;
                    }
                    // we have to go back up
                    n = (TN)n.Parent;
                    continue;
                }

                if (ln == n.Right) // we're ascending from the right side
                {
                    ln = n;
                    n = (TN)n.Parent;
                    continue;
                }
                throw new Exception("Invalid Tree");
            }
        }

        public void ReverseOrderTraverse(TN n, Func<TN, bool> f)
        {
            // go reverse from n
            var ln = n.Right ?? n.Parent;

            while (n != null)
            {
                if (ln == n.Parent)
                {
                    // we're descending

                    ln = n;
                    if (n.Right != null)
                    {
                        // we can go right, so go right
                        n = (TN)n.Right;
                        continue;
                    }

                    if (!f(n)) // going Left or up means all values after this are lesser.
                        return;

                    // we can't go right
                    if (n.Left != null) // but we can go left
                    {
                        n = (TN)n.Left;
                        continue;
                    }

                    // we have to go back up
                    n = (TN)n.Parent;
                    continue;
                }

                if (ln == n.Right) // we're ascending from the right side
                {
                    if (!f(n)) // all values after this are lesser.
                        return;

                    ln = n;
                    if (n.Left != null) // we can go Left
                    {
                        n = (TN)n.Left;
                        continue;
                    }

                    // we have to go back up
                    n = (TN)n.Parent;
                    continue;
                }

                if (ln == n.Left) // we're ascending from the Left side
                {
                    ln = n;
                    n = (TN)n.Parent;
                    continue;
                }
                throw new Exception("Invalid Tree");
            }
        }

        public int Rank(TN n)
        {
            Splay(n);
            return n.LeftCount;
        }

        public IEnumerator<TN> GetEnumerator()
        {
            if (Root == null)
                return Enumerable.Empty<TN>().GetEnumerator();
            var n = LeftMost();
            Splay(n);

            return Near(0, Count).GetEnumerator();
        }

        [ExcludeFromCodeCoverage]
        // only used for debugging
        public override string ToString()
        {
            var sb = new StringBuilder();
            var ids = new Dictionary<Node, string>();
            var seen = new HashSet<Node>();

            var nullId = 0;
            var nodeId = 0;

            Func<Node, string> idfor = x => ids.Vivify(x, () => "o" + nodeId++);

            sb.Append("digraph t {");
            if (Root == null)
            {
                sb.AppendFormat("\"Root\" -> \"null\";");
            }
            else
            {
                sb.Append($"\"Root\" -> \"{idfor(Root)}\";");
            }
            try
            {
                Traverse(Root, n =>
                {
                    if (seen.Contains(n))
                    {
                        sb.Append($"\"CYCLE\" [label=\"{n.Value}\"];");
                        return false;
                    }
                    seen.Add(n);
                    var id = idfor(n);
                    Debug.Assert(n.Value != null);

                    sb.Append($"\"{id}\" [label=\"{n.Value}:{n.LeftCount},{n.RightCount}\"];");

                    if (n.Parent != null)
                    {
                        sb.Append($"\"{id}\"->\"{idfor(n.Parent)}\" [label=\"P\"];");
                    }

                    if (n.Left == null && n.Right == null)
                        return true;

                    if (n.Left == null)
                    {
                        sb.Append($"\"n{nullId}\" [label=\"null\"];");
                        sb.Append($"\"{id}\"->\"n{nullId}\" [label=\"L\"];");
                        ++nullId;
                    }
                    else
                    {
                        Debug.Assert(n.Left.Value != null);
                        sb.Append($"\"{id}\"->\"{idfor(n.Left)}\" [label=\"L\"];");
                    }

                    if (n.Right == null)
                    {
                        sb.Append($"\"n{nullId}\" [label=\"null\"];");
                        sb.Append($"\"{id}\"->\"n{nullId}\" [label=\"R\"];");
                        ++nullId;
                    }
                    else
                    {
                        Debug.Assert(n.Right.Value != null);
                        sb.Append($"\"{id}\"->\"{idfor(n.Right)}\" [label=\"R\"];");
                    }
                    return true;
                });
            }
            catch
            {
                sb.Append($"\"CYCLE\" [label=\"INVALID TREE\"];");
            }
            sb.Append("}");
            return sb.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Balance()
        {
            if (Count < 3)
                return 0;

            return Root == null ? 0 : Balance(Root, 1, MaxRecursionDepth);
        }

        public int Prune(int newDesiredCount, Func<TN, bool> locked = null)
        {
            if (Count == 0 || Count < newDesiredCount)
                return 0;

            Balance();
            return Prune(Root, Count - newDesiredCount, locked);
        }

        private int Prune(Node n, int toRemove, Func<TN, bool> locked)
        {
            Debug.Assert(n != null);
            Debug.Assert(toRemove > 0);

            var totalChildCount = n.LeftCount + n.RightCount;
            Debug.Assert(totalChildCount > 0);

            var removed = 0;

            // split remaining removal amount across left and right children

            int removeLeft;
            int removeRight;
            if (n.LeftCount <= n.RightCount)
            {
                removeLeft = (int)(((long)toRemove * n.LeftCount) / totalChildCount);
                removeRight = toRemove - removeLeft; // lump remainder to the right since it has more or the same (slight bias here)
            }
            else
            {
                removeRight = (int)(((long)toRemove * n.RightCount) / totalChildCount);
                removeLeft = toRemove - removeRight; // lump remainder to the left since it has more
            }

            Debug.Assert(removeRight + removeLeft == toRemove);

            if (removeLeft > 0 && n.LeftCount > 1)
            {
                var removedFromLeft = Prune(n.Left, removeLeft, locked);
                n.LeftCount -= removedFromLeft;
                Debug.Assert(n.LeftCount >= 0);
                removed += removedFromLeft;
                toRemove -= removedFromLeft;
            }

            if (removeRight > 0 && n.RightCount > 1)
            {
                var removedFromRight = Prune(n.Right, removeRight, locked);
                n.RightCount -= removedFromRight;
                Debug.Assert(n.RightCount >= 0);
                removed += removedFromRight;
                toRemove -= removedFromRight;
            }

            if (toRemove == 0)
                return removed;

            // it's now possible our left or right is a 1 count and we still have more to remove so try them.

            if (n.RightCount == 1 && (locked == null || !locked((TN)n.Right)))
            {
                Debug.Assert(n.Right != null);
                n.RightCount = 0;
                n.Right = null;
                ++removed;
                --toRemove;
            }

            if (toRemove == 0)
                return removed;

            if (n.LeftCount == 1 && (locked == null || !locked((TN)n.Left)))
            {
                Debug.Assert(n.Left != null);
                n.LeftCount = 0;
                n.Left = null;
                ++removed;
                --toRemove;
            }

            return removed;
        }
        private int Balance(Node n, int h, int maxH)
        {
            // since we're dealing with weight and not height this may not be a perfect balance,
            // we do a BalanceChildren before and after to limit the recursion depth (after would be
            // more normal, as Prune does)
            if (n == null || n.Left == null && n.Right == null || h >= maxH)
            {
                return h;
            }

            BalanceChildren(ref n);

            var lh = Balance(n.Left, h + 1, maxH);
            var rh = Balance(n.Right, h + 1, maxH);

            BalanceChildren(ref n);

            return lh > rh ? lh : rh;
        }

        private void BalanceChildren(ref Node n)
        {
            var l = n.Left;
            var r = n.Right;
            for (;;)
            {
                var diff = n.LeftCount - n.RightCount;
                if (diff > 1) // Left weighs more than Right
                {
                    Debug.Assert(l != null);
                    var newLeftCount = l.LeftCount;
                    var newRightCount = 1 + l.RightCount + n.RightCount;
                    var newDiff = newLeftCount - newRightCount; // if we rotated right this would be the new diff
                    if (newDiff < 0) newDiff = -newDiff;
                    if (newDiff >= diff) // if the new diff is worse or the same don't bother
                        return;

                    RightRotate(n); // left -> right
                    n = l;
                    Debug.Assert(n.LeftCount == newLeftCount);
                    Debug.Assert(n.RightCount == newRightCount);
                }
                else if (diff < -1) // right weighs more than left
                {
                    Debug.Assert(r != null);
                    diff = -diff;
                    var newLeftCount = 1 + r.LeftCount + n.LeftCount;
                    var newRightCount = r.RightCount;
                    var newDiff = newRightCount - newLeftCount; // if we rotated left this would be the new diff
                    if (newDiff < 0) newDiff = -newDiff;
                    if (newDiff >= diff) // if the new diff is worse or the same don't bother
                        return;

                    LeftRotate(n); // right -> left
                    n = r;
                    Debug.Assert(n.LeftCount == newLeftCount);
                    Debug.Assert(n.RightCount == newRightCount);
                }
                else
                {
                    return;
                }

                l = n.Left;
                r = n.Right;
            }
        }
    }
}