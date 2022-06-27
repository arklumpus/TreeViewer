using PhyloTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TreeViewer.Stats
{
    public class ThreadSafeRandom : Random
    {
        private static Random _globalRandom;
        private static object _globalLock = new object();
        [ThreadStatic] private static Random _local;

        private bool _useGlobalRandom;

        public ThreadSafeRandom(int seed)
        {
            lock (_globalLock)
            {
                _globalRandom = new Random(seed);
                _useGlobalRandom = true;
            }
        }

        public ThreadSafeRandom()
        {
            _useGlobalRandom = false;
        }

        private void InitialiseLocal()
        {
            if (_local == null)
            {
                if (!_useGlobalRandom)
                {
                    byte[] buffer = RandomNumberGenerator.GetBytes(4);
                    _local = new Random(BitConverter.ToInt32(buffer, 0));
                }
                else
                {
                    lock (_globalLock)
                    {
                        _local = new Random(_globalRandom.Next());
                    }
                }
            }
        }

        public override int Next()
        {
            InitialiseLocal();
            return _local.Next();
        }

        public override int Next(int maxValue)
        {
            InitialiseLocal();
            return _local.Next(maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            InitialiseLocal();
            return _local.Next(minValue, maxValue);
        }

        public override double NextDouble()
        {
            InitialiseLocal();
            return _local.NextDouble();
        }

        public override void NextBytes(byte[] buffer)
        {
            InitialiseLocal();
            _local.NextBytes(buffer);
        }
    }


    internal class SimulateTree
    {
        private static ThreadSafeRandom mainRandom = new ThreadSafeRandom();

        public static TreeNode Simulate(int leafCount, TreeNode.NullHypothesis model, bool rooted)
        {
            if (model == TreeNode.NullHypothesis.YHK)
            {
                TreeNode initialTree = new TreeNode(null);
                initialTree.Children.Add(new TreeNode(initialTree));
                initialTree.Children.Add(new TreeNode(initialTree));

                if (!rooted)
                {
                    initialTree.Children.Add(new TreeNode(initialTree));
                }

                List<TreeNode> leaves = new List<TreeNode>();

                leaves.AddRange(initialTree.Children);

                while (leaves.Count < leafCount)
                {
                    int index = mainRandom.Next(0, leaves.Count);

                    TreeNode selectedLeaf = leaves[index];

                    leaves.RemoveAt(index);

                    selectedLeaf.Children.Add(new TreeNode(selectedLeaf));
                    selectedLeaf.Children.Add(new TreeNode(selectedLeaf));
                    leaves.AddRange(selectedLeaf.Children);
                }

                return initialTree;
            }
            else if (model == TreeNode.NullHypothesis.PDA)
            {
                TreeNode initialTree = new TreeNode(null);
                initialTree.Children.Add(new TreeNode(initialTree));
                initialTree.Children.Add(new TreeNode(initialTree));

                if (!rooted)
                {
                    initialTree.Children.Add(new TreeNode(initialTree));
                }

                List<TreeNode> leaves = new List<TreeNode>(initialTree.Children);
                List<TreeNode> nodes;

                if (rooted)
                {
                    nodes = initialTree.GetChildrenRecursive();
                }
                else
                {
                    nodes = new List<TreeNode>(initialTree.Children);
                }

                while (leaves.Count < leafCount)
                {
                    int index = mainRandom.Next(0, nodes.Count);

                    TreeNode selectedNode = nodes[index];

                    if (selectedNode.Children.Count == 0)
                    {
                        leaves.Remove(selectedNode);

                        selectedNode.Children.Add(new TreeNode(selectedNode));
                        selectedNode.Children.Add(new TreeNode(selectedNode));
                        leaves.AddRange(selectedNode.Children);
                        nodes.AddRange(selectedNode.Children);
                    }
                    else
                    {
                        if (selectedNode.Parent != null)
                        {
                            TreeNode newNode = new TreeNode(selectedNode.Parent);
                            selectedNode.Parent.Children.Add(newNode);

                            TreeNode newLeaf = new TreeNode(newNode);
                            newNode.Children.Add(newLeaf);

                            selectedNode.Parent.Children.Remove(selectedNode);
                            selectedNode.Parent = newNode;
                            newNode.Children.Add(selectedNode);

                            nodes.Add(newNode);
                            nodes.Add(newLeaf);
                            leaves.Add(newLeaf);
                        }
                        else
                        {
                            TreeNode newNode = new TreeNode(null);
                            TreeNode newLeaf = new TreeNode(newNode);
                            newNode.Children.Add(newLeaf);

                            selectedNode.Parent = newNode;
                            newNode.Children.Add(selectedNode);

                            nodes.Add(newNode);
                            nodes.Add(newLeaf);
                            leaves.Add(newLeaf);

                            initialTree = newNode;
                        }

                    }
                }

                return initialTree;
            }
            else
            {
                throw new ArgumentException("Invalid tree model");
            }
        }
    }
}
