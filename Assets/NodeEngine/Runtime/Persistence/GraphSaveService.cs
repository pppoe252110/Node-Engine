using System;
using UnityEngine;
using VContainer;

namespace NodeEngine.GraphPersistence
{
    public class GraphSaveService
    {
        private readonly IGraphStorage _storage;
        private readonly GraphSerializer _serializer;

        [Inject]
        public GraphSaveService(IGraphStorage storage, GraphSerializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
        }

        public void Save(string saveName, GraphSnapshot snapshot, bool prettyPrint = true)
        {
            if (string.IsNullOrEmpty(saveName))
                throw new ArgumentException("Save name cannot be empty", nameof(saveName));
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            try
            {
                string json = _serializer.SerializeToJson(snapshot, prettyPrint);
                _storage.Save(saveName, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GraphSaveService] Failed to save '{saveName}': {ex.Message}");
                throw;
            }
        }
    }
}