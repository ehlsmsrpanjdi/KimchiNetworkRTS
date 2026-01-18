using UnityEngine;
using UnityEngine.UI;

public class GameManagerUI : MonoBehaviour
{
    [SerializeField] Button HostBtn;
    [SerializeField] Button ClientBtn;
    [SerializeField] Button StartGameBtn;

    private void Reset()
    {
        HostBtn = this.TryFindChild("HostBtn").GetComponent<Button>();
        ClientBtn = this.TryFindChild("ClientBtn").GetComponent<Button>();
        StartGameBtn = this.TryFindChild("StartGameBtn").GetComponent<Button>();
    }

    private void Start()
    {
        HostBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.StartHost();
        });
        ClientBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.StartClient();
        });
        StartGameBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.GameStart();
        });
    }
}
