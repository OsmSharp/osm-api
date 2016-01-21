// adapted nancyfx source to enable user validation per instance.
// original: https://github.com/NancyFx/Nancy/blob/master/src/Nancy.Authentication.Basic/BasicAuthenticationConfiguration.cs

using Nancy;
using Nancy.Security;
using System;

namespace OsmSharp.Osm.API.Authentication.Basic
{
    /// <summary>
    /// Configuration options for forms authentication
    /// </summary>
    public class BasicAuthenticationConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicAuthenticationConfiguration"/> class.
        /// </summary>
        public BasicAuthenticationConfiguration(Func<string, string, NancyContext, IUserIdentity> userValidator, string realm, UserPromptBehaviour userPromptBehaviour = UserPromptBehaviour.NonAjax)
        {
            if (userValidator == null)
            {
                throw new ArgumentNullException("userValidator");
            }

            if (string.IsNullOrEmpty(realm))
            {
                throw new ArgumentException("realm");
            }

            this.UserValidator = userValidator;
            this.Realm = realm;
            this.UserPromptBehaviour = userPromptBehaviour;
        }

        /// <summary>
        /// Gets the user validator
        /// </summary>
        public Func<string, string, NancyContext, IUserIdentity> UserValidator { get; private set; }

        /// <summary>
        /// Gets the basic authentication realm
        /// </summary>
        public string Realm { get; private set; }

        /// <summary>
        /// Determines when the browser should prompt the user for credentials
        /// </summary>
        public UserPromptBehaviour UserPromptBehaviour { get; private set; }
    }
}
