using System;
using System.Collections.Generic;

namespace AA_WPG
{
	public class WeightedRandom
	{
		public static int GetIndex(int objectsCount, Func<int, int> weightFunc, System.Random random)
		{

			int totalWeight = 0;
			for (int i = 0; i < objectsCount; i++)
				totalWeight += weightFunc(i);
			int randomNumber = random.Next(0, totalWeight);
			int selectedIndex = -1;
			for (int i = 0; i < objectsCount; i++)
			{
				int weight = weightFunc(i);
				if (randomNumber < weight)
				{
					selectedIndex = i;
					break;
				}

				randomNumber = randomNumber - weight;
			}
			return selectedIndex;
		}
	}
}

