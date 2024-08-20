using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RobotMechanics;

public static class RobotMechanics {
    [System.Serializable]
    public class RobotState {
        public RobotMovementMechanics.MovementState movementState;

        public int totalArmor;
        public int intactArmor;
        public int power;
        public int nParts;

        public bool victory;

        public bool isDead;

        public enum Action { NONE, FAIL, REPAIRING, ADDING_ARMOR, ADDING_POWER }
        public Action currentAction;
        public float actionProgress;
    }
    [System.Serializable]
    public class RobotSettings {
        [Header("Movement")]
        public RobotMovementMechanics.MovementSettings movementSettings;
        public float speedMultiplier;

        [Header("Resources")]
        public int startTotalArmor;
        public int startPower;

        [Header("Weight")]
        public float weightPerArmor;
        public float weightPerPower;

        [Header("Exchange")]
        public int partsPerRepair;
        public int partsPerArmor;
        public int partsPerPower;

        [Header("Actions")]
        public float repairTime;
        public float addArmorTime;
        public float addPowerTime;
    }

    [System.Serializable]
    public class RobotControl {
        public RobotMovementMechanics.RobotMovementControl movementControl;
        public bool doRepair;
        public bool doAddArmor;
        public bool doAddPower;
    }

    public static float CalculateWeight(RobotState state, RobotSettings settings) {
        return state.totalArmor * settings.weightPerArmor + state.power * settings.weightPerPower;
    }
    public static float CalculateMovementSpeed(RobotState state, RobotSettings settings) {
        if(state.currentAction != RobotState.Action.NONE && state.currentAction != RobotState.Action.REPAIRING) {
            return 0;
        }
        return settings.speedMultiplier * state.power / CalculateWeight(state, settings);
    }
    public static void Initialize(RobotState state, RobotSettings settings) {
        state.totalArmor = settings.startTotalArmor + (IntroCoordinator.introCompleted ? 1 : 0);
        state.intactArmor = state.totalArmor;
        state.power = settings.startPower + (IntroCoordinator.introCompleted ? 1 : 0);
    }

    public static void UpdateMechanics(RobotState state, RobotSettings settings, RobotControl control, RobotMovementMechanics.RobotCollider collider, float dt) {
        if(state.victory) {
            return;
        }
        UpdateActions(state, settings, control, dt);

        settings.movementSettings.climbSpeed = CalculateMovementSpeed(state, settings);
        state.movementState.position = collider.rb.position;
        var movementInfo = RobotMovementMechanics.UpdateMovementMechanics(state.movementState, settings.movementSettings, control.movementControl, collider, dt);
        collider.rb.MovePosition(state.movementState.position);
    }

    public static void TakeDamage(RobotState state, int damage) {
        FXPlayer.Play(FXPlayer.Instance.damageTakenSound);
        if (state.intactArmor == 0 && IntroCoordinator.introCompleted) {
            state.isDead = true;
            FXPlayer.Play(FXPlayer.Instance.deathSound);
            return;
        }
        state.intactArmor = Mathf.Max(0, state.intactArmor - damage);
    }

    private static void UpdateActions(RobotState state, RobotSettings settings, RobotControl control, float dt) {
        bool shouldRepair = control.doRepair && state.intactArmor < state.totalArmor && state.nParts >= settings.partsPerRepair;
        bool shouldAddArmor = control.doAddArmor && state.nParts >= settings.partsPerArmor;
        bool shouldAddPower = control.doAddPower && state.nParts >= settings.partsPerPower;

        if(!control.doRepair && !control.doAddArmor && !control.doAddPower) {
            state.currentAction = RobotState.Action.NONE;
        }

        if (state.currentAction == RobotState.Action.NONE) {
            if ((control.doRepair && !shouldRepair) || (control.doAddArmor && !shouldAddArmor) || (control.doAddPower && !shouldAddPower)) {
                FXPlayer.Play(FXPlayer.Instance.errorSound);
                state.currentAction = RobotState.Action.FAIL;
            }
        }
        if(state.currentAction == RobotState.Action.FAIL) {
            return;
        }

        var goalAction = shouldRepair ? RobotState.Action.REPAIRING :
            (shouldAddPower ? RobotState.Action.ADDING_POWER :
            (shouldAddArmor ? RobotState.Action.ADDING_ARMOR : RobotState.Action.NONE));

        float timeToGoal = shouldRepair ? settings.repairTime :
            (shouldAddPower ? settings.addPowerTime :
            (shouldAddArmor ? settings.addArmorTime : Mathf.Infinity));

        if(goalAction != state.currentAction) {
            if(goalAction != RobotState.Action.NONE) {
                FXPlayer.Play(FXPlayer.Instance.buildSound);
            }
            state.currentAction = goalAction;
            state.actionProgress = 0;
        }

        state.actionProgress = Mathf.Min(1, state.actionProgress + dt/timeToGoal);

        if(state.actionProgress == 1) {
            if(state.currentAction == RobotState.Action.REPAIRING) {
                state.intactArmor++;
                FXPlayer.Play(FXPlayer.Instance.repairCompleteSound);
                state.nParts -= settings.partsPerRepair;
            } else if(state.currentAction == RobotState.Action.ADDING_ARMOR) {
                state.intactArmor++;
                FXPlayer.Play(FXPlayer.Instance.buildCompleteSound);
                state.totalArmor++;
                state.nParts -= settings.partsPerArmor;
            } else if(state.currentAction == RobotState.Action.ADDING_POWER) {
                state.power++;
                FXPlayer.Play(FXPlayer.Instance.buildCompleteSound);
                state.nParts -= settings.partsPerPower;
            }
            state.currentAction = RobotState.Action.NONE;
        }
    }
    public static void HandleTriggerCollision(RobotState state, RobotSettings settings, RobotControl control, Collider2D collider) {
        // Pick up parts
        var p = collider.GetComponentInParent<PartController>();
        if (p != null) {
            state.nParts++;
            FXPlayer.Play(FXPlayer.Instance.partCollectedSound);
            p.Pickup();
        }

        if(collider.name == "Victory") {
            state.victory = true;
        }
    }
}