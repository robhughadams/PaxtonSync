using Paxton.Net2.OemClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaxtonSync
{
    class PaxtonUser
    {
        private IUserView _wrappedUser;
        private int? _becNumber;

        public PaxtonUser(IUserView user)
        {
            _wrappedUser = user;
        }

        public int? BecNumber
        {
            get
            {
                if (_becNumber.HasValue)
                    return _becNumber;

                int result;
                if (Int32.TryParse(_wrappedUser.Field14_50, out result))
                    return result;

                return null;
            }
            set
            {
                _becNumber = value;
            }
        }

        public string Surname
        {
            get { return _wrappedUser.Surname; }
        }

        public string FirstName
        {
            get { return _wrappedUser.FirstName; }
        }

        public string[] CustomFields
        {
            get
            {
                return new[] 
                {
                    null, //Sample progam pads the first item for no discernable reason
                    _wrappedUser.Field1_100,
                    _wrappedUser.Field2_100,
                    _wrappedUser.Field3_50,
                    _wrappedUser.Field4_50,
                    _wrappedUser.Field5_50,
                    _wrappedUser.Field6_50,
                    _wrappedUser.Field7_50,
                    _wrappedUser.Field8_50,
                    _wrappedUser.Field9_50,
                    _wrappedUser.Field10_50,
                    _wrappedUser.Field11_50,
                    _wrappedUser.Field12_50,
                    _wrappedUser.Field13_Memo,
                    BecNumber.ToString()
                };
            }
        }

        public int UserId { get { return _wrappedUser.UserId; } }

        public int AccessLevelId { get { return _wrappedUser.AccessLevelId; } }

        public int DepartmentId { get { return _wrappedUser.DepartmentId; } }

        public bool AntiPassbackUser { get { return _wrappedUser.AntiPassbackUser; } }

        public bool AlarmUser { get { return _wrappedUser.AlarmUser; } }

        public string MiddleName { get { return _wrappedUser.MiddleName; } }

        public string Telephone { get { return _wrappedUser.Telephone; } }

        public string Extension { get { return _wrappedUser.Extension; } }

        public string PIN { get { return _wrappedUser.PIN; } }

        public string Picture { get { return _wrappedUser.Picture; } }

        public DateTime ActivationDate { get { return _wrappedUser.ActivationDate; } }

        public bool Active { get { return _wrappedUser.Active; } }

        public string Fax { get { return _wrappedUser.Fax; } }

        public DateTime ExpiryDate { get { return _wrappedUser.ExpiryDate; } }
    }
}
