using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class <c>Heap</c> creates a binary heap tree to store type T with the root being lowest f score
/// </summary>
public class Heap<T> where T : IHeapElement<T>
{
    //array of elements of type T (node)
    T[] array;
    //number of elements in the tree
    int currentLength;

    /// <summary>
    /// Function <c>Heap</c> creates a binary heap tree to store type T with the root being lowest f score
    /// </summary>
    public Heap(int heapSize)
    {
        array = new T[heapSize];
    }

    /// <summary>
    /// Function <c>Add</c> adds an element (node) to the binary tree
    /// </summary>
    public void Add(T element)
    {
        element.HeapIndex = currentLength;
        array[currentLength] = element;
        SortUp(element);
        currentLength++;
    }

    /// <summary>
    /// Function <c>SortUp</c> sorts the binary heap to have lowest value as the top most element.
    /// </summary>
    void SortUp(T element)
    {
        int parentIndex = (element.HeapIndex - 1) / 2;

        while (true)
        {
            T parentElement = array[parentIndex];
            if(element.CompareTo(parentElement) > 0)
            {
                Swap(element, parentElement);
            }
            else
            {
                break;
            }

            parentIndex = (element.HeapIndex - 1) / 2;
        }
    }

    /// <summary>
    /// Function <c>Contains</c> checks to see if an element (node) is contained in the binary heap.
    /// </summary>
    public bool Contains(T element)
    {
        return Equals(array[element.HeapIndex], element);
    }

    /// <summary>
    /// Function <c>Count</c> returns number of elements (nodes) in the binary heap.
    /// </summary>
    public int Count
    {
        get
        {
            return currentLength;
        }
    }

    public void UpdateElement(T element)
    {
        SortUp(element);
    }

    /// <summary>
    /// Function <c>swap</c> used for sorting
    /// </summary>
    void Swap(T elementA, T elementB)
    {
        array[elementA.HeapIndex] = elementB;
        array[elementB.HeapIndex] = elementA;
        int elementAIndex = elementA.HeapIndex;
        elementA.HeapIndex = elementB.HeapIndex;
        elementB.HeapIndex = elementAIndex;
    }

    /// <summary>
    /// Function <c>RemoveTop</c> removes the top most/lowest value node from the binary heap.
    /// </summary>
    public T RemoveTop()
    {
        T topElement = array[0];
        currentLength--;
        array[0] = array[currentLength];
        array[0].HeapIndex = 0;
        SortDown(array[0]);
        return topElement;
    }

    /// <summary>
    /// Function <c>SortDown</c> used to sort binary heap once top element is removed.
    /// </summary>
    void SortDown(T element)
    {
        while (true)
        {
            int childIndexLeft = element.HeapIndex * 2 + 1;
            int childIndexRight = element.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if(childIndexLeft < currentLength)
            {
                swapIndex = childIndexLeft;
                if(childIndexRight < currentLength)
                {
                    if (array[childIndexLeft].CompareTo(array[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }

                if(element.CompareTo(array[swapIndex]) < 0)
                {
                    Swap(element, array[swapIndex]);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }

}


public interface IHeapElement<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}