using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyState {

    public enum STATE {
        MovingToPlayerBase,  // Enemy is moving to the player's base.
        MovingToPlayer,      // Enemy is moving forward to the player's position.
        Attacking,           // Enemy is attacking the player.
        Dead                 // Enemy is dead.
    }

    public enum STAGES {
        Enter,
        Update,
        Exit
    }

    public STATE currentState { get; set; }
    protected STAGES stage { get; set; }
    protected GameObject npc { get; set; }  // The current character gameobject.
    //protected PlayerManager player { get; set; }
    protected NavMeshAgent agent { get; set; }

    /*protected EnemyRunner enemyR { get; set; }
    protected EnemyFighter enemyF { get; set; }*/
    protected Enemy enemy { get; set; }
    protected EnemyState nextState { get; set; }

    protected bool inDeadState { get; set; } = false;

    public EnemyState(Enemy enemy, NavMeshAgent agent) {

        npc = enemy.gameObject;
        this.enemy = enemy;
        this.agent = agent;
        stage = STAGES.Enter;
    }

    /*public EnemyState(EnemyFighter enemy, NavMeshAgent agent) {

        npc = enemy.gameObject;
        this.enemyF = enemy;
        this.agent = agent;
        stage = STAGES.Enter;
    }*/

    public virtual void Enter() {
        //Debug.Log($"EnemyState: {npc.name} -> {currentState}");
        stage = STAGES.Update;
    }

    public virtual void Update() {
        if (enemy.IsDead && !inDeadState) {
            ChangeState(new EnemyDeadState(enemy, agent));
            return;
        }

        stage = STAGES.Update;
    }

    public virtual void Exit() {
        stage = STAGES.Exit;
    }

    /// <summary>
    /// This method is used to switch between the different methods that change the state.
    /// </summary>
    public EnemyState Process() {
        if (stage == STAGES.Enter) Enter();
        if (stage == STAGES.Update) Update();
        if (stage == STAGES.Exit) {
            Exit();
            //Debug.Log($"StateProcess: {npc.name} -> {currentState} -> {nextState.currentState}");
            return nextState; // It returns us the state that would touch next.
        }

        // This would return us to the same state we are in if none of the above conditions are met.
        return this;
    }

    /// <summary>
    /// Changes the state of the enemy.
    /// </summary>
    public void ChangeState(EnemyState nextState) {
        this.nextState = nextState;
        stage = STAGES.Exit;
        //Debug.Log($"EnemyState: {npc.name} -> {currentState} -> {nextState.currentState} \n Stage: {stage}");
    }

    /*protected bool CheckIfPlayerIsInRange() {
        float distanceSquared = (npc.transform.position - enemy.enemies.Player.playerTransform.position).sqrMagnitude;

        if (distanceSquared <= enemy.AttackRange * enemy.AttackRange) {
            return true;
        }

        return false;
    }*/

    protected bool TargetDetected() {
        if (enemy.AttackTarget != null && enemy.AttackTarget.gameObject.activeInHierarchy) {
            float distanceSquared = (npc.transform.position - enemy.AttackTarget.position).sqrMagnitude;
            if (distanceSquared <= enemy.AttackRange * enemy.AttackRange) {
                return true;
            }
        }

        enemy.AttackTarget = null;
        return false;
    }

    protected void CheckTargetDetected() {
        if (TargetDetected()) {
            if (enemy.AttackTarget.CompareTag("Player")) {
                ChangeState(new EnemyAttackingPlayerState(enemy, agent));
                return;
            }
        }
    }

    protected void SmoothLookAt(Transform target) {
        Vector3 direction = (target.position - npc.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}