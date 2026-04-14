using NodeEngine.GraphPersistence;
using System.Linq;
using UniMediator.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class NodeEngineLifetimeScope : LifetimeScope
{
    [SerializeField] private NodeRunner _nodeRunner;
    [SerializeField] private NodeSpawnerService _nodeSpawnerService;
    [SerializeField] private GraphSaveLoadCoordinator _graphCoordinator;
    [SerializeField] private SaveLoadUI _saveLoadUI;
    [SerializeField] private NodesList _nodesList;
    [SerializeField] private LineRenderersController _lineRenderersController;
    [SerializeField] private UIZoomPan _UIZoomPan;
    [SerializeField] private BoxSelectionLogic _boxSelectionLogic;
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private ConnectionVisualsHandler _connectionVisualsHandler;
    [SerializeField] private VariableUIRegistry _variableUIRegistry;
    [SerializeField] private NodeKeyboardShortcuts _nodeKeyboardShortcuts;
    [SerializeField] private ContextMenuSystem _contextMenuSystem;
    [SerializeField] private NodesDatabase _nodesDatabase;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_nodesDatabase);
        builder.RegisterInstance(_variableUIRegistry);

        builder.RegisterComponent(_mainCanvas);

        builder.RegisterMediator();

        builder.Register<SelectionService>(Lifetime.Singleton);
        builder.Register<CanvasService>(Lifetime.Singleton);
        builder.Register<IGraphStorage>(resolver =>
            new LocalGraphStorage(Application.persistentDataPath + "/NodeGraphs/", ".json"),
            Lifetime.Singleton);

        var persisterTypes = typeof(IValuePersister).Assembly.GetTypes()
            .Where(t => typeof(IValuePersister).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        foreach (var type in persisterTypes)
        {
            builder.Register(type, Lifetime.Singleton).As<IValuePersister>();
        }

        builder.Register<PersistenceService>(Lifetime.Singleton);

        builder.Register<GraphSaveService>(Lifetime.Singleton);
        builder.Register<GraphLoadService>(Lifetime.Singleton);
        builder.Register<GraphSnapshotBuilder>(Lifetime.Singleton);
        builder.Register<GraphRestorer>(Lifetime.Singleton);
        builder.Register<GraphSerializer>(Lifetime.Singleton);

        builder.Register<TypeChangeService>(Lifetime.Singleton);
        builder.Register<INodeFactory, NodeFactory>(Lifetime.Singleton);
        builder.Register<ConnectionService>(Lifetime.Singleton);

        RegisterAllNodeTypes(builder);

        builder.RegisterComponent(_nodeSpawnerService);
        builder.RegisterComponent(_graphCoordinator);
        builder.RegisterComponent(_saveLoadUI);
        builder.RegisterComponent(_nodesList);
        builder.RegisterComponent(_UIZoomPan);
        builder.RegisterComponent(_boxSelectionLogic);
        builder.RegisterComponent(_nodeKeyboardShortcuts);
        builder.RegisterComponent(_contextMenuSystem);

        builder.RegisterComponent(_nodeRunner).AsImplementedInterfaces();
        builder.RegisterComponent(_connectionVisualsHandler).AsImplementedInterfaces();
        builder.RegisterComponent(_lineRenderersController).AsImplementedInterfaces();
    }

    private void RegisterAllNodeTypes(IContainerBuilder builder)
    {
        var nodeTypes = System.Reflection.Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(BaseNode)) && !t.IsAbstract);

        foreach (var type in nodeTypes)
        {
            builder.Register(type, Lifetime.Transient);
        }
    }
}