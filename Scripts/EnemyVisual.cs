using Godot;
using System;

public partial class EnemyVisual : Node {
	[Export] public Enemy enemy;
	private AnimationPlayer animator;

	public override void _Ready() {
		animator = GetNode<AnimationPlayer>("Animation");
		enemy.Connect("EnemyShoot", Callable.From(() => animator.Play("shoot")));
		enemy.Connect("MoveStateChange", Callable.From(() => OnMoveStateChange()));
	}

	public void OnMoveStateChange() {
		// idle = don't play any animations
		animator.Stop();
		if (enemy.CurrentState == Enemy.MoveState.Walking) {
			animator.Play("walk");
		} else if (enemy.CurrentState == Enemy.MoveState.Falling) {
			animator.Play("fall");
		} else if (enemy.CurrentState == Enemy.MoveState.Charging) {
			animator.Play("charge");
		}
	}
}
