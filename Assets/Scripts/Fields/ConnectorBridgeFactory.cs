// Fields/ConnectorBridgeFactory.cs
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class ConnectorBridgeFactory
{
    // ✅ ADD: Bridge tracking and diagnostics
    public static int TotalBridgesCreated { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetStats()
    {
        TotalBridgesCreated = 0;
    }

    public static IConnectorValueBridge CreateBridge(IConnectorValue value)
    {
        if (value == null) return null;

        var valueType = value.GetType();

        try
        {
            // Always create a new bridge for the value instance
            // The FastConnectorBridge will use static compiled setters and getters for the type
            var innerValue = value.GetInnerValue();
            var innerType = innerValue?.GetType() ?? typeof(object);

            var bridgeType = typeof(FastConnectorBridge<>).MakeGenericType(innerType);
            var bridge = (IConnectorValueBridge)Activator.CreateInstance(bridgeType, value);
            TotalBridgesCreated++;
            return bridge;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Fast bridge failed, falling back to self-bridge: {ex.Message}");

            if (value is IConnectorValueBridge selfBridge)
            {
                return selfBridge;
            }
            else
            {
                Debug.LogError($"No bridge available for {valueType.Name}");
                return null;
            }
        }
    }

#if UNITY_EDITOR
    // ✅ ADD: Diagnostic method to show bridge statistics
    [MenuItem("Tools/Node System/Show Bridge Statistics")]
    public static void LogBridgeStatistics()
    {
        Debug.Log($"=== BRIDGE STATISTICS ===");
        Debug.Log($"Total Bridges Created: {TotalBridgesCreated}");
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

        // Test non-generic performance (with boxing)
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            bridge.SetValueFast(i);
            var val = bridge.GetValueFast();
        }
        stopwatch.Stop();
        Debug.Log($"Non-generic bridge performance: {stopwatch.ElapsedMilliseconds}ms");

        // Test generic performance (without boxing)
        var fastBridge = bridge as FastConnectorBridge<int>;
        if (fastBridge != null)
        {
            stopwatch.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                fastBridge.SetValue(i);
                var val = fastBridge.GetValue();
            }
            stopwatch.Stop();
            Debug.Log($"Generic bridge performance: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
#endif
}