using Godot;
using System;

public partial class AmmoCounter : Label {
	// exported variables
	[Export] Player Player;

	public override void _Ready() {
		Text = $"{Player.Bullets}/{Player.ClipSize}";
	}

	public override void _Process(double delta) {
		Text = $"{Player.Bullets}/{Player.ClipSize}";
	}
}
