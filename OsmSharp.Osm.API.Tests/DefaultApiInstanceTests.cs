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
            Assert.AreEqual(-1, noderesult.old_id);
            Assert.AreEqual(1, noderesult.new_id);
            Assert.AreEqual(1, noderesult.new_version);
        }
    }
}
