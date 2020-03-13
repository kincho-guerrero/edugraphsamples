
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

namespace OnlineMeetingScheduling
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
            Console.WriteLine("1.Create a Teams Online Meeting and schedule it for a group of participants"); ;
            Console.WriteLine("Pres any key to get started!");
            var keyinput = Console.ReadLine();

            Console.WriteLine("Provide a subject for your online meeting:");
            var subjectName = Console.ReadLine();
            Console.Clear();

            Console.WriteLine("Provide the organizer email for the meeting:");
            var organizerEmail = Console.ReadLine();
            Console.Clear();

            Console.WriteLine("Provide an Start Date with format: MM/DD/YY HH:MM");
            var startDate = DateTime.Parse(Console.ReadLine());
            Console.Clear();

            Console.WriteLine("Provide an End Date with format: MM/DD/YY HH:MM");
            var endDate = DateTime.Parse(Console.ReadLine());
            Console.Clear();

            Console.WriteLine("Enter atendees emails separated by ;");
            var atendeeList = Console.ReadLine().Split(";");

            try
            {
                Task<OnlineMeeting> callTask = Task.Run(() => CreateOnlineMeeting(subjectName, organizerEmail, atendeeList, startDate, endDate));
                Console.Clear();

                Console.WriteLine("Creating your online Meeting....");
                callTask.Wait();

                var createdMeeeting = callTask.Result;
                Console.Clear();
                Console.WriteLine($"Your Online Meeting {createdMeeeting.Subject} was created successfuly. \nThis is the meeting url {createdMeeeting.JoinUrl}");
            }
            catch (Exception)
            {
                Console.WriteLine($"There was an error creating your team or online meeting, please try again.");
                throw;
            }
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

        //Get user GUID from email 
        static public async Task<String> GetUserFromEmail(string email)
        {
            var graphClient = await CreateGraphClient();

            var requestUrl = $"https://graph.microsoft.com/beta/users/{email}";

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

        //Create Meet Now Online meeting and schedule it in atendees calendars
        static public async Task<OnlineMeeting> CreateOnlineMeeting(string meetingSubject, string organizerEmail, string[] atendees, DateTime startDateTime, DateTime endDateTime)
        {
            var graphClient = await CreateGraphClient();

            var requestUrl = "https://graph.microsoft.com/beta/app/onlineMeetings";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            await graphClient.AuthenticationProvider.AuthenticateRequestAsync(message);


            var participants = new Microsoft.Graph.MeetingParticipants
            {
                Organizer = new MeetingParticipantInfo
                {
                    Identity = new IdentitySet
                    {
                        User = new Identity
                        {
                            Id = await GetUserFromEmail(organizerEmail)
                        }
                    }
                }
            };

            var attendeeList = new List<MeetingParticipantInfo>();

            foreach (var atendeee in atendees)
            {
                var identity = new IdentitySet 
                { 
                    User = new Identity 
                    { 
                        Id = await GetUserFromEmail(organizerEmail) 
                    } 
                };


                attendeeList.Add( 
                    new MeetingParticipantInfo
                    {
                        Identity = identity
                    });
            }

            participants.Attendees = attendeeList;

            OnlineMeeting onlineMeeting = new OnlineMeeting { Subject = meetingSubject, Participants = participants };

            //Remove audioConferencing property from JSON
            JObject jobt = (JObject)JToken.FromObject(onlineMeeting);
            jobt.Remove("audioConferencing");

            message.Content = new StringContent(jobt.ToString(), System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await graphClient.HttpProvider.SendAsync(message);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var oData = graphClient.HttpProvider.Serializer.DeserializeObject<JObject>(content);

                OnlineMeeting meetingCreated = graphClient.HttpProvider.Serializer.DeserializeObject<OnlineMeeting>(oData.ToString());

                try
                {
                    var a = await ScheduleCalendarEvent(meetingSubject, meetingCreated.JoinUrl, startDateTime, endDateTime, organizerEmail, atendees);
                }
                catch (Exception)
                {

                    throw;
                }

                return meetingCreated;
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

        //Schedule a calendar event to surface the online meeting event 
        static public async Task<Event> ScheduleCalendarEvent(string meetingSubject, string inviteBody, DateTime startDate, DateTime endDate, string organizer, string[] atendees)
        {
            var graphClient = await CreateGraphClient();

            var requestUrl = $"https://graph.microsoft.com/beta/users/{organizer}/calendar/events";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            await graphClient.AuthenticationProvider.AuthenticateRequestAsync(message);

            var calendarEvent = new Event
            {
                Subject = meetingSubject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = inviteBody
                },
                Start = new DateTimeTimeZone
                {
                    DateTime = startDate.ToString(),
                    TimeZone = "Pacific Standard Time"
                },
                End = new DateTimeTimeZone
                {
                    DateTime = endDate.ToString(),
                    TimeZone = "Pacific Standard Time"
                },
                Location = new Location
                {
                    DisplayName = "Microsoft Teams Meeting"
                },
                Attendees = new List<Attendee>()
                {
                }
            };

            foreach (var atendee in atendees)
            {
                calendarEvent.Attendees = new List<Attendee>(calendarEvent.Attendees)
                {
                    new Attendee
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = atendee
                        }
                    }
                }.ToArray();
            }

            message.Content = new StringContent(JsonConvert.SerializeObject(calendarEvent), System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await graphClient.HttpProvider.SendAsync(message);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var oData = graphClient.HttpProvider.Serializer.DeserializeObject<JObject>(content);

                Event createdEvent = graphClient.HttpProvider.Serializer.DeserializeObject<Event>(oData.ToString());

                return createdEvent;
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
