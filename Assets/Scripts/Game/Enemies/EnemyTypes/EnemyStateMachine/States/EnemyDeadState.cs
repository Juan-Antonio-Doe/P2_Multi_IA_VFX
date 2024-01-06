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

        if (!enemy.Rb.isKinematic)
            enemy.Rb.velocity = Vector3.zero;

        enemy.gameObject.SetActive(false);
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

        if (!enemy.Rb.isKinematic) {
            enemy.Rb.velocity = Vector3.zero;
            enemy.Rb.angularVelocity = Vector3.zero;
        }

        agent.enabled = true;
        agent.ResetPath();
    }
}