using Godot;
using System.Collections.Generic;

public partial class SoundCollection : Node3D {
	Dictionary<string, AudioStreamPlayer3D> SFXCollection = new();
	Dictionary<string, AudioStreamPlayer> MusicCollection = new();

	public override void _Ready() {
		foreach (var child in GetChildren()) {
			if (child is AudioStreamPlayer3D sfx) SFXCollection.Add(child.Name, sfx);
			else if (child is AudioStreamPlayer music) MusicCollection.Add(child.Name, music);
		}
	}

	public void Play(string name, Vector3 Position = new(), bool overlap = true) {
		if (MusicCollection.TryGetValue(name, out AudioStreamPlayer music)) {
			if (!music.Playing) music.Play();

		} else if (SFXCollection.TryGetValue(name, out AudioStreamPlayer3D sfx)) {
            if (!Position.IsZeroApprox()) sfx.Position = Position;
			if (overlap) sfx.Play();
			else if (!sfx.Playing) sfx.Play();
			
        } else GD.PushError($"{name} was not found in the collection");
    }

	public void Stop(string name) {
		if (MusicCollection.TryGetValue(name, out AudioStreamPlayer music)) music.Stop();
		else if (SFXCollection.TryGetValue(name, out AudioStreamPlayer3D sfx)) sfx.Stop();
		else GD.PushError($"{name} was not found in the collection");
	}

	public bool IsPlaying(string name) {
		if (MusicCollection.TryGetValue(name, out AudioStreamPlayer music)) {
			return music.Playing;

		} else if (SFXCollection.TryGetValue(name, out AudioStreamPlayer3D sfx)) {
			return sfx.Playing;

		} else GD.PushError($"{name} was not found in the collection");
		return false;
	}
}
