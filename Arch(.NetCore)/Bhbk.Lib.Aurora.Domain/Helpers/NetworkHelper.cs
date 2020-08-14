using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using System.Collections.Generic;
using System.Net;

namespace Bhbk.Lib.Aurora.Domain.Helpers
{
    public static class NetworkHelper
    {
        public static bool ValidateAddress(IEnumerable<tbl_Networks> networks, IPAddress client)
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
    }
}
