using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class DebugCarAnimationManager : MonoBehaviour
{
    private Animator animator;
    public float cancelLeftHandRigDelay;
    public float activateLeftHandRigDelay;
    public Rig leftHandRig;
    public Animator carDoorAnimator;
    public Transform CarDriveTarget;
    public Transform EnterKenarTarget;
    public TwoBoneIKConstraint twoBoneIKConstraint;
    public Transform nextTarget; //sonraki tutacaðý nokta
    public RigBuilder rigBuilder;
    private void Start()
    {
        animator = GetComponent<Animator>();
        leftHandRig.weight = 0;
    }

    public void CarEnterAnimation()
    {
        StartCoroutine(EnterCarRoutine());
    }

    public void CarExitAnimation()
    {
        animator.SetTrigger("ExitCar");
    }

    IEnumerator EnterCarRoutine()
    {
        animator.SetTrigger("EnterCar");
        Invoke(nameof(EnterCarTrigger), 1f);
        Invoke(nameof(EnableHandWeight), 0.1f);
        Invoke(nameof(MoveToSide), 1.25f);
        Invoke(nameof(CloseDoorTrigger), 3f);
        Invoke(nameof(NextTarget),2f);
        Invoke(nameof(DisableHandWeight), 1.75f);
        yield return null;
    }

    void EnterCarTrigger()
    {
        carDoorAnimator.SetTrigger("OpenDoor");
    }

    void CloseDoorTrigger()
    {
        carDoorAnimator.SetTrigger("CloseDoor");
    }

    void MoveToSeat()
    {
        transform.DOMove(CarDriveTarget.position, 3f);
    }

    void EnableHandWeight()
    {
        DOTween.To(() => leftHandRig.weight, x => leftHandRig.weight = x, 1f, 1).SetEase(Ease.OutSine);
    }

    void DisableHandWeight()
    {
        DOTween.To(() => leftHandRig.weight, x => leftHandRig.weight = x, 0.5f, 1).SetEase(Ease.InSine);
    }


    void MoveToSide()
    {
        transform.DOMove(EnterKenarTarget.position, 0.5f).OnComplete(() =>
        {
            Invoke(nameof(MoveToSeat), 0.5f);
        });
    }

    void NextTarget()
    {
        EnableHandWeight();
        twoBoneIKConstraint.data.target = nextTarget;
        rigBuilder.Build();
    }
}