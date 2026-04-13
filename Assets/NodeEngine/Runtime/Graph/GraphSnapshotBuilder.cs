using System.Linq;
using VContainer;

namespace NodeEngine.GraphPersistence
{
    public class GraphSnapshotBuilder
    {
        private readonly GraphSerializer _serializer;
        private readonly NodeSpawnerService _nodeSpawner;
        private readonly ConnectionService _connectionService;

        [Inject]
        public GraphSnapshotBuilder(
            GraphSerializer serializer,
            NodeSpawnerService nodeSpawner,
            ConnectionService connectionService)
        {
            _serializer = serializer;
            _nodeSpawner = nodeSpawner;
            _connectionService = connectionService;
        }

        public GraphSnapshot BuildSnapshot()
        {
            var nodeLogics = _nodeSpawner.GetAllNodes().ToList();
            return _serializer.BuildSnapshot(
                nodeLogics,
                _connectionService.ActiveDataConnections,
                _connectionService.ActiveFlowConnections);
        }
    }
}