using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetherWindows
{
    public static class Script
    {
        private const string SetIpAddress =
            "netsh interface ip set address name={0} source=static 10.0.0.1 255.255.255.0 10.0.0.2 1";

        private const string SetDnsServer1 =
            "netsh interface ip add dns name={0} 8.8.8.8 index=1";

        private const string SetDnsServer2 =
            "netsh interface ip add dns name={0} 8.8.4.4 index=2";

        private const string AddFirewallException =
            "netsh firewall set allowedprogram program=\"{0}\" name=Tether";


        public static async Task Run(IProgress<string> progress)
        {

        }
    }
}
