using Godot;
using System;

public partial class HUD : Control {
	// exported variables
	[Export] public Player Player;
	[Export] public Hitscan HitscanLine;
	
	// HUD elements
	[Export] public Control ReticleParent;

	public override void _Ready() {
		// connect signals
		Player.Connect("PlayerShoot", Callable.From(() => OnPlayerShoot()));
		ReticleParent.GetNode<Timer>("HitMarkerDuration").Connect("timeout", 
			Callable.From(() => ReticleParent.GetNode<Sprite2D>("HitMarker").Visible = false));
	}

	public override void _Process(double delta) {
		ProcessReticleColor();
	}

	// SIGNAL HANDLERS
	private void OnPlayerShoot() {
		ReticleParent.GetNode<AnimatedSprite2D>("ReticleSprite").Play("shoot");
		if (HitscanLine.GetCollider() is Enemy) {
			ReticleParent.GetNode<Sprite2D>("HitMarker").Visible = true;
			ReticleParent.GetNode<Timer>("HitMarkerDuration").Start();
		}
	}

	// HELPER FUNCTIONS
	private void ProcessReticleColor() {
		if (HitscanLine.GetCollider() is Enemy)
			ReticleParent.GetNode<AnimatedSprite2D>("ReticleSprite").SelfModulate = new Color(1, 0, 0, 1); // red
		else
			ReticleParent.GetNode<AnimatedSprite2D>("ReticleSprite").SelfModulate = new Color(1, 1, 1, 1); // white
	}
}
