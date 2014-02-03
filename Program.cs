using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
using PaxtonSync.Properties;

namespace PaxtonSync
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Fetching membership details from www.bec-cave.org.uk");
            var membershipDetails = _DownloadMembershipDetails();

            foreach (var details in membershipDetails)
               Console.WriteLine("{0} {1} {2}", details.BecNumber, details.FirstName, details.LastName);



            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static IReadOnlyCollection<MembershipDetails> _DownloadMembershipDetails()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(String.Format("{0}:{1}", Settings.Default.UserName, Settings.Default.Password))));

                var responseStream = client.GetStreamAsync(Settings.Default.DownloadUrl).Result;
                using (var reader = new JsonTextReader(new StreamReader(responseStream)))
                {
                    var jsonReader = new JsonSerializer();
                    return jsonReader.Deserialize<IReadOnlyCollection<MembershipDetails>>(reader);
                }
            }
        }
    }
}
