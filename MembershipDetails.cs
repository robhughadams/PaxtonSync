using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaxtonSync
{
    class MembershipDetails
    {
        //{\"becNumber\":\"1416\",\"firstName\":\"Robert\",\"lastName\":\"Adams\",\"membershipStatus\":\"Current\"}
        
        public int BecNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public MembershipStatus MembershipStatus { get; set; }
    }

    enum MembershipStatus
    {
        Current,
        LYBNTY,
        Expired,
        Cancelled,
        Deceased
    }
}
