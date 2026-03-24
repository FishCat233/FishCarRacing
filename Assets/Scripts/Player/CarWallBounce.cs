using UnityEngine;

namespace FishCarRacing.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class CarWallBounce : MonoBehaviour
    {
        [Header("筛选")]
        [Tooltip("只对这些 Tag 的碰撞体生效。为空表示不按 Tag 过滤。")]
        [SerializeField] private string[] wallTags = { "Guardrail", "Wall" };

        [Tooltip("只对这些 Layer 生效。为 Everything 表示不按 Layer 过滤。")]
        [SerializeField] private LayerMask wallLayers = ~0;

        [Header("触发条件")]
        [Tooltip("当车速（km/h）大于此值才启用反弹，避免低速轻碰也弹飞。")]
        [SerializeField] private float minSpeedKmhToBounce = 15f;

        [Tooltip("只有当速度朝向墙面（法线反方向）时才反弹。这个阈值越大越严格。")]
        [SerializeField] private float minIntoWallSpeed = 0.5f;

        [Header("反弹参数")]
        [Tooltip("反弹系数：1=理想弹性反射，0=只消除法线分量（不会反弹回来）。")]
        [Range(0f, 1.2f)]
        [SerializeField] private float bounciness = 0.35f;

        [Tooltip("额外施加的法线冲量（m/s 的速度变化量），用于把车顶开防卡死。")]
        [SerializeField] private float extraNormalSpeedChange = 2.0f;

        [Tooltip("最大法线速度变化量，避免超高速时弹得过猛。")]
        [SerializeField] private float maxNormalSpeedChange = 12f;

        [Header("沿墙处理")]
        [Tooltip("沿墙切向速度阻尼（每次碰撞时乘以该系数），1=不处理，0.9=轻微减小。")]
        [Range(0.5f, 1f)]
        [SerializeField] private float tangentDamping = 0.95f;

        [Tooltip("碰撞后把车稍微推出去的距离，减少下一帧继续穿插导致抖动。")]
        [SerializeField] private float depenetrationDistance = 0.03f;

        [Header("引用")]
        [SerializeField] private Rigidbody rb;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision) => TryBounce(collision);
        private void OnCollisionStay(Collision collision) => TryBounce(collision);

        private void TryBounce(Collision collision)
        {
            if (rb == null) return;
            if (!IsWall(collision.collider)) return;

            // 速度门槛
            Vector3 v = rb.velocity;
            Vector3 vPlanar = Vector3.ProjectOnPlane(v, Vector3.up);
            float speedKmh = vPlanar.magnitude * 3.6f;
            if (speedKmh < minSpeedKmhToBounce) return;

            // 取最有代表性的接触法线：选择“最朝向车速度反方向”的那个
            Vector3 bestNormal = Vector3.zero;
            float bestScore = float.NegativeInfinity;
            foreach (var c in collision.contacts)
            {
                // score 越大表示 normal 越“迎面”
                float score = Vector3.Dot(c.normal, -vPlanar.normalized);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestNormal = c.normal;
                }
            }
            if (bestNormal == Vector3.zero) return;

            // 是否真的“撞进墙”
            float intoWallSpeed = Vector3.Dot(v, -bestNormal);
            if (intoWallSpeed < minIntoWallSpeed) return;

            // 分解速度
            Vector3 vn = Vector3.Project(v, bestNormal);           // 法线分量（指向 bestNormal）
            Vector3 vt = v - vn;                                        // 切向分量

            // 如果 vn 本身就在远离墙（跟 normal 同向），不处理
            if (Vector3.Dot(vn, bestNormal) > 0f) return;

            // 反射法线分量：vn' = -vn * bounciness
            Vector3 newVn = -vn * Mathf.Max(0f, bounciness);
            Vector3 newVt = vt * tangentDamping;

            Vector3 targetV = newVt + newVn;

            // 额外推出去：沿法线给一个 speed change
            float extra = Mathf.Clamp(extraNormalSpeedChange, 0f, maxNormalSpeedChange);
            targetV += bestNormal * extra;

            // 限制一次改变量，避免“弹飞”
            Vector3 dv = targetV - v;
            float maxDv = maxNormalSpeedChange;
            if (dv.magnitude > maxDv) dv = dv.normalized * maxDv;

            rb.AddForce(dv, ForceMode.VelocityChange);

            // 轻微推出去，降低下一帧持续穿插
            if (depenetrationDistance > 0f)
            {
                rb.position += bestNormal * depenetrationDistance;
            }
        }

        private bool IsWall(Collider col)
        {
            if (((1 << col.gameObject.layer) & wallLayers) == 0) return false;

            if (wallTags == null || wallTags.Length == 0) return true;

            for (int i = 0; i < wallTags.Length; i++)
            {
                var t = wallTags[i];
                if (!string.IsNullOrEmpty(t) && col.CompareTag(t)) return true;
            }

            return false;
        }
    }
}

