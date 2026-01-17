using DG.Tweening;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class GameStatePanelUI : UIBase
{
    [Header("Refs")]
    [SerializeField] RectTransform panel; // 본체
    [SerializeField] RectTransform button; // 슬라이드 버튼 (GameStateButtonUI)

    [Header("Tween")]
    [SerializeField] float duration = 0.3f;
    [SerializeField] Ease ease = Ease.OutCubic;

    bool isOpen;
    float height;

    private void Reset()
    {
        panel = GetComponent<RectTransform>();
    }

    protected override void Start()
    {
        base.Start();

        // 높이는 패널 기준
        height = panel.rect.height;

        button = UIManager.Instance.GetUI<GameStateButtonUI>().GetComponent<RectTransform>();

        // 시작 위치: 둘 다 height만큼 아래
        SetHiddenImmediate();
    }

    void SetHiddenImmediate()
    {
        // 즉시 위치 세팅 (트윈 없이)
        panel.anchoredPosition =
            new Vector2(panel.anchoredPosition.x, height);

        button.anchoredPosition =
            new Vector2(button.anchoredPosition.x, 0);

        isOpen = false;
    }

    public void Toggle()
    {
        // 기존 트윈 제거 (현재 위치 기준으로 새로 시작)
        DOTween.Kill(panel);
        DOTween.Kill(button);

        float targetY = isOpen ? height : 0f;

        float buttonTargetY = targetY - height;

        isOpen = !isOpen;

        // 동시에 이동
        panel.DOAnchorPosY(targetY, duration)
             .SetEase(ease)
             .SetTarget(panel);

        button.DOAnchorPosY(buttonTargetY, duration)
              .SetEase(ease)
              .SetTarget(button);
    }
}
