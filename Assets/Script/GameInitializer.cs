using UnityEngine;
using Unity.Netcode;

public class GameInitializer : MonoBehaviour
{
    async void Start()
    {
        // Addressables 로딩
        await LoadManager.Instance.LoadTemp();

        Debug.Log("✅ Assets loaded!");

    }
}