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
            var osm = result.DeserializeOsm();
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
            var osm = result.DeserializeOsm();
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
            var osm = result.DeserializeOsm();
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
    }
}
