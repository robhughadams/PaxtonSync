using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using PaxtonSync.Properties;

namespace PaxtonSync
{
	internal static class Program
	{
		internal static void Main(string[] args)
		{
			Console.WriteLine("Fetching membership details from www.bec-cave.org.uk");
			var membershipDetails = _DownloadMembershipDetails();

			membershipDetails = _CombineMembershipsForSameMember(membershipDetails);

			_SyncMembers(membershipDetails);

			//Console.WriteLine("Press enter to exit.");
			//Console.ReadLine();
		}

		private static IEnumerable<MembershipDetails> _CombineMembershipsForSameMember(IEnumerable<MembershipDetails> membershipDetails)
		{
			return from details in membershipDetails
				   group details by details.BecNumber into groupedDetails
				   select groupedDetails.Count() == 1
					   ? groupedDetails.Single()
					   : _CreateSingleMembershipWithCorrectStatus(groupedDetails);
		}

		private static MembershipDetails _CreateSingleMembershipWithCorrectStatus(IGrouping<int, MembershipDetails> groupedDetails)
		{
			var firstDetails = groupedDetails.First();

			var statusToUse = groupedDetails
				.OrderBy(d => d.MembershipStatus)
				.First().MembershipStatus;

			return new MembershipDetails
			{
				BecNumber = firstDetails.BecNumber,
				FirstName = firstDetails.FirstName,
				LastName = firstDetails.LastName,
				MembershipStatus = statusToUse
			};
		}

		private static IEnumerable<MembershipDetails> _DownloadMembershipDetails()
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

		private static void _SyncMembers(IEnumerable<MembershipDetails> membershipDetails)
		{
			using (var net2Client = new PaxtonClient())
			{
				var userRepository = new PaxtonUserRepository(net2Client);

				var paxtonUsers = userRepository.GetAllUsers();

				foreach (var details in membershipDetails)
					_SyncMember(details, paxtonUsers, userRepository);
			}
		}

		private static void _SyncMember(MembershipDetails details, IReadOnlyCollection<PaxtonUser> paxtonUsers, PaxtonUserRepository userRepository)
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

		private static readonly IEnumerable<MembershipStatus> _noAccessMembershipStatuses = new[] { MembershipStatus.Cancelled, MembershipStatus.Deceased, MembershipStatus.Expired };

		private static void _UpdateExistingPaxtonUser(PaxtonUserRepository userRepository, MembershipDetails details, PaxtonUser paxtonUser)
		{
			if (paxtonUser.BecNumber == null)
			{
				Console.WriteLine("No BEC number on Paxton DB - adding it.");

				paxtonUser.BecNumber = details.BecNumber;
				userRepository.UpdateUser(paxtonUser);
			}

			const int _noAccess = 0; //Hopefully safe to hard code this.

			if (_noAccessMembershipStatuses.Contains(details.MembershipStatus)
				&& paxtonUser.AccessLevelId != _noAccess)
			{
				Console.WriteLine("Users membership status is {0} setting access level to 'No Access'", details.MembershipStatus);

				paxtonUser.AccessLevelId = _noAccess;
				userRepository.UpdateUser(paxtonUser);
			}
		}
	}
}
