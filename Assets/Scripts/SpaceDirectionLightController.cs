using UnityEngine;

[ExecuteAlways]
public class SpaceDirectionLightController : MonoBehaviour
{
    [Header("References")]
    public Transform playerShip;
    public Transform[] obstacleTargets;
    public Transform lookAheadTarget;

    [Header("Main Light")]
    public Light directionalLight;
    public float baseIntensity = 1.2f;
    public float boostIntensity = 1.8f;
    public float intensityChangeSpeed = 2.5f;
    public Color baseColor = new Color(0.95f, 0.97f, 1f, 1f);
    public Color boostColor = new Color(1f, 0.92f, 0.75f, 1f);

    [Header("Movement Feel")]
    public float baseForwardTilt = 25f;
    public float speedTiltAmount = 10f;
    public float tiltLerpSpeed = 4f;
    public float lookAheadDistance = 35f;
    public float lookAtLerpSpeed = 3f;
    public float currentSpeed = 0f;
    public float speedForMaxEffect = 100f;

    [Header("Obstacle Focus")]
    public float obstacleFocusDistance = 120f;
    public float obstacleFocusAngle = 18f;
    public float focusIntensityMultiplier = 1.35f;
    public float focusSpotSwing = 0.5f;
    public float obstacleSearchInterval = 0.2f;

    [Header("Player Focus")]
    public float playerFocusIntensityMultiplier = 1.15f;
    public float playerFillLightStrength = 0.6f;
    public Light playerFillLight;

    private float nextObstacleSearchTime;
    private Transform currentFocusTarget;
    private Quaternion desiredRotation;
    private Color currentColor;
    private float currentIntensity;

    void Awake()
    {
        if (directionalLight == null)
            directionalLight = GetComponent<Light>();

        if (directionalLight != null)
            directionalLight.type = LightType.Directional;

        currentColor = baseColor;
        currentIntensity = baseIntensity;
    }

    void Start()
    {
        ApplyPlayerFillLight();
    }

    void Update()
    {
        if (directionalLight == null) return;

        if (playerShip != null && lookAheadTarget == null)
            EnsureLookAheadTarget();

        UpdateObstacleFocus();
        UpdateLightMotion();
        UpdateLightLook();
        ApplyIntensityAndColor();
        UpdatePlayerFillLight();
    }

    void EnsureLookAheadTarget()
    {
        if (lookAheadTarget == null)
        {
            GameObject temp = new GameObject("LookAheadTarget");
            temp.hideFlags = HideFlags.HideAndDontSave;
            lookAheadTarget = temp.transform;
        }
    }

    void UpdateLightMotion()
    {
        float speedT = Mathf.Clamp01(currentSpeed / Mathf.Max(1f, speedForMaxEffect));
        float tilt = baseForwardTilt + speedTiltAmount * speedT;
        Vector3 dir = Quaternion.Euler(tilt, 0f, 0f) * Vector3.down;
        desiredRotation = Quaternion.LookRotation(dir.normalized, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * tiltLerpSpeed);
    }

    void UpdateObstacleFocus()
    {
        if (Time.time < nextObstacleSearchTime) return;
        nextObstacleSearchTime = Time.time + obstacleSearchInterval;

        Transform best = null;
        float bestScore = float.NegativeInfinity;

        if (obstacleTargets != null)
        {
            foreach (var t in obstacleTargets)
            {
                if (t == null) continue;
                float dist = playerShip != null ? Vector3.Distance(playerShip.position, t.position) : Vector3.Distance(transform.position, t.position);
                if (dist > obstacleFocusDistance) continue;

                Vector3 toObstacle = (t.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, toObstacle);
                if (angle > obstacleFocusAngle) continue;

                float score = (obstacleFocusDistance - dist) + (obstacleFocusAngle - angle) * 2f;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = t;
                }
            }
        }

        currentFocusTarget = best;
    }

    void UpdateLightLook()
    {
        Vector3 targetPos;
        if (currentFocusTarget != null)
        {
            targetPos = currentFocusTarget.position + Random.insideUnitSphere * focusSpotSwing;
        }
        else if (playerShip != null)
        {
            targetPos = playerShip.position + playerShip.forward * lookAheadDistance;
        }
        else
        {
            targetPos = transform.position + transform.forward * 10f;
        }

        if (lookAheadTarget != null)
            lookAheadTarget.position = Vector3.Lerp(lookAheadTarget.position, targetPos, Time.deltaTime * lookAtLerpSpeed);

        Vector3 lookDir = (lookAheadTarget != null ? lookAheadTarget.position : targetPos) - transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir.normalized, Vector3.up), Time.deltaTime * lookAtLerpSpeed);
    }

    void ApplyIntensityAndColor()
    {
        float speedT = Mathf.Clamp01(currentSpeed / Mathf.Max(1f, speedForMaxEffect));
        float targetIntensity = baseIntensity + speedT * (boostIntensity - baseIntensity);
        if (currentFocusTarget != null) targetIntensity *= focusIntensityMultiplier;
        if (playerShip != null && currentFocusTarget == null) targetIntensity *= playerFocusIntensityMultiplier;

        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * intensityChangeSpeed);
        directionalLight.intensity = currentIntensity;

        Color targetColor = currentFocusTarget != null ? boostColor : Color.Lerp(baseColor, boostColor, speedT * 0.5f);
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * intensityChangeSpeed);
        directionalLight.color = currentColor;
    }

    void ApplyPlayerFillLight()
    {
        if (playerFillLight == null) return;
        playerFillLight.type = LightType.Point;
        playerFillLight.intensity = playerFillLightStrength;
    }

    void UpdatePlayerFillLight()
    {
        if (playerFillLight == null || playerShip == null) return;
        playerFillLight.transform.position = Vector3.Lerp(playerFillLight.transform.position, playerShip.position + new Vector3(0f, 3f, -4f), Time.deltaTime * 4f);
        playerFillLight.transform.rotation = Quaternion.Slerp(playerFillLight.transform.rotation, Quaternion.LookRotation(playerShip.position - playerFillLight.transform.position, Vector3.up), Time.deltaTime * 4f);
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }

    public void SetObstacleTargets(Transform[] targets)
    {
        obstacleTargets = targets;
    }
}