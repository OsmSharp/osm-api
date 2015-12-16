using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Osm.API.Authentication
{
    /// <summary>
    /// Manages all security and authentication for all modules.
    /// </summary>
    public static class AuthenticationHandler
    {
        /// <summary>
        /// Claim that stamps a user as an admin.
        /// </summary>
        public static string ADMIN_CLAIM = "is_admin";

        /// <summary>
        /// Holds the authentication disabled flag.
        /// </summary>
        private static bool? _authenticationDisabled;

        /// <summary>
        /// Returns true if authentication is disabled.
        /// </summary>
        public static bool AuthenticationDisabled
        {
            get
            {
                if (_authenticationDisabled == null)
                {
                    _authenticationDisabled = false;
                }
                return _authenticationDisabled.Value;
            }
        }

        /// <summary>
        /// Sets up the given module to require authentication.
        /// </summary>
        /// <param name="module"></param>
        public static void RequiresAuthentication(this NancyModule module)
        {
            if (!AuthenticationHandler.AuthenticationDisabled)
            {
                ModuleSecurity.RequiresAuthentication(module);
            }
        }

        /// <summary>
        /// Sets up the given module to require any of the give claims.
        /// </summary>
        public static void RequiresAnyClaim(this NancyModule module, IEnumerable<string> claims)
        {
            if (!AuthenticationHandler.AuthenticationDisabled)
            {
                ModuleSecurity.RequiresAnyClaim(module, claims);
            }
        }

        /// <summary>
        /// Returns true if the given user has the given claim.
        /// </summary>
        public static bool HasClaim(this IUserIdentity user, string claim)
        {
            if (!AuthenticationHandler.AuthenticationDisabled)
            {
                return UserIdentityExtensions.HasClaim(user, claim);
            }
            return true; // all users have all claims when authentication disabled.
        }

        /// <summary>
        /// Returns true if the given user has any of the given claims.
        /// </summary>
        public static bool HasAnyClaim(this IUserIdentity user, IEnumerable<string> claims)
        {
            if (!AuthenticationHandler.AuthenticationDisabled)
            {
                return UserIdentityExtensions.HasAnyClaim(user, claims);
            }
            return true; // all users have all claims when authentication disabled.
        }

        /// <summary>
        /// Sets up the requirement to access user management.
        /// </summary>
        public static void RequiresAdminAccess(this NancyModule module)
        {
            module.RequiresAnyClaim(new[] { ADMIN_CLAIM });
        }
    }
}