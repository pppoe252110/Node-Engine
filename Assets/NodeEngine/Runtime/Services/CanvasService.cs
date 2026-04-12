using UnityEngine;
using VContainer;

/// <summary>
/// Provides canvas references for node positioning.
/// </summary>
public class CanvasService
{
    public Canvas NodesCanvas { get; }
    public Vector2 CanvasSize { get; set; }

    [Inject]
    public CanvasService(Canvas nodesCanvas)
    {
        NodesCanvas = nodesCanvas;
    }
}