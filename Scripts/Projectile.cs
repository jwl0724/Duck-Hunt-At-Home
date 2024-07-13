using Godot;
using System;

// TODO FIX BUG WHERE PROJECTILES GET AFFECTED BY PLAYER MOVEMENT?
public partial class Projectile : RigidBody3D {
	// exported variables
	[Export] public Timer LifespanTimer;
	[Export] public AudioStreamPlayer3D PopSFX;
	[Export] public BubbleVisual Bubble;

	// instance variables
	public int Damage { get; set; } = 25;

	public override void _Ready() {
		ContactMonitor = true;
		MaxContactsReported = 10;
		
		// connect signals
		VisibleOnScreenNotifier3D visibilityNotifier = GetNode<VisibleOnScreenNotifier3D>("Visibility");
		visibilityNotifier.Connect("screen_entered", Callable.From(() => Bubble.Visible = true));
		visibilityNotifier.Connect("screen_exited", Callable.From(() => Bubble.Visible = false));
		LifespanTimer.Connect("timeout", Callable.From(() => PopBubble()));
		Connect("body_entered", Callable.From((PhysicsBody3D body) => OnCollision(body)));
		PopSFX.Connect("finished", Callable.From(() => DeleteBubble()));
	}

    public void PopBubble() {
		Visible = false;
		PopSFX.Play();
	}

	private void DeleteBubble() {
		foreach (var child in GetChildren()) {
			child.QueueFree();
		}
		QueueFree();
	}

	private void OnCollision(PhysicsBody3D body) {
		if (body is not CharacterBody3D characterBody3D) return;
		if (characterBody3D is not Player player) return;
		player.ApplyForce(LinearVelocity * 0.25f);
		player.DamagePlayer(Damage);
		PopBubble();
	}
}
