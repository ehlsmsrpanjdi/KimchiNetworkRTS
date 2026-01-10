using UnityEngine;

public class UIBase : MonoBehaviour
{
    protected virtual void Awake()
    {
        UIManager.Instance.AddUI(this);
    }

    protected virtual void Start()
    {

    }

    public virtual void OnUI()
    {
        gameObject.SetActive(true);
    }


    public virtual void OffUI()
    {
        gameObject.SetActive(false);
    }

    public virtual void NetWorkBinding()
    {

    }
}
