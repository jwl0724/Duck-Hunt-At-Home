using Godot;
using System;

public partial class Projectile : RigidBody3D {
	// exported variables
	[Export] public Timer LifespanTimer;

	// instance variables
	public int Damage { get; private set; } = 25;
	
	public override void _Ready() {
		// connect signals
		LifespanTimer.Connect("timeout", Callable.From(() => OnLifeSpanTimeout()));
	}

	public void PopBubble() {
		QueueFree();
	}

	private void OnLifeSpanTimeout() {
		QueueFree();
	}
}
