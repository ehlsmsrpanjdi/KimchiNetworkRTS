using UnityEngine;

public interface IPoolObj
{
    void OnPush();
    void OnPop();
}


public interface ITakeDamage
{
    void TakeDamage(float _Damage);
}

