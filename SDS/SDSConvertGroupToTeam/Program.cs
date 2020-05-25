using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using TinyCsvParser;

namespace SDSConvertGroupToTeam
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SDS Converter from Group to Team");

            Console.WriteLine("Enter App Id from Azure Portal");
            var appId = Console.ReadLine();

            var scopesString = "Group.ReadWrite.All";
            var scopes = scopesString.Split(";");

            //Upload the CSV file into memory

            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvSectionMapping csvMapper = new CsvSectionMapping();
            CsvParser<SectionUsage> csvParser = new CsvParser<SectionUsage>(csvParserOptions, csvMapper);
            var sections = csvParser.ReadFromFile(@"TestData.csv", Encoding.ASCII);

             // Initialize the auth provider with values from appsettings.json
            var authProvider = new DeviceCodeAuthProvider(appId, scopes);

            // Request a token to sign in the user
            var accessToken = authProvider.GetAccessToken().Result;

            GraphHelper.Initialize(authProvider);

            int choice = -1;

            while(choice != 0){
                Console.WriteLine("Please choose the following options");
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Convert 1 Section From Group to team ");
                Console.WriteLine("2. Bulk convert all sections from groups to teams");

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
                    Console.WriteLine("Enter the section GraphId:");
                    var groupid = Console.ReadLine();
                    GraphHelper.CreateTeamfromGroup(groupid).Wait();
                    break;

                    case 2:

                    foreach (var section in sections)
                    {
                        try
                        {
                        GraphHelper.CreateTeamfromGroup(section.Result.GraphId).Wait();   
                        }
                        catch(Exception e){
                            Console.WriteLine($"Error creating {section.Result.SisName}");
                            Console.WriteLine(e.Message);
                        }

                        Console.WriteLine($"Succesfully Converted: {section.Result.SisName}");
                        Console.WriteLine();         
                    }               
                    break;

                    default:
                    Console.WriteLine("Invalid Choice please try again");
                    break;
                }
            }
        }
    }
}
