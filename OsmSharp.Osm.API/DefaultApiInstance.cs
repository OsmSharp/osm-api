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

using OsmSharp.Math.Geo;
using OsmSharp.Osm.API.Db;
using OsmSharp.Osm.Xml.v0_6;
using System;
using System.Collections.Generic;

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
                version = 0.6,
                versionSpecified = true,
                api = new api()
                {
                    version = new version()
                    {
                        minimum = 0.6,
                        minimumSpecified = true,
                        maximum = 0.6,
                        maximumSpecified = true
                    },
                    area = new area()
                    {
                        maximum = 0.25,
                        maximumSpecified = true
                    },
                    tracepoints = new tracepoints()
                    {
                        per_page = 5000,
                        per_pageSpecified = true
                    },
                    waynodes = new waynodes()
                    {
                        maximum = 2000,
                        maximumSpecified = true
                    },
                    changesets = new changesets()
                    {
                        maximum_elements = 50000,
                        maximum_elementsSpecified = true
                    },
                    timeout = new timeout()
                    {
                        seconds = 300,
                        secondsSpecified = true
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

        /// <summary>
        /// Gets all objects inside the given bounds.
        /// </summary>
        /// <returns></returns>
        public osm GetMap(double left, double bottom, double right, double top)
        {
            var found = _db.GetInsideBox(new GeoCoordinateBox(
                new GeoCoordinate(bottom, left),
                new GeoCoordinate(top, right)));

            var nodes = new List<node>();
            var ways = new List<way>();
            var relations = new List<relation>();

            foreach(var foundOsmGeo in found)
            {
                switch(foundOsmGeo.Type)
                { 
                    case OsmGeoType.Node:
                        nodes.Add((foundOsmGeo as Node).ConvertTo());
                        break;
                    case OsmGeoType.Way:
                        ways.Add((foundOsmGeo as Way).ConvertTo());
                        break;
                    case OsmGeoType.Relation:
                        relations.Add((foundOsmGeo as Relation).ConvertTo());
                        break;
                }
            }

            return new osm()
            {
                version = 0.6,
                versionSpecified = true,
                node = nodes.ToArray(),
                way = ways.ToArray(),
                relation = relations.ToArray()
            };
        }

        /// <summary>
        /// Gets the node with the given id.
        /// </summary>
        /// <returns></returns>
        public osm GetNode(long id)
        {
            var node = _db.GetNode(id);
            if(node == null)
            {
                return null;
            }
            return new osm()
            {
                version = 0.6,
                versionSpecified = true,
                node = new Xml.v0_6.node[] { node.ConvertTo() }
            };
        }

        /// <summary>
        /// Gets the way with the given id.
        /// </summary>
        /// <returns></returns>
        public osm GetWay(long id)
        {
            var way = _db.GetWay(id);
            if (way == null)
            {
                return null;
            }
            return new osm()
            {
                version = 0.6,
                versionSpecified = true,
                way = new Xml.v0_6.way[] { way.ConvertTo() }
            };
        }

        /// <summary>
        /// Gets the relation with the given id.
        /// </summary>
        /// <returns></returns>
        public osm GetRelation(long id)
        {
            var relation = _db.GetRelation(id);
            if (relation == null)
            {
                return null;
            }
            return new osm()
            {
                version = 0.6,
                versionSpecified = true,
                relation = new Xml.v0_6.relation[] { relation.ConvertTo() }
            };
        }

        /// <summary>
        /// Gets the user with the given id.
        /// </summary>
        public osm GetUser(long id)
        {
            var user = _db.GetUser(id);
            if (user == null)
            {
                return null;
            }
            return new osm()
            {
                version = 0.6,
                versionSpecified = true,
                user = user.ToXmlUser()
            };
        }
    }
}
