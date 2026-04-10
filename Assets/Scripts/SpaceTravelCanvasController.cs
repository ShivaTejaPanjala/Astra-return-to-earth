using UnityEngine;
using Plane.UI;

public class SpaceTravelCanvasController : MonoBehaviour
{
    [Header("Skybox")]
    public Material spaceSkyboxMaterial;
    public float skyboxRotationSpeed = 1f;

    [Header("Player")]
    public Transform playerShip;
    [SerializeField] private Transform visualModel;
    public float playerForwardBob = 0.2f;
    public float playerBobSpeed = 2f;

    [Header("Particles")]
    public ParticleSystem travelParticles;
    public ParticleSystem boostParticles;
    public float particleEmissionBase = 20f;
    public float particleEmissionMax = 80f;

    private Material runtimeSkybox;
    private float runtimeSkyboxRotation;
    private Vector3 visualModelStartLocalPos;

    private void Awake()
    {
        if (spaceSkyboxMaterial != null)
        {
            runtimeSkybox = new Material(spaceSkyboxMaterial);
            RenderSettings.skybox = runtimeSkybox;
        }
    }

    private void Start()
    {
        if (travelParticles != null)
            travelParticles.Play();

        if (boostParticles != null)
            boostParticles.Stop();

        if (visualModel != null)
            visualModelStartLocalPos = visualModel.localPosition;
    }

    private void Update()
    {
        UpdateSkybox();
        UpdatePlayerMotionFeel();
        UpdateParticles();
    }

    private void UpdateSkybox()
    {
        if (runtimeSkybox == null) return;

        runtimeSkyboxRotation += skyboxRotationSpeed * Time.deltaTime * 0.05f;
        runtimeSkybox.SetFloat("_Rotation", runtimeSkyboxRotation);
    }

    private void UpdatePlayerMotionFeel()
    {
        if (visualModel == null) return;

        float bob = Mathf.Sin(Time.time * playerBobSpeed) * playerForwardBob;
        visualModel.localPosition = visualModelStartLocalPos + new Vector3(0f, bob, 0f);
    }

    private void UpdateParticles()
    {
        float speed = 1f;

        if (Plane.UI.GameControl.m_Current != null)
            speed = Plane.UI.GameControl.m_Current.m_GameSpeed;

        float t = Mathf.Clamp01(speed / 50f);

        if (travelParticles != null)
        {
            var emission = travelParticles.emission;
            emission.rateOverTime = Mathf.Lerp(particleEmissionBase, particleEmissionMax, t);
        }

        if (boostParticles != null)
        {
            if (t > 0.5f)
            {
                if (!boostParticles.isPlaying)
                    boostParticles.Play();
            }
            else
            {
                if (boostParticles.isPlaying)
                    boostParticles.Stop();
            }
        }
    }
}