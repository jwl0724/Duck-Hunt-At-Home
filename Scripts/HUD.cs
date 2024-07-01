using Godot;
using System;

public partial class HUD : Control {
	// exported variables
	[Export] public Player Player;
	[Export] public Hitscan HitscanLine;
	
	// HUD elements
	[Export] public Control ReticleParent;
	[Export] public Control TimeAndScore;

	public override void _Ready() {
		// connect signals
		Player.Connect("PlayerShoot", Callable.From(() => OnPlayerShoot()));
		ReticleParent.GetNode<Timer>("HitMarkerDuration").Connect("timeout", 
			Callable.From(() => ReticleParent.GetNode<Sprite2D>("HitMarker").Visible = false));
		UpdateScore(0);
		UpdateTime(0);
	}

	public override void _Process(double delta) {
		ProcessReticleColor();
	}

	public void UpdateScore(int score) {
		TimeAndScore.GetNode<Label>("ScoreLabel").Text = $"Score: {score}";
	}

	public void UpdateTime(float time) {
		TimeSpan timeSpan = TimeSpan.FromSeconds(time);
		string timeText = timeSpan.ToString(@"mm\:ss");
		TimeAndScore.GetNode<Label>("TimeLabel").Text = timeText;
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
		if (Player.Health <= 0) {
			ReticleParent.GetNode<AnimatedSprite2D>("ReticleSprite").SelfModulate = new Color(); // invisible
			return;
		}
		if (HitscanLine.GetCollider() is Enemy)
			ReticleParent.GetNode<AnimatedSprite2D>("ReticleSprite").SelfModulate = new Color(1, 0, 0, 1); // red
		else
			ReticleParent.GetNode<AnimatedSprite2D>("ReticleSprite").SelfModulate = new Color(1, 1, 1, 1); // white
	}
}
