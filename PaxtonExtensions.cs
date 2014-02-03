using Paxton.Net2.OemClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaxtonSync
{
    static class PaxtonExtensions
    {
        public static int? BecNumber(this IUserView paxtonUser)
        {
            int result;
            if (Int32.TryParse(paxtonUser.Field14_50, out result))
                return result;

            return null;
        }
    }
}
