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
using OsmSharp.Osm.API.Db;
using OsmSharp.Osm.Xml.v0_6;
using System;

namespace OsmSharp.Osm.API.Tests
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
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            var result = api.GetNode(1);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ApiResultStatusCode.NotFound, result.Status);

            db.LoadFrom(new OsmGeo[]
            {
                Node.Create(1, 10, 100)
            });

            result = api.GetNode(1);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            var osm = result.Data;
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.node);
            Assert.AreEqual(1, osm.node.Length);
            Assert.AreEqual(0.6, osm.version);
            var node = osm.node[0];
            Assert.AreEqual(1, node.id);
            Assert.AreEqual(10, node.lat);
            Assert.AreEqual(100, node.lon);
        }

        /// <summary>
        /// Test getting a way.
        /// </summary>
        [Test]
        public void TestGetWay()
        {
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            var result = api.GetWay(1);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ApiResultStatusCode.NotFound, result.Status);

            db.LoadFrom(new OsmGeo[]
            {
                Way.Create(1, 1, 2, 3)
            });

            result = api.GetWay(1);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            var osm = result.Data;
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.way);
            Assert.AreEqual(1, osm.way.Length);
            Assert.AreEqual(0.6, osm.version);
            var way = osm.way[0];
            Assert.AreEqual(1, way.id);
            Assert.IsNotNull(way.nd);
            Assert.AreEqual(3, way.nd.Length);
            Assert.AreEqual(1, way.nd[0].@ref);
            Assert.AreEqual(2, way.nd[1].@ref);
            Assert.AreEqual(3, way.nd[2].@ref);
        }

        /// <summary>
        /// Test getting a relation.
        /// </summary>
        [Test]
        public void TestGetRelation()
        {
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            var result = api.GetRelation(1);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual(ApiResultStatusCode.NotFound, result.Status);

            db.LoadFrom(new OsmGeo[]
            {
                Relation.Create(1,
                    RelationMember.Create(1, "test", OsmGeoType.Node),
                    RelationMember.Create(2, "another_test", OsmGeoType.Way),
                    RelationMember.Create(3, "yet_another_test", OsmGeoType.Relation))
            });

            result = api.GetRelation(1);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            var osm = result.Data;
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.relation);
            Assert.AreEqual(1, osm.relation.Length);
            Assert.AreEqual(0.6, osm.version);
            var relation = osm.relation[0];
            Assert.IsNotNull(relation);
            Assert.AreEqual(1, relation.id);
            Assert.IsNotNull(relation.member);
            Assert.AreEqual(3, relation.member.Length);
            Assert.AreEqual(1, relation.member[0].@ref);
            Assert.AreEqual("test", relation.member[0].role);
            Assert.AreEqual(memberType.node, relation.member[0].type);
            Assert.AreEqual(2, relation.member[1].@ref);
            Assert.AreEqual("another_test", relation.member[1].role);
            Assert.AreEqual(memberType.way, relation.member[1].type);
            Assert.AreEqual(3, relation.member[2].@ref);
            Assert.AreEqual("yet_another_test", relation.member[2].role);
            Assert.AreEqual(memberType.relation, relation.member[2].type);
        }

        /// <summary>
        /// Test opening a changeset.
        /// </summary>
        [Test]
        public void TestOpenChangeset()
        {
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            var result = api.CreateChangeset(new changeset()
            {
                id = -1,
                tag = new tag[]
                {
                    new tag()
                    {
                        k = "test_tag",
                        v = "test_value"
                    }
                }
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
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            var changesetResult = api.CreateChangeset(new changeset());

            var osmChange = new osmChange()
            {
                create = new create[]
                {
                    new create()
                    {
                        node = new node[]
                        {
                            new node()
                            {
                                id = -1,
                                idSpecified = true,
                                lat = 10,
                                latSpecified = true,
                                lon = 100,
                                lonSpecified = true,
                                user = string.Empty,
                                uid = 124,
                                uidSpecified = true,
                                timestamp = DateTime.Now,
                                timestampSpecified = true,
                                visible = true,
                                visibleSpecified = true
                            }
                        }
                    }
                }
            };

            var result = api.ApplyChangeset(osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.osmresult);
            Assert.AreEqual(1, diffResult.osmresult.Length);
            var osmresult = diffResult.osmresult[0];
            Assert.IsInstanceOf<noderesult>(osmresult);
            var noderesult = osmresult as noderesult;
            Assert.IsTrue(noderesult.old_idSpecified);
            Assert.AreEqual(-1, noderesult.old_id);
            Assert.IsTrue(noderesult.new_idSpecified);
            Assert.AreEqual(1, noderesult.new_id);
            Assert.IsTrue(noderesult.new_versionSpecified);
            Assert.AreEqual(1, noderesult.new_version);
        }

        /// <summary>
        /// Test applying a changeset and adding a node.
        /// </summary>
        [Test]
        public void TestApplyChangesetDeleteNode()
        {
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            var id = db.AddNewNode(new Node()
            {
                Latitude = 10,
                Longitude = 100
            }).Id.Value;

            var changesetResult = api.CreateChangeset(new changeset());

            var osmChange = new osmChange()
            {
                delete = new delete[]
                {
                    new delete()
                    {
                        node = new node[]
                        {
                            new node()
                            {
                                id = id,
                                idSpecified = true,
                                lat = 10,
                                latSpecified = true,
                                lon = 100,
                                lonSpecified = true,
                                user = string.Empty,
                                uid = 124,
                                uidSpecified = true,
                                timestamp = DateTime.Now,
                                timestampSpecified = true,
                                visible = true,
                                visibleSpecified = true,
                                version = 1,
                                versionSpecified = true
                            }
                        }
                    }
                }
            };

            var result = api.ApplyChangeset(osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.osmresult);
            Assert.AreEqual(1, diffResult.osmresult.Length);
            var osmresult = diffResult.osmresult[0];
            Assert.IsInstanceOf<noderesult>(osmresult);
            var noderesult = osmresult as noderesult;
            Assert.IsTrue(noderesult.old_idSpecified);
            Assert.AreEqual(1, noderesult.old_id);
            Assert.IsFalse(noderesult.new_idSpecified);
            Assert.IsFalse(noderesult.new_versionSpecified);
        }

        /// <summary>
        /// Test applying a changeset and adding a node.
        /// </summary>
        [Test]
        public void TestApplyChangesetUpdateNode()
        {
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            var id = db.AddNewNode(new Node()
            {
                Latitude = 10,
                Longitude = 100,
                TimeStamp = DateTime.Now.AddYears(-1)
            }).Id.Value;

            var changesetResult = api.CreateChangeset(new changeset());

            var osmChange = new osmChange()
            {
                modify = new modify[]
                {
                    new modify()
                    {
                        node = new node[]
                        {
                            new node()
                            {
                                id = 1,
                                idSpecified = true,
                                lat = 100,
                                latSpecified = true,
                                lon = 1000,
                                lonSpecified = true,
                                user = string.Empty,
                                uid = 125,
                                uidSpecified = true,
                                timestamp = DateTime.Now,
                                timestampSpecified = true,
                                visible = true,
                                visibleSpecified = true,
                                version = 1,
                                versionSpecified = true
                            }
                        }
                    }
                }
            };

            var result = api.ApplyChangeset(osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.osmresult);
            Assert.AreEqual(1, diffResult.osmresult.Length);
            var osmresult = diffResult.osmresult[0];
            Assert.IsInstanceOf<noderesult>(osmresult);
            var noderesult = osmresult as noderesult;
            Assert.IsTrue(noderesult.old_idSpecified);
            Assert.AreEqual(1, noderesult.old_id);
            Assert.IsTrue(noderesult.new_idSpecified);
            Assert.AreEqual(1, noderesult.new_id);
            Assert.IsTrue(noderesult.new_versionSpecified);
            Assert.AreEqual(2, noderesult.new_version);
        }

        /// <summary>
        /// Test applying a changeset and adding a way.
        /// </summary>
        [Test]
        public void TestApplyChangesetAddWay()
        {
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            var changesetResult = api.CreateChangeset(new changeset());

            var osmChange = new osmChange()
            {
                create = new create[]
                {
                    new create()
                    {
                        way = new way[]
                        {
                            new way()
                            {
                                id = -1,
                                idSpecified = true,
                                user = string.Empty,
                                uid = 124,
                                uidSpecified = true,
                                nd = new nd[]
                                {
                                    new nd()
                                    {
                                        @ref = 1,
                                        refSpecified = true
                                    },
                                    new nd()
                                    {
                                        @ref = 2,
                                        refSpecified = true
                                    }
                                },
                                timestamp = DateTime.Now,
                                timestampSpecified = true,
                                visible = true,
                                visibleSpecified = true
                            }
                        }
                    }
                }
            };

            var result = api.ApplyChangeset(osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.osmresult);
            Assert.AreEqual(1, diffResult.osmresult.Length);
            var osmresult = diffResult.osmresult[0];
            Assert.IsInstanceOf<wayresult>(osmresult);
            var noderesult = osmresult as wayresult;
            Assert.AreEqual(-1, noderesult.old_id);
            Assert.AreEqual(1, noderesult.new_id);
            Assert.AreEqual(1, noderesult.new_version);
        }

        /// <summary>
        /// Test applying a changeset and deleting a way.
        /// </summary>
        [Test]
        public void TestApplyChangesetDeleteWay()
        {
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            var id = db.AddNewWay(new Way()
            {
                Id = -1,
                Nodes = new System.Collections.Generic.List<long>(
                    new long[] { 1, 3 }),
                TimeStamp = DateTime.Now.AddYears(-1)
            }).Id.Value;

            var changesetResult = api.CreateChangeset(new changeset());

            var osmChange = new osmChange()
            {
                delete = new delete[]
                {
                    new delete()
                    {
                        way = new way[]
                        {
                            new way()
                            {
                                id = 1,
                                idSpecified = true,
                                user = string.Empty,
                                uid = 124,
                                uidSpecified = true,
                                nd = new nd[]
                                {
                                    new nd()
                                    {
                                        @ref = 1,
                                        refSpecified = true
                                    },
                                    new nd()
                                    {
                                        @ref = 3,
                                        refSpecified = true
                                    }
                                },
                                timestamp = DateTime.Now,
                                timestampSpecified = true,
                                visible = true,
                                visibleSpecified = true,
                                version = 1,
                                versionSpecified = true
                            }
                        }
                    }
                }
            };

            var result = api.ApplyChangeset(osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.osmresult);
            Assert.AreEqual(1, diffResult.osmresult.Length);
            var osmresult = diffResult.osmresult[0];
            Assert.IsInstanceOf<wayresult>(osmresult);
            var noderesult = osmresult as wayresult;
            Assert.IsTrue(noderesult.old_idSpecified);
            Assert.AreEqual(1, noderesult.old_id);
            Assert.IsFalse(noderesult.new_idSpecified);
            Assert.IsFalse(noderesult.new_versionSpecified);
        }

        /// <summary>
        /// Test applying a changeset and updating a way.
        /// </summary>
        [Test]
        public void TestApplyChangesetUpdateWay()
        {
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            var id = db.AddNewWay(new Way()
            {
                Id = -1,
                Nodes = new System.Collections.Generic.List<long>(
                    new long[] { 1, 3 }),
                TimeStamp = DateTime.Now.AddYears(-1)
            }).Id.Value;

            var changesetResult = api.CreateChangeset(new changeset());

            var osmChange = new osmChange()
            {
                modify = new modify[]
                {
                    new modify()
                    {
                        way = new way[]
                        {
                            new way()
                            {
                                id = 1,
                                idSpecified = true,
                                user = string.Empty,
                                uid = 124,
                                uidSpecified = true,
                                nd = new nd[]
                                {
                                    new nd()
                                    {
                                        @ref = 1,
                                        refSpecified = true
                                    },
                                    new nd()
                                    {
                                        @ref = 2,
                                        refSpecified = true
                                    }
                                },
                                timestamp = DateTime.Now,
                                timestampSpecified = true,
                                visible = true,
                                visibleSpecified = true,
                                version = 1,
                                versionSpecified = true
                            }
                        }
                    }
                }
            };

            var result = api.ApplyChangeset(osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.osmresult);
            Assert.AreEqual(1, diffResult.osmresult.Length);
            var osmresult = diffResult.osmresult[0];
            Assert.IsInstanceOf<wayresult>(osmresult);
            var noderesult = osmresult as wayresult;
            Assert.IsTrue(noderesult.old_idSpecified);
            Assert.AreEqual(1, noderesult.old_id);
            Assert.IsTrue(noderesult.new_idSpecified);
            Assert.AreEqual(1, noderesult.new_id);
            Assert.IsTrue(noderesult.new_versionSpecified);
            Assert.AreEqual(2, noderesult.new_version);
        }

        /// <summary>
        /// Test applying a changeset and adding a relation.
        /// </summary>
        [Test]
        public void TestApplyChangesetAddRelation()
        {
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            var changesetResult = api.CreateChangeset(new changeset());

            var osmChange = new osmChange()
            {
                create = new create[]
                {
                    new create()
                    {
                        relation = new relation[]
                        {
                            new relation()
                            {
                                id = -1,
                                idSpecified = true,
                                user = string.Empty,
                                uid = 124,
                                uidSpecified = true,
                                member = new member[]
                                {
                                    new member()
                                    {
                                        @ref = 1,
                                        refSpecified = true,
                                        role = "test_role",
                                        type = memberType.node,
                                        typeSpecified = true
                                    },
                                    new member()
                                    {
                                        @ref = 2,
                                        refSpecified = true,
                                        role = "another_test_role",
                                        type = memberType.way,
                                        typeSpecified = true
                                    }
                                },
                                timestamp = DateTime.Now,
                                timestampSpecified = true,
                                visible = true,
                                visibleSpecified = true
                            }
                        }
                    }
                }
            };

            var result = api.ApplyChangeset(osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.osmresult);
            Assert.AreEqual(1, diffResult.osmresult.Length);
            var osmresult = diffResult.osmresult[0];
            Assert.IsInstanceOf<relationresult>(osmresult);
            var noderesult = osmresult as relationresult;
            Assert.AreEqual(-1, noderesult.old_id);
            Assert.IsTrue(noderesult.old_idSpecified);
            Assert.AreEqual(1, noderesult.new_id);
            Assert.IsTrue(noderesult.new_idSpecified);
            Assert.AreEqual(1, noderesult.new_version);
            Assert.IsTrue(noderesult.new_versionSpecified);
        }

        /// <summary>
        /// Test applying a changeset and deleting a relation.
        /// </summary>
        [Test]
        public void TestApplyChangesetDeleteRelation()
        {
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            db.AddNewRelation(new Relation()
            {
                Id = -1,
                Members = new System.Collections.Generic.List<RelationMember>(
                    new RelationMember[]
                    {
                        new RelationMember()
                        {
                            MemberId = 1,
                            MemberRole = "test_role",
                            MemberType = OsmGeoType.Node
                        },
                        new RelationMember()
                        {
                            MemberId = 2,
                            MemberRole = "another_test_role",
                            MemberType = OsmGeoType.Way
                        }
                    }),
                TimeStamp = DateTime.Now.AddYears(-1)
            });

            var changesetResult = api.CreateChangeset(new changeset());

            var osmChange = new osmChange()
            {
                delete = new delete[]
                {
                    new delete()
                    {
                        relation = new relation[]
                        {
                            new relation()
                            {
                                id = 1,
                                idSpecified = true,
                                user = string.Empty,
                                uid = 124,
                                uidSpecified = true,
                                member = new member[]
                                {
                                    new member()
                                    {
                                        @ref = 1,
                                        refSpecified = true,
                                        role = "test_role",
                                        type = memberType.node,
                                        typeSpecified = true
                                    },
                                    new member()
                                    {
                                        @ref = 2,
                                        refSpecified = true,
                                        role = "another_test_role",
                                        type = memberType.way,
                                        typeSpecified = true
                                    }
                                },
                                timestamp = DateTime.Now,
                                timestampSpecified = true,
                                version = 1,
                                versionSpecified = true,
                                visible = true,
                                visibleSpecified = true
                            }
                        }
                    }
                }
            };

            var result = api.ApplyChangeset(osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.osmresult);
            Assert.AreEqual(1, diffResult.osmresult.Length);
            var osmresult = diffResult.osmresult[0];
            Assert.IsInstanceOf<relationresult>(osmresult);
            var noderesult = osmresult as relationresult;
            Assert.AreEqual(1, noderesult.old_id);
            Assert.IsTrue(noderesult.old_idSpecified);
            Assert.IsFalse(noderesult.new_idSpecified);
            Assert.IsFalse(noderesult.new_versionSpecified);
        }

        /// <summary>
        /// Test applying a changeset and update a relation.
        /// </summary>
        [Test]
        public void TestApplyChangesetUpdateRelation()
        {
            var db = new MemoryDb();
            var api = new DefaultApiInstance(db);

            db.AddNewRelation(new Relation()
            {
                Id = -1,
                Members = new System.Collections.Generic.List<RelationMember>(
                    new RelationMember[]
                    {
                        new RelationMember()
                        {
                            MemberId = 10,
                            MemberRole = "test_role",
                            MemberType = OsmGeoType.Node
                        },
                        new RelationMember()
                        {
                            MemberId = 20,
                            MemberRole = "another_test_role",
                            MemberType = OsmGeoType.Way
                        }
                    }),
                TimeStamp = DateTime.Now.AddYears(-1)
            });

            var changesetResult = api.CreateChangeset(new changeset());

            var osmChange = new osmChange()
            {
                modify = new modify[]
                {
                    new modify()
                    {
                        relation = new relation[]
                        {
                            new relation()
                            {
                                id = 1,
                                idSpecified = true,
                                user = string.Empty,
                                uid = 124,
                                uidSpecified = true,
                                member = new member[]
                                {
                                    new member()
                                    {
                                        @ref = 1,
                                        refSpecified = true,
                                        role = "test_role",
                                        type = memberType.node,
                                        typeSpecified = true
                                    },
                                    new member()
                                    {
                                        @ref = 2,
                                        refSpecified = true,
                                        role = "another_test_role",
                                        type = memberType.way,
                                        typeSpecified = true
                                    }
                                },
                                timestamp = DateTime.Now,
                                timestampSpecified = true,
                                version = 1,
                                versionSpecified = true,
                                visible = true,
                                visibleSpecified = true
                            }
                        }
                    }
                }
            };

            var result = api.ApplyChangeset(osmChange);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            var diffResult = result.Data;
            Assert.IsNotNull(diffResult.osmresult);
            Assert.AreEqual(1, diffResult.osmresult.Length);
            var osmresult = diffResult.osmresult[0];
            Assert.IsInstanceOf<relationresult>(osmresult);
            var noderesult = osmresult as relationresult;
            Assert.AreEqual(1, noderesult.old_id);
            Assert.IsTrue(noderesult.old_idSpecified);
            Assert.AreEqual(1, noderesult.new_id);
            Assert.IsTrue(noderesult.new_idSpecified);
            Assert.AreEqual(2, noderesult.new_version);
            Assert.IsTrue(noderesult.new_versionSpecified);
        }
    }
}
