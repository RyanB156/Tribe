using System;
using System.Text;

namespace Tribe
{
    public class Node<T> where T : IComparable
    {
        public T Value { get; private set; }
        public Node<T> Next { get; private set; }

        public Node(T value)
        {
            Value = value;
            Next = null;
        }

        public void SetNext(Node<T> next)
        {
            Next = next;
        }
    }

    /// <summary>
    /// PriorityQueue implemented using a singly linked list. O(1) access to minimum element, O(n) insert to put element in the correct place.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T> where T : IComparable
    {
        public int Length { get; private set; }
        public Node<T> Head { get; private set; }

        public PriorityQueue()
        {
            Length = 0;
            Head = null;
        }

        public void Insert(T value)
        {
            Node<T> node;

            if (Head == null) // Insert first element.
            {
                node = new Node<T>(value);
                Head = node;
            }
            else if (value.CompareTo(Head.Value) <= 0) // Insert in front of head.
            {
                node = new Node<T>(value);
                node.SetNext(Head);
                Head = node;
            }
            else // Insert after a node and possibly before another node (between or at the end).
            {
                node = Head;
                while (true)
                {
                    if (node.Next == null)
                        break;
                    else if (value.CompareTo(node.Next.Value) < 0) // new node will come after this node but before the next one.
                        break;
                    else // value will come after the next node.
                        node = node.Next;
                }

                Node<T> newNode = new Node<T>(value);
                if (node.Next != null) // Insert newNode in between node and node.Next
                {
                    newNode.SetNext(node.Next);
                }
                node.SetNext(newNode); // newNode comes after the current node. It may or may not be linked to further nodes.
            }

            Length++;
        }

        public T PopMin()
        {

            T value = Head.Value;
            Node<T> next = Head.Next;
            Head.SetNext(null);
            Head = next;

            Length--;
            return value;
        }

        public T PeekMin()
        {
            if (Length == 0)
                throw new ArgumentException("Priority queue was empty");
            else
                return Head.Value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            Node<T> node = Head;
            while (node != null)
            {
                sb.Append(node.Value + " ");
                node = node.Next;
            }

            return sb.ToString();
        }

    }
}
