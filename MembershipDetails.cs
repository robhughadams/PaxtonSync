﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace PaxtonSync
{
	internal class MembershipDetails
	{
		//{\"becNumber\":\"1416\",\"firstName\":\"Robert\",\"lastName\":\"Adams\",\"membershipStatus\":\"Current\"}

		public int BecNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public MembershipStatus MembershipStatus { get; set; }
	}
}
