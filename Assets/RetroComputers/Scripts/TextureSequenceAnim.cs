using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureSequenceAnim : MonoBehaviour {

	public List<Texture> sequence;
	public float fps = 30;

	private float timer;
	private int texIndex;

	void Start() {
		//make sure frames are in order
		sequence.Sort(delegate(Texture a, Texture b) {
			return a.name.CompareTo(b.name);
		});
	}
	
	void FixedUpdate () {
		timer += Time.deltaTime;

		if (timer > 1/fps) {
			timer = 0;
			++texIndex;

			if(texIndex >= sequence.Count)
				texIndex = 0;

		//renderer.material.mainTexture = sequence[texIndex];
		}
	}
}
