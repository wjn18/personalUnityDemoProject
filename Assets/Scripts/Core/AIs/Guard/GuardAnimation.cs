using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(GuardAI))]
public class GuardAnimationController : MonoBehaviour
{
    [Header("Animator Params")]
    public string speedParam = "Speed";
    public string attackTrigger = "Attack";
    public string chooseParam = "Choose";
    public string isWaitingIdleParam = "IsWaiting";

    [Header("Speed Smooth")]
    public float dampTime = 0.1f;

    [Header("Wait Idle")]
    public float waitIdleDelay = 5f;
    public float moveThreshold = 0.1f;

    private Animator anim;
    private GuardAI guardAI;
    private float idleTimer = 0f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        guardAI = GetComponent<GuardAI>();
    }

    void OnEnable()
    {
        if (guardAI != null)
            guardAI.OnAttack += HandleAttack;
    }

    void OnDisable()
    {
        if (guardAI != null)
            guardAI.OnAttack -= HandleAttack;
    }

    void Update()
    {
        if (anim == null || guardAI == null) return;

        float speed = guardAI.CurrentSpeed;
        anim.SetFloat(speedParam, speed, dampTime, Time.deltaTime);

        if (speed <= moveThreshold)
            idleTimer += Time.deltaTime;
        else
            idleTimer = 0f;

        anim.SetBool(isWaitingIdleParam, idleTimer >= waitIdleDelay);
    }

    void HandleAttack()
    {
        if (anim == null) return;

        int choose = UnityEngine.Random.Range(0, 4);
        anim.SetInteger(chooseParam, choose);
        anim.SetTrigger(attackTrigger);

        idleTimer = 0f;
        anim.SetBool(isWaitingIdleParam, false);
    }
}