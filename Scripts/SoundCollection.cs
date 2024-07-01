using Godot;
using System.Collections.Generic;

public partial class SoundCollection : Node3D {
	Dictionary<string, AudioStreamPlayer3D> SFXCollection = new();

	public override void _Ready() {
		foreach (var child in GetChildren()) {
			if (child is not AudioStreamPlayer3D audio) continue;
			SFXCollection.Add(child.Name, audio);
		}
	}

	public void Play(string name, Vector3 Position = new(), bool overlap = true) {
        if (SFXCollection.TryGetValue(name, out AudioStreamPlayer3D audio)) {
            if (!Position.IsZeroApprox()) audio.Position = Position;
			if (overlap) audio.Play();
			else if (!audio.Playing) audio.Play();
			
        } else GD.PushError($"{name} was not found in the collection");
    }

	public void Stop(string name) {
		if (SFXCollection.TryGetValue(name, out AudioStreamPlayer3D audio)) audio.Stop();
		else GD.PushError($"{name} was not found in the collection");
	}
}
