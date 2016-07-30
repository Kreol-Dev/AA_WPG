using System;
using System.IO;
using System.Text;

namespace AA_WPG
{
	public class ProcessBattle : ConsoleCommand
	{
		System.Random random = new Random((int)DateTime.Now.Ticks);
		StringBuilder log = new StringBuilder();
		Units units;
		public override void Execute(string[] args)
		{
			units = Program.GetCluster<Units>();
			log.Clear();
			var front1FullName = args[0].Split('.');
			var front2FullName = args[1].Split('.');
			int stepsCount = int.Parse(args[2] as string);
			string situationFilePath = args[3];

			string armyTag1 = front1FullName[0];
			string armyTag2 = front2FullName[0];

			string frontName1 = front1FullName[1];
			string frontName2 = front2FullName[1];

			var armies = Program.GetCluster<Armies>();
			var army1 =armies.GetArmy(armyTag1);

			var army2 = armies.GetArmy(armyTag2);

			var front1 = army1.Fronts.Find(f => f.ID == frontName1);
			var front2 = army2.Fronts.Find(f => f.ID == frontName2);

			//var lines = File.ReadAllLines(situationFilePath + ".txt");
			StepResult res = StepResult.Tie;
			Frontline attacker = null;
			Frontline defender = null;
			int i = 0;
			for (i = 0; i < stepsCount && res == StepResult.Tie; i++)
			{
				Console.WriteLine("Step");
				res = SimulateBattle(front1, front2, out attacker, out defender);
			}
			log.Append(String.Format("Battle lasted for {0} steps", i)).Append(Environment.NewLine);
			log.Append(res).Append(Environment.NewLine);
			log.Append(attacker).Append(Environment.NewLine);
			log.Append(defender).Append(Environment.NewLine);
			File.WriteAllText(situationFilePath + "_results.txt", log.ToString());
		
		}

