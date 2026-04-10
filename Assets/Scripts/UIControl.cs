using UnityEngine;
using UnityEngine.UI;

namespace Plane.UI
{
    public class UIControl : MonoBehaviour
    {
        private static UIControl m_Current;
        public static UIControl Current => m_Current;

        [SerializeField] private GameObject m_InGameUI;
        [SerializeField] private GameObject winUI;
        [SerializeField] private GameObject loseUI;
        [SerializeField] private GameObject levelUpUI;
        [SerializeField] private bool autoAttachHoverEffect = true;

        private void Awake()
        {
            if (m_Current != null && m_Current != this)
            {
                Destroy(gameObject);
                return;
            }

            m_Current = this;
        }

        private void Start()
        {
            if (autoAttachHoverEffect)
                ApplyHoverEffectToButtons();

            ShowInGameUI();
        }

        private void ApplyHoverEffectToButtons()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                if (button.gameObject.GetComponent<UIHoverEffect>() == null)
                    button.gameObject.AddComponent<UIHoverEffect>();
            }
        }

        public void ShowInGameUI()
        {
            if (m_InGameUI != null) m_InGameUI.SetActive(true);
            if (winUI != null) winUI.SetActive(false);
            if (loseUI != null) loseUI.SetActive(false);
            if (levelUpUI != null) levelUpUI.SetActive(false);
        }

        public void ShowResult(bool win)
        {
            if (m_InGameUI != null) m_InGameUI.SetActive(false);
            if (levelUpUI != null) levelUpUI.SetActive(false);

            if (winUI != null) winUI.SetActive(win);
            if (loseUI != null) loseUI.SetActive(!win);
        }

        public void ShowLevelUp()
        {
            if (m_InGameUI != null) m_InGameUI.SetActive(false);
            if (winUI != null) winUI.SetActive(false);
            if (loseUI != null) loseUI.SetActive(false);

            if (levelUpUI != null) levelUpUI.SetActive(true);
        }

        public void HideLevelUp()
        {
            if (levelUpUI != null) levelUpUI.SetActive(false);
            if (m_InGameUI != null) m_InGameUI.SetActive(true);
        }

#if UNITY_EDITOR
        public void QuitGame()
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#endif
    }
}