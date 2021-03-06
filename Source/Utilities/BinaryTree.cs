﻿// https://msdn.microsoft.com/en-us/library/ms379572?f=255&MSPPError=-2147217396

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Turnable.Utilities
{
    // TODO: Unit test
    public class Node<T>
    {
        // Private member-variables
        private T data;
        private NodeList<T> neighbors = null;

        public Node() { }
        public Node(T data) : this(data, null) { }
        public Node(T data, NodeList<T> neighbors)
        {
            this.data = data;
            this.neighbors = neighbors;
        }

        public T Value
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        protected NodeList<T> Neighbors
        {
            get
            {
                return neighbors;
            }
            set
            {
                neighbors = value;
            }
        }
    }

    public class NodeList<T> : Collection<Node<T>>
    {
        public NodeList() : base() { }

        public NodeList(int initialSize)
        {
            // Add the specified number of items
            for (int i = 0; i < initialSize; i++)
                base.Items.Add(default(Node<T>));
        }

        public Node<T> FindByValue(T value)
        {
            // search the list for the value
            foreach (Node<T> node in Items)
                if (node.Value.Equals(value))
                    return node;

            // if we reached here, we didn't find a matching node
            return null;
        }
    }

    public class BinaryTreeNode<T> : Node<T>
    {
        public BinaryTreeNode() : base() { }
        public BinaryTreeNode(T data) : base(data, null) { }
        public BinaryTreeNode(T data, BinaryTreeNode<T> left, BinaryTreeNode<T> right)
        {
            base.Value = data;
            NodeList<T> children = new NodeList<T>(2);
            children[0] = left;
            children[1] = right;

            base.Neighbors = children;
        }

        public BinaryTreeNode<T> Left
        {
            get
            {
                if (base.Neighbors == null)
                    return null;
                else
                    return (BinaryTreeNode<T>)base.Neighbors[0];
            }
            set
            {
                if (base.Neighbors == null)
                    base.Neighbors = new NodeList<T>(2);

                base.Neighbors[0] = value;
            }
        }

        public BinaryTreeNode<T> Right
        {
            get
            {
                if (base.Neighbors == null)
                    return null;
                else
                    return (BinaryTreeNode<T>)base.Neighbors[1];
            }
            set
            {
                if (base.Neighbors == null)
                    base.Neighbors = new NodeList<T>(2);

                base.Neighbors[1] = value;
            }
        }
    }

    public class BinaryTree<T>
    {
        private BinaryTreeNode<T> root;

        public BinaryTree()
        {
            root = null;
        }

        public virtual void Clear()
        {
            root = null;
        }

        public BinaryTreeNode<T> Root
        {
            get
            {
                return root;
            }
            set
            {
                root = value;
            }
        }

        public List<BinaryTreeNode<T>> CollectLeafNodes(BinaryTreeNode<T> startingRootNode = null)
        {
            List<BinaryTreeNode<T>> leafNodes = new List<BinaryTreeNode<T>>();

            if (startingRootNode == null)
            {
                RecursivelyCollectLeafNodes(leafNodes, Root);
            }
            else
            {
                RecursivelyCollectLeafNodes(leafNodes, startingRootNode);
            }

            return leafNodes;
        }

        private void RecursivelyCollectLeafNodes(List<BinaryTreeNode<T>> nodes, BinaryTreeNode<T> node)
        {
            if (node == null)
            {
                return;
            }
            if (IsLeaf(node))
            {
                nodes.Add(node);
            }

            RecursivelyCollectLeafNodes(nodes, node.Left);
            RecursivelyCollectLeafNodes(nodes, node.Right);
        }

        public bool IsLeaf(BinaryTreeNode<T> node)
        {
            return (node.Left == null && node.Right == null);
        }
    }
}

