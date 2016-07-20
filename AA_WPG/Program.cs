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
	class Program
	{
		static List<RemoteDataCluster> clustersList;
		static void Main(string[] args)
		{

			GoogleDataUpdater upd = new GoogleDataUpdater();
			var clusters = 
				from type 
				in Assembly.GetExecutingAssembly().GetTypes() 
              	where type.IsSubclassOf(typeof(RemoteDataCluster)) 
    			select (Activator.CreateInstance(type) as RemoteDataCluster);
			clustersList = new List<RemoteDataCluster>(clusters);

			var commands =
				from type
				in Assembly.GetExecutingAssembly().GetTypes()
					           where type.IsSubclassOf(typeof(ConsoleCommand))
				select (Activator.CreateInstance(type) as ConsoleCommand);

			foreach (var cluster in clusters)
				cluster.Init(upd);

			string command = null;
			while (command != "Exit")
			{
				command = Console.ReadLine();
				var cmdAndArgs = command.Split(' ');
				string[] cmdArgs = new string[cmdAndArgs.Length - 1];
				for (int i = 1; i < cmdAndArgs.Length; i++)
					cmdArgs[i - 1] = cmdAndArgs[i];
				
				var cmd = commands.FirstOrDefault( c => c.GetType().Name == command);
				if (cmd == null)
				{
					if (command != "Exit")
					{
						Console.WriteLine("No such command, try this:");
						foreach (var cmdType in commands)
							Console.WriteLine(cmdType.GetType().Name);
					}
				
				
				}
				else
				{
					cmd.Exectute(cmdArgs);
				}
			}

			//foreach (var cluster in clusters)
			//	cluster.Upload(upd);



		}
	}
}