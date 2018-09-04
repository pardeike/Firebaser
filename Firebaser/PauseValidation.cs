using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Firebaser
{
	internal class PauseValidation : IDisposable
	{
		private readonly RemoteCertificateValidationCallback validationCallback;

		public PauseValidation()
		{
			validationCallback = ServicePointManager.ServerCertificateValidationCallback;
			ServicePointManager.ServerCertificateValidationCallback = AllowAny;
		}

		private static bool AllowAny(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		public void Dispose()
		{
			ServicePointManager.ServerCertificateValidationCallback = validationCallback;
		}
	}
}