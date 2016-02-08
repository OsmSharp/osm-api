// adapted nancyfx source to enable user validation per instance.
// original: https://github.com/NancyFx/Nancy/blob/master/src/Nancy.Authentication.Basic/UserPromptBehaviour.cs

namespace OsmSharp.API.Authentication.Basic
{
    /// <summary>
    /// Options to control when the browser prompts the user for credentials
    /// </summary>
    public enum UserPromptBehaviour
    {
        /// <summary>
        /// Never present user with login prompt
        /// </summary>
        Never,

        /// <summary>
        /// Always present user with login prompt
        /// </summary>
        Always,

        /// <summary>
        /// Only prompt the user for credentials on non-ajax requests
        /// </summary>
        NonAjax
    }
}
