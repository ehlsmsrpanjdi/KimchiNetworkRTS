using UnityEngine;
using UnityEngine.UI;

public class GameStateButtonUI : UIBase
{
    [SerializeField] Button button;
    GameStatePanelUI panelUI;

    private void Reset()
    {
        button = GetComponent<Button>();
    }

    protected override void Start()
    {
        base.Start();

        panelUI = UIManager.Instance.GetUI<GameStatePanelUI>();

        button.onClick.AddListener(() =>
        {
            panelUI.Toggle();
        });
    }
}
