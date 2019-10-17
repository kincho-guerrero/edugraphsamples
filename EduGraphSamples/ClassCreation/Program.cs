using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ClassCreation
{
    class Program
    {
        //Variables for Azure AD App ID
        static string tenantId = "YOUR TENANT ID";
        static string clientID = "YOUR CLIENT ID";
        static string clientSecret = "YOUR CLIENT SECRET";

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Graph API EDU Education Samples.");
            Console.WriteLine("This sample will do the following actions using Graph API:");
            Console.WriteLine("1. Create a new customized class team from a JSON template");;
            Console.WriteLine("Pres any key to get started!");
            var keyinput = Console.ReadLine();

            Console.Clear();
            Console.WriteLine("Provide a name for your class:");
            var className = Console.ReadLine();

            Console.Clear();
            Console.WriteLine("Provide a description for your class:");
            var classDsc = Console.ReadLine();

            Console.Clear();
            Console.WriteLine("Provide an email for the Class Team owner:");
            var classOwner = Console.ReadLine();

            try
            {
                Task<string> callTask = Task.Run(() => CreateClassTeam(className, classDsc, classOwner));
                Console.Clear();
                Console.WriteLine("Creating your class team....");

                callTask.Wait();
                var teamId = callTask.Result;
                Console.Clear();
                Console.WriteLine($"Your Class Team {className} was created successfuly. \nThe team Id is {teamId}");
            }
            catch (Exception)
            {
                Console.WriteLine($"There was an error creating your team, please try again.");
                throw;
            }

            Console.WriteLine("\n Press enter to continue ....");
            Console.ReadLine();
            Console.Clear();
           
        }

        //Graph client for App access with bearer tokens 
        static public async Task<GraphServiceClient> CreateGraphClient()
        {
            var authority = $"https://login.microsoftonline.com/{tenantId}";
            var app = ConfidentialClientApplicationBuilder.Create(clientID)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri(authority))
                .Build();

            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var authenticationResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(requestMessage =>
                {
                    requestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue("bearer", authenticationResult.AccessToken);
                    return Task.FromResult(0);
                }));
            return graphClient;
        }


        //Create a class team from a predefined JSON Template
        static public async Task<string>  CreateClassTeam(string className, string classDescription, string ownerEmail)
        {
            string classJson = "";

            using (StreamReader r = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "/ClassStructure/class_template1.json")) { 
            classJson = r.ReadToEnd();
            } ;

            JObject classTemplate = JObject.Parse(classJson);

            var userId = await GetUserFromEmail("fcarrasco@udemo.space");

            var owners = new[] { $@"https://graph.microsoft.com/beta/users('{userId}')"};
            var ownersArray = JArray.Parse(JsonConvert.SerializeObject(owners));

            classTemplate.Add("owners@odata.bind", ownersArray);


            var graphClient = await CreateGraphClient();

            var requestUrl = "https://graph.microsoft.com/beta/teams";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            await graphClient.AuthenticationProvider.AuthenticateRequestAsync(message);

            message.Content = new StringContent(classTemplate.ToString(), System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await graphClient.HttpProvider.SendAsync(message);

            if (response.IsSuccessStatusCode)
            {
                var location = response.Headers.Location.ToString();

                var teamsId = location.Substring(location.IndexOf("'") + 1, location.IndexOf("'") +30);

                return teamsId;
            }
            else
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = response.StatusCode.ToString(),
                        Message = await response.Content.ReadAsStringAsync()
                    }
                    );
            }
        }

       

        //Get user GUID from email 
        static public async Task<String> GetUserFromEmail(string email)
        {
            var graphClient = await CreateGraphClient();

            var requestUrl = $"https://graph.microsoft.com/beta/users/{email}" ;

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            await graphClient.AuthenticationProvider.AuthenticateRequestAsync(message);


            HttpResponseMessage response = await graphClient.HttpProvider.SendAsync(message);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var oData = graphClient.HttpProvider.Serializer.DeserializeObject<User>(content);

                return oData.Id.ToString();

            }
            else
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = response.StatusCode.ToString(),
                        Message = await response.Content.ReadAsStringAsync()
                    }
                    );
            }
        }

    }
}
