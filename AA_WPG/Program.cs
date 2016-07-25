using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace AA_WPG
{
	struct Command
	{
		public string Cmd;
		public string[] Args;
		public Command(string line)
		{
			var cmdAndArgs = line.Split(' ');
			Args = new string[cmdAndArgs.Length - 1];
			for (int i = 1; i < cmdAndArgs.Length; i++)
				Args[i - 1] = cmdAndArgs[i];
			Cmd = cmdAndArgs[0];
		}
	}
	class Program
	{

		static GoogleDataUpdater upd = new GoogleDataUpdater();
		static List<ConsoleCommand> commands;
		static List<RemoteDataCluster> clustersList;
		public static T GetCluster<T>() where T : RemoteDataCluster
		{
			var cluster =  clustersList.Find(x => x.GetType() == typeof(T)) as T;
			if (!cluster.AlreadyInit)
			{
				cluster.AlreadyInit = true;
				cluster.Init(upd);
			}
			return cluster;

		}
		static void Main(string[] args)
		{

			var clusters = 
				from type 
				in Assembly.GetExecutingAssembly().GetTypes() 
              	where type.IsSubclassOf(typeof(RemoteDataCluster)) 
    			select (Activator.CreateInstance(type) as RemoteDataCluster);
			clustersList = new List<RemoteDataCluster>(clusters);

			commands = new List<ConsoleCommand>(
				from type
				in Assembly.GetExecutingAssembly().GetTypes()
					           where type.IsSubclassOf(typeof(ConsoleCommand))
				select (Activator.CreateInstance(type) as ConsoleCommand));

				//cluster.Init(upd);

			string cmdLine = null;
			while (cmdLine != "Exit")
			{
				Console.WriteLine("Enter command, please");
				cmdLine = Console.ReadLine();

				Command command = new Command(cmdLine);
				if (command.Cmd == "Run")
				{
					var lines = File.ReadAllLines("Data/"+command.Args[0]);
					foreach (var line in lines)
					{
						Command subCommand = new Command(line);
						ExecuteCommand(subCommand);
					}
				}
				else
					ExecuteCommand(command);
			}

			//foreach (var cluster in clusters)
			//	cluster.Upload(upd);



		}


		static void ExecuteCommand(Command command)
		{
			var cmd = commands.FirstOrDefault(c => c.GetType().Name == command.Cmd);
			if (cmd == null)
			{
				if (command.Cmd != "Exit")
				{
					Console.WriteLine("No such command, try this:");
					foreach (var cmdType in commands)
						Console.WriteLine(cmdType.GetType().Name);
				}
			}
			else
			{
				cmd.Execute(command.Args);
			}
		}
	}


}