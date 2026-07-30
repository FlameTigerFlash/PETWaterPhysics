using System;
using System.Collections.Generic;

public static class LinkedListExtensions
{
    public static LinkedListNode<T> NextInCircle<T>(this LinkedListNode<T> node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));
        if (node.List == null)
            throw new InvalidOperationException("Node does not belong to linked list.");

        return node.Next ?? node.List.First;
    }

    public static LinkedListNode<T> PreviousInCircle<T>(this LinkedListNode<T> node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));
        if (node.List == null)
            throw new InvalidOperationException("Node does not belong to linked list.");

        return node.Previous ?? node.List.Last;
    }
}
