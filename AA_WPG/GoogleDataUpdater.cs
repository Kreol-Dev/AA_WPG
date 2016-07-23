using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace AA_WPG
{
	public class GoogleDataUpdater
	{
		SheetsService service;
		// If modifying these scopes, delete your previously saved credentials
	 	// at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
		static string[] Scopes = { SheetsService.Scope.Spreadsheets };
		static string ApplicationName = "my_aa_client";
		public GoogleDataUpdater()
		{

			UserCredential credential;

				using (var stream =
				new FileStream("C:\\AA\\client_secret_405359896107-cbcdg0ooutsmf06od9061q6fcq4gqjq0.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
				{
					string credPath = System.Environment.GetFolderPath(
						System.Environment.SpecialFolder.Personal);
					credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

					credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
						GoogleClientSecrets.Load(stream).Secrets,
						Scopes,
						"user",
						CancellationToken.None,
						new FileDataStore(credPath, true)).Result;
					Console.WriteLine("Credential file saved to: " + credPath);
				}

				service = new SheetsService(new BaseClientService.Initializer()
				{
					HttpClientInitializer = credential,
					ApplicationName = ApplicationName,
				});




		}


		public IList<IList<Object>> GetSheetRange(string list, char fromCol, int fromRow, char toCol, int toRow, string sheetId)
		{
			var request = service.Spreadsheets.Values.Get(sheetId, String.Format("{0}!{1}{2}:{3}{4}", list, fromCol, fromRow, toCol, toRow));
			var values = request.Execute();
			return values.Values;
		}

		public void UpdateSheetRange(IList<IList<Object>> values, string list, char fromCol, int fromRow, char toCol, char toRow, string sheetId)
		{
			ValueRange range = new ValueRange();
			range.Values = values;
			var request = service.Spreadsheets.Values.Update(range, sheetId, String.Format("{0}!{1}{2}:{3}{4}", list, fromCol, fromRow, toCol, toRow));
			//var values = request.Execute();
			//return values.Values;
			Console.WriteLine(request.Execute());
		}
	}
}

