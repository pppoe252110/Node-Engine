using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public static class ConnectorBridgeFactory
{
    
    public static int TotalBridgeRequests { get; private set; }
    public static int BridgesActuallyCreated { get; private set; }
    public static int PoolHits { get; private set; }
    public static int DirectAccessHits { get; private set; }

    
    private static readonly Dictionary<IConnectorValue, IConnectorValueBridge> _bridgePool =
        new Dictionary<IConnectorValue, IConnectorValueBridge>();

    
    private static readonly Dictionary<Type, Type> _bridgeTypeCache = new Dictionary<Type, Type>();
    private static readonly object _typeCacheLock = new object();

    
    private static readonly HashSet<Type> _commonTypes = new HashSet<Type>
    {
        typeof(int), typeof(float), typeof(bool), typeof(string),
        typeof(Vector3), typeof(GameObject), typeof(void), typeof(object)
    };

    
    private static readonly Dictionary<Type, Func<IConnectorValue, IConnectorValueBridge>> _directBridgeFactories =
        new Dictionary<Type, Func<IConnectorValue, IConnectorValueBridge>>();

    
    private static readonly Dictionary<WeakReference, IConnectorValueBridge> _weakBridgePool =
        new Dictionary<WeakReference, IConnectorValueBridge>();
    private static int _lastCleanupFrame = 0;
    private const int CLEANUP_FRAME_INTERVAL = 60;

    
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

    
    private static void InitializeDirectBridgeFactories()
    {
        
        _directBridgeFactories[typeof(int)] = value =>
            new FastConnectorBridge<int>(value,
                (v, x) => { if (v is ConnectorValueInt cv) cv.SetValue(x); },
                v => (v is ConnectorValueInt cv) ? cv.GetValue() : default(int)
            );

        
        _directBridgeFactories[typeof(float)] = value =>
            new FastConnectorBridge<float>(value,
                (v, x) => { if (v is ConnectorValueFloat cv) cv.SetValue(x); },
                v => (v is ConnectorValueFloat cv) ? cv.GetValue() : default(float)
            );

        
        _directBridgeFactories[typeof(bool)] = value =>
            new FastConnectorBridge<bool>(value,
                (v, x) => { if (v is ConnectorValueBool cv) cv.SetValue(x); },
                v => (v is ConnectorValueBool cv) ? cv.GetValue() : default(bool)
            );

        
        _directBridgeFactories[typeof(string)] = value =>
            new FastConnectorBridge<string>(value,
                (v, x) => { if (v is ConnectorValueString cv) cv.SetValue(x); },
                v => (v is ConnectorValueString cv) ? cv.GetValue() : string.Empty
            );

        
        _directBridgeFactories[typeof(object)] = value =>
            new FastConnectorBridge<object>(value,
                (v, x) => { if (v is ConnectorValueObject cv) cv.SetValue(x); },
                v => (v is ConnectorValueObject cv) ? cv.GetValue() : null
            );

        
        _directBridgeFactories[typeof(IExecutableConnector)] = value =>
            new ExecutableConnectorBridge((IExecutableConnector)value);

    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IConnectorValueBridge CreateBridge(IConnectorValue value)
    {
        if (value == null) return null;

        TotalBridgeRequests++;
        _perfStopwatch.Restart();

        try
        {
            
            if (_bridgePool.TryGetValue(value, out var pooledBridge))
            {
                PoolHits++;
                return pooledBridge;
            }

            
            var weakPooled = GetFromWeakPool(value);
            if (weakPooled != null)
            {
                _bridgePool[value] = weakPooled;
                return weakPooled;
            }

            
            
            if (value is IExecutableConnector executable)
            {
                DirectAccessHits++; 
                var br = new ExecutableConnectorBridge(executable);
                _bridgePool[value] = br;
                BridgesActuallyCreated++;
                return br;
            }

            
            var innerValue = value.GetInnerValue();
            var innerType = innerValue?.GetType() ?? typeof(object);

            
            if (_directBridgeFactories.TryGetValue(innerType, out var directFactory))
            {
                DirectAccessHits++;
                var br = directFactory(value);
                _bridgePool[value] = br;
                BridgesActuallyCreated++;
                return br;
            }

            
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

            
            if (Time.frameCount - _lastCleanupFrame > CLEANUP_FRAME_INTERVAL)
            {
                CleanupWeakReferences();
                _lastCleanupFrame = Time.frameCount;
            }
        }
    }
    
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
        return false; 
    }
}