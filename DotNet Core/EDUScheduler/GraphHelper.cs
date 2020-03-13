using Microsoft.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EDUScheduler
{
    public class GraphHelper
    {
        private static GraphServiceClient graphClient;
        public static void Initialize(IAuthenticationProvider authProvider)
        {
            graphClient = new GraphServiceClient(authProvider);
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
    }
}