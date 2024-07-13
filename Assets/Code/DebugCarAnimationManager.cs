using DG.Tweening;
using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

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
    public bool moveToSeat;
    public ThirdPersonController thirdPersonController;
    public Button enterAndExitButton;
    [SerializeField] private MSSceneControllerFree mSSceneControllerFree;
    [SerializeField] private PlayerUIVisibilityManager mPlayerUIVisibilityManager;
    internal bool isCharacterFullySeated;

    private void Start()
    {
        animator = GetComponent<Animator>();
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

    public void CarEnterAnimation()
    {
        //Debug.Log("Arabaya binme çalýþtý");
        StartCoroutine(EnterCarRoutine());
        enterAndExitButton.onClick.RemoveAllListeners();

        enterAndExitButton.onClick.AddListener(() => mSSceneControllerFree.Mobile_EnterAndExitVehicle());
        enterAndExitButton.onClick.AddListener(() => mPlayerUIVisibilityManager.UIVisibility());
        enterAndExitButton.onClick.AddListener(() => CarExitAnimation());
    }

    public void CarExitAnimation()
    {
        //Debug.Log("Arabadan inme çalýþtý");
        StartCoroutine(ExitCarRoutine());
        enterAndExitButton.onClick.RemoveAllListeners();

        enterAndExitButton.onClick.AddListener(() => mSSceneControllerFree.Mobile_EnterAndExitVehicle());
        enterAndExitButton.onClick.AddListener(() => mPlayerUIVisibilityManager.UIVisibility());
        enterAndExitButton.onClick.AddListener(() => CarEnterAnimation());
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
        Invoke(nameof(CloseDoorTrigger), 3.25f);
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
        transform.DOMove(CarDriveTarget.position, 2.75f).OnComplete(() =>
        {
            isCharacterFullySeated = true;

        });
        transform.parent = CarDriveTarget;
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
    #endregion

    void ActivatePlayerBehaviors()
    {
        transform.parent = null;
        thirdPersonController.disableLook = false;
        transform.position = EnterCarPoint.position;
        leftHandRig.weight = 0;
        animator.applyRootMotion = false;
        characterController.enabled = true;
    }
}