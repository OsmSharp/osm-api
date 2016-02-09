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

using NUnit.Framework;
using OsmSharp.API.Db.Default;
using OsmSharp.Changesets;
using OsmSharp.Db;
using OsmSharp.Tags;
using System;

namespace OsmSharp.API.Tests
{
    /// <summary>
    /// Contains tests for the default api instance wrapping an IDb.
    /// </summary>
    [TestFixture]
    public class DefaultApiInstanceTests
    {
        /// <summary>
        /// Test getting a node.
        /// </summary>
        [Test]
        public void TestGetNode()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            var result = api.GetNode(1);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ApiResultStatusCode.NotFound, result.Status);

            db.Add(new OsmGeo[]
            {
                new Node()
                {
                    Id = 1,
                    Latitude = 10,
                    Longitude = 100
                }
            });

            result = api.GetNode(1);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            var osm = result.Data;
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.Nodes);
            Assert.AreEqual(1, osm.Nodes.Length);
            Assert.AreEqual(0.6, osm.Version);
            var node = osm.Nodes[0];
            Assert.AreEqual(1, node.Id);
            Assert.AreEqual(10, node.Latitude);
            Assert.AreEqual(100, node.Longitude);
        }

        /// <summary>
        /// Test getting a way.
        /// </summary>
        [Test]
        public void TestGetWay()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            var result = api.GetWay(1);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ApiResultStatusCode.NotFound, result.Status);

            db.Add(new OsmGeo[]
            {
                new Way()
                {
                    Id = 1,
                    Nodes = new long[] { 1, 2, 3 }
                }
            });

            result = api.GetWay(1);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            var osm = result.Data;
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.Ways);
            Assert.AreEqual(1, osm.Ways.Length);
            Assert.AreEqual(0.6, osm.Version);
            var way = osm.Ways[0];
            Assert.AreEqual(1, way.Id);
            Assert.IsNotNull(way.Nodes);
            Assert.AreEqual(3, way.Nodes.Length);
            Assert.AreEqual(1, way.Nodes[0]);
            Assert.AreEqual(2, way.Nodes[1]);
            Assert.AreEqual(3, way.Nodes[2]);
        }

        /// <summary>
        /// Test getting a relation.
        /// </summary>
        [Test]
        public void TestGetRelation()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            var result = api.GetRelation(1);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ApiResultStatusCode.NotFound, result.Status);

            db.Add(new OsmGeo[]
            {
                new Relation()
                {
                    Id = 1,
                    Members = new RelationMember[]
                    {
                        new RelationMember()
                        {
                            Id = 1,
                            Role = "test",
                            Type = OsmGeoType.Node
                        },
                        new RelationMember()
                        {
                            Id = 2,
                            Role = "another_test",
                            Type = OsmGeoType.Way
                        },
                        new RelationMember()
                        {
                            Id = 3,
                            Role = "yet_another_test",
                            Type = OsmGeoType.Relation
                        }
                    }
                }
            });

            result = api.GetRelation(1);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            var osm = result.Data;
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.Relations);
            Assert.AreEqual(1, osm.Relations.Length);
            Assert.AreEqual(0.6, osm.Version);
            var relation = osm.Relations[0];
            Assert.IsNotNull(relation);
            Assert.AreEqual(1, relation.Id);
            Assert.IsNotNull(relation.Members);
            Assert.AreEqual(3, relation.Members.Length);
            Assert.AreEqual(1, relation.Members[0].Id);
            Assert.AreEqual("test", relation.Members[0].Role);
            Assert.AreEqual(OsmGeoType.Node, relation.Members[0].Type);
            Assert.AreEqual(2, relation.Members[1].Type);
            Assert.AreEqual("another_test", relation.Members[1].Role);
            Assert.AreEqual(OsmGeoType.Way, relation.Members[1].Type);
            Assert.AreEqual(3, relation.Members[2].Type);
            Assert.AreEqual("yet_another_test", relation.Members[2].Role);
            Assert.AreEqual(OsmGeoType.Relation, relation.Members[2].Type);
        }

        /// <summary>
        /// Test opening a changeset.
        /// </summary>
        [Test]
        public void TestOpenChangeset()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            var result = api.CreateChangeset(new Changeset()
            {
                Id = -1,
                Tags = new TagsCollection(
                    new Tag("test_tag", "test_value"))
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(1, result.Data);
        }

        /// <summary>
        /// Test applying a changeset and adding a node.
        /// </summary>
        [Test]
        public void TestApplyChangesetAddNode()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            var changesetResult = api.CreateChangeset(new Changeset());

            var osmChange = new OsmChange()
            {
                Create = new OsmGeo[]
                {
                    new Node()
                    {
                        Id = -1,
                        Latitude = 10,
                        Longitude = 100,
                        UserName = string.Empty,
                        UserId = 124,
                        TimeStamp = DateTime.Now,
                        Visible = true
                    }
                }
            };

            var result = api.ApplyChangeset(changesetResult.Data, osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.Results);
            Assert.AreEqual(1, diffResult.Results.Length);
            var osmresult = diffResult.Results[0];
            Assert.IsInstanceOf<NodeResult>(osmresult);
            var noderesult = osmresult as NodeResult;
            Assert.AreEqual(-1, noderesult.OldId);
            Assert.AreEqual(1, noderesult.NewId);
            Assert.AreEqual(1, noderesult.NewVersion);
        }

        /// <summary>
        /// Test applying a changeset and adding a node.
        /// </summary>
        [Test]
        public void TestApplyChangesetDeleteNode()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            db.Add(new Node()
                {
                    Id = 1,
                    Latitude = 10,
                    Longitude = 100
                });

            var changesetResult = api.CreateChangeset(new Changeset());
            var osmChange = new OsmChange()
            {
                Delete = new OsmGeo[]
                {
                    new Node()
                    {
                        Id = 1,
                        Latitude = 10,
                        Longitude = 100,
                        UserName = string.Empty,
                        UserId = 124,
                        TimeStamp = DateTime.Now,
                        Visible = true,
                        Version = 1
                    }
                }
            };

            var result = api.ApplyChangeset(changesetResult.Data, osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.Results);
            Assert.AreEqual(1, diffResult.Results.Length);
            var osmresult = diffResult.Results[0];
            Assert.IsInstanceOf<NodeResult>(osmresult);
            var noderesult = osmresult as NodeResult;
            Assert.AreEqual(1, noderesult.OldId);
            Assert.IsNull(noderesult.NewId);
            Assert.IsNull(noderesult.NewVersion);
        }

        /// <summary>
        /// Test applying a changeset and adding a node.
        /// </summary>
        [Test]
        public void TestApplyChangesetUpdateNode()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            db.Add(new Node()
            {
                Id = 1,
                Latitude = 10,
                Longitude = 100,
                TimeStamp = DateTime.Now.AddYears(-1)
            });

            var changesetResult = api.CreateChangeset(new Changeset());

            var osmChange = new OsmChange()
            {
                Modify = new OsmGeo[]
                {
                    new Node()
                    {
                        Id = 1,
                        Latitude = 100,
                        Longitude = 1000,
                        UserName = string.Empty,
                        UserId = 125,
                        TimeStamp = DateTime.Now,
                        Visible = true,
                        Version = 1
                    }
                }
            };

            var result = api.ApplyChangeset(changesetResult.Data, osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.Results);
            Assert.AreEqual(1, diffResult.Results.Length);
            var osmresult = diffResult.Results[0];
            Assert.IsInstanceOf<NodeResult>(osmresult);
            var noderesult = osmresult as NodeResult;
            Assert.AreEqual(1, noderesult.OldId);
            Assert.AreEqual(1, noderesult.NewId);
            Assert.AreEqual(2, noderesult.NewVersion);
        }

        /// <summary>
        /// Test applying a changeset and adding a way.
        /// </summary>
        [Test]
        public void TestApplyChangesetAddWay()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            var changesetResult = api.CreateChangeset(new Changeset());

            var osmChange = new OsmChange()
            {
                Create = new OsmGeo[]
                {
                    new Way()
                    {
                        Id = -1,
                        UserName = string.Empty,
                        UserId = 124,
                        Nodes = new long[] { 1, 2 },
                        TimeStamp = DateTime.Now,
                        Visible = true
                    }
                }
            };

            var result = api.ApplyChangeset(changesetResult.Data, osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.Results);
            Assert.AreEqual(1, diffResult.Results.Length);
            var osmresult = diffResult.Results[0];
            Assert.IsInstanceOf<WayResult>(osmresult);
            var noderesult = osmresult as WayResult;
            Assert.AreEqual(-1, noderesult.OldId);
            Assert.AreEqual(1, noderesult.NewId);
            Assert.AreEqual(1, noderesult.NewVersion);
        }

        /// <summary>
        /// Test applying a changeset and deleting a way.
        /// </summary>
        [Test]
        public void TestApplyChangesetDeleteWay()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            db.Add(new Way()
            {
                Id = -1,
                Nodes =new long[] { 1, 3 },
                TimeStamp = DateTime.Now.AddYears(-1)
            });

            var changesetResult = api.CreateChangeset(new Changeset());

            var osmChange = new OsmChange()
            {
                Delete = new OsmGeo[]
                {
                    new Way()
                    {
                        Id = 1,
                        UserName = string.Empty,
                        UserId = 124,
                        Nodes = new long[] {1, 3 },
                        TimeStamp = DateTime.Now,
                        Visible = true,
                        Version = 1,
                    }
                }
            };

            var result = api.ApplyChangeset(changesetResult.Data, osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.Results);
            Assert.AreEqual(1, diffResult.Results.Length);
            var osmresult = diffResult.Results[0];
            Assert.IsInstanceOf<WayResult>(osmresult);
            var noderesult = osmresult as WayResult;
            Assert.AreEqual(1, noderesult.OldId);
            Assert.IsNull(noderesult.NewId);
            Assert.IsNull(noderesult.NewVersion);
        }

        /// <summary>
        /// Test applying a changeset and updating a way.
        /// </summary>
        [Test]
        public void TestApplyChangesetUpdateWay()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            db.Add(new Way()
            {
                Id = -1,
                Nodes = new long[] { 1, 3 },
                TimeStamp = DateTime.Now.AddYears(-1)
            });

            var changesetResult = api.CreateChangeset(new Changeset());

            var osmChange = new OsmChange()
            {
                Modify = new OsmGeo[]
                {
                    new Way()
                    {
                        Id = 1,
                        UserName = string.Empty,
                        UserId = 124,
                        Nodes = new long[] { 1, 2 },
                        TimeStamp = DateTime.Now,
                        Visible = true,
                        Version = 1,
                    }
                }
            };

            var result = api.ApplyChangeset(changesetResult.Data, osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.Results);
            Assert.AreEqual(1, diffResult.Results.Length);
            var osmresult = diffResult.Results[0];
            Assert.IsInstanceOf<WayResult>(osmresult);
            var noderesult = osmresult as WayResult;
            Assert.AreEqual(1, noderesult.OldId);
            Assert.AreEqual(1, noderesult.NewId);
            Assert.AreEqual(2, noderesult.NewVersion);
        }

        /// <summary>
        /// Test applying a changeset and adding a relation.
        /// </summary>
        [Test]
        public void TestApplyChangesetAddRelation()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            var changesetResult = api.CreateChangeset(new Changeset());

            var osmChange = new OsmChange()
            {
                Create = new OsmGeo[]
                {
                    new Relation()
                    {
                        Id = -1,
                        UserName = string.Empty,
                        UserId = 124,
                        Members = new RelationMember[]
                        {
                            new RelationMember()
                            {
                                Id = 1,
                                Role = "test_role",
                                Type = OsmGeoType.Node,
                            },
                            new RelationMember()
                            {
                                Id = 2,
                                Role = "another_test_role",
                                Type = OsmGeoType.Way,
                            }
                        },
                        TimeStamp = DateTime.Now,
                        Visible = true
                    }
                }
            };

            var result = api.ApplyChangeset(changesetResult.Data, osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.Results);
            Assert.AreEqual(1, diffResult.Results.Length);
            var osmresult = diffResult.Results[0];
            Assert.IsInstanceOf<RelationResult>(osmresult);
            var noderesult = osmresult as RelationResult;
            Assert.AreEqual(-1, noderesult.OldId);
            Assert.AreEqual(1, noderesult.NewId);
            Assert.AreEqual(1, noderesult.NewVersion);
        }

        /// <summary>
        /// Test applying a changeset and deleting a relation.
        /// </summary>
        [Test]
        public void TestApplyChangesetDeleteRelation()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            db.Add(new Relation()
            {
                Id = 1,
                Members = new RelationMember[]
                    {
                        new RelationMember()
                        {
                            Id = 1,
                            Role = "test_role",
                            Type = OsmGeoType.Node
                        },
                        new RelationMember()
                        {
                            Id = 2,
                            Role = "another_test_role",
                            Type = OsmGeoType.Way
                        }
                    },
                TimeStamp = DateTime.Now.AddYears(-1)
            });

            var changesetResult = api.CreateChangeset(new Changeset());

            var osmChange = new OsmChange()
            {
                Delete = new OsmGeo[]
                {
                    new Relation()
                    {
                        Id = -1,
                        Members = new RelationMember[]
                            {
                                new RelationMember()
                                {
                                    Id = 1,
                                    Role = "test_role",
                                    Type = OsmGeoType.Node
                                },
                                new RelationMember()
                                {
                                    Id = 2,
                                    Role = "another_test_role",
                                    Type = OsmGeoType.Way
                                }
                            },
                        TimeStamp = DateTime.Now.AddYears(-1)
                    }
                }
            };

            var result = api.ApplyChangeset(changesetResult.Data, osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.Results);
            Assert.AreEqual(1, diffResult.Results.Length);
            var osmresult = diffResult.Results[0];
            Assert.IsInstanceOf<RelationResult>(osmresult);
            var noderesult = osmresult as RelationResult;
            Assert.AreEqual(1, noderesult.OldId);
            Assert.IsNull(noderesult.NewId);
            Assert.IsNull(noderesult.NewVersion);
        }

        /// <summary>
        /// Test applying a changeset and update a relation.
        /// </summary>
        [Test]
        public void TestApplyChangesetUpdateRelation()
        {
            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(),
                new MemoryUserDb());
            var api = new DefaultApiInstance(db);

            db.Add(new Relation()
            {
                Id = 1,
                Members = new RelationMember[]
                    {
                        new RelationMember()
                        {
                            Id = 10,
                            Role = "test_role",
                            Type = OsmGeoType.Node
                        },
                        new RelationMember()
                        {
                            Id = 20,
                            Role = "another_test_role",
                            Type = OsmGeoType.Way
                        }
                    },
                TimeStamp = DateTime.Now.AddYears(-1)
            });

            var changesetResult = api.CreateChangeset(new Changeset());

            var osmChange = new OsmChange()
            {
                Modify = new OsmGeo[]
                {
                    new Relation()
                    {
                        Id = 1,
                        UserName = string.Empty,
                        UserId = 124,
                        Members = new RelationMember[]
                        {
                            new RelationMember()
                            {
                                Id = 1,
                                Role = "test_role",
                                Type = OsmGeoType.Node
                            },
                            new RelationMember()
                            {
                                Id = 2,
                                Role = "another_test_role",
                                Type = OsmGeoType.Way
                            }
                        },
                        TimeStamp = DateTime.Now,
                        Version = 1,
                        Visible = true
                    }
                }
            };

            var result = api.ApplyChangeset(changesetResult.Data, osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.Results);
            Assert.AreEqual(1, diffResult.Results.Length);
            var osmresult = diffResult.Results[0];
            Assert.IsInstanceOf<RelationResult>(osmresult);
            var noderesult = osmresult as RelationResult;
            Assert.AreEqual(1, noderesult.OldId);
            Assert.AreEqual(1, noderesult.NewId);
            Assert.AreEqual(2, noderesult.NewVersion);
        }
    }
}