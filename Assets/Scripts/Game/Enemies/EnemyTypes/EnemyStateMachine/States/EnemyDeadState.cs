using UnityEngine;
using UnityEngine.AI;

public class EnemyDeadState : EnemyState {

    public EnemyDeadState(Enemy enemy, NavMeshAgent agent) : base(enemy, agent) {
        currentState = STATE.Dead;

        agent.isStopped = true;
    }

    public override void Enter() {
        base.Enter();

        inDeadState = true;
        agent.enabled = false;
    }

    /*public override void Update() {
        base.Update();
    }*/

    public override void Exit() {
        Reset();

        base.Exit();
    }

    void Reset() {
        enemy.IsDead = false;
        inDeadState = false;
        agent.enabled = true;
        agent.ResetPath();
    }
}