using UnityEngine;
using VContainer;

/// <summary>
/// Provides canvas references for node positioning.
/// Registered as a singleton in the DI container.
/// </summary>
public class CanvasService
{
    public Canvas NodesCanvas { get; }
    public Vector2 CanvasSize { get; set; }

    [Inject]
    public CanvasService(Canvas nodesCanvas)
    {
        NodesCanvas = nodesCanvas;
        CanvasSize = ((RectTransform)nodesCanvas.transform).sizeDelta;
    }
}