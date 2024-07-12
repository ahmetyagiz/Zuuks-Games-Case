using UnityEngine;

public class DebugCarAnimationManager : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void CarEnterAnimation()
    {
        animator.SetTrigger("EnterCar");
    }

    public void CarExitAnimation()
    {
        animator.SetTrigger("ExitCar");
    }
}