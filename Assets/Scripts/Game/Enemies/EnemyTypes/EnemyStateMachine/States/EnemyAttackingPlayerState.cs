using UnityEngine;
using UnityEngine.AI;

public class EnemyAttackingPlayerState : EnemyState {

    private float defaultStoppingDistance { get; set; } = 0.1f;
    private float defaultAttackCooldown { get; set; }

    public EnemyAttackingPlayerState(Enemy enemy, NavMeshAgent agent) : base(enemy, agent) {
        currentState = STATE.Attacking;

        agent.speed = enemy.MoveSpeed * 1.5f;
        defaultStoppingDistance = agent.stoppingDistance;
        agent.stoppingDistance = 1f;
    }

    public override void Enter() {
        defaultAttackCooldown = enemy.AttackCooldown;
        enemy.AttackCooldown = 0f;

        base.Enter();
    }

    public override void Update() {
        base.Update();

        if (!TargetDetected()) {
            ChangeState(new EnemyMovingToPlayerBaseState(enemy, agent));
            return;
        }

        SmoothLookAt(enemy.AttackTarget);
        //enemy.transform.LookAt(enemy.AttackTarget);

        GoToPlayerAndCooldown();
    }

    public override void Exit() {
        agent.ResetPath();
        agent.stoppingDistance = defaultStoppingDistance;
        enemy.AttackCooldown = defaultAttackCooldown;

        base.Exit();
    }

    void GoToPlayerAndCooldown() {
        if (enemy.AttackCooldown > 0f) {
            enemy.AttackCooldown -= Time.deltaTime;
        }
        else {
            if (agent.remainingDistance < agent.stoppingDistance) {
                agent.SetDestination(enemy.AttackTarget.position);
            }
        }

    }
}