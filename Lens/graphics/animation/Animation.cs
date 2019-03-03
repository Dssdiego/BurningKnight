﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lens.graphics.animation {
	public class Animation {
		public AnimationData Data;
		
		private AnimationFrame frame;
		private uint currentFrame;
		public uint StartFrame { get; private set; }
		public uint EndFrame { get; private set; }
		
		public float SpeedModifier = 1f;
		public bool Paused;

		public uint Frame {
			get => currentFrame;
			set => currentFrame = value % (EndFrame - StartFrame + 1);
		}
		
		private string layer;
		private string tag;

		public string Layer {
			get => layer;

			set {
				if (layer == value) {
					return;
				}
				
				layer = value;
				ReadFrame();
			}
		}
		
		public string Tag {
			get => tag;

			set {
				if (tag == value) {
					return;
				}
				
				tag = value;
				ReadFrame();
			}
		}
		
		private float timer;

		public float Timer {
			get => timer;

			set {
				timer = value;

				if (timer >= frame.Duration) {
					timer = 0;
					Frame++;

					if (SkipNextFrame) {
						SkipNextFrame = false;
						Frame++;
					}
					
					ReadFrame();
				}
			}
		}

		public bool PingGoingForward;
		public bool SkipNextFrame;
		
		public Animation(AnimationData data, string layer = null) {
			Data = data;

			if (layer != null) {
				Layer = layer;
			} else {
				ReadFrame();				
			}
		}

		public void Update(float dt) {
			if (!Paused) {
				Timer += dt * SpeedModifier;
			}
		}

		// 		public static void Render(TextureRegion region, Vector2 position, float a, Vector2 origin, Vector2 scale, SpriteEffects flip) {
		public void Render(Vector2 position, bool flipped = false) {
			Graphics.Render(frame.Texture, position, 0, Vector2.Zero /* fixme */, Vector2.One, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
		}
		
		public void Reset() {
			Frame = 0;
		}

		private void ReadFrame() {
			var nullableTag = Data.GetTag(this.tag);

			if (nullableTag == null) {
				return;
			}

			var tag = nullableTag.Value;

			StartFrame = tag.StartFrame;
			EndFrame = tag.EndFrame;
			
			var frame = Data.GetFrame(layer, tag.Direction.GetFrameId(this));

			if (frame != null) {
				this.frame = (AnimationFrame) frame;
			}
		}
	}
}