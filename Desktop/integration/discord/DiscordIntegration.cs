﻿using System;
using System.Runtime.InteropServices;
using BurningKnight.entity.component;
using BurningKnight.entity.creature.player;
using BurningKnight.level;
using BurningKnight.state;
using Lens;
using Lens.assets;
using Lens.util;

namespace Desktop.integration.discord {
	public class DiscordIntegration : Integration {
		public static string CurrentPlayer;
		
		private float lastUpdate;
		private long startTime;

		public bool Broken;

		private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static long CurrentTimeMillis() {
			return (long) (DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
		}

		public override void Init() {
			base.Init();

			startTime = CurrentTimeMillis() / 1000;

			try {
				DiscordRpc.Initialize("459603244256198657", new DiscordRpc.EventHandlers());
			} catch (Exception e) {
				Broken = true;
				Log.Error(e);
			}

			UpdateStatus();
		}

		public override void Update(float dt) {
			base.Update(dt);

			if (Broken) {
				return;
			}
			
			lastUpdate += dt;

			if (lastUpdate >= 3f) {
				lastUpdate = 0;

				try {
					UpdateStatus();
				} catch (Exception e) {
					Log.Error(e);
				}

				DiscordRpc.RunCallbacks();
			}
		}
		
		private void UpdateStatus() {
			if (Run.Level?.Biome == null || Broken) {
				return;
			}
			
			var status = new DiscordRpc.RichPresence();

			if (Run.Level != null) {
				status.details = $"{Level.GetDepthString(true)}";
				var p = LocalPlayer.Locate(Engine.Instance.State.Area);

				if (p != null) {
					var h = p.GetComponent<HatComponent>().Item;

					if (h != null && h.Id != "bk:no_hat") {
						status.state = $"{Locale.GetEnglish(h.Id)}";
					} else {
						status.state = "No hat :(";
					}
				}
			}

			status.largeImageKey = "bk";
			status.largeImageText = "burningknight.net/s";
			status.startTimestamp = startTime;
			
			DiscordRpc.UpdatePresence(status);
		}

		public override void Destroy() {
			base.Destroy();

			if (!Broken) {
				DiscordRpc.Shutdown();
			}
		}
	}
}
