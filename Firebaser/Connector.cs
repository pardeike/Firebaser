﻿using fastJSON;
using System.Collections.Specialized;
using System.Net;
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
		private readonly string firebaseUrl;
		private readonly string secret;

		public Connector(string project, string secret)
		{
			firebaseUrl = "https://" + project + ".firebaseio.com";
			this.secret = secret;
		}

		public TResult Send<TObject, TResult>(Method method, string objectPath, TObject obj = default(TObject), bool shallow = false, NameValueCollection queryParams = null) where TResult : class
		{
			using (new PauseValidation())
			{
				var query = new NameValueCollection { { "auth", secret }, { "shallow", shallow ? "true" : "false" } };
				if (queryParams != null) query.Add(queryParams);
				var client = new WebClient { QueryString = query, Encoding = Encoding.UTF8 };
				var json = JSON.ToJSON(obj, new JSONParameters() { UseExtensions = false });
				if (objectPath.Length > 0 && !objectPath.StartsWith("/")) objectPath = "/" + objectPath;
				var result = method == Method.GET ?
					client.DownloadString(firebaseUrl + objectPath + ".json") :
					client.UploadString(firebaseUrl + objectPath + ".json", method.ToString(), json);
				if (result == null) return default(TResult);
				return typeof(TResult) == typeof(string) ? result as TResult : JSON.ToObject<TResult>(result);
			}
		}

		public TResult Get<TResult>(string objectPath, bool shallow = false, NameValueCollection queryParams = null) where TResult : class
		{
			return Send<object, TResult>(Method.GET, objectPath, null, shallow, queryParams);
		}

		public string Post<TObject>(string objectPath, TObject obj, NameValueCollection queryParams = null) where TObject : class
		{
			return Send<TObject, string>(Method.POST, objectPath, obj, false, queryParams);
		}

		public string Put<TObject>(string objectPath, TObject obj, NameValueCollection queryParams = null) where TObject : class
		{
			return Send<TObject, string>(Method.PUT, objectPath, obj, false, queryParams);
		}

		public string Patch<TObject>(string objectPath, TObject obj, NameValueCollection queryParams = null) where TObject : class
		{
			return Send<TObject, string>(Method.PATCH, objectPath, obj, false, queryParams);
		}

		public string Delete(string objectPath, NameValueCollection queryParams = null)
		{
			return Send<object, string>(Method.DELETE, objectPath, null, false, queryParams);
		}
	}
}