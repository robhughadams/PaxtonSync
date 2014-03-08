using System;
using System.Collections.Generic;
using System.Linq;
using Paxton.Net2.OemClientLibrary;

namespace PaxtonSync
{
	internal class PaxtonUserRepository
	{
		private readonly PaxtonClient _net2Client;

		public PaxtonUserRepository(PaxtonClient net2Client)
		{
			_net2Client = net2Client;
		}

		public IReadOnlyCollection<PaxtonUser> GetAllUsers()
		{
			var allUsers = from pair in _net2Client.ViewUserRecords().UsersList()
			               let user = pair.Value
			               select new PaxtonUser(user);

			return allUsers.ToArray(); //.ToDictionary(u => u.FirstName + " " + u.Surname, u => u);
		}

		public void UpdateUser(PaxtonUser paxtonUser)
		{
			Console.WriteLine("Not really updating user for now...\n{0}", paxtonUser);
			return;


			_net2Client.UpdateUserRecord(
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
				paxtonUser.CustomFields);
		}
	}
}
