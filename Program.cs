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
		private static bool _trialRunOnly;

		internal static void Main(string[] args)
		{
			if (args.Any() && args.First().StartsWith("t"))
			{
				_trialRunOnly = true;
				Logger.WriteLine("*** Trial Run ***");
			}

			Logger.WriteLine("Fetching membership details from www.bec-cave.org.uk");
			var membershipDetails = _DownloadMembershipDetails();

			membershipDetails = _CombineMembershipsForSameMember(membershipDetails);

			_SyncMembers(membershipDetails);
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
				var userRepository = new PaxtonUserRepository(net2Client, _trialRunOnly);

				var paxtonUsers = userRepository.GetAllUsers();

				foreach (var details in membershipDetails)
					_SyncMember(details, paxtonUsers, userRepository);
			}
		}

		private static void _SyncMember(MembershipDetails details, IReadOnlyCollection<PaxtonUser> paxtonUsers, PaxtonUserRepository userRepository)
		{
			Logger.WriteLine("Looking up user: {0} {1} {2}", details.BecNumber, details.FirstName, details.LastName);

			IReadOnlyCollection<PaxtonUser> matchingUsers = paxtonUsers
				.Where(u =>
					u.Surname.Equals(details.LastName, StringComparison.OrdinalIgnoreCase)
						&& u.FirstName.Equals(details.FirstName, StringComparison.OrdinalIgnoreCase))
				.ToArray();

			var matches = matchingUsers.Count();
			if (matches == 0)
			{
				Logger.WriteLine("No matching user found.");
				_AddNewPaxtonUser(userRepository, details);
			}

			if (matches == 1)
			{
				Logger.WriteLine("Single matching user found - good times.");
				_UpdateExistingPaxtonUser(userRepository, details, matchingUsers.First());
			}

			if (matches > 1)
				Logger.WriteLine("Multiple matching users found - bad times.");
		}

		private static void _AddNewPaxtonUser(PaxtonUserRepository userRepository, MembershipDetails details)
		{
			// Assuming the ID's won't change
			const int currentMembersAccess = 4; 
			const int currentMemeberDepartmentId = 2; // 16 is probationary, but not sure I can be arsed with that.
			userRepository.CreateUser(currentMembersAccess, currentMemeberDepartmentId,
				details.FirstName, details.LastName, details.BecNumber);
		}

		private static readonly IEnumerable<MembershipStatus> _noAccessMembershipStatuses = new[] 
		{ 
			MembershipStatus.Cancelled,
			MembershipStatus.Deceased, 
			MembershipStatus.Expired 
		};

		private static void _UpdateExistingPaxtonUser(PaxtonUserRepository userRepository, MembershipDetails details, PaxtonUser paxtonUser)
		{
			if (paxtonUser.BecNumber == null)
			{
				Logger.WriteLine("No BEC number on Paxton DB - adding it.");

				paxtonUser.BecNumber = details.BecNumber;
				userRepository.UpdateUser(paxtonUser);
			}

			const int noAccess = 0; //Hopefully safe to hard code this - matches a value from a lookup table in the Paxton DB.

			if (_StatusShouldHaveNoAccess(details.MembershipStatus))
			{
				if (paxtonUser.AccessLevelId != noAccess)
				{
					Logger.WriteLine("User's membership status is {0} setting access level to 'No Access'", details.MembershipStatus);

					paxtonUser.AccessLevelId = noAccess;
					userRepository.UpdateUser(paxtonUser);
				}
				else
				{
					Logger.WriteLine("User's access level is already set to 'No Access'", details.MembershipStatus);
				}

				return;
			}

			Logger.WriteLine("Current users access level is {0}.", details.MembershipStatus);
		}

		private static bool _StatusShouldHaveNoAccess(MembershipStatus status)
		{
			// Last year, but not this year members still have access from the AGM to the 1st Feb
			var currentMonth = DateTime.UtcNow.Month;
			var inGracePeriod = currentMonth == 1 || (currentMonth > 9 && currentMonth < 12);

			return _noAccessMembershipStatuses.Contains(status)
				|| (status == MembershipStatus.LYBNTY && !inGracePeriod);
		}
	}
}
