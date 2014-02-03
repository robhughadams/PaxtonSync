using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
using PaxtonSync.Properties;
using Paxton.Net2.OemClientLibrary;

namespace PaxtonSync
{
    static class Program
    {
        private const int REMOTE_PORT = 8025;
        private const string REMOTE_HOST = "localhost";

        static void Main(string[] args)
        {
            Console.WriteLine("Fetching membership details from www.bec-cave.org.uk");
            var membershipDetails = _DownloadMembershipDetails();

            _SyncMembers(membershipDetails);

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static IReadOnlyCollection<MembershipDetails> _DownloadMembershipDetails()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(String.Format("{0}:{1}", Settings.Default.WebUser, Settings.Default.WebPass))));

                var responseStream = client.GetStreamAsync(Settings.Default.DownloadUrl).Result;
                using (var reader = new JsonTextReader(new StreamReader(responseStream)))
                {
                    var jsonReader = new JsonSerializer();
                    return jsonReader.Deserialize<IReadOnlyCollection<MembershipDetails>>(reader);
                }
            }
        }

        private static void _SyncMembers(IReadOnlyCollection<MembershipDetails> membershipDetails)
        {
            using (var net2Client = _CreateClient())
            {
                _Login(net2Client);

                var userRepository = new PaxtonUserRepository(net2Client);

                var paxtonUsers = userRepository.GetAllUsers();

                foreach (var details in membershipDetails)
                {
                    Console.WriteLine("Looking up: {0} {1} {2}", details.BecNumber, details.FirstName, details.LastName);

                    var matchingUsers = paxtonUsers
                        .Where(u =>
                            u.Surname.Equals(details.LastName, StringComparison.OrdinalIgnoreCase)
                            && u.FirstName.Equals(details.FirstName, StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    var matches = matchingUsers.Count();
                    if (matches == 0)
                        Console.WriteLine("No matching user found.");

                    if (matches == 1)
                    {
                        Console.WriteLine("Single matching user found - good times.");

                        var paxtonUser = matchingUsers.First();
                        if (paxtonUser.BecNumber == null)
                        {
                            Console.WriteLine("No BEC number on Paxton DB - adding it.");

                            paxtonUser.BecNumber = details.BecNumber;
                            userRepository.UpdateUser(paxtonUser);
                        }
                    }

                    if (matches > 1)
                        Console.WriteLine("Multiple matching users found - bad times.");
                }
            }
        }
        
        private static void _Login(OemClient _net2Client)
        {
            var users = _net2Client
                .GetListOfOperators()
                .UsersDictionary()
                .Where(pair => pair.Value == Settings.Default.PaxtonUser);

            if (!users.Any())
                throw new Exception("Paxton user not found.");

            var userId = users.Single().Key;

            var methodList = _net2Client.AuthenticateUser(userId, Settings.Default.PaxtonPass);
            if (methodList == null)
                throw new Exception("Can't log onto Paxton.");
        }

        private static OemClient _CreateClient()
        {
            var net2Client = new OemClient(REMOTE_HOST, REMOTE_PORT);

            if (net2Client.LastErrorMessage != null)
                throw new Exception(net2Client.LastErrorMessage);

            return net2Client;
        }
    }
}
