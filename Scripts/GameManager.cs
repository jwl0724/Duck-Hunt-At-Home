using Godot;
using System;

public partial class GameManager : Node {
	// exported variables

	// signals

	// instance variables
	private Player player;
	
	public override void _Ready() {
		player = GetNode<Player>("Player");
	}

	
	public override void _Process(double delta) {
		
	}
}
