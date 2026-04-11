using System;
using UnityEngine;
using VContainer;

namespace NodeEngine.GraphPersistence
{
    public class GraphLoadService
    {
        private readonly IGraphStorage _storage;
        private readonly GraphSerializer _serializer;

        [Inject]
        public GraphLoadService(IGraphStorage storage, GraphSerializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
        }

        public GraphSnapshot Load(string saveName)
        {
            if (!_storage.Exists(saveName))
                throw new InvalidOperationException($"Save file '{saveName}' does not exist.");

            try
            {
                string json = _storage.Load(saveName);
                return _serializer.DeserializeFromJson(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GraphLoadService] Failed to load '{saveName}': {ex.Message}");
                throw;
            }
        }

        public bool Exists(string saveName) => _storage.Exists(saveName);
    }
}