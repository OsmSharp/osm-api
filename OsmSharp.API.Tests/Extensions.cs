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

using Nancy.Testing;
using System.IO;
using System.Xml.Serialization;

namespace OsmSharp.API.Tests
{
    /// <summary>
    /// Contains extension methods as helpers during unittesting.
    /// </summary>
    public static class Extensions
    {
        private static XmlSerializer _osmXmlSerializer = 
            new XmlSerializer(typeof(Osm));

        /// <summary>
        /// Deserializes an osm response from a browser response.
        /// </summary>
        public static Osm DeserializeOsmXml(this BrowserResponse result)
        {
            return _osmXmlSerializer.Deserialize(result.Body.AsStream()) as Osm;
        }

        /// <summary>
        /// Deserializes a text response.
        /// </summary>
        public static string DeserializeAsString(this BrowserResponse result)
        {
            using (var streamReader = new StreamReader(result.Body.AsStream()))
            {
                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Adds the given osm object to the browser context.
        /// </summary>
        public static void OsmXmlBody(this BrowserContext context, Osm osm)
        {
            var stream = new MemoryStream();
            _osmXmlSerializer.Serialize(stream, osm);
            stream.Seek(0, SeekOrigin.Begin);
            context.Body(stream, contentType: "application/xml");
        }
    }
}