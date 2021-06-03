using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI;

namespace Chat_Overrides
{
    public static class Extensions
    {
        public static int NewNPCWithoutSlimeMessage(this NPC npc, int X, int Y, int Type, int Start = 0, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, float ai3 = 0f, int Target = 255)
        {
            int nextNPC = -1;
            if (Type == 222)
            {
                for (int i = 199; i >= 0; i--)
                {
                    if (!Main.npc[i].active)
                    {
                        nextNPC = i;
                        break;
                    }
                }
            }
            else
            {
                for (int j = Start; j < 200; j++)
                {
                    if (!Main.npc[j].active)
                    {
                        nextNPC = j;
                        break;
                    }
                }
            }
            if (nextNPC >= 0)
            {
                Main.npc[nextNPC] = new NPC();
                Main.npc[nextNPC].SetDefaults(Type);
                Main.npc[nextNPC].whoAmI = nextNPC;
                NPC.GiveTownUniqueDataToNPCsThatNeedIt(Type, nextNPC);
                Main.npc[nextNPC].position.X = X - Main.npc[nextNPC].width / 2;
                Main.npc[nextNPC].position.Y = Y - Main.npc[nextNPC].height;
                Main.npc[nextNPC].active = true;
                Main.npc[nextNPC].timeLeft = (int)(NPC.activeTime * 1.25);
                Main.npc[nextNPC].wet = Collision.WetCollision(Main.npc[nextNPC].position, Main.npc[nextNPC].width, Terraria.Main.npc[nextNPC].height);
                Main.npc[nextNPC].ai[0] = ai0;
                Main.npc[nextNPC].ai[1] = ai1;
                Main.npc[nextNPC].ai[2] = ai2;
                Main.npc[nextNPC].ai[3] = ai3;
                Main.npc[nextNPC].target = Target;
                return nextNPC;
            }
            return 200;
        }

