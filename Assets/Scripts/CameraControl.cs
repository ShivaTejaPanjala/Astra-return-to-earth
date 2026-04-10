using UnityEngine;

namespace Plane.Gameplay
{
    public class CameraControl : MonoBehaviour
    {
        private static CameraControl m_Current;
        public static CameraControl Current => m_Current;

        [Header("Player Reference")]
        [SerializeField] private Transform playerShip;
        [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -20f);
        [SerializeField] private float followSmooth = 6f;
        [SerializeField] private float lookAhead = 10f;
        [SerializeField] private float forwardTrackingMultiplier = 0.3f;
        [SerializeField] private float tiltAmount = 15f;
        [SerializeField] private float tiltSmooth = 5f;
        [SerializeField] private float baseFOV = 70f;
        [SerializeField] private float maxFOV = 95f;
        [SerializeField] private float fovSpeedFactor = 0.08f;
        [SerializeField] private float shakeRadius = 1f;
        [SerializeField] private float shakeFrequencyX = 30f;
        [SerializeField] private float shakeFrequencyY = 50f;
        [HideInInspector] public bool m_ShakeEnabled = true;

        private float m_ShakeTimer;
        private float m_CurrentShakeRadius;
        private Camera cam;
        private Vector3 lastPlayerPos;
        private Vector3 playerVelocity;

        private void Awake()
        {
            m_Current = this;
            m_CurrentShakeRadius = shakeRadius;
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            if (playerShip == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    playerShip = playerObj.transform;
            }

            if (playerShip != null)
            {
                lastPlayerPos = playerShip.position;
                transform.position = playerShip.position + offset;
                transform.LookAt(playerShip.position + playerShip.forward * lookAhead);
            }

            if (cam != null)
                cam.fieldOfView = baseFOV;
        }

        private void LateUpdate()
        {
            if (playerShip == null)
                return;

            float dt = Time.deltaTime;

            if (dt > 0f)
                playerVelocity = (playerShip.position - lastPlayerPos) / dt;

            lastPlayerPos = playerShip.position;

            Vector3 shakeOffset = Vector3.zero;
            if (m_ShakeTimer > 0f && m_ShakeEnabled)
            {
                float t = Mathf.Clamp01(m_ShakeTimer / 0.5f);
                shakeOffset = new Vector3(
                    m_CurrentShakeRadius * Mathf.Cos(shakeFrequencyX * Time.time) * t,
                    m_CurrentShakeRadius * 0.1f * Mathf.Sin(shakeFrequencyY * Time.time) * t,
                    0f
                );
            }

            Vector3 speedShake = m_ShakeEnabled
                ? new Vector3(
                    0.2f * Mathf.Cos(10f * Time.time),
                    0.1f * Mathf.Sin(16f * Time.time),
                    0f)
                : Vector3.zero;

            Vector3 targetPos = playerShip.position + offset + (playerShip.forward * forwardTrackingMultiplier);

            transform.position = Vector3.Lerp(
                transform.position,
                targetPos + shakeOffset + speedShake,
                followSmooth * dt
            );

            Vector3 lookTarget = playerShip.position + playerShip.forward * lookAhead;
            Vector3 lookDir = lookTarget - transform.position;

            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion baseRotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                float tilt = -playerVelocity.x * tiltAmount * 0.02f;
                Quaternion tiltRotation = Quaternion.Euler(0f, 0f, tilt);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    baseRotation * tiltRotation,
                    tiltSmooth * dt
                );
            }

            if (cam != null)
            {
                float speed = playerVelocity.magnitude;
                float targetFOV = Mathf.Lerp(baseFOV, maxFOV, Mathf.Clamp01(speed * fovSpeedFactor));
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, dt * 3f);
            }
        }

        private void Update()
        {
            if (m_ShakeTimer > 0f)
                m_ShakeTimer -= Time.deltaTime;
            else
                m_ShakeTimer = 0f;
        }

        public void StartShake(float t, float r)
        {
            if (m_ShakeTimer <= 0f || m_CurrentShakeRadius < r)
                m_CurrentShakeRadius = r;

            m_ShakeTimer = t;
        }

        public void TriggerNearMiss(float intensity = 0.6f)
        {
            StartShake(0.2f, intensity);
        }
    }
}