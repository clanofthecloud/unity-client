using System;
using System.Text;
using System.Collections.Generic;

namespace CotcSdk
{
	/**
	 * Root class of the SDK. It is stateless and allows to perform basic operations with the SDK, such as
	 * `Ping`.
	 * 
	 * The #Cloud object may typically be used to log in, which would return a #Gamer object, another very
	 * important object from the SDK.
	 * 
	 * Other objects such as a #CloudIndexing object can also be obtained synchronously in order to perform
	 * categorized operations. These objects may not be stateless though, so please refer to their
	 * respective documentation.
	 */
	public sealed partial class Cloud {

		/**
		 * Provides an API to manipulate game data, such as key/value or leaderboards.
		 * @return an object that allow to manipulate game specific data.
		 */
		public Game Game {
			get { return new Game(this); }
		}

		/**
		 * Allows to manipulate an index. Usage: `Cloud.Index("matches").IndexObject(...);`.
		 * @param indexName name of the index; scopes your searches.
		 * @param domain the domain to manipulate the index on.
		 */
		public CloudIndexing Index(string indexName, string domain = Common.PrivateDomain) {
			return new CloudIndexing(this, indexName, domain);
		}

		/**
		 * Executes a "ping" request to the server. Allows to know whether the server is currently working as expected.
		 * You should hardly ever need this.
		 * @return promise resolved when the request has finished.
		 */
		public IPromise<Done> Ping() {
			HttpRequest req = MakeUnauthenticatedHttpRequest("/v1/ping");
			return Common.RunInTask<Done>(req, (response, task) => {
				task.PostResult(new Done(true, response.BodyJson), response.BodyJson);
			});
		}

		/**
		 * This handler, when set, is called synchronously whenever an HTTP request fails with a recoverable
		 * error.
		 * Some errors won't call this handler and will fail directly, such as when providing invalid
		 * arguments. This handler rather covers network errors.
		 * You need to call one of the methods of the HttpRequestFailedArgs in order to tell what to do next:
		 * either retry or abort the request.
		 * Changing this value only affects the calls made later on, not the requests currently running. You
		 * should set it once at startup.
		 */
		public HttpRequestFailedHandler HttpRequestFailedHandler;

		#region Internal HTTP helpers
		internal HttpRequest MakeUnauthenticatedHttpRequest(string path) {
			HttpRequest result = new HttpRequest();
			if (path.StartsWith("/")) {
				result.Url = Server + path;
			}
			else {
				result.Url = path;
			}
			result.LoadBalancerCount = LoadBalancerCount;
			result.Headers["x-apikey"] = ApiKey;
			result.Headers["x-sdkversion"] = SdkVersion;
			result.Headers["x-apisecret"] = ApiSecret;
			result.FailedHandler = HttpRequestFailedHandler;
			result.TimeoutMillisec = HttpTimeoutMillis;
			result.UserAgent = UserAgent;
			return result;
		}
		#endregion

		#region Private
		internal Cloud(string apiKey, string apiSecret, string environment, int loadBalancerCount, bool httpVerbose, int httpTimeout) {
			this.ApiKey = apiKey;
			this.ApiSecret = apiSecret;
			this.Server = environment;
			LoadBalancerCount = loadBalancerCount;
			Managers.HttpClient.VerboseMode = httpVerbose;
			HttpTimeoutMillis = httpTimeout * 1000;
			UserAgent = String.Format(Common.UserAgent, Managers.SystemFunctions.GetOsName(), Common.SdkVersion);
		}
		#endregion

		#region Members
		private const string SdkVersion = "1";
		private string ApiKey, ApiSecret, Server;
		internal int HttpTimeoutMillis {
			get; private set;
		}
		internal int LoadBalancerCount {
			get; private set;
		}
		internal string UserAgent {
			get; private set;
		}
		#endregion
	}
}
