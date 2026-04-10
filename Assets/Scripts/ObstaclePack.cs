using UnityEngine;
using Plane.UI;

namespace Plane.Gameplay
{
    public class ObstaclePack : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float m_MoveSpeed = 18f;
        [SerializeField] private float destroyBehindCameraDistance = 35f;
        [SerializeField] private float spawnFarDistance = 220f;
        [SerializeField] private float scaleUpSpeed = 1.6f;
        [SerializeField] private float maxWorldScale = 6f;

        [Header("Obstacle Size")]
        [SerializeField] private Vector3 scaleMultiplier = Vector3.one;
        [SerializeField] private float minScale = 0.08f;
        [SerializeField] private float maxScale = 0.22f;

        [Header("Rotation")]
        [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 30f, 0f);

        [Header("Spawn Feel")]
        [SerializeField] private bool startTiny = true;
        [SerializeField] private float distanceScaleCurve = 0.02f;
        [SerializeField] private float minSpawnX = -12f;
        [SerializeField] private float maxSpawnX = 12f;
        [SerializeField] private float minSpawnY = -5f;
        [SerializeField] private float maxSpawnY = 6f;

        [SerializeField] private float spawnRadius = 18f;
        [SerializeField] private float speedRandomMin = 0.85f;
        [SerializeField] private float speedRandomMax = 1.35f;

        private Transform targetCamera;
        private Transform playerShip;
        private Vector3 initialScale;
        private Vector3 spawnPos;
        private bool initialized;
        private float localSpeedMultiplier;

        private Vector3 moveDirection; 

        private void Start()
        {
            if (Camera.main != null)
                targetCamera = Camera.main.transform;

            playerShip = GameObject.FindGameObjectWithTag("Player")?.transform;

            float randomScale = Random.Range(minScale, maxScale);
            initialScale = scaleMultiplier * randomScale;
            transform.localScale = startTiny ? initialScale : initialScale * 2f;

            localSpeedMultiplier = Random.Range(speedRandomMin, speedRandomMax);

            if (playerShip != null)
            {
                Vector2 circle = Random.insideUnitCircle * spawnRadius;

                float x = playerShip.position.x + circle.x;
                float y = playerShip.position.y + circle.y;

                float z = playerShip.position.z + spawnFarDistance;
                z = Mathf.Max(z, playerShip.position.z + 50f); 

                spawnPos = new Vector3(x, y, z);

                transform.position = spawnPos;

                moveDirection = (playerShip.position - transform.position).normalized;

                float forwardCheck = Vector3.Dot(moveDirection, Vector3.back);
                if (forwardCheck < 0.4f)
                {
                    moveDirection = (Vector3.back + Random.insideUnitSphere * 0.2f).normalized;
                }

                // Slight randomness for realism
                moveDirection += Random.insideUnitSphere * 0.1f;
                moveDirection.Normalize();

                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
            else
            {
                float x = Random.Range(minSpawnX, maxSpawnX);
                float y = Random.Range(minSpawnY, maxSpawnY);
                spawnPos = new Vector3(x, y, spawnFarDistance);

                transform.position = spawnPos;
                moveDirection = Vector3.back;
            }

            initialized = true;
        }

        private void Update()
{
    if (!initialized) return;

    float speed = (GameControl.m_Current != null ? GameControl.m_Current.m_GameSpeed : m_MoveSpeed) * localSpeedMultiplier;
    float delta = speed * Time.deltaTime;

    transform.position += moveDirection * delta;

    transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);

    float traveled = spawnFarDistance - transform.position.z;
    float growT = Mathf.Clamp01(traveled * distanceScaleCurve);
    float scaleFactor = Mathf.Lerp(1f, maxWorldScale / Mathf.Max(0.001f, initialScale.magnitude), growT);
    transform.localScale = Vector3.Lerp(transform.localScale, initialScale * scaleFactor, Time.deltaTime * scaleUpSpeed);

    if (targetCamera != null && transform.position.z < targetCamera.position.z - destroyBehindCameraDistance)
        Destroy(gameObject);
    else if (transform.position.z < -500f)
        Destroy(gameObject);
}

    }
}