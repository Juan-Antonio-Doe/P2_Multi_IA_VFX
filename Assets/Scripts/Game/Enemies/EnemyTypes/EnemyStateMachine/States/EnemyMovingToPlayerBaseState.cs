using UnityEngine.AI;

public class EnemyMovingToPlayerBaseState : EnemyState {

    public EnemyMovingToPlayerBaseState(Enemy enemy, NavMeshAgent agent) : base(enemy, agent) {
        currentState = STATE.MovingToPlayerBase;

        agent.speed = enemy.MoveSpeed;
        agent.isStopped = false;
        agent.autoBraking = false;
    }

    public override void Update() {
        base.Update();

        CheckersOnUpdate();

        agent.SetDestination(enemy.enemies.PlayerBase.position);
    }

    public override void Exit() {
        agent.ResetPath();

        base.Exit();
    }

    void CheckersOnUpdate() {
        CheckTargetDetected();
    }
}