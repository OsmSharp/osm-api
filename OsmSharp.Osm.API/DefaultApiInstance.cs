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

using OsmSharp.Osm.API.Db;
using OsmSharp.Osm.API.Domain;
using System;

namespace OsmSharp.Osm.API
{
    /// <summary>
    /// A default api instance.
    /// </summary>
    public class DefaultApiInstance : IApiInstance
    {
        private readonly IDb _db;

        /// <summary>
        /// Creates a new api instance.
        /// </summary>
        public DefaultApiInstance(IDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Gets the capabilities.
        /// </summary>
        /// <returns></returns>
        public osm GetCapabilities()
        {
            return new osm()
            {
                api = new api()
                {
                    version = new version()
                    {
                        minimum = 0.6m,
                        maximum = 0.6m
                    },
                    area = new area()
                    {
                        maximum = 0.25m
                    },
                    tracepoints = new tracepoints()
                    {
                        per_page = 5000
                    },
                    waynodes = new waynodes()
                    {
                        maximum = 2000
                    },
                    changesets = new changesets()
                    {
                        maximum_elements = 50000
                    },
                    timeout = new timeout()
                    {
                        seconds = 300
                    },
                    status = new status()
                    {
                        api = "online",
                        database = "online",
                        gpx = "online"
                    }
                }
            };
        }

        public Domain.osm GetNode(long id)
        {
            throw new NotImplementedException();
        }

        public Domain.osm GetWay(long id)
        {
            throw new NotImplementedException();
        }

        public Domain.osm GetRelation(long id)
        {
            throw new NotImplementedException();
        }
    }
}
