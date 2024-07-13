using DG.Tweening;
using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class DebugCarAnimationManager : MonoBehaviour
{
    private Animator animator;
    public CharacterController characterController;
    public float cancelLeftHandRigDelay;
    public float activateLeftHandRigDelay;
    public Rig leftHandRig;
    public Animator carDoorAnimator;
    public Transform CarDriveTarget;
    public Transform EnterKenarTarget;
    public Transform EnterCarPoint;
    public TwoBoneIKConstraint twoBoneIKConstraint;
    public Transform nextTarget; //sonraki tutacaðý nokta
    public RigBuilder rigBuilder;
    private bool moveToSeat;
    public ThirdPersonController thirdPersonController;

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
        StartCoroutine(ExitCarRoutine());
    }

    #region Car Enter Animations
    IEnumerator EnterCarRoutine()
    {
        thirdPersonController.disableLook = true;
        animator.applyRootMotion = true;
        characterController.enabled = false;
        transform.position = EnterCarPoint.position;
        Vector3 lookDir = new Vector3(EnterCarPoint.position.x, transform.position.y, EnterCarPoint.position.z);
        transform.DOLookAt(lookDir, 0.5f);
        animator.SetTrigger("EnterCar");
        Invoke(nameof(OpenDoorTrigger), 1f);
        Invoke(nameof(EnableHandWeight), 0.1f);
        moveToSeat = true;
        Invoke(nameof(MoveToSide), 1.25f);
        Invoke(nameof(CloseDoorTrigger), 3f);
        Invoke(nameof(NextTarget),2f);
        Invoke(nameof(DisableHandWeight), 1.75f);
        yield return null;
    }

    void OpenDoorTrigger()
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
            if (moveToSeat)
            {
                Invoke(nameof(MoveToSeat), 0.5f);
            }
            else
            {

            }
        });
    }

    void NextTarget()
    {
        EnableHandWeight();
        twoBoneIKConstraint.data.target = nextTarget;
        rigBuilder.Build();
    }
    #endregion

    #region

    IEnumerator ExitCarRoutine()
    {
        animator.SetTrigger("ExitCar");
        Invoke(nameof(OpenDoorTrigger), 1f);
        Invoke(nameof(EnableHandWeight), 0.1f);
        moveToSeat = false;
        Invoke(nameof(MoveToSide), 2.75f);
        Invoke(nameof(CloseDoorTrigger), 3f);
        //Invoke(nameof(NextTarget), 2f);
        //Invoke(nameof(DisableHandWeight), 3.55f);
        Invoke(nameof(ActivatePlayerBehaviors), 4);
        yield return null;
    }
    #endregion

    void ActivatePlayerBehaviors()
    {
        thirdPersonController.disableLook = false;
        transform.position = EnterCarPoint.position;
        leftHandRig.weight = 0;
        animator.applyRootMotion = false;
        characterController.enabled = true;
    }
}