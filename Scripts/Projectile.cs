using Godot;
using System;

public partial class Projectile : RigidBody3D {
	// exported variables
	[Export] public Timer LifespanTimer;

	// static variables
	public static readonly int knockbackStrength = 10;

	// instance variables
	public int Damage { get; set; } = 25;

	public override void _Ready() {
		ContactMonitor = true;
		MaxContactsReported = 2;
		// connect signals
		LifespanTimer.Connect("timeout", Callable.From(() => PopBubble()));
		Connect("body_entered", Callable.From((PhysicsBody3D body) => OnCollision(body)));
	}

    public void PopBubble() {
		QueueFree();
	}

	private void OnCollision(PhysicsBody3D body) {
		if (body is not CharacterBody3D characterBody3D) return;
		if (characterBody3D.Name != "Player") return;
		PopBubble();
	}
}
