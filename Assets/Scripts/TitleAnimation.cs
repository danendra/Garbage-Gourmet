// using UnityEngine;

// public class TitleBounce : MonoBehaviour
// {
//     [SerializeField] public float scaleAmount = 0.05f;
//     [SerializeField] public float rotateAmount = 2f;
//     [SerializeField] public float speed = 2f;

//     Vector3 baseScale;
//     Quaternion baseRotation;

//     void Start()
//     {
//         baseScale = transform.localScale;
//         baseRotation = transform.localRotation;
//     }

//     void Update()
//     {
//         float t = Time.time * speed;

//         float scale = 1f + Mathf.Sin(t) * scaleAmount;
//         float rot = Mathf.Sin(t * 0.9f) * rotateAmount;

//         transform.localScale = baseScale * scale;
//         transform.localRotation = baseRotation * Quaternion.Euler(0, 0, rot);
//     }
// }