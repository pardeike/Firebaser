using fastJSON;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Firebaser
{
	// Firebaser uses the Firebase Realtime Database
	// https://firebase.google.com/docs/database/
	// and in particular the REST API defined in
	// https://firebase.google.com/docs/database/rest/start

	// Keys (and therefore object fields) must follow the following rule:
	//
	// If you create your own keys, they must be UTF-8 encoded, can be a maximum of 768 bytes,
	// and cannot contain., $, #, [, ], /, or ASCII control characters 0-31 or 127.

	public enum Method
	{
		GET,
		POST,
		PUT,
		PATCH,
		DELETE
	}

	public class Connector
	{
		private readonly string hostName;
		private readonly string secret;

		const long refreshCheckInterval = 2; //sec

		static DateTime nextNetworkCheck = DateTime.Now;
		static bool cachedNetworkAvailability = false;

		public Connector(string project, string secret)
		{
			hostName = project + ".firebaseio.com";
			this.secret = secret;
		}

		public bool IsAvailable(bool forceCheck = false)
		{
			var now = DateTime.Now;
			if (now > nextNetworkCheck || forceCheck)
			{
				nextNetworkCheck = now.AddSeconds(refreshCheckInterval);
				try
				{
					var hostEntry = Dns.GetHostEntry(hostName);
					if (hostEntry.AddressList.Length == 0)
					{
						cachedNetworkAvailability = false;
						return cachedNetworkAvailability;
					}
					var remoteAddress = hostEntry.AddressList[0];
					var endpoint = new IPEndPoint(remoteAddress, 80);
					var socket = new Socket(remoteAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
					socket.Connect(endpoint);
					socket.Shutdown(SocketShutdown.Both);
					socket.Close();
					cachedNetworkAvailability = true;
				}
				catch (Exception)
				{
					cachedNetworkAvailability = false;
				}
			}
			return cachedNetworkAvailability;
		}

		public TResult Send<TObject, TResult>(Method method, string objectPath, TObject obj = default(TObject), bool shallow = false, NameValueCollection queryParams = null)
		{
			if (!IsAvailable())
				return default(TResult);

			using (new PauseValidation())
			{
				var query = new NameValueCollection { { "auth", secret }, { "shallow", shallow ? "true" : "false" } };
				if (queryParams != null) query.Add(queryParams);
				var client = new WebClient { QueryString = query, Encoding = Encoding.UTF8 };
				var json = JSON.ToJSON(obj, new JSONParameters() { UseExtensions = false });
				if (objectPath.Length > 0 && !objectPath.StartsWith("/")) objectPath = "/" + objectPath;
				string result = null;
				var path = "https://" + hostName + objectPath + ".json";
				switch (method)
				{
					case Method.GET:
						result = client.DownloadString(path);
						break;
					case Method.POST:
					case Method.PUT:
					case Method.PATCH:
						result = client.UploadString(path, method.ToString(), json);
						break;
					case Method.DELETE:
						client.Headers.Add("X-HTTP-Method-Override", Method.DELETE.ToString());
						result = client.DownloadString(path);
						break;
				}
				if (result == null) return default(TResult);
				return typeof(TResult) == typeof(string) ? (TResult)(result as object) : JSON.ToObject<TResult>(result);
			}
		}

		public TResult Get<TResult>(string objectPath, bool shallow = false, NameValueCollection queryParams = null)
		{
			return Send<object, TResult>(Method.GET, objectPath, null, shallow, queryParams);
		}

		public string Post<TObject>(string objectPath, TObject obj, NameValueCollection queryParams = null)
		{
			return Send<TObject, string>(Method.POST, objectPath, obj, false, queryParams);
		}

		public string Put<TObject>(string objectPath, TObject obj, NameValueCollection queryParams = null)
		{
			return Send<TObject, string>(Method.PUT, objectPath, obj, false, queryParams);
		}

		public string Patch<TObject>(string objectPath, TObject obj, NameValueCollection queryParams = null)
		{
			return Send<TObject, string>(Method.PATCH, objectPath, obj, false, queryParams);
		}

		public string Delete(string objectPath, NameValueCollection queryParams = null)
		{
			return Send<object, string>(Method.DELETE, objectPath, null, false, queryParams);
		}
	}
}