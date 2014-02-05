using System;
using System.Collections.Generic;
using System.Linq;
using Paxton.Net2.OemClientLibrary;
using PaxtonSync.Properties;

namespace PaxtonSync
{
	internal class PaxtonClient : IDisposable
	{
		private const int _remotePort = 8025;
		private const string _remoteHost = "localhost";

		private readonly OemClient _net2Client;

		public PaxtonClient()
		{
			_net2Client = _CreateClient();
			_Login();
		}

		private void _Login()
		{
			var users = _net2Client
				.GetListOfOperators()
				.UsersDictionary()
				.Where(pair => pair.Value == Settings.Default.PaxtonUser)
				.ToArray();

			if (!users.Any())
				throw new Exception("Paxton user not found.");

			var userId = users.Single().Key;

			var methodList = _net2Client.AuthenticateUser(userId, Settings.Default.PaxtonPass);
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

		public void Dispose()
		{
			_net2Client.Dispose();
		}

		public IUsers ViewUserRecords()
		{
			return _net2Client.ViewUserRecords();
		}

		public void UpdateUserRecord(int userId, int accessLevelId, int departmentId, bool antiPassbackInd, bool alarmUserInd, string firstName, string middleName, string surname, string telephoneNo, string telephoneExtension, string pinCode, string pictureFileName, DateTime activationDate, bool activeInd, string faxNo, DateTime expiryDate, string[] customFields)
		{
			var result = _net2Client.UpdateUserRecord(userId, accessLevelId, departmentId, antiPassbackInd, alarmUserInd, firstName, middleName, surname, telephoneNo, telephoneExtension, pinCode, pictureFileName, activationDate, activeInd, faxNo, expiryDate, customFields);

			if (!result)
				 throw new Exception(_net2Client.LastErrorMessage);
		}
	}
}