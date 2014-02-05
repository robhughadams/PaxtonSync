using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Paxton.Net2.OemClientLibrary;
using PaxtonSync.Properties;

namespace PaxtonSync
{
	internal static class Program
	{
		private const int _remotePort = 8025;
		private const string _remoteHost = "localhost";

		internal static void Main(string[] args)
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

					IReadOnlyCollection<PaxtonUser> matchingUsers = paxtonUsers
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
						_UpdateExistingPaxtonUser(userRepository, details, matchingUsers.First());
					}

					if (matches > 1)
						Console.WriteLine("Multiple matching users found - bad times.");
				}
			}
		}

		private static readonly IEnumerable<MembershipStatus> _noAccessMembershipStatuses = new[] { MembershipStatus.Cancelled, MembershipStatus.Deceased, MembershipStatus.Expired };

		private const int _noAccess = 0; //Hopefully safe to hard code this.

		private static void _UpdateExistingPaxtonUser(PaxtonUserRepository userRepository, MembershipDetails details, PaxtonUser paxtonUser)
		{
			if (paxtonUser.BecNumber == null)
			{
				Console.WriteLine("No BEC number on Paxton DB - adding it.");

				paxtonUser.BecNumber = details.BecNumber;
				userRepository.UpdateUser(paxtonUser);
			}

			if (_noAccessMembershipStatuses.Contains(details.MembershipStatus)
				&& paxtonUser.AccessLevelId != _noAccess)
			{
				Console.WriteLine("Users membership status is {0} setting access level to 'No Access'", details.MembershipStatus);

				paxtonUser.AccessLevelId = _noAccess;
				userRepository.UpdateUser(paxtonUser);
			}
		}

		private static void _Login(OemClient net2Client)
		{
			var users = net2Client
				.GetListOfOperators()
				.UsersDictionary()
				.Where(pair => pair.Value == Settings.Default.PaxtonUser)
				.ToArray();

			if (!users.Any())
				throw new Exception("Paxton user not found.");

			var userId = users.Single().Key;

			var methodList = net2Client.AuthenticateUser(userId, Settings.Default.PaxtonPass);
			if (methodList == null)
				throw new Exception("Can't log onto Paxton.");
		}

		private static OemClient _CreateClient()
		{
			var net2Client = new OemClient(_remoteHost, _remotePort);

			if (net2Client.LastErrorMessage != null)
				throw new Exception(net2Client.LastErrorMessage);

			return net2Client;
		}
	}
}
