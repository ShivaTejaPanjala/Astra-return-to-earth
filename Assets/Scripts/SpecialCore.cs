using UnityEngine;

namespace Plane.Gameplay
{
    public class SpecialCore : MonoBehaviour
    {
        [Header("Appearance")]
        [SerializeField] private Color specialColor = new Color(1f, 0.3f, 0.7f, 0.35f);
        [SerializeField, Range(0f, 1f)] private float alpha = 0.35f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 18f;
        [SerializeField] private float destroyBehindCameraDistance = 25f;
        [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 70f, 0f);

        [Header("Special Settings")]
        [SerializeField] private float laserDuration = 30f;
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
            Color c = specialColor;
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

            PlayerShipController ship = other.GetComponent<PlayerShipController>();
            if (ship == null)
                return;

            ship.EnableLaser(laserDuration);
            if (Plane.UI.GameControl.m_Current != null)
               Plane.UI.GameControl.m_Current.ActivateShootMode(laserDuration);

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