// adapted nancyfx source to enable user validation per instance.
// original: https://github.com/NancyFx/Nancy/blob/master/src/Nancy.Authentication.Basic/BasicHttpExtensions.cs

using Nancy;
using Nancy.Bootstrapper;
using System.Collections.Generic;

namespace OsmSharp.API.Authentication.Basic
{
    /// <summary>
    /// Some simple helpers give some nice authentication syntax in the modules.
    /// </summary>
    public static class BasicHttpExtensions
    {
        /// <summary>
        /// Module requires basic authentication
        /// </summary>
        /// <param name="module">Module to enable</param>
        /// <param name="configuration">Basic authentication configuration</param>
        public static void EnableBasicAuthentication(this INancyModule module, BasicAuthenticationConfiguration configuration)
        {
            BasicAuthentication.Enable(module, configuration);
        }

        /// <summary>
        /// Module requires basic authentication
        /// </summary>
        /// <param name="pipeline">Bootstrapper to enable</param>
        /// <param name="configuration">Basic authentication configuration</param>
        public static void EnableBasicAuthentication(this IPipelines pipeline, BasicAuthenticationConfiguration configuration)
        {
            BasicAuthentication.Enable(pipeline, configuration);
        }

        /// <summary>
        /// Module requires basic authentication
        /// </summary>
        /// <param name="pipeline"></param>
        public static void EnableBasicAuthentication(this IPipelines pipeline)
        {
            pipeline.EnableBasicAuthentication(
                new BasicAuthenticationConfiguration(
                    (username, password, context) =>
                    {
                        var instance = context.Request.Path.Substring(1, context.Request.Path.IndexOf('/', 1) - 1);

                        IApiInstance api;
                        if (!ApiBootstrapper.TryGetInstance(instance, out api))
                        { // api instance not found.
                            return null;
                        }

                        var result = api.ValidateUser(username, password);
                        if (result.IsError)
                        { // user not validated correctly.
                            return null;
                        }
                        if (result.Data > 0)
                        { // user validated an userid returned.
                            return new UserIdentity()
                            {
                                UserId = (int)result.Data,
                                UserName = username,
                                Claims = new List<string>()
                            };
                        }

                        // Not recognised => anonymous.
                        return null;
                    }, "OSM-API"));
        }
    }
}