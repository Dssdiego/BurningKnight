﻿using System;
using BurningKnight.entity.events;
using BurningKnight.physics;
using BurningKnight.util;
using ImGuiNET;
using Lens.entity;
using Lens.entity.component;
using Lens.util;
using Lens.util.file;
using Lens.util.math;
using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics;

namespace BurningKnight.entity.component {
	public class BodyComponent : SaveableComponent {
		public Body Body;
		public Vector2 Acceleration;
		public Vector2 Knockback;
		public float KnockbackModifier = 1;
		public Vector2 Offset;
		public bool Slow;
		public bool CanCollide = true;
		public bool Confused;
		
		public Vector2 Velocity {
			get => Slow ? Body.LinearVelocity * 2 : Body.LinearVelocity;
			set => Body.LinearVelocity = Slow ? value / 2 : value;
		}

		public Vector2 Position {
			get => Body.Position;
			set => Body.Position = value;
		}

		public float Angle {
			get => Body.Rotation;
			set => Body.Rotation = value;
		}
		
		protected virtual void PositionChangedListener() {
			if (Body != null) {
				try {
					Body.Position = Entity.Position + Offset;
				} catch (Exception e) {
					Log.Error(e);
				}
			}
		}

		public override void Init() {
			base.Init();
			Entity.PositionChanged += PositionChangedListener;
			
			if (Body != null) {
				try {
					Body.Position = Entity.Position + Offset;
				} catch (Exception e) {
					Log.Error(e);
				}
			}
		}

		public override void Destroy() {
			base.Destroy();
			Entity.PositionChanged -= PositionChangedListener;

			if (Body != null) {
				try {
					Physics.RemoveBody(Body);
				} catch (Exception e) {
					Log.Error(e);
				}

				Body = null;
			}
		}

		public virtual bool ShouldCollide(Entity entity) {
			if (Entity is CollisionFilterEntity filter) {
				return filter.ShouldCollide(entity);
			}
			
			return true;
		}
		
		public virtual void OnCollision(Entity entity) {
			Entity.HandleEvent(new CollisionStartedEvent {
				Entity = entity,
				Body = this
			});
		}

		public virtual void OnCollisionEnd(Entity entity) {
			Entity.HandleEvent(new CollisionEndedEvent {
				Entity = entity,
				Body = this
			});
		}

		public void KnockbackFrom(Entity entity, float force = 1f, float rnd = 0) {
			if (entity == null) {
				return;
			}

			if (entity.TryGetComponent<StatsComponent>(out var s)) {
				force *= s.Knockback;
			}
			
			if (entity == Entity) {
				KnockbackFrom(Rnd.AnglePI(), force, rnd);
			} else {
				KnockbackFrom(entity.Center, force, rnd);
			}
		}

		public void KnockbackFrom(Vector2 point, float force = 1f, float rnd = 0) {
			KnockbackFrom(Entity.AngleTo(point) - (float) Math.PI, force, rnd);
		}

		public virtual void KnockbackFrom(float a, float force = 1f, float rnd = 0) {		
			force *= KnockbackModifier * 30;

			if (force <= 0.02f) {
				return;
			}
			
			if (rnd > 0.01f) {
				a += Rnd.Float(-rnd, rnd);
			}

			Knockback.X += (float) Math.Cos(a) * force;
			Knockback.Y += (float) Math.Sin(a) * force;
		}

		public override void Update(float dt) {
			base.Update(dt);
			
			if (Body == null) {
				return;
			}

			var velocity = Body.LinearVelocity;
			velocity.X += Acceleration.X + Knockback.X * dt * 60;
			velocity.Y += Acceleration.Y + Knockback.Y * dt * 60;

			Knockback.X -= Knockback.X * dt * 10f;
			Knockback.Y -= Knockback.Y * dt * 10f;

			if (Entity.GraphicsComponent != null && !Entity.GraphicsComponent.CustomFlip && velocity.Length() > 0.1f) {
				Entity.GraphicsComponent.Flipped = velocity.X < 0;
			}
			
			Body.LinearVelocity = velocity;
			Entity.Position = Body.Position - Offset;
		}

		public override void Load(FileReader reader) {
			base.Load(reader);
			Body?.SetTransform(Entity.Position, 0);
		}

		public override void RenderDebug() {
			ImGui.DragFloat("Knockback modifier", ref KnockbackModifier);
			
			if (Body == null) {
				ImGui.BulletText("Body is null");
			} else {
				var vel = Body.LinearVelocity;
				var v = new System.Numerics.Vector2(vel.X, vel.Y);

				if (ImGui.DragFloat2("Velocity", ref v)) {
					Body.LinearVelocity = vel;
				}
			
				if (ImGui.Button("Sync body")) {
					PositionChangedListener();
				}
			}
		}

		public virtual void Resize(float x, float y, float w, float h, bool center = false) {
			
		}
	}
}