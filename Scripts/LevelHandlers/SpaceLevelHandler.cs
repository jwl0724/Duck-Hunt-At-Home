using Godot;
using System;

public partial class SpaceLevelHandler : Node3D {
	[Export] Player Player;
	
	public override void _Ready() {
		Player.Gravity = 2;
		Player.JumpSpeed = 4;
		Player.GrappleSpeed = 300;
		(GetParent() as GameManager).AlwaysGlow = true;
	}
}
