using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using TinyCsvParser;

namespace EDUScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("EDU Graph API Scheduler");

            var appConfig = LoadAppSettings();

            if (appConfig == null){
                Console.WriteLine("Missing AppId or Scope in settings .json");
            }
            
            var appId = appConfig["appId"];
            var scopesString = appConfig["scopes"];
            var scopes = scopesString.Split(";");

            //Upload the CSV files into memory

            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvClassScheduleMapping csvMapper = new CsvClassScheduleMapping();
            CsvParser<ClassSchedule> csvParser = new CsvParser<ClassSchedule>(csvParserOptions, csvMapper);
            var schedules = csvParser.ReadFromFile(@"TestData.csv", Encoding.ASCII);
                        

            foreach (var details in schedules)
            {
                Console.WriteLine(details.Result.SISID + " " + details.Result.GroupID );
            }

             // Initialize the auth provider with values from appsettings.json
            var authProvider = new DeviceCodeAuthProvider(appId, scopes);

            // Request a token to sign in the user
            var accessToken = authProvider.GetAccessToken().Result;

            GraphHelper.Initialize(authProvider);

            var user = GraphHelper.GetMeAsync().Result;
            Console.WriteLine($"Welcome {user.DisplayName}!\n");

            int choice = -1;

            while(choice != 0){
                Console.WriteLine("Please choose the following options");
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Display ");
                Console.WriteLine("2. List Calendar Events");

                try{
                    choice = int.Parse(Console.ReadLine());
                }
                catch (System.FormatException){
                    choice = -1;
                }

                switch(choice){
                    case 0:
                    Console.WriteLine("Closing");
                    break;

                    case 1:
                    // Display access token
                    Console.WriteLine($"Access token: {accessToken}\n");
                    break;

                    case 2:
                    break;

                    default:
                    Console.WriteLine("Invalid Choice please try again");
                    break;
                }
            }
        }
        
        static IConfigurationRoot LoadAppSettings(){
            var appConfig = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

            if (string.IsNullOrEmpty(appConfig["appId"]) ||
                string.IsNullOrEmpty(appConfig["scopes"]))
            {
                return null;
            }

            return appConfig;
        }
    }
}
