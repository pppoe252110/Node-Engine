using Radishmouse;
using System;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Testing : MonoBehaviour
{
    //[SerializeField] private Vector2 referenceResolution = new(1080, 1920);
    //[Range(0f, 1f)]
    //[SerializeField] private float _match = 1f;
    //[SerializeField] private RectTransform _target;
    //[Range(0f, 1f)]
    //public float value = 0f;
    public RectTransform a;
    public RectTransform b;
    public UILineRenderer lineRenderer;
    [Range(0f, 1f)]
    public float h = 0.5f;
    //public RectTransform c;
    //public RectTransform d;

    //private void Start()
    //{
    //    string path = EditorUtility.OpenFilePanel("Select DLL", "", "dll");

    //    System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

    //    var assembly = Assembly.LoadFile(path);
    //    var types = assembly.GetTypes();

    //    foreach (var type in types)
    //    {
    //        if (type.IsClass)
    //        {
    //            var i = Activator.CreateInstance(type);

    //            Debug.Log(i);

    //            var methods = type.GetMethods().Where(m => m.DeclaringType != typeof(object)).ToList();
    //            foreach (var method in methods)
    //            {
    //                Debug.Log(method.Name);
    //            }

    //            var properties = type.GetFields();
    //            foreach (var property in properties)
    //            {
    //                Debug.Log(property.Name);
    //            }
    //        }
    //    }

    //    sw.Stop();
    //    Debug.Log(sw.ElapsedMilliseconds);
    //}

    private void Update()
    {

        //var targetAspect = (Mathf.Lerp(referenceResolution.y, referenceResolution.x, _match) / Mathf.Lerp(referenceResolution.x, referenceResolution.y, _match))
        //    / ((float)Mathf.Lerp(Screen.height, Screen.width, _match) / (float)Mathf.Lerp(Screen.width, Screen.height, _match));
        //_target.localScale = Vector3.one * targetAspect;
        //Debug.Log(targetAspect);
    }

    Vector2 Bezier(Vector2 a, Vector2 b, float t)
    {
        return Vector2.Lerp(a, b, t);
    }

    Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        return Vector2.Lerp(Bezier(a, b, t), Bezier(b, c, t), t);
    }

    Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        return Vector2.Lerp(Bezier(a, b, c, t), Bezier(b, c, d, t), t);
    }
    //private void OnValidate()
    //{
    //    Vector2 ab = Vector2.Lerp(a.anchoredPosition, b.anchoredPosition, value);
    //    Vector2 bc = Vector2.Lerp(b.anchoredPosition, c.anchoredPosition, value);
    //    Vector2 cd = Vector2.Lerp(c.anchoredPosition, d.anchoredPosition, value);

    //    Vector2 abc = Vector2.Lerp(ab, bc, value);
    //    Vector2 bcd = Vector2.Lerp(bc, cd, value);
    //    Vector2 abcd = Vector2.Lerp(abc, bcd, value);

    //    (transform as RectTransform).anchoredPosition = abcd;        
    //}
}
