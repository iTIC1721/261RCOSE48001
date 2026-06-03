// ButtonSFXHandler.cs
// 버튼에 부착되는 런타임 컴포넌트.
// AudioManager.Instance 를 런타임에 찾으므로 씬에 AudioManager가 없어도 동작합니다.
//
// IPointerClickHandler 를 직접 구현하므로 Button.onClick.RemoveAllListeners() 호출에
// 영향을 받지 않습니다.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSFXHandler : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("AudioLibrary에 등록된 SFX 키")]
    public string sfxKey = "UI_Click";

    [Range(0f, 1f)]
    [Tooltip("재생 볼륨 스케일")]
    public float volumeScale = 1f;

    [Tooltip("true = PlaySFXPooled (동시 다발 재생) / false = PlaySFX")]
    public bool usePooled = true;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    // Button 내부도 IPointerClickHandler 를 구현하고 있습니다.
    // EventSystem은 같은 GameObject의 모든 IPointerClickHandler 를 순서대로 호출하므로
    // onClick.RemoveAllListeners() 와 무관하게 항상 실행됩니다.
    public void OnPointerClick(PointerEventData eventData)
    {
        // 버튼이 비활성화 상태이면 재생하지 않음
        if (_button == null || !_button.IsInteractable()) return;

        var manager = AudioManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("[ButtonSFXHandler] AudioManager.Instance를 찾을 수 없습니다.");
            return;
        }

        if (usePooled)
            manager.PlaySFXPooled(sfxKey, volumeScale);
        else
            manager.PlaySFX(sfxKey, volumeScale);
    }
}