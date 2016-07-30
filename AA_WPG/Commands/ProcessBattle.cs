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
			int stepsCount = int.Parse(args[2]);
			string situationFilePath = args[3];

			string armyTag1 = front1FullName[0];
			string armyTag2 = front2FullName[0];

			string frontName1 = front1FullName[1];
			string frontName2 = front2FullName[1];
			float firstMod = float.Parse(args[4]);
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
		float savedDamage1 = 0;
		float savedDamage2 = 0;
		int phase = 0;
		StepResult SimulateBattle(Frontline front1, Frontline front2, out Frontline attacker, out Frontline defender)
		{

			log.Append("Phase: ").Append(phase++).Append(Environment.NewLine);
			int initiative1 = random.Next(0, front1.Army.Initiative);
			int initiative2 = random.Next(0, front2.Army.Initiative);
			float multiplier = 1;

			float attackerDamage = 0;
			float defenderDamage = 0;
				
			if (initiative1 > initiative2)
			{
				attacker = front1;
				defender = front2;
				multiplier = (float)initiative2 / (float)initiative1;
				attackerDamage += savedDamage1;
				defenderDamage += savedDamage2;
				log.Append("Attacks: ").Append(front1.ID).Append("Initiative = ").Append(initiative1).Append(Environment.NewLine);
				log.Append("Defends: ").Append(front2.ID).Append("Initiative = ").Append(initiative2).Append(Environment.NewLine);
			}
			else
			{
				attacker = front2;
				defender = front1;
				multiplier = (float)initiative1 / (float)initiative2;

				attackerDamage += savedDamage2;
				defenderDamage += savedDamage1;
				log.Append("Attacks: ").Append(front2.ID).Append("Initiative = ").Append(initiative2).Append(Environment.NewLine);
				log.Append("Defends: ").Append(front1.ID).Append("Initiative = ").Append(initiative1).Append(Environment.NewLine);
			}
			Frontline refAttacker = attacker;
			Frontline refDefender = defender;


			savedDamage1 = 0;
			savedDamage2 = 0;
			for (int i = 0; i < attacker.Units.Length; i++)
				attacker.Units[i].WasAttacked = false;
			for (int i = 0; i < defender.Units.Length; i++)
				defender.Units[i].WasAttacked = false;
			float attackerYearMod = 1;
			float defenderYearMod = 1;
			if(attacker.Army.Year - defender.Army.Year < 0)
				defenderYearMod = 1 + (defender.Army.Year - attacker.Army.Year) * units.TimeAheadModifier;
			else
				attackerYearMod = 1 + (attacker.Army.Year - defender.Army.Year) * units.TimeAheadModifier;
			bool damageAttackerAir = false;
			bool damageAttackerLand = false;
			bool damageAttackerSea = false;
			bool damageDefenderAir = false;
			bool damageDefenderLand = false;
			bool damageDefenderSea = false;
			Action<Frontline, UnitType> unitTypeAttack = (front, type) =>
			{
				if (front == refAttacker)
					switch (type)
					{
						case UnitType.Air: damageAttackerAir = true; break;
						case UnitType.Land: damageAttackerLand = true; break;
						case UnitType.Sea: damageAttackerSea = true; break;
					}
				else
					switch (type)
					{
						case UnitType.Air: damageDefenderAir = true; break;
						case UnitType.Land: damageDefenderLand = true; break;
						case UnitType.Sea: damageDefenderSea = true; break;
					}

			};
			for (int i = 0; i < attacker.Units.Length; i++)
			{
				var frontUnit = attacker.Units[i];
				for (int j = 0; j < frontUnit.Count; j++)
				{
					log.Append("Unit ").Append(frontUnit.Unit.Name).Append(" attacks ");
					if (random.NextDouble() < frontUnit.Unit.Organization)
					{
						log.Append("with max efficiency he chooses ");
						//Choose max efficiency
						float maxDelta = 0;
						float attack = 0;
						float defence = 0;
						int chosenId = 0;
						bool crit = random.Next(0, 15) == 0;
						for (int k = 0; k < defender.Units.Length; k++)
						{
							if (defender.Units[k].Count == 0)
								continue;
							int targetId = k;
							try
							{
								float attackEfficiency = frontUnit.Unit.Effectiveness[units.units[defender.Units[targetId].Unit.Index]];
								attackEfficiency *= attacker.Army.Weapons;
								attackEfficiency *= attackerYearMod;
								//Console.WriteLine(attackEfficiency);
								switch (defender.Units[targetId].Unit.Type)
								{
									case UnitType.Air:
										attackEfficiency *= attacker.Army.Pilots;
										break;
									case UnitType.Land:
										attackEfficiency *= attacker.Army.Soldiers;
										break;
									case UnitType.Sea:
										attackEfficiency *= attacker.Army.Seamen;
										break;
								}
								//defender.Units[targetId].WasAttacked = true;
								float defenceEfficiency = 0;
								try
								{
									defenceEfficiency = defender.Units[targetId].Unit.Effectiveness[units.units[frontUnit.Unit.Index]];
									defenceEfficiency *= multiplier;
									defenceEfficiency *= defender.Army.Weapons;
									defenceEfficiency *= defenderYearMod;
									switch (defender.Units[targetId].Unit.Type)
									{
										case UnitType.Air:
											defenceEfficiency *= defender.Army.Pilots;
											break;
										case UnitType.Land:
											defenceEfficiency *= defender.Army.Soldiers;
											break;
										case UnitType.Sea:
											defenceEfficiency *= defender.Army.Seamen;
											break;
									}

								}
								catch { }
								if (crit)
								{
									var localDelta = attackEfficiency - defenceEfficiency;
									if (localDelta > maxDelta)
									{

										maxDelta = localDelta;
										attack = attackEfficiency;
										defence = defenceEfficiency;
										chosenId = targetId;
									}
								}
								else
								{
									if (attackEfficiency > maxDelta)
									{

										maxDelta = attackEfficiency;
										attack = attackEfficiency;
										defence = defenceEfficiency;
										chosenId = targetId;
									}
								}

							}
							catch { }
						
						}
						attackerDamage += attack;
						defenderDamage += defence;


						unitTypeAttack(defender, defender.Units[chosenId].Unit.Type);
						unitTypeAttack(attacker, frontUnit.Unit.Type);
						log.Append(defender.Units[chosenId].Unit.Name).Append(" with ").Append(attack).Append(" attack efficiency and ").Append(defence).Append(" defence efficiency of enemy").AppendLine();
							
					}
					else
					{

						log.Append("at random he chooses ");
						//Choose at random
						var targetId = random.Next(0, defender.Units.Length);
						while(defender.Units[targetId].Count == 0)
							targetId = random.Next(0, defender.Units.Length);
						try
						{
							float attackEfficiency = frontUnit.Unit.Effectiveness[units.units[defender.Units[targetId].Unit.Index]];

							unitTypeAttack(defender, defender.Units[targetId].Unit.Type);
							attackEfficiency *= attacker.Army.Weapons;
							attackEfficiency *= attackerYearMod;
							switch (defender.Units[targetId].Unit.Type)
							{
								case UnitType.Air:
									attackEfficiency *= attacker.Army.Pilots;
									break;
								case UnitType.Land:
									attackEfficiency *= attacker.Army.Soldiers;
									break;
								case UnitType.Sea:
									attackEfficiency *= attacker.Army.Seamen;
									break;
							}
							attackerDamage += attackEfficiency;
							log.Append(defender.Units[targetId].Unit.Name).Append(" with ").Append(attackEfficiency).Append(" attack efficiency and ");
							try
							{
								float defenceEfficiency = defender.Units[targetId].Unit.Effectiveness[units.units[frontUnit.Unit.Index]];
								defenceEfficiency *= multiplier;
								defenceEfficiency *= defender.Army.Weapons;
								defenceEfficiency *= defenderYearMod;
								switch (defender.Units[targetId].Unit.Type)
								{
									case UnitType.Air:
										defenceEfficiency *= defender.Army.Pilots;
										break;
									case UnitType.Land:
										defenceEfficiency *= defender.Army.Soldiers;
										break;
									case UnitType.Sea:
										defenceEfficiency *= defender.Army.Seamen;
										break;
								}
								defenderDamage += defenceEfficiency;

								unitTypeAttack(attacker, frontUnit.Unit.Type);
								//attacker.Units[j].WasAttacked = true;
								log.Append(defenceEfficiency).Append(" defence efficiency of enemy").AppendLine();
							}
							catch { }

						}
						catch { }

					}
				}
				log.Append(Environment.NewLine);

			}
			bool attackerDead = false;
			bool defenderDead = false;
			bool finishedDamageDistribution = false;



			Func<Frontline, UnitType, bool> unitTypeAttacked = (front, type) => 
			{
				if (front == refAttacker)
					switch (type)
					{
						case UnitType.Air: return damageAttackerAir;
						case UnitType.Land: return damageAttackerLand;
						case UnitType.Sea: return damageAttackerSea;
					}
				else
					switch (type)
					{
						case UnitType.Air: return damageDefenderAir;
						case UnitType.Land: return damageDefenderLand;
						case UnitType.Sea: return damageDefenderSea;
					}
				return false;
			
			};


			while (!finishedDamageDistribution)
			{
				var damageIndex = WeightedRandom.GetIndex(defender.Units.Length, x => { if(refDefender.Units[x].Count == 0 || !unitTypeAttacked(refDefender, refDefender.Units[x].Unit.Type)) return 0; return refAttacker.Army.Units[refDefender.Units[x].Unit.Index].Priority; }, random);
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
						break;
					}
					else
					{
						Console.WriteLine("Weird battle on attack");
					}
				}

				if (defender.Units[damageIndex].Unit.HP > attackerDamage)
				{
					if (attacker == front1)
						savedDamage1 = attackerDamage;
					else
						savedDamage2 = attackerDamage;
					finishedDamageDistribution = true;
				}
				else
				{
					attackerDamage -= defender.Units[damageIndex].Unit.HP;
					defender.Units[damageIndex].Count -= 1;
					log.Append(defender.ID).Append(" unit ").Append(defender.Units[damageIndex].Unit.Name).Append(" destroyed").AppendLine();
				}
			}

			finishedDamageDistribution = false;
			while (!finishedDamageDistribution)
			{
				var damageIndex = WeightedRandom.GetIndex(attacker.Units.Length, x => { if (refAttacker.Units[x].Count == 0|| !unitTypeAttacked(refAttacker, refAttacker.Units[x].Unit.Type)) return 0; return refDefender.Army.Units[refAttacker.Units[x].Unit.Index].Priority; }, random);
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
						break;
					}
					else
					{
						Console.WriteLine("Weird battle on defense");
					}
				}
				if (attacker.Units[damageIndex].Unit.HP > defenderDamage)
				{
					if (defender == front1)
						savedDamage1 = defenderDamage;
					else
						savedDamage2 = defenderDamage;
					finishedDamageDistribution = true;
				}
				else
				{
					defenderDamage -= attacker.Units[damageIndex].Unit.HP;
					attacker.Units[damageIndex].Count -= 1;

					log.Append(attacker.ID).Append(" unit ").Append(attacker.Units[damageIndex].Unit.Name).Append(" destroyed").AppendLine();
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

