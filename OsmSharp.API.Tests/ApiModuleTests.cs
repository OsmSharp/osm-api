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
using OsmSharp.Changesets;

namespace OsmSharp.API.Tests
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
            var api = new Mocks.MockApiInstance(
                new Node[] {
                    new Node()
                    {
                        Id = 1,
                        Latitude = 10,
                        Longitude = 100
                    }
                },
                new Way[0],
                new Relation[0]);
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/test/api/0.6/node/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var osm = result.DeserializeOsmXml();
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.Nodes);
            Assert.AreEqual(1, osm.Nodes.Length);
            var node = osm.Nodes[0];
            Assert.IsNotNull(node);
            Assert.AreEqual(1, node.Id);
            Assert.AreEqual(10, node.Latitude);
            Assert.AreEqual(100, node.Longitude);

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
                new Way()
                {
                    Id = 1,
                    Nodes = new long[] { 1, 2, 3 }
                }
            }, new Relation[0]);
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/test/api/0.6/way/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var osm = result.DeserializeOsmXml();
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.Ways);
            Assert.AreEqual(1, osm.Ways.Length);
            var way = osm.Ways[0];
            Assert.IsNotNull(way);
            Assert.AreEqual(1, way.Id);
            Assert.IsNotNull(way.Nodes);
            Assert.AreEqual(3, way.Nodes.Length);
            Assert.AreEqual(1, way.Nodes[0]);
            Assert.AreEqual(2, way.Nodes[1]);
            Assert.AreEqual(3, way.Nodes[2]);

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/test/api/0.6/relation/1", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var osm = result.DeserializeOsmXml();
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.Relations);
            Assert.AreEqual(1, osm.Relations.Length);
            var relation = osm.Relations[0];
            Assert.IsNotNull(relation);
            Assert.AreEqual(1, relation.Id);
            Assert.IsNotNull(relation.Members);
            Assert.AreEqual(3, relation.Members.Length);
            Assert.AreEqual(1, relation.Members[0].Id);
            Assert.AreEqual("test", relation.Members[0].Role);
            Assert.AreEqual(OsmGeoType.Node, relation.Members[0].Type);
            Assert.AreEqual(2, relation.Members[1].Id);
            Assert.AreEqual("another_test", relation.Members[1].Role);
            Assert.AreEqual(OsmGeoType.Way, relation.Members[1].Type);
            Assert.AreEqual(3, relation.Members[2].Id);
            Assert.AreEqual("yet_another_test", relation.Members[2].Role);
            Assert.AreEqual(OsmGeoType.Relation, relation.Members[2].Type);

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
            api.Capabilities = new Capabilities()
            {
                Version = new Version()
                {
                    Maximum = 0.6,
                    Minimum = 0.6
                },
                Area = new Area()
                {
                    Maximum = 0.25
                },
                Changesets = new OsmSharp.API.Changesets()
                {
                    MaximumElements = 50000
                },
                Status = new Status()
                {
                    Api = "online",
                    Database = "online",
                    Gpx = "online"
                },
                Timeout = new Timeout()
                {
                    Seconds = 300
                },
                Tracepoints = new Tracepoints()
                {
                    PerPage = 5000
                },
                WayNodes = new WayNodes()
                {
                    Maximum = 2000
                }
            };
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/xml"));

            var result = browser.Get("/test/api/capabilities", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var osm = result.DeserializeOsmXml();
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.Api);

            result = browser.Get("/test/api/0.6/capabilities", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            osm = result.DeserializeOsmXml();
            Assert.IsNotNull(osm);
            Assert.IsNotNull(osm.Api);
        }

        /// <summary>
        /// Tests get permissions.
        /// </summary>
        [Test]
        public void TestGetPermissions()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

            var result = browser.Put("/test/api/0.6/changeset/create", with =>
            {
                with.HttpRequest();
                with.OsmXmlBody(new Osm()
                {
                    Changesets = new Changeset[]
                    {
                        new Changeset()
                        {
                            Tags = new Tags.TagsCollection(new Tags.Tag[]
                            {
                                new Tags.Tag()
                                {
                                    Key = "test_key",
                                    Value = "test_value"
                                }
                            })
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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

            var result = browser.Put("/notatest/api/0.6/changeset/1/close", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            result = browser.Put("/test/api/0.6/changeset/1/close", with =>
            {
                with.HttpRequest();
            });

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        /// <summary>
        /// Tests get changeset download.
        /// </summary>
        [Test]
        public void TestGetChangesetDownload()
        {
            var api = new Mocks.MockApiInstance(null, null, null);
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(cfg =>
            {
                cfg.Module<ApiModule>();
                cfg.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new Authentication.UserIdentity { UserName = "Bob" };
                });
            }, to =>
            {
                to.Accept("application/xml");
                to.BasicAuth("demo", "demo");
            });

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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
            ApiBootstrapper.GetInstance = (name) =>
            {
                if (name == "test")
                {
                    return api;
                }
                return null;
            };

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