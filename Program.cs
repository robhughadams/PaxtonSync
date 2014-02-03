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

                var allUsers = from pair in net2Client.ViewUserRecords().UsersList()
                               let user = pair.Value
                               select user;

                var paxtonUsers = allUsers.ToArray(); //.ToDictionary(u => u.FirstName + " " + u.Surname, u => u);

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
                        if (paxtonUser.BecNumber() == null)
                        {
                            UpdateBecNumber(net2Client, paxtonUser, details.BecNumber);
                        }
                    }

                    if (matches > 1)
                        Console.WriteLine("Multiple matching users found - bad times.");
                }
            }
        }

        private static void UpdateBecNumber(OemClient net2Client, IUserView paxtonUser, int becNumber)
        {
            Console.WriteLine("No BEC number on Paxton DB - adding it.");

            var result = net2Client.UpdateUserRecord(
                paxtonUser.UserId,
                paxtonUser.AccessLevelId,
                paxtonUser.DepartmentId,
                paxtonUser.AntiPassbackUser,
                paxtonUser.AlarmUser,
                paxtonUser.FirstName,
                paxtonUser.MiddleName,
                paxtonUser.Surname,
                paxtonUser.Telephone,
                paxtonUser.Extension,
                paxtonUser.PIN,
                paxtonUser.Picture,
                paxtonUser.ActivationDate,
                paxtonUser.Active,
                paxtonUser.Fax,
                paxtonUser.ExpiryDate,
                _GetCustomFields(paxtonUser, becNumber));

            if (!result)
                throw new Exception(net2Client.LastErrorMessage);
        }

        private static string[] _GetCustomFields(IUserView paxtonUser, int becNumber)
        {
            return new[] {
                null, //Sample progam pads the first item for no discernable reason
                paxtonUser.Field1_100,
                paxtonUser.Field2_100,
                paxtonUser.Field3_50,
                paxtonUser.Field4_50,
                paxtonUser.Field5_50,
                paxtonUser.Field6_50,
                paxtonUser.Field7_50,
                paxtonUser.Field8_50,
                paxtonUser.Field9_50,
                paxtonUser.Field10_50,
                paxtonUser.Field11_50,
                paxtonUser.Field12_50,
                paxtonUser.Field13_Memo,
                becNumber.ToString()
            };
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
