using UnityEngine;

namespace Plane.Gameplay
{
    public class BoostCore : MonoBehaviour
    {
        [Header("Appearance")]
        [SerializeField] private Color boostColor = new Color(0.2f, 0.8f, 1f, 0.35f);
        [SerializeField, Range(0f, 1f)] private float alpha = 0.35f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 18f;
        [SerializeField] private float destroyBehindCameraDistance = 25f;
        [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 60f, 0f);

        [Header("Boost Settings")]
        [SerializeField] private float speedMultiplier = 2f;
        [SerializeField] private float boostDuration = 30f;
        [SerializeField] private AudioClip collectSfx;

        private Renderer[] renderers;
        private Transform targetCamera;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true);

            if (Camera.main != null)
                targetCamera = Camera.main.transform;

            ApplyTransparency();
        }

        private void Update()
        {
            float speed = moveSpeed;

            if (Plane.UI.GameControl.m_Current != null)
                speed *= Plane.UI.GameControl.m_Current.m_GameSpeed;

            transform.position += Vector3.back * speed * Time.deltaTime;
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);

            if (targetCamera != null && transform.position.z < targetCamera.position.z - destroyBehindCameraDistance)
                Destroy(gameObject);
        }

        private void ApplyTransparency()
        {
            Color c = boostColor;
            c.a = alpha;

            foreach (var r in renderers)
            {
                if (r != null && r.sharedMaterial != null)
                    r.sharedMaterial.color = c;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (Plane.UI.GameControl.m_Current != null)
                Plane.UI.GameControl.m_Current.StartSpeedBoost(speedMultiplier, boostDuration);

            PlayCollectSound();
            Destroy(gameObject);
        }

        private void PlayCollectSound()
        {
            if (collectSfx == null)
                return;

            AudioSource.PlayClipAtPoint(collectSfx, transform.position);
        }
    }
}