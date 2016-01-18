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

using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using OsmSharp.Osm.Xml.v0_6;

namespace OsmSharp.Osm.API.Tests
{
    /// <summary>
    /// Contains tests for the API module.
    /// </summary>
    [TestFixture]
    public class ApiModuleTests
    {
        /// <summary>
        /// Tests getting a node.
        /// </summary>
        [Test]
        public void TestGetNode()
        {
            var api = new Mocks.MockApiInstance(new Node[] {
                Node.Create(1, 10, 100) }, new Way[0], new Relation[0]);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/test/api/0.6/node/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var osm = result.DeserializeOsmXml();
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.node);
            Assert.AreEqual(1, osm.node.Length);
            var node = osm.node[0];
            Assert.IsNotNull(node);
            Assert.AreEqual(1, node.id);
            Assert.AreEqual(10, node.lat);
            Assert.AreEqual(100, node.lon);

            result = browser.Get("/test/api/0.6/node/10", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
        }

        /// <summary>
        /// Tests getting a way.
        /// </summary>
        [Test]
        public void TestGetWay()
        {
            var api = new Mocks.MockApiInstance(new Node[0], new Way[] {
                Way.Create(1, 1, 2, 3)
            }, new Relation[0]);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/test/api/0.6/way/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var osm = result.DeserializeOsmXml();
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.way);
            Assert.AreEqual(1, osm.way.Length);
            var way = osm.way[0];
            Assert.IsNotNull(way);
            Assert.AreEqual(1, way.id);
            Assert.IsNotNull(way.nd);
            Assert.AreEqual(3, way.nd.Length);
            Assert.AreEqual(1, way.nd[0].@ref);
            Assert.AreEqual(2, way.nd[1].@ref);
            Assert.AreEqual(3, way.nd[2].@ref);

            result = browser.Get("/test/api/0.6/way/10", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
        }

        /// <summary>
        /// Tests getting a relation.
        /// </summary>
        [Test]
        public void TestGetRelation()
        {
            var api = new Mocks.MockApiInstance(new Node[0], new Way[0], new Relation[]
                {
                    Relation.Create(1, 
                        RelationMember.Create(1, "test", OsmGeoType.Node),
                        RelationMember.Create(2, "another_test", OsmGeoType.Way),
                        RelationMember.Create(3, "yet_another_test", OsmGeoType.Relation))
                });
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/test/api/0.6/relation/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var osm = result.DeserializeOsmXml();
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.relation);
            Assert.AreEqual(1, osm.relation.Length);
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

            result = browser.Get("/test/api/0.6/relation/10", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
        }

