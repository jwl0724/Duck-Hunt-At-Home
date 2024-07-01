using Godot;
using System;

public partial class HealthBar : TextureProgressBar {
	[Export] Player Player;
	public override void _Ready() {
		MaxValue = Player.MaxHealth;
		Value = MaxValue;
	}

	public override void _Process(double delta) {
		Value = Player.Health;
	}
}
