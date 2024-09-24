using UnityEngine;

public class UIInstance : MonoBehaviour
{
    public static Canvas NodesCanvas
    {
        get
        {
            return instance._nodesCanvas;
        }
    }
    public static float NodesCanvasSize
    {
        get
        {
            return instance._nodesCanvasSize;
        }
    }

    [SerializeField]private Canvas _nodesCanvas;
    [SerializeField]private float _nodesCanvasSize = 1f;


    private static UIInstance instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<UIInstance>();
            return _instance;
        }
    }
    private static UIInstance _instance;

    public static void SetNodesCanvasSize(float size)
    {
        instance._nodesCanvasSize = size;
    }
}
