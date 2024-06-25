using Godot;
using System;

public partial class Projectile : RigidBody3D {
	// exported variables
	[Export] public float Gravity = -0.5f;
	[Export] public Timer LifespanTimer;

	// instance variables
	private SphereMesh mesh;

	// signals
	[Signal] public delegate void CollideWithPlayerEventHandler();

	public override void _Ready() {
		ContactMonitor = true;
		MaxContactsReported = 10;

		// connect signals
		Connect("body_entered", Callable.From((PhysicsBody3D body) => OnCollision(body)));
		LifespanTimer.Connect("timeout", Callable.From(() => OnLifeSpanTimeout()));
	}

	public override void _Process(double delta) {
		
	}
	
	private void OnCollision(PhysicsBody3D body) {
		GD.Print(body.GetType());
	}

	private void OnLifeSpanTimeout() {
		QueueFree();
	}
}
