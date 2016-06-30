using System;
using OsmSharp.API.Authentication.Basic;

namespace OsmSharp.API
{
	/// <summary>
	/// Contains hooks to override/setup default authentication.
	/// </summary>
	public static class AuthenticationHooks
	{
		/// <summary>
		/// The authentication hook.
		/// </summary>
		public static Action<ApiModule> AuthenticationHook = (module) =>
		{
			module.EnableBasicAuthentication();
		};

		/// <summary>
		/// Does the authentication setup according to the hooks setup.
		/// </summary>
		/// <returns>The authentication.</returns>
		/// <param name="module">Module.</param>
		public static void SetupAuthentication(this ApiModule module)
		{
			if (AuthenticationHooks.AuthenticationHook == null)
			{
				OsmSharp.Logging.Logger.Log("AuthenticationHooks.SetupAuthentication", Logging.TraceEventType.Warning,
											"No authentication hook set, no authentication was setup.");
			}
			else
			{
				AuthenticationHooks.AuthenticationHook(module);
			}
		}
	}
}

