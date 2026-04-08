using UnityEngine;

public class BaseInteraction : MonoBehaviour
{
    [Header("Refs")]
    public BaseRuntime baseRuntime;
    public BaseInteractUI interactUI;

    [Header("Interaction")]
    public float interactionRange = 50f;
    public float repairHoldTime = 4f;

    [Header("Player")]
    public Transform player;

    private float repairTimer = 0f;

    private bool lastCanInteract = false;
    private BaseRuntime.BaseState lastState;

    void Start()
    {
        Debug.Log($"[{name}] BaseInteraction Start");

        // 强制绑定自己身上的 BaseRuntime
        baseRuntime = GetComponent<BaseRuntime>();

        // 强制绑定自己子物体里的 BaseInteractUI
        interactUI = GetComponentInChildren<BaseInteractUI>(true);

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        Debug.Log($"[{name}] baseRuntime = {GetPath(baseRuntime != null ? baseRuntime.transform : null)}");
        Debug.Log($"[{name}] interactUI   = {GetPath(interactUI != null ? interactUI.transform : null)}");
        Debug.Log($"[{name}] player       = {GetPath(player)}");

        if (interactUI != null)
            interactUI.Hide();

        if (baseRuntime != null)
            lastState = baseRuntime.state;
    }

    void Update()
    {
        if (baseRuntime == null || interactUI == null || player == null)
            return;

        if (baseRuntime.state != lastState)
        {
            Debug.Log($"[{name}] Base state changed -> {baseRuntime.state}");
            lastState = baseRuntime.state;
        }



        float dist = Vector3.Distance(player.position, transform.position);
        bool inRange = dist <= interactionRange;
        bool isBroken = baseRuntime.state == BaseRuntime.BaseState.Broken;
        bool canInteract = inRange && isBroken;


        if (baseRuntime.state == BaseRuntime.BaseState.Broken && Time.frameCount % 30 == 0)
        { 
            Debug.Log($"[{name}] BROKEN status | dist={dist:F2}, range={interactionRange}, inRange={inRange}");
        }

        if (canInteract != lastCanInteract)
        {
            Debug.Log($"[{name}] canInteract changed -> {canInteract} | dist={dist:F2}, range={interactionRange}, state={baseRuntime.state}");
            lastCanInteract = canInteract;
        }

        if (!canInteract)
        {
            repairTimer = 0f;
            interactUI.Hide();
            return;
        }

        bool holdingE = Input.GetKey(KeyCode.E);

        if (holdingE)
        {
            repairTimer += Time.deltaTime;

            if (repairTimer >= repairHoldTime)
            {
                Debug.Log($"[{name}] Repair complete -> RepairAndTake()");
                baseRuntime.RepairAndTake();
                repairTimer = 0f;
                interactUI.Hide();
                return;
            }
        }
        else
        {
            repairTimer = 0f;
        }

        float progress = holdingE ? repairTimer / repairHoldTime : 0f;
        Debug.Log($"[{name}] Show UI on {GetPath(interactUI.transform)}");
        interactUI.Show("Hold E Repair", progress);
    }

    string GetPath(Transform t)
    {
        if (t == null) return "NULL";

        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}