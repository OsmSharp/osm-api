// The MIT License (MIT)

// Copyright (c) 2015 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;

namespace OsmSharp.Osm.API
{
    /// <summary>
    /// The API boostrapper.
    /// </summary>
    public static class ApiBootstrapper
    {
        private static Dictionary<string, IApiInstance> _instances = 
            new Dictionary<string,IApiInstance>();

        /// <summary>
        /// Sets the api instances.
        /// </summary>
        public static void SetInstance(string name, IApiInstance instance)
        {
            _instances[name] = instance;
        }

        /// <summary>
        /// Returns true when there is an active instance.
        /// </summary>
        public static bool IsActive(string name)
        {
            return _instances.ContainsKey(name);
        }

        /// <summary>
        /// Gets the api instance.
        /// </summary>
        public static bool TryGetInstance(string name, out IApiInstance instance)
        {
            return _instances.TryGetValue(name, out instance);
        }
    }
}
