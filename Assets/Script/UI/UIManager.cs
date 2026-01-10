using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UIManager();
            }
            return instance;
        }
    }

    Dictionary<Type, UIBase> uiDictionary = new Dictionary<Type, UIBase>();


    //없으면?  안됨
    public T GetUI<T>() where T : UIBase
    {
        Type uiType = typeof(T);

        uiDictionary.TryGetValue(uiType, out UIBase baseUI);

        if (baseUI == null)
        {
            //LogHelper.LogWarrning("Null UI 접근" + uiType);
            return null;
        }

        T castUI = baseUI as T;

        return castUI;
    }



    public bool OnUI<T>() where T : UIBase
    {
        T gainUI = GetUI<T>();
        if (null != gainUI)
        {
            gainUI.OnUI();
            return true;
        }
        return false;
    }

    public bool OffUI<T>() where T : UIBase
    {
        T gainUI = GetUI<T>();
        if (null != gainUI)
        {
            gainUI.OnUI();
            return true;
        }
        return false;
    }

    public void AddUI(UIBase _UI)
    {
        Type uiType = _UI.GetType();

        if (true == uiDictionary.ContainsKey(_UI.GetType()))
        {
            uiDictionary[uiType] = _UI;
            Debug.Log("UI 교체됨 근데 교체 안되게 만들예정인데 왜 교체됐지");
            return;
        }
        uiDictionary.Add(uiType, _UI);
    }

    public void NetWorkBinding()
    {
        foreach (var ui in uiDictionary)
        {
            ui.Value.NetWorkBinding();
        }
    }

}
