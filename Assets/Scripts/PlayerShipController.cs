using System.Collections;
using UnityEngine;
using Plane.UI;

namespace Plane.Gameplay
{
    public class PlayerShipController : MonoBehaviour
    {
        public Vector2 m_Angle = Vector2.zero;
        public Transform m_Base;
        public GameObject m_ExplodeParticle;
        public static PlayerShipController m_Main;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float forwardSpeed = 0f;
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private float tiltX = 50f;
        [SerializeField] private float tiltY = 50f;
        [SerializeField] private float collisionRadius = 2.5f;

        [Header("Laser")]
        [SerializeField] private float laserRange = 200f;
        [SerializeField] private float laserFireRate = 0.25f;

        [Header("Particles")]
        [SerializeField] private ParticleSystem travelParticles;
        [SerializeField] private ParticleSystem hitParticles;
        [SerializeField] private float particleMinRate = 10f;
        [SerializeField] private float particleMaxRate = 80f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip crashSfx;

        [Header("Spawn")]
        [SerializeField] private Vector3 startPosition = new Vector3(0f, 0f, 10f);

        [Header("Options")]
        [SerializeField] private bool autoMoveForward = false;

        private bool laserActive;
        private float laserCooldown;
        private float laserTimer;
        private Vector3 currentVelocity;
        private Vector3 movementVelocity;
        private bool isDead;
        private float inputX;
        private float inputY;
        private Vector3 lastPosition;

        private void Awake()
        {
            m_Main = this;
        }

        private void Start()
        {
            transform.position = startPosition;
            lastPosition = transform.position;

            if (travelParticles != null)
            {
                var main = travelParticles.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;

                if (!travelParticles.isPlaying)
                    travelParticles.Play();
            }
        }

        private void Update()
        {
            if (isDead)
                return;

            ReadInput();
            UpdateMovement();
            HandleLaserInput();
            UpdateTravelParticles();
            CheckCollision();
            lastPosition = transform.position;
        }

        private void ReadInput()
        {
            inputX = 0f;
            inputY = 0f;

            if (Input.GetKey(KeyCode.LeftArrow)) inputX = -1f;
            else if (Input.GetKey(KeyCode.RightArrow)) inputX = 1f;

            if (Input.GetKey(KeyCode.UpArrow)) inputY = 1f;
            else if (Input.GetKey(KeyCode.DownArrow)) inputY = -1f;
        }

        private void UpdateMovement()
{
    Vector3 targetVelocity = new Vector3(
        inputX * moveSpeed,
        inputY * moveSpeed,
        autoMoveForward ? forwardSpeed : 0f
    );

    movementVelocity = Vector3.SmoothDamp(
        movementVelocity,
        targetVelocity,
        ref currentVelocity,
        smoothTime
    );

    transform.position += movementVelocity * Time.deltaTime;

    m_Angle.x = Mathf.Lerp(m_Angle.x, tiltX * inputX, 5f * Time.deltaTime);
    m_Angle.y = Mathf.Lerp(m_Angle.y, tiltY * inputY, 5f * Time.deltaTime);

    if (m_Base != null)
        m_Base.localRotation = Quaternion.Euler(-m_Angle.y, 0f, -m_Angle.x);
}

        private void HandleLaserInput()
        {
            if (!laserActive)
                return;

            if (laserCooldown > 0f)
                laserCooldown -= Time.deltaTime;

            if (laserTimer > 0f)
            {
                laserTimer -= Time.deltaTime;
                if (laserTimer <= 0f)
                    laserActive = false;
            }

            if (Input.GetKey(KeyCode.Space) && laserCooldown <= 0f)
            {
                FireLaser();
                laserCooldown = laserFireRate;
            }
        }

        private void FireLaser()
        {
            Vector3 direction = m_Base != null ? m_Base.forward : transform.forward;
            Vector3 origin = transform.position + direction * 2f;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, laserRange))
            {
                ObstaclePack obstacle = hit.collider.GetComponentInParent<ObstaclePack>();
                if (obstacle != null)
                    Destroy(obstacle.gameObject);
            }
        }

        public void EnableLaser(float duration)
        {
            laserActive = true;
            laserTimer = duration;
            laserCooldown = 0f;
        }

        private void CheckCollision()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, collisionRadius);

            foreach (Collider hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    continue;

                if (hit.GetComponent<BoostCore>() != null ||
                    hit.GetComponent<BonusCore>() != null ||
                    hit.GetComponent<SpecialCore>() != null)
                    continue;

                ObstaclePack obstacle = hit.GetComponentInParent<ObstaclePack>();
                if (obstacle == null)
                    continue;

                if (m_ExplodeParticle != null)
                    Instantiate(m_ExplodeParticle, transform.position, Quaternion.identity);

                if (hitParticles != null)
                    hitParticles.Play();

                if (crashSfx != null)
                {
                    if (audioSource != null)
                        audioSource.PlayOneShot(crashSfx);
                    else if (Camera.main != null)
                        AudioSource.PlayClipAtPoint(crashSfx, transform.position);
                }

                CameraControl.Current?.StartShake(0.4f, 1.8f);

                if (GameControl.m_Current != null)
                    GameControl.m_Current.HandleGameOver();

                isDead = true;
                StartCoroutine(DisableAfterCrash());
                break;
            }
        }

        private IEnumerator DisableAfterCrash()
        {
            yield return new WaitForSeconds(0.3f);
            gameObject.SetActive(false);
        }

        private void UpdateTravelParticles()
        {
            if (travelParticles == null)
                return;

            var emission = travelParticles.emission;
            float speed = new Vector3(
                transform.position.x - lastPosition.x,
                transform.position.y - lastPosition.y,
                transform.position.z - lastPosition.z
            ).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

            float t = Mathf.InverseLerp(0f, moveSpeed + forwardSpeed, speed);
            emission.rateOverTime = Mathf.Lerp(particleMinRate, particleMaxRate, t);
        }
    }
}