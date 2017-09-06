/* This package contains a generic, static class that can diff two IEnumerables with 
 * user-supplied diffing functions. Visit the project page for samples and more information.
 * This package is a single, easily includable .cs file that has no dependencies on anything 
 * else. Part of Code Blocks (http://codeblocks.codeplex.com) 
 */

// #define THREAD_SAFE_TIMER
// Define this symbol to make all Timer<TMarker> instances thread safe, but slower due to the overhead of Interlocked.XXX methods
// http://stackoverflow.com/questions/1034070/performance-of-interlocked-increment

using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using System.Collections;

#if THREAD_SAFE_TIMER
using System.Collections.Concurrent;
using System.Threading;
#endif

internal static class NativeMethods
{
    [DllImport("Kernel32.dll")]
    public static extern bool QueryPerformanceCounter(out long count);

    [DllImport("Kernel32.dll")]
    public static extern bool QueryPerformanceFrequency(out long frequency);
}

public sealed class Timer<TMarker> : IEnumerable<TMarker>
{
    private sealed class MarkerStats
    {
        public long Min = long.MaxValue;
        public long Max = long.MinValue;
        public long Total = 0L;
        public long Count = 0L;
    }

    private readonly long _frequency;
    private readonly Stack<long> _startTimes = new Stack<long>();
#if THREAD_SAFE_TIMER
    private readonly ConcurrentDictionary<TMarker, MarkerStats> _markerStats = new ConcurrentDictionary<TMarker, MarkerStats>();
#else
    private readonly Dictionary<TMarker, MarkerStats> _markerStats = new Dictionary<TMarker, MarkerStats>();
#endif

    public static readonly Timer<TMarker> Global = new Timer<TMarker>();

    public Timer()
    {
        if (!NativeMethods.QueryPerformanceFrequency(out _frequency))
            throw new NotSupportedException("High performance timers are not supported on this system");
    }

    public void Start()
    {
        long startTime;
        NativeMethods.QueryPerformanceCounter(out startTime);
        _startTimes.Push(startTime);
    }

    private long Stop()
    {
        if (_startTimes.Count == 0)
            throw new InvalidOperationException("Unmatched starts/stops");

        long stopTime;
        NativeMethods.QueryPerformanceCounter(out stopTime);

        return stopTime - _startTimes.Pop();
    }

    public void Stop(TMarker marker)
    {
        Lap(marker, true);
    }

    public void Lap(TMarker marker, bool stop = false)
    {
        var time = Stop();
        if (!stop)
            Start();

        MarkerStats stats = null;
        if (!_markerStats.TryGetValue(marker, out stats))
        {
            stats = new MarkerStats();
#if THREAD_SAFE_TIMER
            if (!_markerStats.TryAdd(marker, stats))
                stats = _markerStats[marker]; // Item has been added by now
#else
            _markerStats.Add(marker, stats);
#endif
        }

#if THREAD_SAFE_TIMER
        Interlocked.Increment(ref stats.Count);
        Interlocked.Exchange(ref stats.Min, Math.Min(stats.Min, time));
        Interlocked.Exchange(ref stats.Max, Math.Max(stats.Max, time));
        Interlocked.Add(ref stats.Total, time);
#else
        stats.Count++;
        stats.Min = stats.Min < time ? stats.Min : time;
        stats.Max = stats.Max > time ? stats.Max : time;
        stats.Total += time;
#endif
    }

    public void Add(TMarker marker, float timeInSeconds)
    {
        MarkerStats stats = null;
        if (!_markerStats.TryGetValue(marker, out stats))
        {
            stats = new MarkerStats();
#if THREAD_SAFE_TIMER
            if (!_markerStats.TryAdd(marker, stats))
                stats = _markerStats[marker]; // Item has been added by now
#else
            _markerStats.Add(marker, stats);
#endif
        }

        var time = (long)(timeInSeconds * _frequency);
#if THREAD_SAFE_TIMER
        Interlocked.Increment(ref stats.Count);
        Interlocked.Exchange(ref stats.Min, Math.Min(stats.Min, time));
        Interlocked.Exchange(ref stats.Max, Math.Max(stats.Max, time));
        Interlocked.Add(ref stats.Total, time);
#else
        stats.Count++;
        stats.Min = stats.Min < time ? stats.Min : time;
        stats.Max = stats.Max > time ? stats.Max : time;
        stats.Total += time;
#endif
    }

    public void Reset()
    {
        _startTimes.Clear();
        _markerStats.Clear();
    }

    #region IEnumerable<TMarker> Members

    public IEnumerator<TMarker> GetEnumerator()
    {
        foreach (var kvp in _markerStats)
            yield return kvp.Key;
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    public double TotalSeconds(TMarker marker)
    {
        MarkerStats stats = null;
        if (_markerStats.TryGetValue(marker, out stats))
            return stats.Total / (double)_frequency;

        throw new KeyNotFoundException(marker.ToString());
    }

    public double AverageSeconds(TMarker marker)
    {
        MarkerStats stats = null;
        if (_markerStats.TryGetValue(marker, out stats))
            return (stats.Total / (double)_frequency) / stats.Count;

        throw new KeyNotFoundException(marker.ToString());
    }

    public double MaxSeconds(TMarker marker)
    {
        MarkerStats stats = null;
        if (_markerStats.TryGetValue(marker, out stats))
            return stats.Max / (double)_frequency;

        throw new KeyNotFoundException(marker.ToString());
    }

    public double MinSeconds(TMarker marker)
    {
        MarkerStats stats = null;
        if (_markerStats.TryGetValue(marker, out stats))
            return stats.Min / (double)_frequency;

        throw new KeyNotFoundException(marker.ToString());
    }

    public long Count(TMarker marker)
    {
        MarkerStats stats = null;
        if (_markerStats.TryGetValue(marker, out stats))
            return stats.Count;

        throw new KeyNotFoundException(marker.ToString());
    }
}
