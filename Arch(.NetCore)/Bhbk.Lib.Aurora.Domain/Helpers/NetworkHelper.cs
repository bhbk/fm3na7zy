using Bhbk.Lib.Aurora.Data_EF6.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Bhbk.Lib.Aurora.Domain.Helpers
{
    public static class NetworkHelper
    {
        public static bool ValidateAddress(Network network, IPAddress client)
        {
            IPNetwork cidr;

            if (IPNetwork.TryParse(network.Address, out cidr))
                if (cidr.Contains(client))
                    return true;

            return false;
        }

        public static bool ValidateAddress(IEnumerable<Network> networks, IPAddress client)
        {
            var found = false;

            foreach (var network in networks)
            {
                IPNetwork cidr;

                if (IPNetwork.TryParse(network.Address, out cidr))
                    if (cidr.Contains(client))
                    {
                        found = true;
                        continue;
                    }
            }

            return found;
        }

        public static List<IPAddress> GetIPAddresses(string hostname)
        {
            var addresses = Dns.GetHostAddresses(hostname);

            var filteredAddresses = addresses
                .Where(x => x.AddressFamily == AddressFamily.InterNetwork
                    && !x.ToString().StartsWith("127."));

            return filteredAddresses.ToList();
        }
    }
}
