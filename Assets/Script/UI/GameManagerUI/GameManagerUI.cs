using UnityEngine;
using UnityEngine.UI;

public class GameManagerUI : MonoBehaviour
{
    [SerializeField] Button HostBtn;
    [SerializeField] Button ClientBtn;
    [SerializeField] Button StartGameBtn;

    bool isParticipated = false;

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
            if (isParticipated == true)
            {
                return;
            }
            isParticipated = true;
            GameManager.Instance.StartHost();
        });
        ClientBtn.onClick.AddListener(() =>
        {
            if (isParticipated == true)
            {
                return;
            }
            isParticipated = true;
            GameManager.Instance.StartClient();
            Destroy(this.gameObject);
        });
        StartGameBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.GameStart();
            Destroy(this.gameObject);
        });
    }
}
