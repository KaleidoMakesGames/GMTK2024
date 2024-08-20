using Assets;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;

public class IntroCoordinator : MonoBehaviour
{
    public bool skip;
    public bool skipIntro {
        get {
            if(PlayerPrefs.HasKey("SkipIntro")) {
                return PlayerPrefs.GetFloat("SkipIntro") > 0;
            }
            return skip;
        }
    }

    public RobotController robotController;
    public CameraController cameraController;
    public PlayerInput input;
    public float climbUpHeight;
    public Collider2D startClimbCollider;
    public Transform indicator;
    public Transform climbIndicator;
    public HazardSpawner hazardSpawner;
    public enum Mode { ROAM, ATSPOT, CLIMBINGUP, DONE}

    public Mode currentMode;
    public Mode lastMode;

    private static IntroCoordinator _Instance;
    public static IntroCoordinator Instance {
        get {
            if(_Instance == null) {
                _Instance = FindFirstObjectByType<IntroCoordinator>();
            }
            return _Instance;
        }
    }

    public static bool introCompleted {
        get {
            return Instance.introDone;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (skipIntro) {
            introDone = true;
            currentMode = Mode.DONE;
            robotController.transform.position = new Vector2(0, FindFirstObjectByType<RockDestroyerCheck>().transform.position.y + 5);
            robotController.state.movementState.mode = RobotMovementMechanics.MovementMode.CLIMB;
            DialogueDisplay.Type("Command: SCALE\n[Keyboard WSAD]", 3.0f, delegate () { });
            FXPlayer.Play(FXPlayer.Instance.bassSound);
        } else {
            DialogueDisplay.Type("** Initializing **", 1.0f, delegate () {
                DialogueDisplay.Type("BUILD #" + Mathf.FloorToInt(Random.value * 1000) + ".\n", 1.0f, delegate () {
                    DialogueDisplay.Type("Command: APPROACH\n[Keyboard A + D]");
                });
            });
        }
        DoIntro();
    }

    void UpdateState() {
        switch (currentMode) {
            case Mode.ROAM:
                if (robotController.transform.position.x >= 0) {
                    currentMode = Mode.ATSPOT;
                    FXPlayer.Play(FXPlayer.Instance.bassSound);
                    DialogueDisplay.Type("STATUS: AT WALL", 1.0f, delegate () {
                        DialogueDisplay.Type("Command: SCALE\n[Keyboard W]");
                    });
                }
                break;
            case Mode.ATSPOT:
                if(robotController.control.movementControl.drive.y > 0) {
                    currentMode = Mode.CLIMBINGUP;
                    FXPlayer.Play(FXPlayer.Instance.bassSound);
                    DialogueDisplay.Type("** TRIAL INITIATED **");
                }
                break;
            case Mode.CLIMBINGUP:
                if (robotController.transform.position.y >= climbUpHeight) {
                    currentMode = Mode.DONE;
                    DialogueDisplay.Type("Command: SCALE\n[Keyboard WSAD]", 2.0f, delegate() { });
                }
                break;
        }
    }

    void DoIntro() {
        if (currentMode != Mode.DONE) {
            UpdateState();
        }

        startClimbCollider.enabled = currentMode == Mode.DONE;
        indicator.gameObject.SetActive(currentMode == Mode.ROAM);
        climbIndicator.gameObject.SetActive(currentMode != Mode.ROAM);
        cameraController.trackX = currentMode == Mode.ROAM;
        hazardSpawner.enabled = currentMode == Mode.DONE;
        robotController.control.movementControl.releaseClimb = false;

        switch (currentMode) {
            case Mode.ROAM:
                robotController.control.movementControl.drive = new Vector2(input.drive.x, 0);
                break;
            case Mode.CLIMBINGUP:
                robotController.control.movementControl.drive = Vector2.up;
                break;
            case Mode.ATSPOT:
                robotController.control.movementControl.drive = Vector2.zero;
                break;
            case Mode.DONE:
                if (introCompleted) {
                    DoBanter();
                } else {
                    CheckForDamage();
                    CheckForOre();
                    CheckForHeal();
                    CheckForArmorAdd();
                    CheckForInsufficient1();
                    CheckForInsufficient2();
                    CheckForPowerAdd();
                }
                break;
        }

    }

    bool _hasBeenDamaged = false;
    void CheckForDamage() {
        if(robotController.state.intactArmor < robotController.state.totalArmor && !_hasBeenDamaged) {
            _hasBeenDamaged = true;
            FXPlayer.Play(FXPlayer.Instance.bassSound);
            DialogueDisplay.Type("WARNING: ARMOR DAMAGED\n\nCommand: SEEK SCRAP");
        }
    }

    bool _hasPickedUpPart = false;
    int healthAtPickup = 0;
    void CheckForOre() {
        if(robotController.state.nParts > 0 && !_hasPickedUpPart) {
            DialogueDisplay.Type("** SCRAP OBTAINED **\n\nCommand: REPAIR\n[Keyboard Q]");
            _hasPickedUpPart = true;
            healthAtPickup = robotController.state.intactArmor;
        }
    }

    bool _hasHealed = false;
    void CheckForHeal() {
        if(_hasBeenDamaged && robotController.state.intactArmor > healthAtPickup && !_hasHealed) {
            DialogueDisplay.Type("** ARMOR REPAIRED **\n\nWARNING: AVOID HAZARDS\n\nCommand: FORTIFY\n[Keyboard E]");
            _hasHealed = true;
        }
    }

    bool _hasTriedInsufficient = false;
    void CheckForInsufficient1() {
        if(_hasHealed && robotController.control.doAddArmor && !_hasTriedInsufficient) {
            DialogueDisplay.Type("** INSUFFICIENT SCRAP **\n\nCommand: COLLECT 2");
            _hasTriedInsufficient = true;
        }
    }

    bool _hasAdded = false;
    void CheckForArmorAdd() {
        if(robotController.state.totalArmor > robotController.settings.startTotalArmor && !_hasAdded) {
            DialogueDisplay.Type("** ARMOR CAPACITY INCREASED **", 2.0f, delegate () {
                DialogueDisplay.Type("WARNING:\nChassis mass increased.\nSpeed reduced.\n\nCOMMAND: EMPOWER\n[Keyboard R]");
                });
            _hasAdded = true;
        }
    }
    bool _hasTriedInsufficient2 = false;
    void CheckForInsufficient2() {
        if (_hasHealed && robotController.control.doAddPower && !_hasTriedInsufficient2) {
            DialogueDisplay.Type("** INSUFFICIENT SCRAP **\n\nCommand: COLLECT 3");
            _hasTriedInsufficient2 = true;
        }
    }
    public bool introDone = false;
    bool _hasEmpowered = false;
    void CheckForPowerAdd() {
        if (robotController.state.power > robotController.settings.startPower && !_hasEmpowered) {
            DialogueDisplay.Type("** POWER INCREASED **", 2.0f, delegate () {
                DialogueDisplay.Type("WARNING:\nChassis width increased.\nAvoid hazards.\n\nCOMMAND: SCALE\n[Keyboard WSAD]", 5.0f, delegate () { introDone = true;
                    PlayerPrefs.SetFloat("SkipIntro", 1);
                });
            });
            _hasEmpowered = true;
        }
    }

    public float banterShowTime;
    public float banterDelayMin;
    public float banterDelayMax;
    private float _remainingDelay;
    [SerializeField][HideInInspector]private int currentBanter;
    void DoBanter() {
        if(introCompleted) {
            if(_remainingDelay == 0) {
                _remainingDelay = -1;
                string toShow = Banter.items[Random.Range(0, Banter.items.Length)];
                DialogueDisplay.Type(toShow, banterShowTime, delegate () {
                    _remainingDelay = Random.Range(banterDelayMin, banterDelayMax);
                });
            }
            if(_remainingDelay > 0) {
                _remainingDelay = Mathf.Max(0, _remainingDelay - Time.deltaTime);
            }
        }
    }


    // Update is called once per frame
    void LateUpdate() {
        DoIntro();
    }
}
