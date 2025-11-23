using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public static class ConnectorBridgeFactory
{
    // Statistics
    public static int TotalBridgeRequests { get; private set; }
    public static int BridgesActuallyCreated { get; private set; }
    public static int PoolHits { get; private set; }
    public static int DirectAccessHits { get; private set; }

    // 🚀 PRIMARY POOL: Instance-based for exact matches
    private static readonly Dictionary<IConnectorValue, IConnectorValueBridge> _bridgePool =
        new Dictionary<IConnectorValue, IConnectorValueBridge>();

    // 🚀 TYPE CACHE: Thread-safe bridge type compilation
    private static readonly Dictionary<Type, Type> _bridgeTypeCache = new Dictionary<Type, Type>();
    private static readonly object _typeCacheLock = new object();

    // 🚀 COMMON TYPES: Pre-optimized for direct access
    private static readonly HashSet<Type> _commonTypes = new HashSet<Type>
    {
        typeof(int), typeof(float), typeof(bool), typeof(string),
        typeof(Vector3), typeof(GameObject), typeof(void), typeof(object)
    };

    // 🚀 DIRECT ACCESS BRIDGES: Ultra-fast for common types
    private static readonly Dictionary<Type, Func<IConnectorValue, IConnectorValueBridge>> _directBridgeFactories =
        new Dictionary<Type, Func<IConnectorValue, IConnectorValueBridge>>();

    // 🚀 WEAK REFERENCES: Prevent memory leaks for long-lived objects
    private static readonly Dictionary<WeakReference, IConnectorValueBridge> _weakBridgePool =
        new Dictionary<WeakReference, IConnectorValueBridge>();
    private static int _lastCleanupFrame = 0;
    private const int CLEANUP_FRAME_INTERVAL = 60;

    // 🚀 PERFORMANCE TRACKING
    private static readonly System.Diagnostics.Stopwatch _perfStopwatch = new System.Diagnostics.Stopwatch();
    private static long _totalBridgeCreationTime = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetStats()
    {
        TotalBridgeRequests = 0;
        BridgesActuallyCreated = 0;
        PoolHits = 0;
        DirectAccessHits = 0;
        _totalBridgeCreationTime = 0;

        _bridgePool.Clear();
        _bridgeTypeCache.Clear();
        _directBridgeFactories.Clear();
        _weakBridgePool.Clear();

        InitializeDirectBridgeFactories();
    }

    static ConnectorBridgeFactory()
    {
        InitializeDirectBridgeFactories();
    }

    // 🚀 DIRECT BRIDGE FACTORIES: Pre-compiled for maximum performance
    private static void InitializeDirectBridgeFactories()
    {
        // int
        _directBridgeFactories[typeof(int)] = value =>
            new FastConnectorBridge<int>(value,
                (v, x) => { if (v is ConnectorValueInt cv) cv.SetValue(x); },
                v => (v is ConnectorValueInt cv) ? cv.GetValue() : default(int)
            );

        // float
        _directBridgeFactories[typeof(float)] = value =>
            new FastConnectorBridge<float>(value,
                (v, x) => { if (v is ConnectorValueFloat cv) cv.SetValue(x); },
                v => (v is ConnectorValueFloat cv) ? cv.GetValue() : default(float)
            );

        // bool
        _directBridgeFactories[typeof(bool)] = value =>
            new FastConnectorBridge<bool>(value,
                (v, x) => { if (v is ConnectorValueBool cv) cv.SetValue(x); },
                v => (v is ConnectorValueBool cv) ? cv.GetValue() : default(bool)
            );

        // string
        _directBridgeFactories[typeof(string)] = value =>
            new FastConnectorBridge<string>(value,
                (v, x) => { if (v is ConnectorValueString cv) cv.SetValue(x); },
                v => (v is ConnectorValueString cv) ? cv.GetValue() : string.Empty
            );

        // object
        _directBridgeFactories[typeof(object)] = value =>
            new FastConnectorBridge<object>(value,
                (v, x) => { if (v is ConnectorValueObject cv) cv.SetValue(x); },
                v => (v is ConnectorValueObject cv) ? cv.GetValue() : null
            );

        // void
        _directBridgeFactories[typeof(IExecutableConnector)] = value =>
            new ExecutableConnectorBridge((IExecutableConnector)value);

    }

    // 🚀 MAIN METHOD: Ultra-optimized bridge creation
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IConnectorValueBridge CreateBridge(IConnectorValue value)
    {
        if (value == null) return null;

        TotalBridgeRequests++;
        _perfStopwatch.Restart();

        try
        {
            // 🚀 STEP 1: Check instance pool (fastest path)
            if (_bridgePool.TryGetValue(value, out var pooledBridge))
            {
                PoolHits++;
                return pooledBridge;
            }

            // 🚀 STEP 2: Check weak reference pool
            var weakPooled = GetFromWeakPool(value);
            if (weakPooled != null)
            {
                _bridgePool[value] = weakPooled;
                return weakPooled;
            }

            // 🚀 STEP 3: NEW! Check for executable interface FIRST
            // This ensures actions are handled by their specialized bridge.
            if (value is IExecutableConnector executable)
            {
                DirectAccessHits++; // This is a direct access path
                var br = new ExecutableConnectorBridge(executable);
                _bridgePool[value] = br;
                BridgesActuallyCreated++;
                return br;
            }

            // If not an executable, get the inner value's type for standard processing
            var innerValue = value.GetInnerValue();
            var innerType = innerValue?.GetType() ?? typeof(object);

            // 🚀 STEP 4: Try direct access for other common types
            if (_directBridgeFactories.TryGetValue(innerType, out var directFactory))
            {
                DirectAccessHits++;
                var br = directFactory(value);
                _bridgePool[value] = br;
                BridgesActuallyCreated++;
                return br;
            }

            // 🚀 STEP 5: Generic bridge creation with caching
            var bridgeType = GetOrCreateBridgeType(innerType);
            var bridge = (IConnectorValueBridge)Activator.CreateInstance(bridgeType, value);

            _bridgePool[value] = bridge;
            BridgesActuallyCreated++;

            return bridge;
        }
        finally
        {
            _perfStopwatch.Stop();
            _totalBridgeCreationTime += _perfStopwatch.ElapsedMilliseconds;

            // 🚀 Periodic cleanup
            if (Time.frameCount - _lastCleanupFrame > CLEANUP_FRAME_INTERVAL)
            {
                CleanupWeakReferences();
                _lastCleanupFrame = Time.frameCount;
            }
        }
    }
    // 🚀 THREAD-SAFE BRIDGE TYPE CREATION
    private static Type GetOrCreateBridgeType(Type innerType)
    {
        if (_bridgeTypeCache.TryGetValue(innerType, out var bridgeType))
            return bridgeType;

        lock (_typeCacheLock)
        {
            if (_bridgeTypeCache.TryGetValue(innerType, out bridgeType))
                return bridgeType;

            bridgeType = typeof(FastConnectorBridge<>).MakeGenericType(innerType);
            _bridgeTypeCache[innerType] = bridgeType;
            return bridgeType;
        }
    }

    // 🚀 WEAK REFERENCE POOL MANAGEMENT
    private static IConnectorValueBridge GetFromWeakPool(IConnectorValue value)
    {
        var toRemove = new List<WeakReference>();
        IConnectorValueBridge result = null;

        foreach (var kvp in _weakBridgePool)
        {
            if (kvp.Key.IsAlive)
            {
                if (kvp.Key.Target == value)
                {
                    result = kvp.Value;
                    toRemove.Add(kvp.Key);
                    break;
                }
            }
            else
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var deadRef in toRemove)
            _weakBridgePool.Remove(deadRef);

        return result;
    }

    private static void CleanupWeakReferences()
    {
        var deadRefs = new List<WeakReference>();

        foreach (var kvp in _weakBridgePool)
        {
            if (!kvp.Key.IsAlive)
                deadRefs.Add(kvp.Key);
        }

        foreach (var deadRef in deadRefs)
            _weakBridgePool.Remove(deadRef);
    }

    // 🚀 MEMORY MANAGEMENT
    public static void RemoveBridge(IConnectorValue value)
    {
        if (value != null && _bridgePool.TryGetValue(value, out var bridge))
        {
            _bridgePool.Remove(value);
            _weakBridgePool[new WeakReference(value)] = bridge;
        }
    }

    public static void ClearBridgesForNode(NodeBase node)
    {
        if (node == null) return;

        var keysToRemove = new List<IConnectorValue>();
        foreach (var kvp in _bridgePool)
        {
            if (IsValueFromNode(kvp.Key, node))
                keysToRemove.Add(kvp.Key);
        }

        foreach (var key in keysToRemove)
            RemoveBridge(key);
    }

    private static bool IsValueFromNode(IConnectorValue value, NodeBase node)
    {
        return false; // Implement based on your architecture
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Node System/Show Bridge Statistics")]
    public static void LogBridgeStatistics()
    {
        Debug.Log($"=== BRIDGE STATISTICS ===");
        Debug.Log($"Total Bridge Requests: {TotalBridgeRequests}");
        Debug.Log($"Bridges Actually Created: {BridgesActuallyCreated}");
        Debug.Log($"Pool Hits: {PoolHits}");
        Debug.Log($"Direct Access Hits: {DirectAccessHits}");
        Debug.Log($"Bridges in Pool: {_bridgePool.Count}");
        Debug.Log($"Weak References: {_weakBridgePool.Count}");
        Debug.Log($"Cached Bridge Types: {_bridgeTypeCache.Count}");

        if (TotalBridgeRequests > 0)
        {
            float poolEfficiency = (float)PoolHits / TotalBridgeRequests * 100;
            float directAccessRate = (float)DirectAccessHits / TotalBridgeRequests * 100;
            float totalEfficiency = (float)(PoolHits + DirectAccessHits) / TotalBridgeRequests * 100;

            Debug.Log($"Pool Efficiency: {poolEfficiency:0.0}%");
            Debug.Log($"Direct Access Rate: {directAccessRate:0.0}%");
            Debug.Log($"Total Efficiency: {totalEfficiency:0.0}%");

            int savedCreations = TotalBridgeRequests - BridgesActuallyCreated;
            Debug.Log($"Bridges Saved: {savedCreations} ({(float)savedCreations / TotalBridgeRequests * 100:0.0}% reduction)");

            Debug.Log($"Average Creation Time: {(double)_totalBridgeCreationTime / TotalBridgeRequests:0.000}ms");
        }
    }

    [MenuItem("Tools/Node System/Run Comprehensive Bridge Test")]
    public static void RunComprehensiveBridgeTest()
    {
        Debug.Log("🧪 STARTING COMPREHENSIVE BRIDGE TEST...");

        // Save initial state for test isolation
        var initialRequests = TotalBridgeRequests;
        var initialCreations = BridgesActuallyCreated;
        var initialPoolHits = PoolHits;
        var initialDirectHits = DirectAccessHits;

        var totalStopwatch = new System.Diagnostics.Stopwatch();
        totalStopwatch.Start();

        // 🚀 TEST 1: Instance Pooling Validation
        Debug.Log("🔬 Testing Instance Pooling...");
        var value1 = new ConnectorValueInt(1);
        var value2 = new ConnectorValueInt(2);
        var value3 = value1; // Same instance

        var bridge1 = CreateBridge(value1);
        var bridge2 = CreateBridge(value2);
        var bridge3 = CreateBridge(value3);

        bool poolingWorks = bridge1 == bridge3 && bridge1 != bridge2;
        Debug.Log(poolingWorks ? "✅ Instance pooling: PASSED" : "❌ Instance pooling: FAILED");

        // 🚀 TEST 2: Performance & Stress Test
        Debug.Log("🔬 Running Performance & Stress Test...");
        const int PERFORMANCE_ITERATIONS = 50000;
        var random = new System.Random(12345);

        // Create diverse test values
        var testValues = new List<IConnectorValue>();
        for (int i = 0; i < 500; i++)
        {
            switch (i % 6)
            {
                case 0: testValues.Add(new ConnectorValueInt(i)); break;
                case 1: testValues.Add(new ConnectorValueFloat(i * 1.5f)); break;
                case 2: testValues.Add(new ConnectorValueBool(i % 2 == 0)); break;
                case 3: testValues.Add(new ConnectorValueString($"test_{i}")); break;
                case 4: testValues.Add(new ConnectorValueObject(new UnityEngine.Vector3(i, i, i))); break;
                case 5: testValues.Add(new ConnectorValueVoid()); break;
            }
        }

        var performanceStopwatch = new System.Diagnostics.Stopwatch();
        performanceStopwatch.Start();

        // Mixed access pattern: sequential + random
        for (int i = 0; i < PERFORMANCE_ITERATIONS; i++)
        {
            if (i % 3 == 0)
            {
                // Sequential access (tests cache locality)
                CreateBridge(testValues[i % testValues.Count]);
            }
            else
            {
                // Random access (tests cache efficiency)
                int index = random.Next(testValues.Count);
                CreateBridge(testValues[index]);
            }
        }
        performanceStopwatch.Stop();

        // 🚀 TEST 3: Direct Access Validation
        Debug.Log("🔬 Testing Direct Access Optimization...");
        var directAccessValues = new IConnectorValue[]
        {
            new ConnectorValueInt(42),
            new ConnectorValueFloat(3.14f),
            new ConnectorValueBool(true),
            new ConnectorValueString("direct_test"),
            new ConnectorValueVoid(),
            new ConnectorValueObject(null)
        };

        // Access multiple times to ensure direct access is used
        foreach (var value in directAccessValues)
        {
            for (int i = 0; i < 10; i++)
            {
                CreateBridge(value);
            }
        }

        totalStopwatch.Stop();

        // 🎯 COMPREHENSIVE RESULTS
        Debug.Log("📊 === COMPREHENSIVE TEST RESULTS ===");

        // Calculate test-specific metrics
        int testRequests = TotalBridgeRequests - initialRequests;
        int testCreations = BridgesActuallyCreated - initialCreations;
        int testPoolHits = PoolHits - initialPoolHits;
        int testDirectHits = DirectAccessHits - initialDirectHits;

        Debug.Log($"Total Test Time: {totalStopwatch.ElapsedMilliseconds}ms");
        Debug.Log($"Performance Test Time: {performanceStopwatch.ElapsedMilliseconds}ms");
        Debug.Log($"Total Bridge Requests: {testRequests}");
        Debug.Log($"Actual Bridge Creations: {testCreations}");
        Debug.Log($"Pool Hits: {testPoolHits}");
        Debug.Log($"Direct Access Hits: {testDirectHits}");
        Debug.Log($"Bridges per second: {(float)testRequests / totalStopwatch.Elapsed.TotalSeconds:0}");

        // Efficiency Calculations
        if (testRequests > 0)
        {
            float poolEfficiency = (float)testPoolHits / testRequests * 100;
            float directAccessRate = (float)testDirectHits / testRequests * 100;
            float totalEfficiency = (float)(testPoolHits + testDirectHits) / testRequests * 100;
            int savedCreations = testRequests - testCreations;

            Debug.Log($"Pool Efficiency: {poolEfficiency:0.0}%");
            Debug.Log($"Direct Access Rate: {directAccessRate:0.0}%");
            Debug.Log($"Total Efficiency: {totalEfficiency:0.0}%");
            Debug.Log($"Bridges Saved: {savedCreations} ({(float)savedCreations / testRequests * 100:0.0}% reduction)");
        }

        // 🚀 Memory Analysis
        Debug.Log("💾 === MEMORY ANALYSIS ===");
        Debug.Log($"Current Pool Size: {_bridgePool.Count}");
        Debug.Log($"Weak Reference Pool Size: {_weakBridgePool.Count}");
        Debug.Log($"Type Cache Size: {_bridgeTypeCache.Count}");
        Debug.Log($"GC Memory: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB");

        // 🎯 Performance Grading
        float avgTimePerBridge = (float)totalStopwatch.ElapsedMilliseconds / testRequests;
        Debug.Log($"Average Time Per Bridge: {avgTimePerBridge:0.000}ms");

        if (avgTimePerBridge < 0.001f)
            Debug.Log("🎉 PERFORMANCE: EXCELLENT (Sub-millisecond performance)");
        else if (avgTimePerBridge < 0.01f)
            Debug.Log("✅ PERFORMANCE: GOOD (Fast bridge creation)");
        else if (avgTimePerBridge < 0.1f)
            Debug.Log("⚠️ PERFORMANCE: ACCEPTABLE (Moderate speed)");
        else
            Debug.Log("🚨 PERFORMANCE: POOR (Needs optimization)");

        // ✅ Validation Checks
        Debug.Log("✅ === VALIDATION CHECKS ===");
        Debug.Log(poolingWorks ? "✅ Instance Pooling: PASSED" : "❌ Instance Pooling: FAILED");
        Debug.Log(testDirectHits > 0 ? "✅ Direct Access: PASSED" : "❌ Direct Access: FAILED");
        Debug.Log(_bridgeTypeCache.Count > 0 ? "✅ Type Caching: PASSED" : "❌ Type Caching: FAILED");
        Debug.Log(testPoolHits > 0 ? "✅ Pooling Efficiency: PASSED" : "❌ Pooling Efficiency: FAILED");

        // Show cached types for debugging
        Debug.Log("🔍 Cached Bridge Types:");
        foreach (var kvp in _bridgeTypeCache.Take(10)) // Show first 10 to avoid spam
        {
            Debug.Log($"  - {kvp.Key.Name}");
        }
        if (_bridgeTypeCache.Count > 10)
            Debug.Log($"  ... and {_bridgeTypeCache.Count - 10} more types");

        // Final cleanup
        System.GC.Collect();
        Debug.Log($"Memory After GC: {System.GC.GetTotalMemory(true) / 1024 / 1024}MB");

        Debug.Log("🧪 COMPREHENSIVE BRIDGE TEST COMPLETE!");
    }

    [MenuItem("Tools/Node System/Clear Bridge Cache")]
    public static void ClearCache()
    {
        _bridgePool.Clear();
        _weakBridgePool.Clear();
        Debug.Log("🧹 Bridge cache cleared!");
        LogBridgeStatistics();
    }
#endif
}