using DG.Tweening;
using StarterAssets;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class CarAnimationManager : MonoBehaviour
{
    /// <summary>
    /// Önce kapýyý seçmeliyim sonra ilgili transformlarý almalýyým.
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
    private TwoBoneIKConstraint selectedTwoBoneIK;
    [SerializeField] private TwoBoneIKConstraint leftTwoBoneIKConstraint;
    [SerializeField] private TwoBoneIKConstraint rightTwoBoneIKConstraint;

    private Transform handDoorTarget;
    private Transform nextDoorTarget;

    [SerializeField] private Transform nextLeftDoorTarget; //sonraki tutacaðý nokta
    [SerializeField] private Transform nextRightDoorTarget; //sonraki tutacaðý nokta

    [SerializeField] private Transform leftSteerWheelTarget;
    [SerializeField] private Transform rightSteerWheelTarget;
    [Header("UI Elements")]
    [SerializeField] private Button enterAndExitButton;

    internal bool isCharacterFullySeated;
    internal bool moveToSeat;
    private bool seatChanging;
    private Transform closestDoor;

    private void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        thirdPersonController = GetComponent<ThirdPersonController>();

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

    void EnterExitButtonInteractable(bool block)
    {
        enterAndExitButton.interactable = block;
    }
    void AddEventListeners(string enterOrExitCar)
    {
        enterAndExitButton.onClick.RemoveAllListeners();
        enterAndExitButton.onClick.AddListener(() => mSSceneControllerFree.Mobile_EnterAndExitVehicle());
        enterAndExitButton.onClick.AddListener(() => mPlayerUIVisibilityManager.UIVisibility());

        if (enterOrExitCar == "CarEnter")
        {
            enterAndExitButton.onClick.AddListener(() => CarEnterAnimation());
        }
        else if (enterOrExitCar == "CarExit")
        {
            enterAndExitButton.onClick.AddListener(() => CarExitAnimation());
        }
    }

    void GetSelectedDoorTransforms()
    {
        mSSceneControllerFree.FindClosestDoorTransform();
        closestDoor = mSSceneControllerFree.closestDoor;

        if (closestDoor.name == "door_FR")
        {
            animator.runtimeAnimatorController = rightEnterAnimator;
            enterCarPoint = enterCarPointRight;
            EnterKenarTarget = enterKenarTargetRight;
            seatTransform = seatTransformRight;
            DOTween.To(() => leftTwoBoneIKConstraint.weight, x => leftTwoBoneIKConstraint.weight = x, 0f, 1).SetEase(Ease.InSine);
            DOTween.To(() => rightTwoBoneIKConstraint.weight, x => rightTwoBoneIKConstraint.weight = x, 1f, 1).SetEase(Ease.OutSine);
            selectedTwoBoneIK = rightTwoBoneIKConstraint;
        }
        else if (closestDoor.name == "door_FL")
        {
            animator.runtimeAnimatorController = leftEnterAnimator;
            enterCarPoint = enterCarPointLeft;
            EnterKenarTarget = enterKenarTargetLeft;
            seatTransform = seatTransformLeft;
            DOTween.To(() => leftTwoBoneIKConstraint.weight, x => leftTwoBoneIKConstraint.weight = x, 1f, 1).SetEase(Ease.OutSine);
            DOTween.To(() => rightTwoBoneIKConstraint.weight, x => rightTwoBoneIKConstraint.weight = x, 0f, 1).SetEase(Ease.InSine);
            selectedTwoBoneIK = leftTwoBoneIKConstraint;
        }

        carDoorAnimator = closestDoor.GetComponent<Animator>();
        handDoorTarget = closestDoor.GetChild(0);
        nextDoorTarget = closestDoor.GetChild(1);
    }

    public void CarEnterAnimation()
    {
        GetSelectedDoorTransforms();
        EnterCarRoutine();
        AddEventListeners("CarExit");
        EnterExitButtonInteractable(false);
    }

    public void CarExitAnimation()
    {
        GetSelectedDoorTransforms();
        ExitCarRoutine();
        AddEventListeners("CarEnter");
        EnterExitButtonInteractable(false);
    }

    #region Car Enter Animations
    void EnterCarRoutine()
    {
        seatChanging = false;
        selectedTwoBoneIK.data.target = handDoorTarget;
        rigBuilder.Build();
        thirdPersonController.disableLook = true;
        animator.applyRootMotion = true;
        characterController.enabled = false;
        transform.position = enterCarPoint.position;
        transform.DOLookAt(handDoorTarget.position, 0.01f, AxisConstraint.Y, Vector3.up);
        animator.SetTrigger("EnterCar");
        Invoke(nameof(OpenDoorTrigger), 1f);
        moveToSeat = true;
        Invoke(nameof(MoveToSide), 1.25f);
        Invoke(nameof(CloseDoorTrigger), 3.25f);
        Invoke(nameof(NextTarget), 2f);
        Invoke(nameof(DisableHandWeight), 2.25f);
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
            if (closestDoor.name == "door_FR")
            {
                Invoke(nameof(UseSteerWheelAnimation), 0.75f);

                transform.DOJump(seatTransformLeft.position, 0.06f, 2, 1.25f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    seatChanging = true;
                    isCharacterFullySeated = true;
                    EnterExitButtonInteractable(true);
                    transform.parent = seatTransformRight;
                });
            }
            else
            {
                isCharacterFullySeated = true;
                EnterExitButtonInteractable(true);
                Invoke(nameof(UseSteerWheelAnimation), 0.75f);
                transform.parent = seatTransform;
            }
        });
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
        selectedTwoBoneIK.data.target = nextDoorTarget;
        rigBuilder.Build();
    }
    #endregion

    #region Car Exit Animations

    void ExitCarRoutine()
    {
        if (seatChanging == true)
        {
            transform.DOLookAt(-handDoorTarget.position, 0.01f, AxisConstraint.Y);
        }
        selectedTwoBoneIK.data.target = handDoorTarget;
        rigBuilder.Build();
        isCharacterFullySeated = false;
        animator.SetTrigger("ExitCar");
        Invoke(nameof(OpenDoorTrigger), 1f);
        Invoke(nameof(EnableHandWeight), 0.1f);
        moveToSeat = false;
        Invoke(nameof(MoveToSide), 2.75f);
        Invoke(nameof(CloseDoorTrigger), 3f);
        Invoke(nameof(ActivatePlayerBehaviors), 4.6f);
    }

    void EnableHandWeight()
    {
        DOTween.To(() => selectedTwoBoneIK.weight, x => selectedTwoBoneIK.weight = x, 1f, 1).SetEase(Ease.OutSine);
    }

    void DisableHandWeight()
    {
        DOTween.To(() => selectedTwoBoneIK.weight, x => selectedTwoBoneIK.weight = x, 0f, 0.2f).SetEase(Ease.InSine);
    }

    void ActivatePlayerBehaviors()
    {
        EnterExitButtonInteractable(true);
        transform.parent = null;
        leftTwoBoneIKConstraint.weight = 0f;
        rightTwoBoneIKConstraint.weight = 0f;
        thirdPersonController.disableLook = false;
        transform.DOMove(enterCarPoint.position, 0.5f);
        animator.applyRootMotion = false;
        characterController.enabled = true;
    }
    #endregion


    void UseSteerWheelAnimation()
    {
        leftTwoBoneIKConstraint.data.target = leftSteerWheelTarget;
        DOTween.To(() => leftTwoBoneIKConstraint.weight, x => leftTwoBoneIKConstraint.weight = x, 1f, 1).SetEase(Ease.OutSine);

        rightTwoBoneIKConstraint.data.target = rightSteerWheelTarget;
        DOTween.To(() => rightTwoBoneIKConstraint.weight, x => rightTwoBoneIKConstraint.weight = x, 1f, 1).SetEase(Ease.OutSine);

        rigBuilder.Build();
    }
}