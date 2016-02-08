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
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace OsmSharp.API.Responses
{
    /// <summary>
    /// An OSM XML response.
    /// </summary>
    public class OsmXmlResponse : Response
    {
        /// <summary>
        /// Holds the default content type.
        /// </summary>
        private static string DefaultContentType
        {
            get
            {
                return "application/xml" + "; charset=UTF8";
            }
        }

        /// <summary>
        /// Creates a new OSM XML response.
        /// </summary>
        public OsmXmlResponse(Osm model)
        {
            this.Contents = model == null ? NoBody : GetXmlContents(model);
            this.ContentType = DefaultContentType;
            this.StatusCode = HttpStatusCode.OK;
        }

        /// <summary>
        /// Gets a stream for a given feature collection.
        /// </summary>
        /// <returns></returns>
        private static Action<Stream> GetXmlContents(Osm model)
        {
            return stream =>
            {
                var emptyNamespace = new XmlSerializerNamespaces();
                emptyNamespace.Add(String.Empty, String.Empty);
                using (var writer = XmlWriter.Create(stream, OsmXmlResponse.XmlWriterSettings))
                {
                    OsmXmlResponse.Serializer.Serialize(writer, model, emptyNamespace);
                }
            };
        }

        private static XmlSerializer Serializer = new XmlSerializer(typeof(Osm), string.Empty);

        /// <summary>
        /// The default xml writer settings.
        /// </summary>
        private static XmlWriterSettings XmlWriterSettings = new XmlWriterSettings()
        {
            Encoding = new System.Text.UTF8Encoding(false),
            Indent = false,
            NewLineOnAttributes = false,
            NewLineHandling= NewLineHandling.None
        };
    }
}