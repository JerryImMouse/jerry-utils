// MIT License
//
// Copyright (c) 2017-2024 Space Wizards Federation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
//     copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Reflection;
using Jerry.Utilities.Utility;

namespace Jerry.Utilities.IoC.Referenced;

public struct DepIdx
{
    private static readonly ReaderWriterLockSlim SlowStoreLock = new();
    private static readonly Dictionary<Type, DepIdx> SlowStore = new();
    
    internal readonly int Value;
    
    internal static DepIdx Index<T>() => Store<T>.Index;
    
    internal static DepIdx Index(Type t)
    {
        using (SlowStoreLock.ReadGuard())
        {
            if (SlowStore.TryGetValue(t, out var idx))
                return idx;
        }

        // Doesn't exist in the store, get a write lock and add it.
        using (SlowStoreLock.WriteGuard())
        {
            var idx = (DepIdx)typeof(Store<>)
                .MakeGenericType(t)
                .GetField(nameof(Store<int>.Index), BindingFlags.Static | BindingFlags.Public)!
                .GetValue(null)!;

            SlowStore[t] = idx;
            return idx;
        }
    }
    internal static int ArrayIndex<T>() => Index<T>().Value;
    internal static int ArrayIndex(Type type) => Index(type).Value;

    internal static void AssignArray<T>(ref T[] array, DepIdx idx, T value)
    {
        RefArray(ref array, idx) = value;
    }

    internal static ref T RefArray<T>(ref T[] array, DepIdx idx)
    {
        var curLength = array.Length;
        if (curLength <= idx.Value)
        {
            var newLength = MathHelper.NextPowerOfTwo(Math.Max(8, idx.Value));
            Array.Resize(ref array, newLength);
        }

        return ref array[idx.Value];
    }
    
    private static int _DepIdxMaster = -1;
    
    private static class Store<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        public static readonly DepIdx Index = new(Interlocked.Increment(ref _DepIdxMaster));
    }
    internal DepIdx(int value)
    {
        Value = value;
    }
    public bool Equals(DepIdx other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is DepIdx other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value;
    }

    public static bool operator ==(DepIdx left, DepIdx right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DepIdx left, DepIdx right)
    {
        return !left.Equals(right);
    }
}