		enum StepResult { AttackerWon, DefenderWon, EveryoneIsDead, Tie };
		int savedInitiative1 = 0;
		int savedInitiative2 = 0;
		StepResult SimulateBattle(Frontline front1, Frontline front2, out Frontline attacker, out Frontline defender)
		{
			
			int initiative1 = random.Next(0, front1.Army.Initiative) + savedInitiative1;
			int initiative2 = random.Next(0, front2.Army.Initiative) + savedInitiative2;
			savedInitiative1 = 0;
			savedInitiative2 = 0;
			float multiplier = 1;
			if (initiative1 > initiative2)
			{
				attacker = front1;
				defender = front2;
				multiplier = (float)initiative2 / (float)initiative1;
			}
			else
			{
				attacker = front2;
				defender = front1;
				multiplier = (float)initiative1 / (float)initiative2;
			}
			Frontline refAttacker = attacker;
			Frontline refDefender = defender;
			float attackerDamage = 0;
			float defenderDamage = 0;
			for (int i = 0; i < attacker.Units.Length; i++)
				attacker.Units[i].WasAttacked = false;
			for (int i = 0; i < defender.Units.Length; i++)
				defender.Units[i].WasAttacked = false;
			for (int i = 0; i < attacker.Units.Length; i++)
			{
				var frontUnit = attacker.Units[i];
				for (int j = 0; j < frontUnit.Count; j++)
				{
					if (random.NextDouble() < frontUnit.Unit.Organization)
					{
						//Choose max efficiency
						float maxDelta = 0;
						for (int k = 0; k < defender.Units.Length; k++)
						{
							if (defender.Units[k].Count == 0)
								continue;
							int targetId = k;
							try
							{
								float attackEfficiency = frontUnit.Unit.Effectiveness[units.units[defender.Units[targetId].Unit.Index]];
								defender.Units[targetId].WasAttacked = true;
								float defenceEfficiency = 0;
								try
								{
									defenceEfficiency = defender.Units[targetId].Unit.Effectiveness[units.units[frontUnit.Unit.Index]];
									defenceEfficiency *= multiplier;
									attacker.Units[j].WasAttacked = true;
								}
								catch { }
								var localDelta = attackEfficiency - defenceEfficiency;
								if (localDelta > maxDelta)
									maxDelta = localDelta;
							}
							catch { }
						
						}
						if(maxDelta > 0)
							attackerDamage += maxDelta;
					}
					else
					{
						//Choose at random
						var targetId = random.Next(0, defender.Units.Length);
						while(defender.Units[targetId].Count == 0)
							targetId = random.Next(0, defender.Units.Length);
						try
						{
							float attackEfficiency = frontUnit.Unit.Effectiveness[units.units[defender.Units[targetId].Unit.Index]];
							defender.Units[targetId].WasAttacked = true;
							attackerDamage += attackEfficiency;
							try
							{
								float defenceEfficiency = defender.Units[targetId].Unit.Effectiveness[units.units[frontUnit.Unit.Index]];
								defenceEfficiency *= multiplier;
								defenderDamage += defenceEfficiency;
								attacker.Units[j].WasAttacked = true;
							}
							catch { }

						}
						catch { }

					}
				}
			}
			bool attackerDead = false;
			bool defenderDead = false;
			bool finishedDamageDistribution = false;
			while (!finishedDamageDistribution)
			{
				var damageIndex = WeightedRandom.GetIndex(defender.Units.Length, x => { if(refDefender.Units[x].Count == 0 || !refDefender.Units[x].WasAttacked) return 0; return refAttacker.Army.Units[refDefender.Units[x].Unit.Index].Priority; }, random);
				if (damageIndex == -1)
				{
					//There is no one to attack, check if all dead
					bool allDead = true;
					for (int i = 0; i < defender.Units.Length; i++)
						if (defender.Units[i].Count > 0)
						{
							allDead = false;
							break;
						}
					if (allDead)
					{
						defenderDead = true;
						//Attacker probably won, stop damage distribution
						finishedDamageDistribution = true;
					}
					else
					{
						Console.WriteLine("Weird battle on attack");
					}
				}

				if (defender.Units[damageIndex].Unit.HP > attackerDamage)
				{
					if (attacker == front1)
						savedInitiative1 = (int)attackerDamage;
					else
						savedInitiative2 = (int)attackerDamage;
					finishedDamageDistribution = true;
				}
				else
				{
					attackerDamage -= defender.Units[damageIndex].Unit.HP;
					defender.Units[damageIndex].Count -= 1;
				}
			}

			finishedDamageDistribution = false;
			while (!finishedDamageDistribution)
			{
				var damageIndex = WeightedRandom.GetIndex(attacker.Units.Length, x => { if (refAttacker.Units[x].Count == 0|| !refAttacker.Units[x].WasAttacked) return 0; return refDefender.Army.Units[refAttacker.Units[x].Unit.Index].Priority; }, random);
				if (damageIndex == -1)
				{
					//There is no one to attack, check if all dead
					bool allDead = true;
					for (int i = 0; i < attacker.Units.Length; i++)
						if (attacker.Units[i].Count > 0)
						{
							allDead = false;
							break;
						}
					if (allDead)
					{
						attackerDead = true;
						//Attacker probably lost, stop damage distribution
						finishedDamageDistribution = true;
					}
					else
					{
						Console.WriteLine("Weird battle on defense");
					}
				}
				if (attacker.Units[damageIndex].Unit.HP > defenderDamage)
				{
					if (defender == front1)
						savedInitiative1 = (int)defenderDamage;
					else
						savedInitiative2 = (int)defenderDamage;
					finishedDamageDistribution = true;
				}
				else
				{
					defenderDamage -= attacker.Units[damageIndex].Unit.HP;
					attacker.Units[damageIndex].Count -= 1;
				}
			}

			if (attackerDead && defenderDead)
				return StepResult.EveryoneIsDead;
			else if (attackerDead)
				return StepResult.DefenderWon;
			else if (defenderDead)
				return StepResult.AttackerWon;
			else
				return StepResult.Tie;

		}


	}
}

