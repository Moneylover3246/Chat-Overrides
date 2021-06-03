using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Chat_Overrides
{
	public class ItemRarity
	{
		public static void Initialize()
		{
			ItemRarity._rarities.Clear();
			ItemRarity._rarities.Add(-11, Colors.RarityAmber);
			ItemRarity._rarities.Add(-1, Colors.RarityTrash);
			ItemRarity._rarities.Add(0, Colors._waterfallColors[0]);
			ItemRarity._rarities.Add(1, Colors.RarityBlue);
			ItemRarity._rarities.Add(2, Colors.RarityGreen);
			ItemRarity._rarities.Add(3, Colors.RarityOrange);
			ItemRarity._rarities.Add(4, Colors.RarityRed);
			ItemRarity._rarities.Add(5, Colors.RarityPink);
			ItemRarity._rarities.Add(6, Colors.RarityPurple);
			ItemRarity._rarities.Add(7, Colors.RarityLime);
			ItemRarity._rarities.Add(8, Colors.RarityYellow);
			ItemRarity._rarities.Add(9, Colors.RarityCyan);
			ItemRarity._rarities.Add(10, Color.MediumVioletRed);
			ItemRarity._rarities.Add(11, Color.Purple);
		}
		public static Color GetColor(int rarity)
		{
			Color result = new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor);
			if (ItemRarity._rarities.ContainsKey(rarity))
			{
				return ItemRarity._rarities[rarity];
			}
			return result;
		}

		private static Dictionary<int, Color> _rarities = new Dictionary<int, Color>();
	}
}
