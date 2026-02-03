using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Single Canvas 기반 체력바
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image fillImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    [Header("Settings")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 2f, 0);

    private Transform target;
    private Camera mainCamera;

    private void Reset()
    {
        fillImage = this.TryFindChild("FillImage").GetComponent<Image>();
    }

    void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // ✅ 월드 좌표 → 스크린 좌표 변환
        Vector3 worldPosition = target.position + worldOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        // ✅ 카메라 뒤에 있으면 숨기기
        if (screenPosition.z < 0)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        canvasGroup.alpha = 1f;
        rectTransform.position = screenPosition;
    }

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(Transform targetTransform)
    {
        target = targetTransform;
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 체력 업데이트
    /// </summary>
    public void UpdateHealth(float currentHP, float maxHP)
    {
        if (maxHP <= 0) return;

        float ratio = Mathf.Clamp01(currentHP / maxHP);
        fillImage.fillAmount = ratio;

        // ✅ 색상 변경 (초록 → 빨강)
        fillImage.color = Color.Lerp(Color.red, Color.green, ratio);
    }

    /// <summary>
    /// 반환
    /// </summary>
    public void Release()
    {
        target = null;
        HealthBarManager.Instance.ReturnHealthBar(this);
    }
}