using Godot;
using System;

public partial class HUD : Control {
	// exported variables
	[Export] public Player Player;
	
	// HUD elements
	[Export] public TextureRect Crosshair;

	public override void _Ready() {
		Player.Connect("PlayerShoot", Callable.From(() => OnPlayerShoot()));
	}

	public override void _Process(double delta) {
		Crosshair.Scale = Crosshair.Scale.Lerp(new Vector2(1, 1), 0.1f);
	}

	// SIGNAL HANDLERS
	private void OnPlayerShoot() {
		Crosshair.Scale = Crosshair.Scale.Lerp(Crosshair.Scale * 1.5f, 0.9f);
	}
}
