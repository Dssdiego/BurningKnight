﻿using System;
using System.Collections.Generic;
using BurningKnight.level.builders;
using BurningKnight.level.rooms;
using BurningKnight.level.tile;
using BurningKnight.level.variant;
using BurningKnight.state;
using Lens.util.file;
using Lens.util.math;
using Microsoft.Xna.Framework;

namespace BurningKnight.level.biome {
	public class Biome {
		public const string Castle = "castle";
		public const string Hub = "hub";
		public const string Library = "library";
		public const string Desert = "desert";
		public const string Ice = "ice";
		public const string Jungle = "jungle";
		public const string Tech = "tech";
		public const string Cave = "cave";
		
		public readonly string Music;
		public readonly string Id;
		public readonly string Tileset;
		public readonly Color Bg;

		public Biome(string music, string id, string tileset, Color bg) {
			Music = music;
			Id = id;
			Tileset = tileset;
			Bg = bg;
		}

		public bool IsPresent(string[] biomes) {
			return IsPresent(Id, biomes);
		}

		public virtual string GetMusic() {
			return Music;
		}

		public static bool IsPresent(string id, string[] biomes) {
			if (biomes == null || biomes.Length == 0) {
				return true;
			}

			foreach (var b in biomes) {
				if (b == id) {
					return true;
				}
			}

			return false;
		}

		public virtual void Apply() {
			
		}

		public virtual void ModifyPainter(Level level, Painter painter) {
			level?.Variant?.ModifyPainter(painter);
		}

		public virtual void ModifyRooms(List<RoomDef> rooms) {
			
		}

		public virtual bool SpawnAllMobs() {
			return false;
		}

		public virtual Builder GetBuilder() {
			var R = Rnd.Float();

			if (R < 0.33f) {
				return new LineBuilder().SetPathLength(0.5f, new float[] { 1, 2, 3 });
			}

			if (R < 0.66f) {
				return new LoopBuilder().SetShape(2,
					Rnd.Float(0.4f, 0.7f),
					Rnd.Float(0f, 0.5f));
			}
			
			return new CastleBuilder();
		}

		public virtual Tile GetFilling() {
			return Run.Level.Variant.Id == LevelVariant.Chasm ? Tile.Chasm : Tile.WallA;
		}

		public virtual bool HasCobwebs() {
			return true;
		}

		public virtual bool HasPaintings() {
			return true;
		}

		public virtual bool HasTorches() {
			return true;
		}

		public virtual bool HasSpikes() {
			return Run.Type != RunType.BossRush;
		}

		public virtual bool HasBrekables() {
			return true;
		}

		public virtual bool HasTnt() {
			return true;
		}

		public virtual bool HasPlants() {
			return Run.Level.Variant.Id == LevelVariant.Forest;
		}

		public virtual bool HasTrees() {
			return Run.Level.Variant.Id == LevelVariant.Forest;
		}
		
		public virtual int GetNumRegularRooms() {
			return Run.Depth + 2;
		}

		public virtual int GetNumTrapRooms() {
			return Rnd.Chance(60) ? 1 : 0;
		}

		public virtual int GetNumSpecialRooms() {
			return 1;
		}

		public virtual int GetNumSecretRooms() {
			return Run.Depth <= 0 || Rnd.Chance() ? 0 : 1;
		}

		public string GetDefaultStepSound(Tile tile) {
			return "player_step_default_1";
		}

		public virtual string GetStepSound(Tile tile) {
			switch (tile) {
				case Tile.Water: return $"player_step_water_{Rnd.Int(1, 4)}";
				case Tile.Grass: return $"player_step_grass_{Rnd.Int(1, 4)}";
				case Tile.Sand: case Tile.Dirt: return $"player_step_sand_{Rnd.Int(1, 4)}";
				case Tile.Snow: return $"player_step_snow_{Rnd.Int(1, 4)}"; 
				case Tile.Ice: return $"player_step_ice_{Rnd.Int(1, 4)}";
				case Tile.FloorD: return $"player_step_gold_{Rnd.Int(1, 4)}";

				case Tile.FloorA: 
				case Tile.FloorB:
				case Tile.FloorC: {
					return $"player_step_stone_{Rnd.Int(1, 4)}";
				}
			}
			
			return GetDefaultStepSound(tile);
		}
		
		private static Color mapColor = new Color(101, 115, 146);

		public virtual Color GetMapColor() {
			return mapColor;
		}

		public virtual string GetItemUnlock() {
			return null;
		}
	}
}