﻿using System;
using System.Collections.Generic;
using System.Linq;
using Lens.util;
using Lens.util.camera;

namespace Lens.entity {
	public class EntityList {
		public List<Entity> Entities = new List<Entity>();
		public List<Entity> ToAdd = new List<Entity>();
		public List<Entity> ToRemove = new List<Entity>();

		private Area Area;

		public EntityList(Area area) {
			Area = area;
		}
		
		public void Add(Entity entity) {
			ToAdd.Add(entity);
			ToRemove.Remove(entity);
		}
		
		public void Remove(Entity entity) {
			ToRemove.Add(entity);
			ToAdd.Remove(entity);

			if (entity.Area == Area) {
				entity.Done = true;
			}
		}
		
		private bool CheckOnScreen(Entity entity) {
			if (Camera.Instance == null) {
				return true;
			}

			return Camera.Instance.Sees(entity);
		}

		public void AutoRemove() {
			if (ToRemove.Count > 0) {
				try {
					for (var i = ToRemove.Count - 1; i >= 0; i--) {
						var entity = ToRemove[i];

						if (entity.Area == Area) {
							entity.Destroy();
						}

						entity.Area.Tagged.Remove(entity);
						Entities.Remove(entity);
					}

					ToRemove.Clear();
				} catch (Exception e) {
					Log.Error(e);
				}
			}
		}

		public void AddNew() {
			if (ToAdd.Count > 0) {
				for (int i = 0; i < ToAdd.Count; i++) {
					try {
						var entity = ToAdd[i];
						Entities.Add(entity);

						if (!Area.NoInit && entity.Components == null) {
							entity.Area = Area;
							entity.Init();
						}
					} catch (Exception e) {
						Log.Error(e);
					}
				}

				ToAdd.Clear();
			}
		}

		public void Update(float dt) {
			AutoRemove();
			AddNew();

			foreach (var entity in Entities) {
				entity.OnScreen = CheckOnScreen(entity);

				if ((entity.OnScreen || entity.AlwaysActive) && entity.Active) {
					try {
						entity.Update(dt);
					} catch (Exception e) {
						Log.Error(e);
					}
				}

				if (entity.Done) {
					ToRemove.Add(entity);
				}
			}

			Entities = Entities.OrderBy(e => e.Depth).ThenBy(e => e.Bottom).ToList();
		}

		public void Render() {
			foreach (var entity in Entities) {
				if ((entity.OnScreen || entity.AlwaysVisible) && entity.Visible) {
					try {
						entity.Render();
					} catch (Exception e) {
						Log.Error(e);
					}
				}
			}
		}

		public void RenderDebug() {
			foreach (var entity in Entities) {
				if (entity.OnScreen || entity.AlwaysVisible) {
					entity.RenderDebug();
				}
			}
		}

		public Entity Find(Func<Entity, bool> filter) {
			foreach (var e in Entities) {
				if (filter(e)) {
					return e;
				}
			}

			return null;
		}

		public Entity Find<T>() where T : Entity {
			var t = typeof(T);
			return Find(e => e.GetType() == t);
		}
	}
}