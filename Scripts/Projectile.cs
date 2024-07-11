using Godot;
using System;

// TODO FIX BUG WHERE PROJECTILES GET AFFECTED BY PLAYER MOVEMENT?
public partial class Projectile : RigidBody3D {
	// exported variables
	[Export] public Timer LifespanTimer;
	[Export] public AudioStreamPlayer3D PopSFX;

	// static variables
	public static readonly int KnockbackStrength = 5;

	// instance variables
	public int Damage { get; set; } = 25;

	public override void _Ready() {
		ContactMonitor = true;
		MaxContactsReported = 10;
		
		// connect signals
		LifespanTimer.Connect("timeout", Callable.From(() => PopBubble()));
		Connect("body_entered", Callable.From((PhysicsBody3D body) => OnCollision(body)));
		PopSFX.Connect("finished", Callable.From(() => QueueFree()));
	}

    public void PopBubble() {
		Visible = false;
		PopSFX.Play();
	}

	private void OnCollision(PhysicsBody3D body) {
		if (body is not CharacterBody3D characterBody3D) return;
		if (characterBody3D.Name != "Player") return;
		PopBubble();
	}
}
