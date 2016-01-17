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

using System.Linq;
using System.Collections.Generic;
using OsmSharp.Math.Geo;
using OsmSharp.Osm.API.Db.Domain;
using System;

namespace OsmSharp.Osm.API.Db
{
    /// <summary>
    /// An in-memory db.
    /// </summary>
    public class MemoryDb : IDb
    {
        private readonly List<Node> _nodes;
        private readonly List<Way> _ways;
        private readonly List<Relation> _relations;
        private readonly List<User> _users;

        /// <summary>
        /// Creates a new memory db.
        /// </summary>
        public MemoryDb()
        {
            _nodes = new List<Node>();
            _ways = new List<Way>();
            _relations = new List<Relation>();
            _users = new List<User>();
        }

        /// <summary>
        /// Loads all objects in the given enumerable.
        /// </summary>
        public void LoadFrom(IEnumerable<OsmGeo> source)
        { 
            foreach(var osmGeo in source)
            {
                switch(osmGeo.Type)
                {
                    case OsmGeoType.Node:
                        _nodes.Add(osmGeo as Node);
                        break;
                    case OsmGeoType.Way:
                        _ways.Add(osmGeo as Way);
                        break;
                    case OsmGeoType.Relation:
                        _relations.Add(osmGeo as Relation);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets all objects inside the given box.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OsmGeo> GetInsideBox(GeoCoordinateBox box)
        {
            var nodesInBox = new HashSet<long>();
            var nodesToInclude = new HashSet<long>();
            var waysToInclude = new HashSet<long>();
            var relationsToInclude = new HashSet<long>();

            for(var i = 0; i < _nodes.Count; i++)
            {
                if (box.Contains(_nodes[i].Longitude.Value, _nodes[i].Latitude.Value))
                {
                    nodesInBox.Add(_nodes[i].Id.Value);
                }
            }
            
            for (var i = 0; i < _ways.Count; i++)
            {
                if (_ways[i].Nodes != null)
                {
                    for (var n = 0; n < _ways[i].Nodes.Count; n++)
                    {
                        if (nodesInBox.Contains(_ways[i].Nodes[n]))
                        {
                            waysToInclude.Add(_ways[i].Id.Value);
                            for(var n1 = 0; n1 < _ways[i].Nodes.Count; n1++)
                            {
                                nodesToInclude.Add(_ways[i].Nodes[n1]);
                            }
                            break;
                        }
                    }
                }
            }
            for (var i = 0; i < _relations.Count; i++)
            {
                if (_relations[i].Members != null)
                {
                    for (var m = 0; m < _relations[i].Members.Count; m++)
                    {
                        var relationFound = false;
                        var member = _relations[i].Members[m];
                        switch(member.MemberType.Value)
                        { 
                            case OsmGeoType.Node:
                                if(nodesInBox.Contains(member.MemberId.Value))
                                {
                                    relationFound = true;
                                }
                                break;
                            case OsmGeoType.Way:
                                if (waysToInclude.Contains(member.MemberId.Value))
                                {
                                    relationFound = true;
                                }
                                break;
                        }
                        if (relationFound)
                        {
                            relationsToInclude.Add(_relations[i].Id.Value);
                            break;
                        }
                    }
                }
            }

            var found = new List<OsmGeo>();
            found.AddRange(_nodes.Where(x => nodesInBox.Contains(x.Id.Value) || nodesToInclude.Contains(x.Id.Value)));
            found.AddRange(_ways.Where(x => waysToInclude.Contains(x.Id.Value)));
            found.AddRange(_relations.Where(x => relationsToInclude.Contains(x.Id.Value)));
            return found;
        }

        /// <summary>
        /// Adds new a new node.
        /// </summary>
        public Node AddNewNode(Node node)
        {
            var id = 1L;
            for(var i = 0; i < _nodes.Count; i++)
            {
                if(_nodes[i].Id >= id)
                {
                    id = _nodes[i].Id.Value + 1;
                }
            }
            var newNode = new Node()
                {
                    Id = id,
                    ChangeSetId = node.ChangeSetId,
                    Latitude = node.Latitude,
                    Longitude = node.Longitude,
                    Tags = node.Tags,
                    TimeStamp = node.TimeStamp,
                    UserId = node.UserId,
                    UserName = node.UserName,
                    Version = 1,
                    Visible = node.Visible
                };
            _nodes.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// Gets the node with the given id.
        /// </summary>
        public Node GetNode(long id)
        {
            return _nodes.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Updates the given node.
        /// </summary>
        public bool UpdateNode(Node node)
        {
            for(var i = 0; i < _nodes.Count; i++)
            {
                if(_nodes[i].Id == node.Id.Value)
                {
                    if(_nodes[i].Version + 1 != node.Version)
                    {
                        throw new Exception("Version # doesn't match.");
                    }
                    _nodes[i] = node;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes the node with the given id.
        /// </summary>
        public bool DeleteNode(long id)
        {
            return _nodes.RemoveAll(x => x.Id == id) > 0;
        }

        /// <summary>
        /// Adds new a new way.
        /// </summary>
        public Way AddNewWay(Way way)
        {
            var id = 1L;
            for (var i = 0; i < _ways.Count; i++)
            {
                if (_ways[i].Id > id)
                {
                    id = _ways[i].Id.Value + 1;
                }
            }
            var newWay = new Way()
            {
                Id = id,
                ChangeSetId = way.ChangeSetId,
                Nodes = way.Nodes,
                Tags = way.Tags,
                TimeStamp = way.TimeStamp,
                UserId = way.UserId,
                UserName = way.UserName,
                Version = 1,
                Visible = way.Visible
            };
            _ways.Add(newWay);
            return newWay;
        }

        /// <summary>
        /// Gets the way with the given id.
        /// </summary>
        public Way GetWay(long id)
        {
            return _ways.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Updates the given way.
        /// </summary>
        public bool UpdateWay(Way way)
        {
            for (var i = 0; i < _ways.Count; i++)
            {
                if (_ways[i].Id == way.Id.Value)
                {
                    if (_ways[i].Version + 1 != way.Version)
                    {
                        throw new Exception("Version # doesn't match.");
                    }
                    _ways[i] = way;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes the way with the given id.
        /// </summary>
        public bool DeleteWay(long id)
        {
            return _ways.RemoveAll(x => x.Id == id) > 0;
        }

        /// <summary>
        /// Adds a new relation.
        /// </summary>
        public Relation AddNewRelation(Relation relation)
        {
            var id = 1L;
            for (var i = 0; i < _relations.Count; i++)
            {
                if (_relations[i].Id > id)
                {
                    id = _relations[i].Id.Value + 1;
                }
            }
            var newRelation = new Relation()
            {
                Id = id,
                ChangeSetId = relation.ChangeSetId,
                Members = relation.Members,
                Tags = relation.Tags,
                TimeStamp = relation.TimeStamp,
                UserId = relation.UserId,
                UserName = relation.UserName,
                Version = 1,
                Visible = relation.Visible
            };
            _relations.Add(newRelation);
            return newRelation;
        }

        /// <summary>
        /// Gets the relation with the given id.
        /// </summary>
        public Relation GetRelation(long id)
        {
            return _relations.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Updates the given relation.
        /// </summary>
        public bool UpdateRelation(Relation relation)
        {
            for (var i = 0; i < _relations.Count; i++)
            {
                if (_relations[i].Id == relation.Id.Value)
                {
                    if (_relations[i].Version + 1 != relation.Version)
                    {
                        throw new Exception("Version # doesn't match.");
                    }
                    _relations[i] = relation;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes the relation with the given id.
        /// </summary>
        public bool DeleteRelation(long id)
        {
            return _relations.RemoveAll(x => x.Id == id) > 0;
        }

        /// <summary>
        /// Adds new a new user.
        /// </summary>
        public User AddNewUser(User user)
        {
            var id = 1;
            for (var i = 0; i < _users.Count; i++)
            {
                if (_users[i].Id > id)
                {
                    id = _users[i].Id + 1;
                }
            }
            var newUser = new User()
            {
                Id = id,
                AccountCreated = user.AccountCreated,
                BlocksReceived = user.BlocksReceived,
                ChangeSetCount = user.ChangeSetCount,
                ContributorTermsAgreed = user.ContributorTermsAgreed,
                ContributorTermsPublicDomain = user.ContributorTermsPublicDomain,
                Description = user.Description,
                DisplayName = user.DisplayName,
                Home = user.Home,
                Image = user.Image,
                Roles = user.Roles,
                TraceCount = user.TraceCount
            };
            _users.Add(newUser);
            return newUser;
        }

        /// <summary>
        /// Gets the user with the given id.
        /// </summary>
        public User GetUser(long id)
        {
            return _users.FirstOrDefault(x => x.Id == id);
        }
    }
}