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

using Nancy.Hosting.Self;
using OsmSharp.API.Authentication;
using OsmSharp.API.Db.Default;
using OsmSharp.Db;
using OsmSharp.Db.Memory;
using System;
using System.IO;

namespace OsmSharp.API.Selfhost
{
    class Program
    {
        static void Main(string[] args)
        {
            // WARNING: generate something different for your own apps!!
            SaltedHashAlgorithm.GlobalSalt = "K3a@Tb~*ETDczTe]8xpY?7RtbKgz63^5.M#&Db~MwM?!*";

            // Build dummy user db.
            var userDb = new MemoryUserDb();
            var user = new User()
            {
                Id = 1,
                DisplayName = "demo"
            };
            userDb.AddUser(user, SaltedHashAlgorithm.HashPassword(user.DisplayName, "demo"));

            var db = new OsmSharp.API.Db.Default.Db(
                new MemoryHistoryDb(), userDb);

            // add some test-data.
            using (var stream = new FileInfo(@"D:\work\data\OSM\kempen.osm.pbf").OpenRead())
            {
                var source = new OsmSharp.Streams.PBFOsmStreamSource(stream);
                db.Add(source);
            }

            ApiBootstrapper.SetInstance("default", new DefaultApiInstance(db));

            // start listening.
            var uri = new Uri("http://localhost:1234");
            using (var host = new NancyHost(uri))
            {
                host.Start();

                Console.WriteLine("The API is running at " + uri);
                Console.WriteLine("Press [Enter] to close.");
                Console.ReadLine();
            }
        }
    }
}
