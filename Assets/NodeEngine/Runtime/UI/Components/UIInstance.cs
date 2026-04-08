using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private Canvas _nodesCanvas;
    [SerializeField] private float _nodesCanvasSize = 1f;

    public static Vector2 GetMousePosition(Vector2 vec)
    {
        Vector2 referenceResolution = NodesCanvas.GetComponent<CanvasScaler>().referenceResolution;
        Vector2 currentResolution = new Vector2(Screen.width, Screen.height);

        float widthRatio = currentResolution.x / referenceResolution.x;
        float heightRatio = currentResolution.y / referenceResolution.y;

        float ratio = Mathf.Lerp(heightRatio, widthRatio, NodesCanvas.GetComponent<CanvasScaler>().matchWidthOrHeight);

        return vec / ratio;
    }

    private static UIInstance instance
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<UIInstance>();
            return _instance;
        }
    }
    private static UIInstance _instance;

    public static void SetNodesCanvasSize(float size)
    {
        instance._nodesCanvasSize = size;
    }
}
