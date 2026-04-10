using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using UniMediator.Runtime.VContainer;

public class NodeEngineLifetimeScope : LifetimeScope
{
    [SerializeField] private ConnectionManager _connectionManager;
    [SerializeField] private NodeRunner _nodeRunner;
    [SerializeField] private NodeSpawnerService _nodeSpawnerService;
    [SerializeField] private GraphSaveLoadCoordinator _graphCoordinator;
    [SerializeField] private SaveLoadUI _saveLoadUI;
    [SerializeField] private NodesList _nodesList;
    [SerializeField] private LineRenderersController _lineRenderersController;
    [SerializeField] private UIZoomPan _UIZoomPan;
    [SerializeField] private Canvas _mainCanvas;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(_mainCanvas);

        // Register Mediator, ignoring components already registered manually
        builder.RegisterMediator();

        builder.Register<CanvasService>(Lifetime.Singleton);
        builder.Register<IGraphStorage>(resolver =>
            new LocalGraphStorage(Application.persistentDataPath + "/NodeGraphs/", ".json"),
            Lifetime.Singleton);

        builder.Register<GraphSerializer>(Lifetime.Singleton);
        builder.Register<TypeChangeService>(Lifetime.Singleton);
        builder.Register<INodeFactory, NodeFactory>(Lifetime.Singleton);

        RegisterAllNodeTypes(builder);

        builder.RegisterComponent(_connectionManager);
        builder.RegisterComponent(_nodeRunner).AsImplementedInterfaces();
        builder.RegisterComponent(_nodeSpawnerService);
        builder.RegisterComponent(_graphCoordinator);
        builder.RegisterComponent(_saveLoadUI);
        builder.RegisterComponent(_nodesList);
        builder.RegisterComponent(_lineRenderersController).AsImplementedInterfaces();
        builder.RegisterComponent(_UIZoomPan);
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