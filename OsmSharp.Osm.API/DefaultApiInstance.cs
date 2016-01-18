// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

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

        private long _nextChangesetId = 1;
        private Dictionary<long, changeset> _openChangesets = new Dictionary<long, changeset>();

        /// <summary>
        /// Gets the capabilities.
        /// </summary>
        /// <returns></returns>
        public ApiResult<osm> GetCapabilities()
        {
            return new ApiResult<osm>(new osm()
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
            });
        }

        /// <summary>
        /// Gets all objects inside the given bounds.
        /// </summary>
        /// <returns></returns>
        public ApiResult<osm> GetMap(double left, double bottom, double right, double top)
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

            return new ApiResult<osm>(new osm()
            {
                version = 0.6,
                versionSpecified = true,
                node = nodes.ToArray(),
                way = ways.ToArray(),
                relation = relations.ToArray()
            });
        }

        /// <summary>
        /// Gets the node with the given id.
        /// </summary>
        /// <returns></returns>
        public ApiResult<osm> GetNode(long id)
        {
            var node = _db.GetNode(id);
            if(node == null)
            {
                return new ApiResult<osm>(ApiResultStatusCode.NotFound, "Node not found.");
            }
            return new ApiResult<osm>(new osm()
            {
                version = 0.6,
                versionSpecified = true,
                node = new Xml.v0_6.node[] { node.ConvertTo() }
            });
        }

        /// <summary>
        /// Gets the way with the given id.
        /// </summary>
        /// <returns></returns>
        public ApiResult<osm> GetWay(long id)
        {
            var way = _db.GetWay(id);
            if (way == null)
            {
                return new ApiResult<osm>(ApiResultStatusCode.NotFound, "Way not found.");
            }
            return new ApiResult<osm>(new osm()
            {
                version = 0.6,
                versionSpecified = true,
                way = new Xml.v0_6.way[] { way.ConvertTo() }
            });
        }

        /// <summary>
        /// Gets the relation with the given id.
        /// </summary>
        /// <returns></returns>
        public ApiResult<osm> GetRelation(long id)
        {
            var relation = _db.GetRelation(id);
            if (relation == null)
            {
                return new ApiResult<osm>(ApiResultStatusCode.NotFound, "Relation not found.");
            }
            return new ApiResult<osm>(new osm()
            {
                version = 0.6,
                versionSpecified = true,
                relation = new Xml.v0_6.relation[] { relation.ConvertTo() }
            });
        }

        /// <summary>
        /// Gets the user with the given id.
        /// </summary>
        public ApiResult<osm> GetUser(long id)
        {
            var user = _db.GetUser(id);
            if (user == null)
            {
                return null;
            }
            return new ApiResult<osm>(new osm()
            {
                version = 0.6,
                versionSpecified = true,
                user = user.ToXmlUser()
            });
        }

        /// <summary>
        /// Creates a new changeset.
        /// </summary>
        public ApiResult<long> CreateChangeset(changeset changeset)
        {
            lock(_openChangesets)
            {
                var id = _nextChangesetId;
                _nextChangesetId++;

                _openChangesets.Add(id, changeset);
                return new ApiResult<long>(id);
            }
        }

        /// <summary>
        /// Applies the given changeset.
        /// </summary>
        public ApiResult<diffResult> ApplyChangeset(osmChange osmChange)
        {
            // validate changeset.
            var validated = this.ValidateChangeset(osmChange);
            if(validated.IsError)
            {
                return validated.Convert<diffResult>();
            }
            if(!validated.Data)
            {
                return new ApiResult<diffResult>(ApiResultStatusCode.Exception,
                    "Changeset could not be applied, validation failed.");
            }

            if (osmChange.create != null)
            {
                var nodeIds = new Dictionary<long, long>();
                var wayIds = new Dictionary<long, long>();
                var relationIds = new Dictionary<long, long>();
                for (var i = 0; i < osmChange.create.Length; i++)
                {
                    var create = osmChange.create[i];
                    if (create.node != null)
                    {
                        for (var n = 0; n < create.node.Length; n++)
                        {
                            var newNode = _db.AddNewNode(create.node[n].ConvertFrom());
                            nodeIds.Add(create.node[n].id, newNode.Id.Value);
                        }
                    }
                    if (create.way != null)
                    {
                        for (var w = 0; w < create.way.Length; w++)
                        {
                            for(var n = 0; n < create.way[w].nd.Length; n++)
                            {
                                long newId;
                                if(nodeIds.TryGetValue(create.way[w].nd[n].@ref, out newId))
                                {
                                    create.way[w].nd[n].@ref = newId;
                                }
                            }
                            var newWay = _db.AddNewWay(create.way[w].ConvertFrom());
                            wayIds.Add(create.way[w].id, newWay.Id.Value);
                        }
                    }
                    if(create.relation != null)
                    {
                        for(var r = 0; r < create.relation.Length; r++)
                        {
                            var relation = create.relation[r];
                            for(var m = 0; m < relation.member.Length; m++)
                            {
                                var member = relation.member[m];
                                long newNodeId;
                                switch (member.type)
                                {
                                    case memberType.node:
                                        nodeIds.TryGetValue(member.@ref, out newNodeId);
                                        member.@ref = newNodeId;
                                        break;
                                    case memberType.way:
                                        wayIds.TryGetValue(member.@ref, out newNodeId);
                                        member.@ref = newNodeId;
                                        break;
                                    case memberType.relation:
                                        relationIds.TryGetValue(member.@ref, out newNodeId);
                                        member.@ref = newNodeId;
                                        break;
                                }
                            }

                            var newRelation = _db.AddNewRelation(relation.ConvertFrom());
                            relationIds.Add(relation.id, newRelation.Id.Value);
                        }
                    }
                }
            }

            if(osmChange.modify != null)
            {
                for(var i = 0; i < osmChange.modify.Length; i++)
                {
                    var modify = osmChange.modify[i];
                    for(var n = 0; n < modify.node.Length; n++)
                    {
                        _db.UpdateNode(modify.node[n].ConvertFrom());
                    }
                    for (var w = 0; w < modify.way.Length; w++)
                    {
                        _db.UpdateWay(modify.way[w].ConvertFrom());
                    }
                    for (var r = 0; r < modify.relation.Length; r++)
                    {
                        _db.UpdateRelation(modify.relation[r].ConvertFrom());
                    }
                }
            }

            if (osmChange.delete != null)
            {
                for (var i = 0; i < osmChange.delete.Length; i++)
                {
                    var delete = osmChange.delete[i];
                    if (delete.node != null)
                    {
                        for (var j = 0; j < delete.node.Length; j++)
                        {
                            _db.DeleteNode(delete.node[j].id);
                        }
                    }
                    if (delete.way != null)
                    {
                        for (var j = 0; j < delete.way.Length; j++)
                        {
                            _db.DeleteWay(delete.way[j].id);
                        }
                    }
                    if (delete.relation != null)
                    {
                        for (var j = 0; j < delete.relation.Length; j++)
                        {
                            _db.DeleteRelation(delete.relation[j].id);
                        }
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// Validates changeset agains the current state of the data.
        /// </summary>
        public ApiResult<bool> ValidateChangeset(osmChange osmChange)
        {
            if (osmChange.create != null)
            {
                foreach (var creation in osmChange.create)
                {
                    if (creation.node != null)
                    {
                        for (var i = 0; i < creation.node.Length; i++)
                        {
                            if (creation.node[i].version > 1)
                            {
                                return new ApiResult<bool>(false);
                            }
                        }
                    }

                    if (creation.way != null)
                    {
                        for (var i = 0; i < creation.way.Length; i++)
                        {
                            if (creation.way[i].version > 1)
                            {
                                return new ApiResult<bool>(false);
                            }
                        }
                    }

                    if (creation.relation != null)
                    {
                        for (var i = 0; i < creation.relation.Length; i++)
                        {
                            if (creation.relation[i].version > 1)
                            {
                                return new ApiResult<bool>(false);
                            }
                        }
                    }
                }
            }
            if (osmChange.modify != null)
            {
                foreach (var modification in osmChange.modify)
                {
                    if(modification.node != null)
                    {
                        for(var i = 0; i < modification.node.Length; i++)
                        {
                            var node = _db.GetNode(modification.node[i].id);
                            if (node == null)
                            {
                                return new ApiResult<bool>(false);
                            }
                            if (node.Version != modification.node[i].version)
                            {
                                return new ApiResult<bool>(false);
                            }
                        }
                    }

                    if (modification.way != null)
                    {
                        for (var i = 0; i < modification.way.Length; i++)
                        {
                            var way = _db.GetNode(modification.way[i].id);
                            if (way == null)
                            {
                                return new ApiResult<bool>(false);
                            }
                            if (way.Version != modification.way[i].version)
                            {
                                return new ApiResult<bool>(false);
                            }
                        }
                    }

                    if (modification.relation != null)
                    {
                        for (var i = 0; i < modification.relation.Length; i++)
                        {
                            var relation = _db.GetNode(modification.relation[i].id);
                            if (relation == null)
                            {
                                return new ApiResult<bool>(false);
                            }
                            if (relation.Version != modification.relation[i].version)
                            {
                                return new ApiResult<bool>(false);
                            }
                        }
                    }
                }
            }
            if (osmChange.delete != null)
            {
                foreach (var modification in osmChange.delete)
                {
                    if (modification.node != null)
                    {
                        for (var i = 0; i < modification.node.Length; i++)
                        {
                            var node = _db.GetNode(modification.node[i].id);
                            if (node.Version != modification.node[i].version)
                            {
                                return new ApiResult<bool>(false);
                            }
                        }
                    }

                    if (modification.way != null)
                    {
                        for (var i = 0; i < modification.way.Length; i++)
                        {
                            var way = _db.GetWay(modification.way[i].id);
                            if (way.Version != modification.way[i].version)
                            {
                                return new ApiResult<bool>(false);
                            }
                        }
                    }

                    if (modification.relation != null)
                    {
                        for (var i = 0; i < modification.relation.Length; i++)
                        {
                            var relation = _db.GetRelation(modification.relation[i].id);
                            if (relation.Version != modification.relation[i].version)
                            {
                                return new ApiResult<bool>(false);
                            }
                        }
                    }
                }
            }

            return new ApiResult<bool>(true);
        }
    }
}
