using System;
using System.Collections.Generic;
using System.Linq;
using Paxton.Net2.OemClientLibrary;

namespace PaxtonSync
{
	internal class PaxtonUserRepository
	{
		private readonly PaxtonClient _net2Client;
		private readonly bool _trialRunOnly;

		public PaxtonUserRepository(PaxtonClient net2Client, bool trialRunOnly)
		{
			_net2Client = net2Client;
			_trialRunOnly = trialRunOnly;
		}

		public IReadOnlyCollection<PaxtonUser> GetAllUsers()
		{
			var allUsers = from pair in _net2Client.ViewUserRecords().UsersList()
			               let user = pair.Value
			               select new PaxtonUser(user);

			return allUsers.ToArray();
		}

		public void UpdateUser(PaxtonUser paxtonUser)
		{
			if (_trialRunOnly)
			{
				Logger.WriteLine("{0}\tTrial run - not updating user.", paxtonUser);
				return;
			}

			_UpdatePaxtonUser(paxtonUser, true);
		}

		private void _UpdatePaxtonUser(PaxtonUser paxtonUser, bool active)
		{
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
				active, //if we're updating a use we want to make sure it's also active instead of just copying paxtonUser.Active,
				paxtonUser.Fax,
				paxtonUser.ExpiryDate,
				paxtonUser.CustomFields);
		}

		public void CreateUser(int accessLevelId, string firstName, string surname, int becNumber)
		{
			if (_trialRunOnly)
			{
				Logger.WriteLine("{0} {1}\tTrial run - not adding user.", firstName, surname);
				return;
			}

			var CustomFields = new string[]
			{
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					becNumber.ToString()
			};

			const int departmentId = 0;

			_net2Client.AddUserRecord(
				accessLevelId,
				departmentId,
				firstName,
				surname,
				CustomFields);
		}

		public void DeleteUser(PaxtonUser paxtonUser)
		{
			if (_trialRunOnly)
			{
				Logger.WriteLine("{0}\tTrial run - not deleting user.", paxtonUser);
				return;
			}

			_UpdatePaxtonUser(paxtonUser, false);
		}
	}
}
