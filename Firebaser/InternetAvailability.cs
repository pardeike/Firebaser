using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Firebaser
{
	public static class InternetAvailability
	{
		const long refreshCheckInterval = 2000; //ms
		const string hostName = "google.com";
		const int maxPingTimeout = 300; //ms

		static long nextNetworkCheck = 0;
		static bool cachedNetworkAvailability = false;
		static IPAddress networkCheckIP = null;

		public static bool IsAvailable(bool forceCheck = false)
		{
			var now = DateTime.Now.Ticks;
			var firstTime = nextNetworkCheck == 0;
			if (now > nextNetworkCheck || forceCheck)
			{
				nextNetworkCheck = now + refreshCheckInterval;
				if (NetworkInterface.GetIsNetworkAvailable())
				{
					try
					{
						Ping(firstTime || forceCheck);
					}
					catch (Exception)
					{
						cachedNetworkAvailability = false;
					}
				}
			}
			return cachedNetworkAvailability;
		}

		private static void Ping(bool synchronous)
		{
			if (networkCheckIP == null)
			{
				var hostEntry = Dns.GetHostEntry(hostName);
				if (hostEntry.AddressList.Length > 0)
					networkCheckIP = hostEntry.AddressList[0];
			}
			if (networkCheckIP != null)
			{
				var ping = new Ping();
				if (synchronous)
				{
					var result = ping.Send(networkCheckIP, 2 * maxPingTimeout);
					cachedNetworkAvailability = result.Status == IPStatus.Success;
				}
				else
				{
					ping.PingCompleted += (obj, sender) =>
					{
						cachedNetworkAvailability = (sender.Reply.Status == IPStatus.Success);
					};
					var buffer = new byte[32];
					ping.SendAsync(networkCheckIP, maxPingTimeout);
				}
			}
			else
				cachedNetworkAvailability = false;
		}
	}
}