        /// <summary>
        /// Test get capabilities call.
        /// </summary>
        [Test]
        public void TestGetCapabilities()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            api.Capabilities = new api()
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
            };
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/test/api/capabilities", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var osm = result.DeserializeOsmXml();
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.api);

            result = browser.Get("/test/api/0.6/capabilities", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            osm = result.DeserializeOsmXml();
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.api);
        }

        /// <summary>
        /// Tests get permissions.
        /// </summary>
        [Test]
        public void TestGetPermissions()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/permissions", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/permissions", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get changeset.
        /// </summary>
        [Test]
        public void TestGetChangeset()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/changeset/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/changeset/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests creating a changeset.
        /// </summary>
        [Test]
        public void TestCreateChangetset()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Put("/test/api/0.6/changeset/create", with =>
            {
                with.HttpRequest();
                with.OsmXmlBody(new osm()
                {
                    changeset = new changeset[]
                    {
                        new changeset()
                        {
                            tag = new tag[]
                            {
                                new tag()
                                {
                                    k = "test_key",
                                    v = "test_value"
                                }
                            }
                        }
                    }
                });
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual("1", result.DeserializeAsString());
        }

        /// <summary>
        /// Tests put changeset.
        /// </summary>
        [Test]
        public void TestPutChangeset()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Put("/notatest/api/0.6/changeset/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Put("/test/api/0.6/changeset/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests put changeset close.
        /// </summary>
        [Test]
        public void TestPutChangesetClose()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Put("/notatest/api/0.6/changeset/1/close", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Put("/test/api/0.6/changeset/1/close", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get changeset download.
        /// </summary>
        [Test]
        public void TestGetChangesetDownload()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/changeset/1/download", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/changeset/1/download", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests post expand changeset bbox.
        /// </summary>
        [Test]
        public void TestPostChangesetExpandBB()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Post("/notatest/api/0.6/changeset/1/expand_bbox", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Post("/test/api/0.6/changeset/1/expand_bbox", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests put node create.
        /// </summary>
        [Test]
        public void TestPutNodeCreate()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Put("/notatest/api/0.6/node/create", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Put("/test/api/0.6/node/create", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests put way create.
        /// </summary>
        [Test]
        public void TestPutWayCreate()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Put("/notatest/api/0.6/way/create", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Put("/test/api/0.6/way/create", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests put relation create.
        /// </summary>
        [Test]
        public void TestPutRelationCreate()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Put("/notatest/api/0.6/relation/create", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Put("/test/api/0.6/relation/create", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests put node update.
        /// </summary>
        [Test]
        public void TestPutNodeUpdate()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Put("/notatest/api/0.6/node/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Put("/test/api/0.6/node/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests put way update.
        /// </summary>
        [Test]
        public void TestPutWayUpdate()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Put("/notatest/api/0.6/way/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Put("/test/api/0.6/way/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests put relation update.
        /// </summary>
        [Test]
        public void TestPutRelationUpdate()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Put("/notatest/api/0.6/relation/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Put("/test/api/0.6/relation/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests delete node.
        /// </summary>
        [Test]
        public void TestDeleteNode()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Delete("/notatest/api/0.6/node/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Delete("/test/api/0.6/node/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests delete way.
        /// </summary>
        [Test]
        public void TestDeleteWay()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Delete("/notatest/api/0.6/way/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Delete("/test/api/0.6/way/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests delete relation.
        /// </summary>
        [Test]
        public void TestDeleteRelation()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Delete("/notatest/api/0.6/relation/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Delete("/test/api/0.6/relation/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get node history.
        /// </summary>
        [Test]
        public void TestGetNodeHistory()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/node/1/history", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/node/1/history", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get way history.
        /// </summary>
        [Test]
        public void TestGetWayHistory()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/way/1/history", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/way/1/history", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get relation history.
        /// </summary>
        [Test]
        public void TestGetRelationHistory()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/relation/1/history", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/relation/1/history", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get node version.
        /// </summary>
        [Test]
        public void TestGetNodeVersion()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/node/1/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/node/1/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get way version.
        /// </summary>
        [Test]
        public void TestGetWayVersion()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/way/1/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/way/1/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get relation version.
        /// </summary>
        [Test]
        public void TestGetRelationVersion()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/relation/1/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/relation/1/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get node relations.
        /// </summary>
        [Test]
        public void TestGetNodeRelations()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/node/1/relations", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/node/1/relations", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get way relations.
        /// </summary>
        [Test]
        public void TestGetWayRelations()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/way/1/relations", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/way/1/relations", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get relation relations.
        /// </summary>
        [Test]
        public void TestGetRelationRelations()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/relation/1/relations", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/relation/1/relations", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get node full.
        /// </summary>
        [Test]
        public void TestGetNodeFull()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/node/1/full", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/node/1/full", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get way full.
        /// </summary>
        [Test]
        public void TestGetWayFull()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/way/1/full", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/way/1/full", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }

        /// <summary>
        /// Tests get relation full.
        /// </summary>
        [Test]
        public void TestGetRelationFull()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.SetInstance("test", api);

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/notatest/api/0.6/relation/1/full", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Get("/test/api/0.6/relation/1/full", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotImplemented, result.StatusCode);
        }
    }
}
