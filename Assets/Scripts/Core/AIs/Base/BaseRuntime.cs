using System;
using UnityEngine;

public class BaseRuntime : MonoBehaviour, IDamageable
{
    public enum BaseState
    {
        NormalEnemy,
        Broken,
        Taken
    }

    [Header("Config")]
    public BaseConfig config;

    [Header("Visual")]
    public Transform modelRoot;

    [Header("Runtime")]
    public float hp;
    public BaseState state;

    GameObject currentModel;

    public bool isTaken => state == BaseState.Taken;

    public event Action<float, float> OnHPChanged;
    public event Action<bool> OnTakenChanged;
    public event Action<BaseState> OnStateChanged;
    public event Action OnBroken;
    public event Action OnTaken;   // 新增：基地成功被占领时触发一次

    void Awake()
    {
        if (config == null)
        {
            Debug.LogError($"{name}: BaseRuntime 没有绑定 config!");
            hp = 1f;
            return;
        }

        state = config.takenAtStart ? BaseState.Taken : BaseState.NormalEnemy;
        hp = config.maxHP;

        RefreshModel();
        RaiseAll();
    }

    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (config == null) return;

        // Broken 状态下不再吃伤害
        if (state == BaseState.Broken) return;

        hp = Mathf.Max(0, hp - Mathf.Max(0, amount));
        OnHPChanged?.Invoke(hp, config.maxHP);

        if (hp <= 0f)
        {
            if (state == BaseState.NormalEnemy)
            {
                EnterBrokenState();
            }
            else if (state == BaseState.Taken)
            {
                RevertToEnemyState();
            }
        }
    }

    void EnterBrokenState()
    {
        state = BaseState.Broken;
        hp = 0f;

        RefreshModel();

        OnHPChanged?.Invoke(hp, config.maxHP);
        OnTakenChanged?.Invoke(false);
        OnStateChanged?.Invoke(state);
        OnBroken?.Invoke();

        Debug.Log($"{name}: Base entered BROKEN state");
    }

    void RevertToEnemyState()
    {
        state = BaseState.NormalEnemy;
        hp = config.maxHP;

        RefreshModel();

        OnHPChanged?.Invoke(hp, config.maxHP);
        OnTakenChanged?.Invoke(false);
        OnStateChanged?.Invoke(state);

        Debug.Log($"{name}: Taken base was destroyed by enemies, reverted to NORMAL ENEMY state");
    }

    public void RepairAndTake()
    {
        if (config == null) return;
        if (state != BaseState.Broken) return;

        state = BaseState.Taken;
        hp = config.maxHP;

        RefreshModel();

        OnHPChanged?.Invoke(hp, config.maxHP);
        OnTakenChanged?.Invoke(true);
        OnStateChanged?.Invoke(state);
        OnTaken?.Invoke();   // 新增

        Debug.Log($"{name}: Base repaired and TAKEN");
    }

    public void SetTaken(bool taken)
    {
        if (taken)
        {
            if (state == BaseState.Broken)
            {
                RepairAndTake();
            }
            else if (state == BaseState.NormalEnemy)
            {
                state = BaseState.Taken;
                hp = config.maxHP;

                RefreshModel();
                RaiseAll();
                OnTaken?.Invoke();   // 新增
            }
        }
        else
        {
            if (state == BaseState.Taken)
            {
                state = BaseState.NormalEnemy;
                hp = config.maxHP;

                RefreshModel();
                RaiseAll();
            }
        }
    }

    void RaiseAll()
    {
        OnHPChanged?.Invoke(hp, config.maxHP);
        OnTakenChanged?.Invoke(isTaken);
        OnStateChanged?.Invoke(state);
    }

    void RefreshModel()
    {
        if (modelRoot == null)
        {
            Debug.LogWarning($"{name}: modelRoot 没有绑定");
            return;
        }

        if (currentModel != null)
        {
            Destroy(currentModel);
        }

        GameObject prefabToUse = null;

        switch (state)
        {
            case BaseState.NormalEnemy:
                prefabToUse = config.normalModel;
                break;

            case BaseState.Broken:
                prefabToUse = config.brokenModel;
                break;

            case BaseState.Taken:
                prefabToUse = config.takenModel != null ? config.takenModel : config.normalModel;
                break;
        }

        if (prefabToUse != null)
        {
            currentModel = Instantiate(prefabToUse, modelRoot);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
            currentModel.transform.localScale = Vector3.one;
        }
    }
}