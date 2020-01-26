using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingArray<T> {
    // array of the actual data
    private T[] data;
    // number of items currently in the array
    public int count;
    // number of items this rolling array can contain
    public int capacity;
    // id of the first element of the RollingArray inside the data array:
    // To avoid re-assigning the whole array on each add, we're writing only the last value (
    // overwriting the first one when the RollingArray is full).
    private int startIndex;

    public RollingArray(int capacity) {
        data = new T[capacity];
        this.capacity = capacity;
        this.count = 0;
        startIndex = 0;
    }

    public bool IsReadOnly => false;

    public void Add(T item) {
        int index;
        if (count < capacity) {
            index = count;
            count += 1;
        } else {
            index = startIndex;
            startIndex = (startIndex + 1) % capacity;
        }
        data[startIndex] = item;
    }

    public T this[int index] {
        get {
            if (index < 0) index += count;
            index = (index + startIndex) % capacity;
            return data[index];
        }
        set {
            if (index < 0) index += count;
            index = (index + startIndex) % capacity;
            data[index] = value;
        }
    }

    public static implicit operator T[](RollingArray<T> input) {
        T[] output = new T[input.count];
        for (int i = 0; i < input.count; i++) output[i] = input[i];
        return output;
    }

    public void Clear() {
        Array.Clear(data, 0, capacity);
        this.count = 0;
        this.startIndex = 0;
    }

    public void fill(T value) {
        for (int i = 0; i < capacity; i++) this[i] = value;
        startIndex = 0;
        count = capacity;
    }
}

public class RollingArrayFloat : RollingArray<float>
{
    public RollingArrayFloat(int capacity) : base(capacity) {
    }

    public float sum() {
        float total = 0;
        for (int i = 0; i < this.count; i++) total += this[i];
        return total;
    }

    public float average() {
        return this.sum() / this.count;
    }
}