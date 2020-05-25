using Microsoft.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDSConvertGroupToTeam
{
    public class GraphHelper
    {
        private static GraphServiceClient graphClient;
        public static void Initialize(IAuthenticationProvider authProvider)
        {
            graphClient = new GraphServiceClient(authProvider);
            graphClient.BaseUrl = "https://graph.microsoft.com/beta";
        }

        public static async Task<User> GetMeAsync()
        {
            try{
                return await graphClient.Me.Request().GetAsync();
            }
            catch(ServiceException ex)
            {
                Console.WriteLine($"Error getting data from user {ex.Message}");
                return null;
            }
        }

        public static async Task<Channel> CreateChannel(string groupId)
        {
            var channel = new Channel
            {
                DisplayName = "🎥 Clases en Línea",
                Description = "En este canal se agendaran las sesiones o clases en línea"
            };

            try{
                return await graphClient.Teams[groupId].Channels
                .Request()
                .AddAsync(channel);
            }
            catch(ServiceException ex)
            {
                Console.WriteLine($"Error getting data from user {ex.Message}");
                return null;
            }
        }


        public static async Task<Team> CreateTeamfromGroup(string groupId)
        {
            var team = new Team
            {
                AdditionalData = new Dictionary<string, object>()
                {
                    {"group@odata.bind",$"https://graph.microsoft.com/v1.0/groups('{groupId}')"},
                    {"template@odata.bind", "https://graph.microsoft.com/beta/teamsTemplates('educationClass')"}
                }
            };

            return await graphClient.Teams
                .Request()
                .AddAsync(team);
        }


    }
}