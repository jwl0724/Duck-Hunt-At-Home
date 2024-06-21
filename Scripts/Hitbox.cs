using Godot;
using System;

public partial class Hitbox : Area3D {
	[Signal] public delegate void EntityDiedEventHandler();
	private CharacterBody3D parentBody;
	public override void _Ready() {
		parentBody = GetParent<CharacterBody3D>();
		Connect("area_entered", Callable.From((Area3D body) => OnAreaEntered(body)));
	}
	public override void _Process(double delta) {
		
	}

	private void OnAreaEntered(Area3D body) {
		// TODO LATER WHEN ENEMIES ARE ADDED
	}
}
