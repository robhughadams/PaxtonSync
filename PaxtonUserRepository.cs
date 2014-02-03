using Paxton.Net2.OemClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaxtonSync
{
    class PaxtonUserRepository
    {
        OemClient _net2Client;

        public PaxtonUserRepository(OemClient net2Client)
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
            var result = _net2Client.UpdateUserRecord(
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

            if (!result)
                throw new Exception(_net2Client.LastErrorMessage);
        }
    }
}
