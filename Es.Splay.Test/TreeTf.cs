using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CH.Combinatorics;
using NUnit.Framework;

namespace Es.Splay.Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    internal sealed class TreeTf
    {
        private sealed class Data : IComparable<Data>
        {
            public string Name;
            public int Score;

            public int CompareTo(Data other)
            {
                if (Name == other.Name)
                    return 0;
                if (Score > other.Score)
                    return 1;
                if (Score < other.Score)
                    return -1;
                return 0;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        [Test]
        public void TestComplexRemoveCase()
        {
            var t = new SplayTree<Data>();
            var data = new Data {Name = "z",Score=2};
            var zL = new SplayTree<Data>.Node(new Data {Name = "zL", Score=1});
            var uR = new SplayTree<Data>.Node(new Data {Name="uR", Score = 4});
            var u = new SplayTree<Data>.Node(new Data {Name="u", Score = 3 })
            {
                RightCount = 1,
                Right = uR
            };
            var o = new SplayTree<Data>.Node(new Data {Name="o", Score = 5})
            {
                LeftCount = 2,
                Left = u
            };
            var zR = new SplayTree<Data>.Node(new Data {Name = "zR", Score=6})
            {
                LeftCount = 3,
                Left = o
            };
            var z = new SplayTree<Data>.Node(data)
            {
                LeftCount = 1,
                RightCount = 4,
                Left = zL,
                Right = zR
            };
            zL.Parent = z;
            zR.Parent = z;
            uR.Parent = u;
            u.Parent = o;
            o.Parent = zR;

            t.Root = z;
            t.Count = 6;
            t.Remove(data);
            t.Validate();

            // handle an odd traverse case here as well.
            SplayTree<Data>.Node p = null;
            t.Traverse(t.Root, q =>
            {
                var r = q.Left==null || q.Left != p;
                p = q;
                return r;
            });
        }

        [Test]
        public void TestEdgeCases()
        {
            var t = new SplayTree<Data>();
            var s = t.ToString(); // doesn't throw on empty tree 
            t.Prune(0);
            t.Prune(-1);
            t.Balance();
            Assert.AreEqual(0,t.NearBy(new Data(), 1, 1).Count());
            Assert.Throws<Exception>(() => t.Worst());
            Assert.Throws<Exception>(() => t.Best());


            var data = new Data { Name = "z", Score = 2 };
            var zL = new SplayTree<Data>.Node(new Data { Name = "zL", Score = 1 });
            var o = new SplayTree<Data>.Node(new Data { Name = "o", Score = 5 })
            {
                LeftCount = 1,
            };
            var zR = new SplayTree<Data>.Node(new Data { Name = "zR", Score = 6 })
            {
                LeftCount = 3,
                Left = o
            };
            var z = new SplayTree<Data>.Node(data)
            {
                LeftCount = 1,
                RightCount = 4,
                Left = zL,
                Right = zR
            };
            zL.Parent = z;
            zR.Parent = z;
            o.Parent = zR;

            o.Left = z; // cycle

            t.Root = z;
            t.Count = 3;
            s = t.ToString();
            Assert.That(s.Contains("CYCLE"));
            t.Traverse(z, _ => false);
            Assert.Throws<Exception>(() => t.ForwardOrderTraverse(data, _ => true));
            Assert.Throws<Exception>(() => t.ReverseOrderTraverse(o.Data, _ => true));
        }

        [Test]
        public void TestGetEnumerator()
        {
            var t = new SplayTree<int>();
            t.Add(1);
            t.Add(6);
            t.Add(3);
            t.Add(4);
            t.Add(2);
            t.Add(5);
            var xs = new HashSet<int>();
            foreach (int x in (IEnumerable) t)
            {
                xs.Add(x);
            }
            Assert.AreEqual(6,xs.Count);
        }

        [Test]
        public void TestRemove()
        {
            Func<int, Data> dx = x => new Data { Name = $"n{x}", Score = x };

            var data = Enumerable.Range(0, 5).ToArray();

            var i = 0;
            foreach (var o in data.Permute())
            {
                var  t = new SplayTree<Data>();
                foreach (var n in data)
                {
                    t.Add(dx(n));
                    t.Validate();
                }
                Console.WriteLine($"{t}");
                Console.Write($"{i:D2}");
                ++i;
                var count = data.Length;
                Assert.AreEqual(t.Count,count);

                var xs = new HashSet<int>(t.Select(x=>x.Score));
                
                foreach (var x in o)
                {
                    Console.Write($" {x}");
                    Assert.That(t.Remove(dx(x)));
                    xs.Remove(x);
                    Assert.AreEqual(xs.Count,t.Count);
                    foreach (var xx in t) { Assert.That(xs.Contains(xx.Score));}
                    t.ForwardOrderTraverse(
                        dx(-1), 
                        xx => { Assert.That(xs.Contains(xx.Score));
                                                             return true;
                    });
                    t.ReverseOrderTraverse(
                        dx(99),
                        xx => {
                            Assert.That(xs.Contains(xx.Score));
                            return true;
                        });
                    foreach (var xx in t) { Assert.That(xs.Contains(xx.Score)); }
                    t.Validate();
                    --count;
                    Assert.AreEqual(t.Count, count);
                }
                Console.WriteLine();
            }
                
        }

        [Test]
        public void TestSplayTree()
        {
            var t = new SplayTree<Data>();

            const int count = 44;

            var toRemove = new HashSet<int> {0, 3, 7, 31, 43};

            Func<int,Data> dx = x => new Data {Name = $"n{x}", Score = x};

            for (var n = 0; n < count; ++n)
            {
                t.Add(dx(n));
                t.Validate();
            }

            foreach (var n in toRemove)
            {
                Console.WriteLine($"t {t}");
                var removeResult = t.Remove(dx(n));
                Assert.IsTrue(removeResult);
                Console.WriteLine($"Removed {n}");
                var arg0 = t.ToString();
                if (arg0.Contains("CYCLE"))
                {
                    Console.WriteLine($"{arg0}");
                    Assert.Fail();
                }
                t.Validate();
            }

            foreach (var n in toRemove)
            {
                Assert.IsFalse(t.Remove(dx(n)));
                t.Validate();
            }

            var expectedRank = 0;
            var expectedRanks = new int[count];

            for (var n = 0; n < count; ++n)
            {
                int rank;
                var data = dx(n);
                if (toRemove.Contains(n))
                {
                    Assert.IsFalse(t.TryGetRank(data, out rank));
                    expectedRanks[n] = -1;
                }
                else
                {
                    Assert.IsTrue(t.TryGetRank(data, out rank));
                    expectedRanks[n] = expectedRank;
                    ++expectedRank;
                    Assert.AreEqual(expectedRanks[n], rank);
                }
            }
            for (var n = 0; n < count; ++n)
            {
                int rank;
                if (toRemove.Contains(n))
                {
                    Assert.IsFalse(t.TryGetRank(dx(n), out rank));
                }
                else
                {
                    Assert.IsTrue(t.TryGetRank(dx(n), out rank));
                    Assert.AreEqual(expectedRanks[n], rank);
                }
            }

            foreach (var n in toRemove)
            {
                t.Add(dx(n));
                t.Validate();
            }

            for (var n = 0; n < count; ++n)
            {
                int rank;
                Assert.IsTrue(t.TryGetRank(dx(n), out rank));
                Assert.AreEqual(n, rank);
            }

            int startRank;

            var data1 = dx(33);
            Assert.IsTrue(t.TryGetRank(data1,out startRank));
            var l = t.NearBy(data1, 2, 2).ToList();
            t.Validate();

            Console.WriteLine("near (2 on either size) 33: {1} [{0}]",
                string.Join(", ", l.Select(x => x.ToString())),
                startRank);
            Assert.AreEqual(33, startRank);
            Assert.AreEqual(5, l.Count);
            Assert.AreEqual(31, l[0].Score);
            Assert.AreEqual(32, l[1].Score);
            Assert.AreEqual(33, l[2].Score);
            Assert.AreEqual(34, l[3].Score);
            Assert.AreEqual(35, l[4].Score);

            data1 = dx(42);
            Assert.IsTrue(t.TryGetRank(data1, out startRank));
            l = t.NearBy(data1,2,2).ToList();
            t.Validate();
            Console.WriteLine("near (2 on either size) 42: {1} [{0}]", string.Join(", ", l.Select(x => x.ToString())), startRank);
            Assert.AreEqual(42, startRank);
            Assert.AreEqual(4, l.Count);
            Assert.AreEqual(40, l[0].Score);
            Assert.AreEqual(41, l[1].Score);
            Assert.AreEqual(42, l[2].Score);
            Assert.AreEqual(43, l[3].Score);

            data1 = dx(-1);
            l = t.NearBy(data1, 2, 2).ToList();
            t.Validate();
            Console.WriteLine("near (2 on either size) -1: {1} [{0}]",string.Join(", ", l.Select(x => x.ToString())),startRank);
            Assert.AreEqual(3, l.Count);
            Assert.AreEqual(0, l[0].Score);
            Assert.AreEqual(1, l[1].Score);
            Assert.AreEqual(2, l[2].Score);

            data1 = dx(999);
            l = t.NearBy(data1, 2, 2).ToList();
            t.Validate();
            Console.WriteLine("near (1 on either size) 999: {1} [{0}]", string.Join(", ", l.Select(x => x.ToString())), startRank);
            Assert.AreEqual(3, l.Count);
            Assert.AreEqual(41, l[0].Score);
            Assert.AreEqual(42, l[1].Score);
            Assert.AreEqual(43, l[2].Score);

            Console.WriteLine(
                "--remove------------------------------------------------------------------------------------");
            t.Remove(dx(25)); // tests a specific hard to get to case in Remove
            t.Validate();

            t.Add(dx(100));
            t.Validate();
            t.Add(dx(101));
            t.Validate();
            t.Add(dx(102));
            t.Validate();
            t.Add(dx(103));
            t.Validate();
            t.Add(dx(104));
            t.Validate();
            t.Add(dx(104));
            t.Validate();
            t.Add(dx(105));
            t.Validate();

            data1 = dx(33);
            Assert.IsTrue(t.TryGetRank(data1, out startRank));
            l = t.NearBy(data1,2,2).ToList();
            t.Validate();

            Console.WriteLine("near (2 on either size) 33: {1} [{0}]", string.Join(", ", l.Select(x => x.ToString())), startRank);

            Assert.AreEqual(32, startRank);
            Assert.AreEqual(5, l.Count);
            Assert.AreEqual(31, l[0].Score);
            Assert.AreEqual(32, l[1].Score);
            Assert.AreEqual(33, l[2].Score);
            Assert.AreEqual(34, l[3].Score);
            Assert.AreEqual(35, l[4].Score);

            data1 = dx(33);
            Assert.IsTrue(t.TryGetRank(data1, out startRank));
            l = t.NearBy(data1, 2, 2).ToList();
            t.Validate();
            Console.WriteLine("near (2 on either size) id 33: {1} [{0}]",
                string.Join(", ", l.Select(x => x.ToString())),
                startRank);
            Assert.AreEqual(32, startRank);
            Assert.AreEqual(5, l.Count);
            Assert.AreEqual(31, l[0].Score);
            Assert.AreEqual(32, l[1].Score);
            Assert.AreEqual(33, l[2].Score);
            Assert.AreEqual(34, l[3].Score);
            Assert.AreEqual(35, l[4].Score);

            Assert.AreEqual(t.Count, t.Count());

            var worst = t.Worst();
            Assert.AreEqual(105, worst.Score);

            var best = t.Best();
            Assert.AreEqual(0, best.Score);

            t.Clear();
            t.ForwardOrderTraverse(data1, _ =>
            {
                Assert.Fail();
                return true;
            });
            t.ReverseOrderTraverse(data1, _ =>
            {
                Assert.Fail();
                return true;
            });
        }

        [Test]
        public void TestTreeBalance()
        {
            ISplayTree<Data> t = new SplayTree<Data>();
            const int count = 32;

            for (var i = 0; i < count; ++i)
            {
                t.Add(new Data {Name = $"n{i}", Score = i});
            }
            t.Balance();
            var balanced = t.ToString();
            const int newDesiredCount = 15;
            var removed = t.Prune(newDesiredCount);
            Assert.AreEqual(count - newDesiredCount, removed);
            var pruned = t.ToString();

            Console.WriteLine(balanced);
            Console.WriteLine(pruned);
        }

        [Test]
        public void TestPerf()
        {
            ISplayTree<Data> t = new SplayTree<Data>();
            const int count = 200000;

            for (var i = 0; i < count; ++i)
            {
                t.Add(new Data {Name = "n" + i, Score = i});
            }
            var sw = Stopwatch.StartNew();
            var h = t.Balance();
            var balanceTime = sw.ElapsedMilliseconds;
            sw.Restart();
            var h2 = t.Balance();
            var balanceTime2 = sw.ElapsedMilliseconds;
            var j = 0;
            var srs = new int[20];
            var rs = new IList<Data>[20];
            sw.Restart();
            for (var n = 40000; n < 60000; n += 1000)
            {
                var data = new Data {Name = $"n{n}", Score = n};
                int rank;
                Assert.IsTrue(t.TryGetRank(data, out rank));

                srs[j] = rank;
                rs[j] = t.NearBy(data, 0, 200).ToList();
                ++j;
            }
            sw.Stop();
            Console.WriteLine(
                $"balance {balanceTime}ms h={h}, rebalance {balanceTime2}ms h={h2}, avg query time {sw.ElapsedMilliseconds/j}ms");
            j = 0;
            foreach (var r in rs)
            {
                Console.WriteLine($"{j} -> {string.Join(",", r.Select(x => x.Score))}");
                ++j;
            }

            sw.Restart();
            var initialCount = t.Count;
            var removed = t.Prune(initialCount/2);
            sw.Stop();

            Console.WriteLine(
                $"prune {sw.ElapsedMilliseconds}ms initialCount={initialCount}, removed={removed}, finalCount={t.Count}");
        }

        [Test]
        public void TestPerfVsHashset()
        {
            ISplayTree<int> t = new SplayTree<int>();
            ISet<int> h = new HashSet<int>();
            const int sn = 100;
            const int count = 1000*sn;

            var sw = Stopwatch.StartNew();
            var j = 0;

            sw.Restart();
            for (var i = 0; i < count * sn; ++i)
            {
                t.Add(i);
                ++j;
            }
            sw.Stop();
            Console.WriteLine($"splay tree {j} adds, avg time {(double)sw.ElapsedMilliseconds / j}ms");
            j = 0;
            sw.Restart();
            for (var i = 0; i < count * sn; ++i)
            {
                h.Add(i);
                ++j;
            }
            sw.Stop();
            Console.WriteLine($"hash set {j} adds, avg time {(double)sw.ElapsedMilliseconds / j}ms");
            j = 0;
            sw.Restart();
            t.Balance();
            for (var n = 400*sn; n < 600*sn; n += 1)
            {
                int r;
                t.TryGetRank(n, out r);
                ++j;
            }
            sw.Stop();
            Console.WriteLine($"splay tree {j} queries, avg query time {(double)sw.ElapsedMilliseconds / j}ms");
            j = 0;
            sw.Restart();
            for (var n = 400*sn; n < 600*sn; n += 1)
            {
               var b = h.Contains(n);
                ++j;
            }
            sw.Stop();
            Console.WriteLine($"hash {j} queries, avg query time {(double)sw.ElapsedMilliseconds / j}ms");
        }
    }
}