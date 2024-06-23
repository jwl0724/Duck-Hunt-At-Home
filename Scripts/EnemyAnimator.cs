using Godot;
using System;

public partial class EnemyAnimator : Node {
	[Export] public Enemy enemy;
	private AnimationPlayer animator;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		animator = GetNode<AnimationPlayer>("Animation");
		
		// connect shoot signal 
		Connect("EnemyShoot", Callable.From(() => animator.Play("shoot")));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		if (enemy.CurrentState == Enemy.MoveState.Idle) {
			animator.Stop();			
		} else if (enemy.CurrentState == Enemy.MoveState.Walking) {
			animator.Play("walk");
		} else if (enemy.CurrentState == Enemy.MoveState.Falling) {
			animator.Play("fall");
		} else if (enemy.CurrentState == Enemy.MoveState.Charging) {
			animator.Play("charge");
		}
	}
}
