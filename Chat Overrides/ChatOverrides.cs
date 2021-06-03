using System;
using System.IO;
using System.Linq;
using System.Timers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Chat_Overrides
{
	[ApiVersion(2, 1)]
	public class ChatOverrides : TerrariaPlugin
	{
		public override string Name => "Chat Overrides";
		public override string Description => "Allows custom chat messages when certain events happen";
		public override string Author => "Moneylover3246";
		public override Version Version => new Version("1.0.1");

		public static Config Config = new Config();
		public string SavePath = "ChatOverrides.json";
		public static string[] TeamName = new string[6];

		private readonly Timer StatusTimer = new Timer(100);
		public ChatOverrides(Main game) : base(game)
		{
		}

		public override void Initialize()
		{
			ServerApi.Hooks.NetGetData.Register(this, OnGetData);
			ServerApi.Hooks.NpcStrike.Register(this, OnNPCStrike);
			GeneralHooks.ReloadEvent += OnReload;
			Config = Config.Read(Path.Combine(TShock.SavePath, SavePath));
			ItemRarity.Initialize();
			StatusTimer.Start();
			StatusTimer.Elapsed += OnStatusTimerElapsed;
			TeamName[0] = "white";
			TeamName[1] = "red";
			TeamName[2] = "green";
			TeamName[3] = "blue";
			TeamName[4] = "yellow";
			TeamName[5] = "pink";
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
				ServerApi.Hooks.NpcStrike.Deregister(this, OnNPCStrike);
				GeneralHooks.ReloadEvent -= OnReload;
				StatusTimer.Stop();
			}
			base.Dispose(disposing);
		}

		private void OnReload(ReloadEventArgs args)
		{
			Config = Config.Read(Path.Combine(TShock.SavePath, SavePath));
		}

		private void OnGetData(GetDataEventArgs args)
		{
			using (BinaryReader reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
			{
				switch (args.MsgID)
				{
					case PacketTypes.PlayerTeam:
						{
							int playerid = reader.ReadByte();
							if (Main.netMode == 2)
							{
								playerid = args.Msg.whoAmI;
							}
							int team = reader.ReadByte();
							Player player = Main.player[playerid];
							int oldteam = player.team;
							player.team = team;
							Color teamColor = Main.teamColor[team];
							NetMessage.TrySendData(45, -1, args.Msg.whoAmI, null, playerid, 0f, 0f, 0f, 0, 0, 0);
							LocalizedText teamJoinText = Lang.mp[13 + team];
							
							if (team == 5)
							{
								teamJoinText = Lang.mp[22];
							}
							for (int i = 0; i < 255; i++)
							{
								if (i == args.Msg.whoAmI || (oldteam > 0 && Main.player[i].team == oldteam) || (team > 0 && Main.player[i].team == team))
								{
									TShock.Players[i].SendMessage(string.Format(Config.PlayerJoinTeamMessage,
										string.Format(teamJoinText.Value, player.name),
										player.name,
										TeamName[team],
										teamColor.Hex3()), Color.White);
								}
							}
							args.Handled = true;
							break;
						}
					case PacketTypes.SpawnBossorInvasion:
						{
							short player = reader.ReadInt16();
							short npcType = reader.ReadInt16();
							if (npcType > 0 && npcType != 126)
							{
								TShock.Utils.GetRandomClearTileWithInRange(TShock.Players[player].TileX, TShock.Players[player].TileY, 100, 50, out int spawnTileX, out int spawnTileY);
								int npcindex = new NPC().NewNPCWithoutSlimeMessage(spawnTileX * 16, spawnTileY * 16, npcType);
								NPC realNPC = Main.npc[npcindex];
								NetMessage.SendData(23, -1, -1, null, npcindex);
								string format = string.Format(
									Config.BossSpawnedMessage,
									realNPC.type == 125 ? "The Twins" : realNPC.GivenOrTypeName,
									realNPC.type == 125 ? "have" : "has",
									realNPC.lifeMax);
								Console.WriteLine(format);
								TSPlayer.All.SendMessage(format, Color.White);
								args.Handled = true;
							}
						}
						break;
					case PacketTypes.PlayerDeathV2:
						{
							int player = reader.ReadByte();
							player = args.Msg.whoAmI;
							PlayerDeathReason reason = PlayerDeathReason.FromReader(reader);
							string newDeathReason = "";
							int damage = reader.ReadInt16();
							int direction = (reader.ReadByte() - 1);
							bool pvp = ((BitsByte)reader.ReadByte())[0];

							TSPlayer killedPlayer = TShock.Players[player];
							Item item = new Item();
							item.SetDefaults(reason._sourceItemType);
							item.Prefix(reason._sourceItemPrefix);

							string[] deathMessageDescriptions = new string[]
							{
								" was eviscerated by",
								" was murdered by",
								"'s face was torn off by",
								"'s entrails were ripped by",
								" was destroyed by",
								"'s skull was crushed by",
								" got massacred by",
								" got impaled by",
								" was torn in half by",
								" was decapitated by",
								" let their arms get torn off by",
								" watched their innards become outards by",
								" was brutally dissected by",
								"'s extremities were detached by",
								"'s body was mangled by",
								"'s vital organs were ruptured by",
								" was turned into a pile of flesh by",
								" got snapped in half by",
								" was cut down the middle by",
								" was chopped up by",
								"'s plead for death was answered by",
								"'s meat was ripped off the bone by",
								"'s flailing about was finally stopped by",
							};
							Random random = new Random();
							string description = deathMessageDescriptions[random.Next(0, deathMessageDescriptions.Length)];
							if (pvp)
							{
								TSPlayer killer = TShock.Players[reason._sourcePlayerIndex];
								newDeathReason = string.Format(Config.DeathMessageFromPvP,
									killedPlayer.Name,
									killer.Name,
									description,
									item.Name,
									ItemRarity.GetColor(item.rare),
									item.prefix,
									new Color(killedPlayer.Group.R, killedPlayer.Group.G, killedPlayer.Group.B).Hex3(),
									new Color(killer.Group.R, killer.Group.G, killer.Group.B).Hex3(),
									killer.TPlayer.statLife,
									killer.TPlayer.statMana,
									killer.TPlayer.statDefense);
							}
							else if (reason._sourceNPCIndex != -1)
							{
								NPC npc = Main.npc[reason._sourceNPCIndex];
								newDeathReason = string.Format(Config.DeathMessageFromNPC,
									npc.GivenOrTypeName,
									npc.life,
									npc.lifeMax,
									npc.type,
									npc.defDefense);
							}

							if (newDeathReason != "")
							{
								reason = PlayerDeathReason.ByCustomReason(newDeathReason);
							}
							Main.player[player].KillMe(reason, damage, direction, pvp);
							NetMessage.SendPlayerDeath(player, reason, damage, direction, pvp, -1, args.Msg.whoAmI);
							args.Handled = true;
						}
						break;
				}
			}
		}

		private void OnNPCStrike(NpcStrikeEventArgs args)
		{
			NPC npc = args.Npc;
			double damage = args.Damage;
			int defense = npc.defense;
			damage = Main.CalculateDamageNPCsTake((int)damage, defense);
			if (npc.boss && (npc.life - damage) <= 0 && !string.IsNullOrEmpty(Config.BossDefeatedMessage) && npc.active)
			{
				if (npc.ichor)
				{
					defense -= 15;
				}
				if (npc.betsysCurse)
				{
					defense -= 40;
				}
				if (defense < 0)
				{
					defense = 0;
				}

				if (args.Critical)
				{
					damage *= 2.0;
				}
				if (npc.takenDamageMultiplier > 1f)
				{
					damage *= npc.takenDamageMultiplier;
				}
				if ((npc.takenDamageMultiplier > 1f || args.Damage != 9999) && npc.lifeMax > 1)
				{
					if (npc.friendly)
					{
						Color color = args.Critical ? CombatText.DamagedFriendlyCrit : CombatText.DamagedFriendly;
						CombatText.NewText(new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height), color, (int)damage, args.Critical, false);
					}
					else
					{
						Color color2 = args.Critical ? CombatText.DamagedHostileCrit : CombatText.DamagedHostile;
						if (args.FromNet)
						{
							color2 = args.Critical ? CombatText.OthersDamagedHostileCrit : CombatText.OthersDamagedHostile;
						}
						CombatText.NewText(new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height), color2, (int)damage, args.Critical, false);
					}
				}
				if (damage >= 1.0)
				{
					npc.justHit = true;
					if (npc.aiStyle == 8)
					{
						npc.ai[0] = 400f;
						npc.TargetClosest(true);
					}
					if (npc.aiStyle == 97)
					{
						npc.localAI[1] = 1f;
						npc.TargetClosest(true);
					}
					if (npc.type == 346 && npc.life >= npc.lifeMax * 0.5 && npc.life - damage < npc.lifeMax * 0.5)
					{
						Gore.NewGore(npc.position, npc.velocity, 517, 1f);
					}
					if (!npc.immortal)
					{
						if (npc.realLife >= 0)
						{
							Main.npc[npc.realLife].life -= (int)damage;
							npc.life = Main.npc[npc.realLife].life;
							npc.lifeMax = Main.npc[npc.realLife].lifeMax;
						}
						else
						{
							npc.life -= (int)damage;
						}
					}
					if (args.KnockBack > 0f && npc.knockBackResist > 0f)
					{
						float knockback = args.KnockBack * npc.knockBackResist;
						if (npc.onFire2)
						{
							knockback *= 1.1f;
						}
						if (knockback > 8f)
						{
							float tax = knockback - 8f;
							tax *= 0.9f;
							knockback = 8f + tax;
						}
						if (knockback > 10f)
						{
							float tax = knockback - 10f;
							tax *= 0.8f;
							knockback = 10f + tax;
						}
						if (knockback > 12f)
						{
							float tax = knockback - 12f;
							tax *= 0.7f;
							knockback = 12f + tax;
						}
						if (knockback > 14f)
						{
							float tax = knockback - 14f;
							tax *= 0.6f;
							knockback = 14f + tax;
						}
						if (knockback > 16f)
						{
							knockback = 16f;
						}
						if (args.Critical)
						{
							knockback *= 1.4f;
						}
						int minDamage = (int)damage * 10;
						if (Main.expertMode)
						{
							minDamage = (int)damage * 15;
						}
						if (minDamage > npc.lifeMax)
						{
							if (args.HitDirection < 0 && npc.velocity.X > -knockback)
							{
								if (npc.velocity.X > 0f)
								{
									npc.velocity.X -= knockback;
								}
								npc.velocity.X -= knockback;
								if (npc.velocity.X < -knockback)
								{
									npc.velocity.X = -knockback;
								}
							}
							else if (args.HitDirection > 0 && npc.velocity.X < knockback)
							{
								if (npc.velocity.X < 0f)
								{
									npc.velocity.X += knockback;
								}
								npc.velocity.X += knockback;
								if (npc.velocity.X > knockback)
								{
									npc.velocity.X = knockback;
								}
							}
							if (!npc.noGravity)
							{
								knockback *= -0.75f;
							}
							else
							{
								knockback *= -0.5f;
							}
							if (npc.velocity.Y > knockback)
							{
								npc.velocity.Y += knockback;
								if (npc.velocity.Y < knockback)
								{
									npc.velocity.Y = knockback;
								}
							}
						}
						else
						{
							if (!npc.noGravity)
							{
								npc.velocity.Y = -knockback * 0.75f * npc.knockBackResist;
							}
							else
							{
								npc.velocity.Y = -knockback * 0.5f * npc.knockBackResist;
							}
							npc.velocity.X = knockback * args.HitDirection * npc.knockBackResist;
						}
					}
					if ((npc.type == 113 || npc.type == 114) && npc.life <= 0)
					{
						for (int i = 0; i < 200; i++)
						{
							if (Main.npc[i].active && (Main.npc[i].type == 113 || Main.npc[i].type == 114))
							{
								Main.npc[i].HitEffect(args.HitDirection, damage);
							}
						}
					}
					else
					{
						npc.HitEffect(args.HitDirection, damage);
					}
					if (npc.HitSound != null)
					{
						SoundEngine.PlaySound(npc.HitSound, npc.position);
					}
					npc.NPCLootOverride();
					string name = npc.GivenOrTypeName;
					string singularOrplural = "has";
					if (npc.type == 125 || npc.type == 126)
					{
						int otherNPC = npc.type == 125 ? 126 : 125;
						if (NPC.AnyNPCs(otherNPC))
							return;
						name = "The Twins";
						singularOrplural = "have";
					}

					TSPlayer.All.SendMessage(string.Format(Config.BossDefeatedMessage,
						name, singularOrplural, npc.defDefense), Color.White);
				}
				args.Handled = true;
			}
		}

		private void OnStatusTimerElapsed(object o, ElapsedEventArgs args)
		{
			if (!Config.SidebarEnabled && string.IsNullOrEmpty(Config.SidebarText))
				return;
			foreach (TSPlayer player in TShock.Players.Where(p => p != null && Netplay.Clients[p.Index].State == 10))
			{
				RemoteClient client = Netplay.Clients[player.Index];
				int statusMax = client.StatusMax += 5;
				player.SendData(PacketTypes.Status, Config.SidebarText, statusMax, 3);
			}
		}
	}
}
