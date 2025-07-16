using UnityEngine;

public class ForceFieldObj : MonoBehaviour
{
    // 鼠标位置
    private Vector3 center;
    // 引力位置
    private Vector3 gravitationPos;
    [SerializeField, Header("力场半径")] private float forceRadius;
    [SerializeField, Header("斥力")] private float repulsion;
    [SerializeField, Header("吸引力")] private float targetAttractive;
    [SerializeField, Header("吸引力缩放")] private float speed = 1f;
    // 用于坐标系转换的相机
    private Camera cam;
    [SerializeField, Header("应用缩放效果")] private bool useScale;

    void Start()
    {
        gravitationPos = transform.position;
        cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("Main Camera not found!");
            }
    }

    void Update()
    {
        if (forceRadius == 0) return;
        var inputMouse = Input.mousePosition;
        center = new Vector3(inputMouse.x, inputMouse.y, 90);
        center = cam.ScreenToWorldPoint(center);
        center = new Vector3(center.x, center.y, 90);

        var velocity = Vector3.zero;
        var len = (transform.position - center).magnitude; // 计算距离
                                                           // 如果距离小于范围，就是处于力场之中
        Debug.Log(len);
        Debug.Log(forceRadius);

        if (len < forceRadius)
        {
            var rate = (forceRadius - len) / forceRadius;
            var intensity = repulsion * rate;
            velocity += (transform.position - center) * intensity * Time.deltaTime; // 计算斥力
        }

        velocity += (gravitationPos - transform.position) * Time.deltaTime * targetAttractive * speed; // 计算引力

        transform.position += velocity;

        //计算大小，跟随物体与引力中心离的越近越小
        transform.localScale = useScale ? Mathf.Clamp01((this.transform.position - center).magnitude / forceRadius) * Vector3.one : transform.localScale;
    }
}
