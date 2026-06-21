using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 씬에 배치하는 DailyLimit 입력 UI.
/// InputField에 숫자를 입력하고 저장 버튼을 누르면 PlayerPrefs에 기록됩니다.
/// </summary>
public class DailyLimitSettingUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button saveButton;
    [SerializeField] private TextMeshProUGUI feedbackText; // 선택 사항

    private const string DailyLimitKey = "setting_daily_limit";
    private const int DailyLimitDefault = 20;
    private const int DailyLimitMin = 1;
    private const int DailyLimitMax = 100;

    private void Start()
    {
        int current = PlayerPrefs.GetInt(DailyLimitKey, DailyLimitDefault);
        inputField.text = current.ToString();

        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        saveButton.onClick.AddListener(OnSave);
    }

    private void OnSave()
    {
        if (!int.TryParse(inputField.text, out int value))
        {
            ShowFeedback("숫자를 입력해주세요.");
            return;
        }

        value = Mathf.Clamp(value, DailyLimitMin, DailyLimitMax);
        inputField.text = value.ToString(); // 클램프된 값 반영

        PlayerPrefs.SetInt(DailyLimitKey, value);
        PlayerPrefs.Save();

        ShowFeedback($"하루 학습 단어 수가 {value}개로 저장되었습니다.");
        Debug.Log($"[DailyLimitSettingUI] dailyLimit 저장: {value}");
    }

    private void ShowFeedback(string msg)
    {
        if (feedbackText != null)
            feedbackText.text = msg;
    }
}