using BurningKnight.assets;
using BurningKnight.assets.achievements;
using BurningKnight.assets.items;
using BurningKnight.entity.component;
using BurningKnight.entity.creature.drop;
using BurningKnight.entity.creature.player;
using BurningKnight.entity.events;
using BurningKnight.level.entities.chest;
using BurningKnight.state;
using BurningKnight.ui.dialog;
using Lens.entity;
using Lens.util;
using Lens.util.file;
using Lens.util.math;
using Lens.util.timer;

namespace BurningKnight.entity.creature.npc {
	public class Maanex : Npc {
		private bool interacted;
		private bool played;
		private byte cost;
		private float t;
		
		public override void AddComponents() {
			base.AddComponents();
			
			Width = 12;
			Height = 15;

			var min = Run.Depth * 2;
			cost = (byte) Rnd.Int(min, min + 5);
			
			AddComponent(new AnimationComponent("maanex"));
			GetComponent<DropsComponent>().Add(new SingleDrop("bk:maanex_head"));

			if (Run.Depth == 0) {
				AddComponent(new CloseDialogComponent("maanex_0", "maanex_1", "maanex_2", "maanex_3", "maanex_4"));
			} else {
				if (!interacted) {
					AddComponent(new InteractableComponent(Interact) {
						CanInteract = e => !interacted
					});
					
					AddComponent(new SensorBodyComponent(-Padding, -Padding, Width + Padding * 2, Height + Padding * 2));
				}
			}

			Dialogs.RegisterCallback("maanex_6", (d, c) => {
				if (((ChoiceDialog) d).Choice == 0) {
					if (!c.To.TryGetComponent<ConsumablesComponent>(out var component) || component.Coins < cost) {
						return Dialogs.Get("maanex_11");
					}

					var room = GetComponent<RoomComponent>().Room;

					if (room == null) {
						return null;
					}

					component.Coins -= cost;
					interacted = true;

					foreach (var chest in room.Tagged[Tags.Chest]) {
						((Chest) chest).CanOpen = true;
					}

					return Dialogs.Get("maanex_8");
				}

				return null;
			});
			
			Subscribe<Chest.OpenedEvent>();
			Subscribe<RoomChangedEvent>();
		}

		public override void PostInit() {
			base.PostInit();
			
			var h = GetComponent<HealthComponent>();
			h.Unhittable = false;
			h.InitMaxHealth = 50;
			h.SetHealth(50, this);
		}

		private string GetDialog(Entity e) {
			var hat = e.GetComponent<HatComponent>().Item;

			if (hat != null && hat.Id == "bk:maanex_head") {
				return "maanex_12";
			}

			if (Rnd.Chance(0.01f)) {
				return "spanish_inquisition";
			}

			return $"maanex_{(played ? 7 : 5)}";
		}

		public override bool HandleEvent(Event e) {
			if (e is Chest.OpenedEvent coe) {
				if (coe.Chest.GetComponent<RoomComponent>().Room == GetComponent<RoomComponent>().Room) {
					if (coe.Chest.Empty) {
						GetComponent<DialogComponent>().StartAndClose("maanex_9", 5f);
					} else {
						GetComponent<DialogComponent>().StartAndClose("maanex_10", 5f);
					}
					
					played = true;
					foreach (var chest in GetComponent<RoomComponent>().Room.Tagged[Tags.Chest]) {
						var c = (Chest) chest;

						if (c.Scale > 0.9f) {
							c.CanOpen = false;
						}
					}
				}
			} else if (e is RoomChangedEvent rce) {
				if (rce.Who is Player) {
					var r = GetComponent<RoomComponent>().Room;
					
					if (rce.New == r) {
						GetComponent<DialogComponent>().Start(GetDialog(rce.Who));
					} else if (rce.Old == r) {
						GetComponent<DialogComponent>().Close();
					}
				}
			} else if (e is DiedEvent de) {
				Items.Unlock("bk:maanex_head");
				ExplosionMaker.Make(this);

				if (de.From is Player p && p.GetComponent<HatComponent>().Item?.Id == "bk:maanex_head") {
					Achievements.Unlock("bk:maanex");
				}
			} else if (e is HealthModifiedEvent hme && hme.Amount < 0) {
				GetComponent<DialogComponent>().StartAndClose(Bruh[Rnd.Int(Bruh.Length)], 2);
			}
			
			return base.HandleEvent(e);
		}

		public static string[] Bruh = {
			"bruh", "bruuh", "BRUH"
		};

		public override void Update(float dt) {
			base.Update(dt);
			t += dt;
			
			if ((!interacted || played) && t >= 0.1f) {
				var r = GetComponent<RoomComponent>().Room;

				if (r == null) {
					return;
				}
				
				foreach (var chest in r.Tagged[Tags.Chest]) {
					((Chest) chest).CanOpen = false;
				}
			}
		}

		public bool Interact(Entity e) {
			var d = GetComponent<DialogComponent>();
			
			d.Dialog.Str.SetVariable("cost", cost);
			d.Start("maanex_6", e);
			
			return true;
		}

		public override void Load(FileReader stream) {
			base.Load(stream);

			interacted = stream.ReadBoolean();
			cost = stream.ReadByte();
			played = stream.ReadBoolean();
		}

		public override void Save(FileWriter stream) {
			base.Save(stream);
			
			stream.WriteBoolean(interacted);
			stream.WriteByte(cost);
			stream.WriteBoolean(played);
		}
	}
}