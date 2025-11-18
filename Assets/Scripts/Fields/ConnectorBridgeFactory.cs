using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class ConnectorBridgeFactory
{
    private static readonly ConcurrentDictionary<Type, IConnectorValueBridge> _bridgeCache = new();

    // ✅ ADD: Bridge tracking and diagnostics
    public static int TotalBridgesCreated { get; private set; }
    public static int CacheHits { get; private set; }
    public static int CacheMisses { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetStats()
    {
        TotalBridgesCreated = 0;
        CacheHits = 0;
        CacheMisses = 0;
        _bridgeCache.Clear();
    }

    public static IConnectorValueBridge CreateBridge(IConnectorValue value)
    {
        if (value == null) return null;

        var valueType = value.GetType();

        if (_bridgeCache.TryGetValue(valueType, out var bridge))
        {
            CacheHits++;
            Debug.Log($"[Bridge] Cache HIT for {valueType.Name} -> {bridge.GetType().Name}");
            return bridge;
        }

        CacheMisses++;
        Debug.Log($"[Bridge] Cache MISS for {valueType.Name}, creating FAST bridge...");

        // ✅ ALWAYS try to create FastConnectorBridge first
        var innerValue = value.GetInnerValue();
        var innerType = innerValue?.GetType() ?? typeof(object);

        try
        {
            var bridgeType = typeof(FastConnectorBridge<>).MakeGenericType(innerType);
            bridge = (IConnectorValueBridge)Activator.CreateInstance(bridgeType, value);
            TotalBridgesCreated++;
            Debug.Log($"[Bridge] Successfully created FastConnectorBridge<{innerType.Name}>");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Bridge] Fast bridge failed, falling back to self-bridge: {ex.Message}");

            // Fallback to self-bridge only if fast bridge fails
            if (value is IConnectorValueBridge selfBridge)
            {
                bridge = selfBridge;
                Debug.Log($"[Bridge] Using self-bridge as fallback");
            }
            else
            {
                Debug.LogError($"[Bridge] No bridge available for {valueType.Name}");
                return null;
            }
        }

        _bridgeCache[valueType] = bridge;
        return bridge;
    }

#if UNITY_EDITOR
    // ✅ ADD: Diagnostic method to show all cached bridges
    [MenuItem("Tools/Node System/Show Bridge Cache")]
    public static void LogBridgeCache()
    {
        Debug.Log($"=== BRIDGE CACHE STATUS ===");
        Debug.Log($"Total Bridges Created: {TotalBridgesCreated}");
        Debug.Log($"Cache Hits: {CacheHits}");
        Debug.Log($"Cache Misses: {CacheMisses}");
        Debug.Log($"Cache Size: {_bridgeCache.Count}");
        Debug.Log($"Cache Hit Rate: {((CacheHits + CacheMisses) > 0 ? (float)CacheHits / (CacheHits + CacheMisses) * 100 : 0):F1}%");
        foreach (var kvp in _bridgeCache)
        {
            Debug.Log($"  {kvp.Key.Name} -> {kvp.Value.GetType().Name} (ValueType: {kvp.Value.ValueType?.Name})");
        }
    }
    [MenuItem("Tools/Node System/Test Bridge Performance")]
    public static void TestBridgePerformance()
    {
        Debug.Log("=== BRIDGE PERFORMANCE COMPARISON ===");

        var testValue = new ConnectorValueInt(0);
        var bridge = ConnectorBridgeFactory.CreateBridge(testValue);

        Debug.Log($"Bridge type: {bridge.GetType().Name}");
        Debug.Log($"Is self-bridge: {bridge is ConnectorValueInt}");
        Debug.Log($"Is fast bridge: {bridge is FastConnectorBridge<int>}");

        // Test performance
        var stopwatch = new System.Diagnostics.Stopwatch();

        // Test self-bridge performance
        stopwatch.Start();
        for (int i = 0; i < 1000000; i++)
        {
            bridge.SetValueFast(i);
            var val = bridge.GetValueFast();
        }
        stopwatch.Stop();
        Debug.Log($"Self-bridge performance: {stopwatch.ElapsedMilliseconds}ms");

        // Test what fast bridge would be
        try
        {
            var fastBridge = new FastConnectorBridge<int>(testValue);
            stopwatch.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                fastBridge.SetValue(i);
                var val = fastBridge.GetValue();
            }
            stopwatch.Stop();
            Debug.Log($"Fast-bridge performance: {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create fast bridge: {e.Message}");
        }
    }
#endif
    // ✅ ADD: Method to check if a type has a bridge
    public static bool HasBridgeForType(Type valueType)
    {
        return _bridgeCache.ContainsKey(valueType);
    }

    // ✅ ADD: Get all bridge types in cache
    public static List<Type> GetCachedBridgeTypes()
    {
        return _bridgeCache.Values.Select(b => b.GetType()).Distinct().ToList();
    }
}