using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace ServerUI
{
    class Networking
    {
       static public async Task<bool> sendIP(string IP)
        {
            string address = "http://r96368pp.bget.ru//qd_IP.php/";
            WebRequest request = WebRequest.Create(address + "?ip=" + IP);
            WebResponse response = await request.GetResponseAsync();
            StreamReader stream = new StreamReader(response.GetResponseStream());
            string reply = stream.ReadToEnd();
            if (reply.ToString() == "ok")
            {
                return true;
            }
            else return false; ;
        }
        static public async Task<string> getPublicIP()
        {
            string direction;
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            WebResponse response = await request.GetResponseAsync();
            StreamReader stream = new StreamReader(response.GetResponseStream());
            direction = stream.ReadToEnd();

            //Search for the ip in the html
            int first = direction.IndexOf("Address: ") + 9;
            int last = direction.LastIndexOf("");
            direction = direction.Substring(first, last - first);
            return direction;
        }
        static public string GetLocalIp()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter == null) return null;
            var hostname =
                NetworkInformation.GetHostNames()
                    .SingleOrDefault(
                        hn =>
                            hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId);

            // the ip address
            return hostname?.CanonicalName;
        }
    }
}
