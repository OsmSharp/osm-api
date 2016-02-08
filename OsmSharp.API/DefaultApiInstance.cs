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

using OsmSharp.API.Db;
using System.Collections.Generic;
using OsmSharp.Changesets;
using OsmSharp.Db;

namespace OsmSharp.API
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
        private Dictionary<long, Changeset> _openChangesets = new Dictionary<long, Changeset>();

        /// <summary>
        /// Gets the capabilities.
        /// </summary>
        public ApiResult<Osm> GetCapabilities()
        {
            return new ApiResult<Osm>(new Osm()
            {
                Version = 0.6,
                Generator = "OsmSharp.API",
                Api = new Capabilities()
                {
                    Version = new Version()
                    {
                        Minimum = 0.6,
                        Maximum = 0.6
                    },
                    Area = new Area()
                    {
                        Maximum = 0.25
                    },
                    Tracepoints = new Tracepoints()
                    {
                        PerPage = 5000
                    },
                    WayNodes = new WayNodes()
                    {
                        Maximum = 2000
                    },
                    Changesets = new Changesets()
                    {
                        MaximumElements = 50000
                    },
                    Timeout = new Timeout()
                    {
                        Seconds = 300
                    },
                    Status = new Status()
                    {
                        Api = "online",
                        Database = "online",
                        Gpx = "online"
                    }
                }
            });
        }

        /// <summary>
        /// Gets all objects inside the given bounds.
        /// </summary>
        public ApiResult<Osm> GetMap(float left, float bottom, float right, float top)
        {
            var found = _db.Get(bottom, left, top, right);

            var nodes = new List<Node>();
            var ways = new List<Way>();
            var relations = new List<Relation>();

            foreach (var foundOsmGeo in found)
            {
                switch(foundOsmGeo.Type)
                { 
                    case OsmGeoType.Node:
                        nodes.Add(foundOsmGeo as Node);
                        break;
                    case OsmGeoType.Way:
                        ways.Add(foundOsmGeo as Way);
                        break;
                    case OsmGeoType.Relation:
                        relations.Add(foundOsmGeo as Relation);
                        break;
                }
            }

            return new ApiResult<Osm>(
                new Osm()
                {
                    Generator = "OsmSharp.API",
                    Version = 0.6,
                    Nodes = nodes.ToArray(),
                    Ways = ways.ToArray(),
                    Relations = relations.ToArray()
                });
        }

        /// <summary>
        /// Gets the node with the given id.
        /// </summary>
        public ApiResult<Osm> GetNode(long id)
        {
            var node = _db.GetNode(id);
            if(node == null)
            {
                return new ApiResult<Osm>(ApiResultStatusCode.NotFound, "Node not found.");
            }
            return new ApiResult<Osm>(new Osm()
            {
                Version = 0.6,
                Nodes = new Node[] { node }
            });
        }

        /// <summary>
        /// Gets the way with the given id.
        /// </summary>
        public ApiResult<Osm> GetWay(long id)
        {
            var way = _db.GetWay(id);
            if (way == null)
            {
                return new ApiResult<Osm>(ApiResultStatusCode.NotFound, "Way not found.");
            }
            return new ApiResult<Osm>(new Osm()
            {
                Version = 0.6,
                Ways = new Way[] { way }
            });
        }

        /// <summary>
        /// Gets the relation with the given id.
        /// </summary>
        public ApiResult<Osm> GetRelation(long id)
        {
            var relation = _db.GetRelation(id);
            if (relation == null)
            {
                return new ApiResult<Osm>(ApiResultStatusCode.NotFound, "Relation not found.");
            }
            return new ApiResult<Osm>(new Osm()
            {
                Version = 0.6,
                Relations = new Relation[] { relation }
            });
        }

        /// <summary>
        /// Gets the user with the given id.
        /// </summary>
        public ApiResult<Osm> GetUser(long id)
        {
            var user = _db.GetUser(id);
            if (user == null)
            {
                return null;
            }
            return new ApiResult<Osm>(new Osm()
            {
                Version = 0.6,
                User = user
            });
        }

        /// <summary>
        /// Creates a new changeset.
        /// </summary>
        public ApiResult<long> CreateChangeset(Changeset changeset)
        {
            lock (_openChangesets)
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
        public ApiResult<DiffResult> ApplyChangeset(long id, OsmChange osmChange)
        {
            // validate changeset.
            var validated = this.ValidateChangeset(osmChange);
            if (validated.IsError)
            {
                return validated.Convert<DiffResult>();
            }
            if (!validated.Data)
            {
                return new ApiResult<DiffResult>(ApiResultStatusCode.Exception,
                    "Changeset could not be applied, validation failed.");
            }

            var diffResultResult = _db.ApplyChangeset(id, osmChange);
            if (diffResultResult.Status == DiffResultStatus.BestEffortOK ||
                diffResultResult.Status == DiffResultStatus.OK)
            {
                return new ApiResult<DiffResult>(diffResultResult.Result);
            }
            switch(diffResultResult.Status)
            {
                case DiffResultStatus.Conflict:
                case DiffResultStatus.TooBig:
                case DiffResultStatus.UknownError:
                    return new ApiResult<DiffResult>(ApiResultStatusCode.Unknown,
                        string.Format("{0} : {1}", diffResultResult.Status.ToInvariantString().ToLowerInvariant(),
                            diffResultResult.Message));
            }
            return new ApiResult<DiffResult>(ApiResultStatusCode.Unknown,
                string.Format("Unknown status: {0}.", 
                    diffResultResult.Status.ToInvariantString().ToLowerInvariant()));
        }

        /// <summary>
        /// Validates changeset against the current state of the data.
        /// </summary>
        private ApiResult<bool> ValidateChangeset(OsmChange osmChange)
        {
            if (osmChange.Create != null)
            {
                for (var i = 0; i < osmChange.Create.Length; i++)
                {
                    if (osmChange.Create[i] != null && 
                        osmChange.Create[i].Version > 1)
                    {
                        return new ApiResult<bool>(false);
                    }
                }
            }
            if (osmChange.Modify != null)
            {
                for (var i = 0; i < osmChange.Modify.Length; i++)
                {
                    var modification = osmChange.Modify[i];
                    if (modification != null)
                    {
                        if (!modification.Id.HasValue)
                        {
                            return new ApiResult<bool>(false);
                        }

                        var osmGeo = _db.Get(modification.Type, modification.Id.Value);
                        if (osmGeo == null)
                        {
                            return new ApiResult<bool>(false);
                        }
                        if (osmGeo.Version != modification.Version)
                        {
                            return new ApiResult<bool>(false);
                        }
                    }
                }
            }
            if (osmChange.Delete != null)
            {
                for (var i = 0; i < osmChange.Delete.Length; i++)
                {
                    var delete = osmChange.Modify[i];
                    if (!delete.Id.HasValue)
                    {
                        return new ApiResult<bool>(false);
                    }

                    var osmGeo = _db.Get(delete.Type, delete.Id.Value);
                    if (osmGeo == null)
                    {
                        return new ApiResult<bool>(false);
                    }
                    if (osmGeo.Version != delete.Version)
                    {
                        return new ApiResult<bool>(false);
                    }
                }
            }

            return new ApiResult<bool>(true);
        }

        /// <summary>
        /// Closes the changeset with the given id.
        /// </summary>
        public ApiResult<bool> CloseChangeset(long id)
        {
            return new ApiResult<bool>(true);
        }

        /// <summary>
        /// Validates a user and returns it's user id.
        /// </summary>
        public ApiResult<long> ValidateUser(string username, string password)
        {
            var user = _db.GetUserByName(username);
            if(user == null)
            { // username not found.
                return new ApiResult<long>(ApiResultStatusCode.NotFound, "User with given username not found.");
            }
            var hash = Authentication.SaltedHashAlgorithm.HashPassword(username, password);
            var dbHash = _db.GetUserPasswordHash(user.Id);
            if(!hash.Equals(dbHash))
            { // password not correct.
                return new ApiResult<long>(ApiResultStatusCode.NotFound, "Password not correct.");
            }

            return new ApiResult<long>(user.Id);
        }
    }
}