#region --- License ---

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

#endregion --- License ---

using JetBrains.Annotations;

namespace Jerry.Utilities.Utility;

/// <summary>
/// Convenience utilities for working with various locking classes.
/// </summary>
public static class LockUtility
{
    /// <summary>
    /// Enter a read lock on a <see cref="ReaderWriterLockSlim"/>. Dispose the returned value to exit the read lock.
    /// </summary>
    /// <remarks>
    /// This is intended to be used with a <see langword="using" /> statement or block.
    /// </remarks>
    [MustUseReturnValue]
    public static RWReadGuard ReadGuard(this ReaderWriterLockSlim rwLock)
    {
        rwLock.EnterReadLock();
        return new RWReadGuard(rwLock);
    }

    /// <summary>
    /// Enter a write lock on a <see cref="ReaderWriterLockSlim"/>. Dispose the returned value to exit the write lock.
    /// </summary>
    /// <remarks>
    /// This is intended to be used with a <see langword="using" /> statement or block.
    /// </remarks>
    [MustUseReturnValue]
    public static RWWriteGuard WriteGuard(this ReaderWriterLockSlim rwLock)
    {
        rwLock.EnterWriteLock();
        return new RWWriteGuard(rwLock);
    }

    /// <summary>
    /// Wait on a <see cref="SemaphoreSlim"/>. Dispose the returned value to release.
    /// </summary>
    /// <remarks>
    /// This is intended to be used with a <see langword="using" /> statement or block.
    /// </remarks>
    [MustUseReturnValue]
    public static SemaphoreGuard WaitGuard(this SemaphoreSlim semaphore)
    {
        semaphore.Wait();
        return new SemaphoreGuard(semaphore);
    }

    /// <summary>
    /// Wait on a <see cref="SemaphoreSlim"/> asynchronously. Dispose the returned value to release.
    /// </summary>
    /// <remarks>
    /// This is intended to be used with a <see langword="using" /> statement or block.
    /// </remarks>
    [MustUseReturnValue]
    public static async ValueTask<SemaphoreGuard> WaitGuardAsync(this SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        return new SemaphoreGuard(semaphore);
    }

    // ReSharper disable once InconsistentNaming
    public struct RWReadGuard : IDisposable
    {
        public readonly ReaderWriterLockSlim RwLock;
        public bool Disposed { get; private set; }

        public RWReadGuard(ReaderWriterLockSlim rwLock)
        {
            RwLock = rwLock;
            Disposed = false;
        }

        public void Dispose()
        {
            if (Disposed)
                throw new InvalidOperationException($"Double dispose of {nameof(RWReadGuard)}");

            Disposed = true;
            RwLock.ExitReadLock();
        }
    }

    // ReSharper disable once InconsistentNaming
    public struct RWWriteGuard : IDisposable
    {
        public readonly ReaderWriterLockSlim RwLock;
        public bool Disposed { get; private set; }

        public RWWriteGuard(ReaderWriterLockSlim rwLock)
        {
            RwLock = rwLock;
            Disposed = false;
        }

        public void Dispose()
        {
            if (Disposed)
                throw new InvalidOperationException($"Double dispose of {nameof(RWWriteGuard)}");

            Disposed = true;
            RwLock.ExitWriteLock();
        }
    }

    public struct SemaphoreGuard : IDisposable
    {
        public readonly SemaphoreSlim Semaphore;
        public bool Disposed { get; private set; }

        public SemaphoreGuard(SemaphoreSlim semaphore)
        {
            Semaphore = semaphore;
            Disposed = false;
        }

        public void Dispose()
        {
            if (Disposed)
                throw new InvalidOperationException($"Double dispose of {nameof(SemaphoreGuard)}");

            Disposed = true;
            Semaphore.Release();
        }
    }
}