        public static void NPCLootOverride(this NPC npc)
        {
            if (Main.netMode == 1 || npc.type >= 668)
            {
                return;
            }
            bool flag = false;
            Player closestPlayer = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)];
            if (!flag)
            {
                npc.CountKillForAchievements();
                if (npc.GetWereThereAnyInteractions())
                {
                    if (npc.IsNPCValidForBestiaryKillCredit())
                    {
                        Main.BestiaryTracker.Kills.RegisterKill(npc);
                    }
                    npc.CountKillForBannersAndDropThem();
                }
            }
            if (npc.type == 23 && Main.hardMode)
            {
                return;
            }
            if (npc.SpawnedFromStatue && NPCID.Sets.NoEarlymodeLootWhenSpawnedFromStatue[npc.type] && !Main.hardMode)
            {
                return;
            }
            if (npc.SpawnedFromStatue && NPCID.Sets.StatueSpawnedDropRarity[npc.type] != -1f && (Main.rand.NextFloat() >= NPCID.Sets.StatueSpawnedDropRarity[npc.type] || !npc.AnyInteractions()))
            {
                return;
            }
            bool flag2 = NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;
            npc.DoDeathEvents_BeforeLoot(closestPlayer);
            npc.NPCLoot_DropItems(closestPlayer);
            npc.HandleDeathEvents(closestPlayer);
            if (!flag2 && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && Main.hardMode)
            {
				if (!string.IsNullOrEmpty(ChatOverrides.Config.JungleGrowsRestless))
                {
					TSPlayer.All.SendMessage(ChatOverrides.Config.JungleGrowsRestless, Color.White);
                }
            }
            npc.NPCLoot_DropMoney(closestPlayer);
            npc.NPCLoot_DropHeals(closestPlayer);
        }

        public static void HandleDeathEvents(this NPC npc, Player closestPlayer)
        {
			npc.DoDeathEvents_AdvanceSlimeRain(closestPlayer);
			npc.DoDeathEvents_SummonDungeonSpirit(closestPlayer);
			int npcType = npc.type;
			if (npcType <= 262)
			{
				if (npcType <= 113)
				{
					if (npcType <= 22)
					{
						if (npcType == 4)
						{
							NPC.SetEventFlagCleared(ref NPC.downedBoss1, 13);
							goto IL_A01;
						}
						if (npcType - 13 > 2)
						{
							if (npcType != 22)
							{
								goto IL_A01;
							}
							if (Collision.LavaCollision(npc.position, npc.width, npc.height))
							{
								NPC.SpawnWOF(npc.position);
								goto IL_A01;
							}
							goto IL_A01;
						}
					}
					else if (npcType <= 50)
					{
						if (npcType != 35)
						{
							if (npcType != 50)
							{
								goto IL_A01;
							}
							if (Main.slimeRain)
							{
								Main.StopSlimeRain(true);
								AchievementsHelper.NotifyProgressionEvent(16);
							}
							NPC.SetEventFlagCleared(ref NPC.downedSlimeKing, 11);
							if (Main.netMode == 2)
							{
								NetMessage.SendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
								goto IL_A01;
							}
							goto IL_A01;
						}
						else
						{
							if (npc.boss)
							{
								NPC.SetEventFlagCleared(ref NPC.downedBoss3, 15);
								goto IL_A01;
							}
							goto IL_A01;
						}
					}
					else if (npcType != 109)
					{
						if (npcType != 113)
						{
							goto IL_A01;
						}
						npc.CreateBrickBoxForWallOfFlesh();
						bool hardMode = Main.hardMode;
						WorldGen.StartHardmode();
						if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && !hardMode)
						{
							if (!string.IsNullOrEmpty(ChatOverrides.Config.JungleGrowsRestless))
							{
								TSPlayer.All.SendMessage(ChatOverrides.Config.JungleGrowsRestless, Color.White);
							}
						}
						NPC.SetEventFlagCleared(ref hardMode, 19);
						goto IL_A01;
					}
					else
					{
						if (NPC.downedClown)
						{
							goto IL_A01;
						}
						NPC.downedClown = true;
						if (Main.netMode == 2)
						{
							NetMessage.SendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
							goto IL_A01;
						}
						goto IL_A01;
					}
				}
				else if (npcType <= 134)
				{
					if (npcType - 125 > 1)
					{
						if (npcType != 127)
						{
							if (npcType != 134)
							{
								goto IL_A01;
							}
							if (npc.boss)
							{
								NPC.SetEventFlagCleared(ref NPC.downedMechBoss1, 16);
								NPC.downedMechBossAny = true;
								goto IL_A01;
							}
							goto IL_A01;
						}
						else
						{
							if (npc.boss)
							{
								NPC.SetEventFlagCleared(ref NPC.downedMechBoss3, 18);
								NPC.downedMechBossAny = true;
								goto IL_A01;
							}
							goto IL_A01;
						}
					}
					else
					{
						if (npc.boss)
						{
							NPC.SetEventFlagCleared(ref NPC.downedMechBoss2, 17);
							NPC.downedMechBossAny = true;
							goto IL_A01;
						}
						goto IL_A01;
					}
				}
				else if (npcType <= 222)
				{
					if (npcType == 216)
					{
						NPC.SpawnBoss((int)npc.position.X, (int)npc.position.Y, 662, npc.target);
						goto IL_A01;
					}
					if (npcType != 222)
					{
						goto IL_A01;
					}
					NPC.SetEventFlagCleared(ref NPC.downedQueenBee, 8);
					if (Main.netMode == 2)
					{
						NetMessage.SendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
						goto IL_A01;
					}
					goto IL_A01;
				}
				else
				{
					if (npcType == 245)
					{
						NPC.SetEventFlagCleared(ref NPC.downedGolemBoss, 6);
						goto IL_A01;
					}
					if (npcType != 262)
					{
						goto IL_A01;
					}
					bool flag = NPC.downedPlantBoss;
					NPC.SetEventFlagCleared(ref NPC.downedPlantBoss, 12);
					if (flag)
					{
						goto IL_A01;
					}
					if (Main.netMode == 0)
					{
						Main.NewText(Lang.misc[33].Value, 50, byte.MaxValue, 130);
						goto IL_A01;
					}
					if (Main.netMode == 2)
					{
						ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.misc[33].Key, new object[0]), new Color(50, 255, 130), -1);
						goto IL_A01;
					}
					goto IL_A01;
				}
			}
			else
			{
				if (npcType <= 493)
				{
					if (npcType <= 327)
					{
						if (npcType == 266)
						{
							goto IL_85F;
						}
						if (npcType != 325)
						{
							if (npcType != 327)
							{
								goto IL_A01;
							}
							if (Main.pumpkinMoon)
							{
								NPC.SetEventFlagCleared(ref NPC.downedHalloweenKing, 5);
								goto IL_A01;
							}
							goto IL_A01;
						}
						else
						{
							if (Main.pumpkinMoon)
							{
								NPC.SetEventFlagCleared(ref NPC.downedHalloweenTree, 4);
								goto IL_A01;
							}
							goto IL_A01;
						}
					}
					else if (npcType <= 370)
					{
						switch (npcType)
						{
							case 344:
								if (Main.snowMoon)
								{
									NPC.SetEventFlagCleared(ref NPC.downedChristmasTree, 21);
									goto IL_A01;
								}
								goto IL_A01;
							case 345:
								if (Main.snowMoon)
								{
									NPC.SetEventFlagCleared(ref NPC.downedChristmasIceQueen, 20);
									goto IL_A01;
								}
								goto IL_A01;
							case 346:
								if (Main.snowMoon)
								{
									NPC.SetEventFlagCleared(ref NPC.downedChristmasSantank, 22);
									goto IL_A01;
								}
								goto IL_A01;
							default:
								if (npcType != 370)
								{
									goto IL_A01;
								}
								NPC.SetEventFlagCleared(ref NPC.downedFishron, 7);
								goto IL_A01;
						}
					}
					else
					{
						switch (npcType)
						{
							case 398:
								NPC.SetEventFlagCleared(ref NPC.downedMoonlord, 10);
								NPC.LunarApocalypseIsUp = false;
								goto IL_A01;
							case 399:
							case 400:
							case 401:
							case 403:
							case 404:
							case 406:
							case 408:
							case 410:
							case 428:
							case 430:
							case 431:
							case 432:
							case 433:
							case 434:
							case 435:
							case 436:
							case 437:
							case 438:
								goto IL_A01;
							case 402:
							case 405:
							case 407:
							case 409:
							case 411:
								if (NPC.ShieldStrengthTowerStardust > 0)
								{
									Projectile.NewProjectile(npc.GetProjectileSpawnSource(), npc.Center.X, npc.Center.Y, 0f, 0f, 629, 0, 0f, Main.myPlayer, NPC.FindFirstNPC(493), 0f);
									goto IL_A01;
								}
								goto IL_A01;
							case 412:
							case 413:
							case 414:
							case 415:
							case 416:
							case 417:
							case 418:
							case 419:
								break;
							case 420:
							case 421:
							case 423:
							case 424:
								if (NPC.ShieldStrengthTowerNebula > 0)
								{
									Projectile.NewProjectile(npc.GetProjectileSpawnSource(), npc.Center.X, npc.Center.Y, 0f, 0f, 629, 0, 0f, Main.myPlayer, NPC.FindFirstNPC(507), 0f);
									goto IL_A01;
								}
								goto IL_A01;
							case 422:
								NPC.downedTowerVortex = true;
								NPC.TowerActiveVortex = false;
								WorldGen.UpdateLunarApocalypse();
								WorldGen.MessageLunarApocalypse();
								goto IL_A01;
							case 425:
							case 426:
							case 427:
							case 429:
								if (NPC.ShieldStrengthTowerVortex > 0)
								{
									Projectile.NewProjectile(npc.GetProjectileSpawnSource(), npc.Center.X, npc.Center.Y, 0f, 0f, 629, 0, 0f, Main.myPlayer, NPC.FindFirstNPC(422), 0f);
									goto IL_A01;
								}
								goto IL_A01;
							case 439:
								NPC.SetEventFlagCleared(ref NPC.downedAncientCultist, 9);
								WorldGen.TriggerLunarApocalypse();
								goto IL_A01;
							default:
								if (npcType != 493)
								{
									goto IL_A01;
								}
								NPC.downedTowerStardust = true;
								NPC.TowerActiveStardust = false;
								WorldGen.UpdateLunarApocalypse();
								WorldGen.MessageLunarApocalypse();
								goto IL_A01;
						}
					}
				}
				else if (npcType <= 578)
				{
					if (npcType <= 517)
					{
						if (npcType == 507)
						{
							NPC.downedTowerNebula = true;
							NPC.TowerActiveNebula = false;
							WorldGen.UpdateLunarApocalypse();
							WorldGen.MessageLunarApocalypse();
							goto IL_A01;
						}
						if (npcType != 517)
						{
							goto IL_A01;
						}
						NPC.downedTowerSolar = true;
						NPC.TowerActiveSolar = false;
						WorldGen.UpdateLunarApocalypse();
						WorldGen.MessageLunarApocalypse();
						goto IL_A01;
					}
					else if (npcType != 518)
					{
						switch (npcType)
						{
							case 552:
							case 553:
							case 554:
								if (!DD2Event.Ongoing)
								{
									goto IL_A01;
								}
								DD2Event.AnnounceGoblinDeath(npc);
								if (DD2Event.ShouldDropCrystals())
								{
									Item.NewItem(npc.position, npc.Size, 3822, 1, false, 0, false, false);
									goto IL_A01;
								}
								goto IL_A01;
							case 555:
							case 556:
							case 557:
							case 558:
							case 559:
							case 560:
							case 561:
							case 562:
							case 563:
							case 564:
							case 565:
							case 568:
							case 569:
							case 570:
							case 571:
							case 572:
							case 573:
							case 574:
							case 575:
							case 576:
							case 577:
							case 578:
								if (DD2Event.ShouldDropCrystals())
								{
									Item.NewItem(npc.position, npc.Size, 3822, 1, false, 0, false, false);
									goto IL_A01;
								}
								goto IL_A01;
							case 566:
							case 567:
								goto IL_A01;
							default:
								goto IL_A01;
						}
					}
				}
				else if (npcType <= 636)
				{
					if (npcType == 614)
					{
						int dmg = 175;
						if (npc.SpawnedFromStatue)
						{
							dmg = 0;
						}
						Projectile.NewProjectile(npc.GetProjectileSpawnSource(), npc.Center.X, npc.Center.Y, 0f, 0f, 281, dmg, 0f, Main.myPlayer, -2f, npc.releaseOwner + 1);
						goto IL_A01;
					}
					if (npcType != 636)
					{
						goto IL_A01;
					}
					NPC.SetEventFlagCleared(ref NPC.downedEmpressOfLight, 23);
					goto IL_A01;
				}
				else
				{
					if (npcType == 657)
					{
						NPC.SetEventFlagCleared(ref NPC.downedQueenSlime, 24);
						goto IL_A01;
					}
					if (npcType != 661)
					{
						goto IL_A01;
					}
					if (Main.netMode == 1 || !npc.GetWereThereAnyInteractions())
					{
						goto IL_A01;
					}
					int type = 636;
					if (!NPC.AnyNPCs(type))
					{
						Vector2 vector = npc.Center + new Vector2(0f, -200f) + Main.rand.NextVector2Circular(50f, 50f);
						NPC.SpawnBoss((int)vector.X, (int)vector.Y, type, closestPlayer.whoAmI);
						goto IL_A01;
					}
					goto IL_A01;
				}
				if (NPC.ShieldStrengthTowerSolar > 0)
				{
					Projectile.NewProjectile(npc.GetProjectileSpawnSource(), npc.Center.X, npc.Center.Y, 0f, 0f, 629, 0, 0f, Main.myPlayer, NPC.FindFirstNPC(517), 0f);
					goto IL_A01;
				}
				goto IL_A01;
			}
		IL_85F:
			if (npc.boss)
			{
				if (!NPC.downedBoss2 || Main.rand.Next(2) == 0)
				{
					WorldGen.spawnMeteor = true;
				}
				NPC.SetEventFlagCleared(ref NPC.downedBoss2, 14);
			}
		IL_A01:
			if (npc.boss)
			{
				npc.DoDeathEvents_DropBossPotionsAndHearts();
				if (Main.netMode == 2)
				{
					NetMessage.SendData(7, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
				}
			}
		}
    }
}
