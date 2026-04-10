using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

[System.Serializable]
public class PlayerScoreData
{
    public string playerName;
    public int highScore;
}

public class MenuPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_Text placeholderText;
    [SerializeField] private TMP_Text highScoreDisplay;

    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip enterSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private string gameSceneName = "Game";

    [SerializeField] private Color normalPlaceholderColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color errorPlaceholderColor = new Color(1f, 0.5f, 0.5f, 1f);

    [SerializeField] private float blinkInterval = 0.12f;
    [SerializeField] private int blinkCount = 4;

    private string currentPlayer;
    private string profileKey;
    private string progressKey;
    private Coroutine blinkRoutine;

    private void Awake()
    {
        if (playerNameInput != null)
        {
            playerNameInput.onEndEdit.AddListener(OnNameSubmitted);
            playerNameInput.onValueChanged.AddListener(OnNameChanged);
        }

        LoadLastUsername();
        SetFieldNormal();
    }

    private void OnDestroy()
    {
        if (playerNameInput != null)
        {
            playerNameInput.onEndEdit.RemoveListener(OnNameSubmitted);
            playerNameInput.onValueChanged.RemoveListener(OnNameChanged);
        }
    }

    private void LoadLastUsername()
    {
        if (playerNameInput == null)
            return;

        string lastName = PlayerPrefs.GetString("LastPlayerName", "");
        if (!string.IsNullOrWhiteSpace(lastName))
            playerNameInput.text = lastName;

        UpdateHighScoreDisplay(lastName);
    }

    private void OnNameSubmitted(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            StartGame();
    }

    private void OnNameChanged(string text)
    {
        UpdateHighScoreDisplay(text.Trim());
    }

    private bool ValidateName()
    {
        if (playerNameInput == null || string.IsNullOrWhiteSpace(playerNameInput.text))
        {
            PlaySFX(errorSound);
            BlinkError();
            return false;
        }

        currentPlayer = playerNameInput.text.Trim();
        profileKey = "PlayerProfile_" + currentPlayer;
        progressKey = profileKey + "_Progress";
        SetFieldNormal();
        return true;
    }

    private void SetFieldNormal()
    {
        if (placeholderText != null)
            placeholderText.color = normalPlaceholderColor;
    }

    private void SetFieldError()
    {
        if (placeholderText != null)
            placeholderText.color = errorPlaceholderColor;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        if (audioSource != null)
            audioSource.PlayOneShot(clip);
        else if (Camera.main != null)
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }

    private void BlinkError()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkErrorRoutine());
    }

    private IEnumerator BlinkErrorRoutine()
    {
        SetFieldError();

        for (int i = 0; i < blinkCount; i++)
        {
            if (placeholderText != null)
                placeholderText.color = errorPlaceholderColor;

            yield return new WaitForSeconds(blinkInterval);

            if (placeholderText != null)
                placeholderText.color = normalPlaceholderColor;

            yield return new WaitForSeconds(blinkInterval);
        }

        SetFieldError();
        blinkRoutine = null;
    }

    public void StartGame()
    {
        if (!ValidateName())
            return;

        PlaySFX(enterSound);
        SaveProfileNameOnly();
        SceneManager.LoadScene(gameSceneName);
    }

    public void LoadGame()
    {
        if (!ValidateName())
            return;

        if (PlayerPrefs.HasKey(progressKey))
        {
            PlaySFX(clickSound);
            PlayerPrefs.SetString("PlayerName", currentPlayer);
            PlayerPrefs.SetString("LastPlayerName", currentPlayer);
            PlayerPrefs.Save();
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            PlaySFX(errorSound);
            BlinkError();
        }
    }

    public void SaveGame()
    {
        if (!ValidateName())
            return;

        PlaySFX(clickSound);
        PlayerPrefs.SetString(profileKey, currentPlayer);
        PlayerPrefs.SetString("PlayerName", currentPlayer);
        PlayerPrefs.SetString("LastPlayerName", currentPlayer);
        PlayerPrefs.SetInt(progressKey, 1);
        PlayerPrefs.Save();
        SetFieldNormal();
    }

    public void ExitGame()
    {
        PlaySFX(clickSound);
        PlayerPrefs.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SaveProfileNameOnly()
    {
        PlayerPrefs.SetString("PlayerName", currentPlayer);
        PlayerPrefs.SetString("LastPlayerName", currentPlayer);
        PlayerPrefs.SetString(profileKey, currentPlayer);

        if (!PlayerPrefs.HasKey(progressKey))
            PlayerPrefs.SetInt(progressKey, 1);

        PlayerPrefs.Save();
    }

    private void UpdateHighScoreDisplay(string playerName)
    {
        if (highScoreDisplay == null || string.IsNullOrWhiteSpace(playerName))
        {
            if (highScoreDisplay != null)
                highScoreDisplay.text = "";
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, $"playerScore_{playerName}.json");
        int highScore = 0;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerScoreData data = JsonUtility.FromJson<PlayerScoreData>(json);
            highScore = data.highScore;
        }

        highScoreDisplay.text = $"{playerName.ToUpper()} HIGHSCORE: {highScore}";
    }
}