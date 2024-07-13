using DG.Tweening;
using StarterAssets;
using System.Collections;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class CarAnimationManager : MonoBehaviour
{
    /// <summary>
    /// �nce kap�y� se�meliyim sonra ilgili transformlar� almal�y�m.
    /// </summary>
    private Animator animator;
    [SerializeField] private AnimatorController leftEnterAnimator;
    [SerializeField] private AnimatorController rightEnterAnimator;

    private CharacterController characterController;
    private ThirdPersonController thirdPersonController;

    [Header("Scene Controllers")]
    [SerializeField] private MSSceneControllerFree mSSceneControllerFree;
    [SerializeField] private PlayerUIVisibilityManager mPlayerUIVisibilityManager;

    [Header("Animation Rigging")]
    [SerializeField] private Rig leftHandRig;
    [SerializeField] private RigBuilder rigBuilder;
    private Animator carDoorAnimator;
    private Transform enterCarPoint;
    [SerializeField] private Transform enterCarPointLeft;
    [SerializeField] private Transform enterCarPointRight;
    private Transform EnterKenarTarget;
    [SerializeField] private Transform enterKenarTargetLeft;
    [SerializeField] private Transform enterKenarTargetRight;
    private Transform seatTransform;
    [SerializeField] private Transform seatTransformLeft;
    [SerializeField] private Transform seatTransformRight;
    [SerializeField] private TwoBoneIKConstraint twoBoneIKConstraint;
    private Transform handTarget;
    private Transform nextTarget; //sonraki tutaca�� nokta

    [Header("UI Elements")]
    [SerializeField] private Button enterAndExitButton;

    internal bool isCharacterFullySeated;
    internal bool moveToSeat;
    private Transform closestDoor;

    private void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        leftHandRig.weight = 0;

        InitCarEnterExitCheck();
    }

    public void InitCarEnterExitCheck()
    {
        if (mSSceneControllerFree.startInPlayer)
        {
            enterAndExitButton.onClick.AddListener(() => CarEnterAnimation());
        }
        else
        {
            enterAndExitButton.onClick.AddListener(() => CarExitAnimation());
        }
    }

    void GetSelectedDoorTransforms()
    {
        mSSceneControllerFree.FindClosestDoorTransform();
        this.closestDoor = mSSceneControllerFree.closestDoor;
        carDoorAnimator = closestDoor.GetComponent<Animator>();

        if (closestDoor.name == "door_FR")
        {
            animator.runtimeAnimatorController = rightEnterAnimator;
            enterCarPoint = enterCarPointRight;
            EnterKenarTarget = enterKenarTargetRight;
            seatTransform = seatTransformRight;
        }
        else
        {
            animator.runtimeAnimatorController = leftEnterAnimator;
            enterCarPoint = enterCarPointLeft;
            EnterKenarTarget = enterKenarTargetLeft;
            seatTransform = seatTransformLeft;
        }

        handTarget = closestDoor.GetChild(0).transform;
        nextTarget = closestDoor.GetChild(1).transform;
    }

    public void CarEnterAnimation()
    {
        //Debug.Log("Arabaya binme �al��t�");
        GetSelectedDoorTransforms();
        StartCoroutine(EnterCarRoutine());
        enterAndExitButton.onClick.RemoveAllListeners();

        enterAndExitButton.onClick.AddListener(() => mSSceneControllerFree.Mobile_EnterAndExitVehicle());
        enterAndExitButton.onClick.AddListener(() => mPlayerUIVisibilityManager.UIVisibility());
        enterAndExitButton.onClick.AddListener(() => CarExitAnimation());
    }

    public void CarExitAnimation()
    {
        //Debug.Log("Arabadan inme �al��t�");
        StartCoroutine(ExitCarRoutine());
        enterAndExitButton.onClick.RemoveAllListeners();

        enterAndExitButton.onClick.AddListener(() => mSSceneControllerFree.Mobile_EnterAndExitVehicle());
        enterAndExitButton.onClick.AddListener(() => mPlayerUIVisibilityManager.UIVisibility());
        enterAndExitButton.onClick.AddListener(() => CarEnterAnimation());
    }

    #region Car Enter Animations
    IEnumerator EnterCarRoutine()
    {
        twoBoneIKConstraint.data.target = handTarget;
        rigBuilder.Build();
        thirdPersonController.disableLook = true;
        animator.applyRootMotion = true;
        characterController.enabled = false;
        transform.position = enterCarPoint.position;
        transform.DOLookAt(handTarget.position, 0.5f, AxisConstraint.Y, Vector3.up);
        animator.SetTrigger("EnterCar");
        Invoke(nameof(OpenDoorTrigger), 1f);
        Invoke(nameof(EnableHandWeight), 0.1f);
        moveToSeat = true;
        Invoke(nameof(MoveToSide), 1.25f);
        Invoke(nameof(CloseDoorTrigger), 3.25f);
        Invoke(nameof(NextTarget), 2f);
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
        transform.DOMove(seatTransform.position, 2.75f).OnComplete(() =>
        {
            transform.parent = seatTransform;

            isCharacterFullySeated = true;
        });
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
        });
    }

    void NextTarget()
    {
        EnableHandWeight();
        twoBoneIKConstraint.data.target = nextTarget;
        rigBuilder.Build();
    }
    #endregion

    #region Car Exit Animations

    IEnumerator ExitCarRoutine()
    {
        isCharacterFullySeated = false;
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

    void ActivatePlayerBehaviors()
    {
        transform.parent = null;
        thirdPersonController.disableLook = false;
        transform.position = enterCarPoint.position;
        leftHandRig.weight = 0;
        animator.applyRootMotion = false;
        characterController.enabled = true;
    }
    #endregion
}