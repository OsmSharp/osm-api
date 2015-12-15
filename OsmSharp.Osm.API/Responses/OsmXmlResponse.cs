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

using Nancy;
using OsmSharp.Collections.Tags;
using OsmSharp.Osm.API.Domain;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace OsmSharp.Osm.API.Responses
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
        public OsmXmlResponse(osm model)
        {
            this.Contents = model == null ? NoBody : GetXmlContents(model);
            this.ContentType = DefaultContentType;
            this.StatusCode = HttpStatusCode.OK;
        }

        /// <summary>
        /// Gets a stream for a given feature collection.
        /// </summary>
        /// <returns></returns>
        private static Action<Stream> GetXmlContents(osm model)
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

        private static XmlSerializer Serializer = new XmlSerializer(typeof(osm), string.Empty);

        /// <summary>
        /// The default xml writer settings.
        /// </summary>
        private static XmlWriterSettings XmlWriterSettings = new XmlWriterSettings()
        {
            
            Indent = true
        };

        /// <summary>
        /// Adds a node to the output.
        /// </summary>
        private static void AddNode(Stream stream, Node node)
        {
            var nd = new OsmSharp.Osm.Xml.v0_6.node();

            // set id
            nd.idSpecified = false;
            if (node.Id.HasValue)
            {
                nd.id = node.Id.Value;
                nd.idSpecified = true;
            }

            // set changeset.
            nd.changesetSpecified = false;
            if (node.ChangeSetId.HasValue)
            {
                nd.changeset = node.ChangeSetId.Value;
                nd.changesetSpecified = true;
            }

            // set visible.
            nd.visibleSpecified = false;
            if (node.Visible.HasValue)
            {
                nd.visible = node.Visible.Value;
                nd.visibleSpecified = true;
            }

            // set timestamp.
            nd.timestampSpecified = false;
            if (node.TimeStamp.HasValue)
            {
                nd.timestamp = node.TimeStamp.Value;
                nd.timestampSpecified = true;
            }

            // set latitude.
            nd.latSpecified = false;
            if (node.Latitude.HasValue)
            {
                nd.lat = node.Latitude.Value;
                nd.latSpecified = true;
            }

            // set longitude.
            nd.lonSpecified = false;
            if (node.Longitude.HasValue)
            {
                nd.lon = node.Longitude.Value;
                nd.lonSpecified = true;
            }

            // set uid
            nd.uidSpecified = false;
            if (node.UserId.HasValue)
            {
                nd.uid = node.UserId.Value;
                nd.uidSpecified = true;
            }

            // set user
            nd.user = node.UserName;

            // set tags.
            nd.tag = OsmXmlResponse.ConvertToXmlTags(node.Tags);

            // set version.
            if (node.Version.HasValue)
            {
                nd.version = node.Version.Value;
                nd.versionSpecified = true;
            }

            var emptyNamespace = new XmlSerializerNamespaces();
            emptyNamespace.Add(String.Empty, String.Empty);

            // serialize node.
            var memoryStream = new MemoryStream();
            var writer = XmlWriter.Create(memoryStream, OsmXmlResponse.XmlWriterSettings);
            var serNode = new XmlSerializer(typeof(Osm.Xml.v0_6.node), string.Empty);
            serNode.Serialize(writer, nd, emptyNamespace);
            memoryStream.WriteTo(stream);
            stream.Flush();
        }

        /// <summary>
        /// Adds a way to the output.
        /// </summary>
        private static void AddWay(Stream stream, Way way)
        {
            var wa = new OsmSharp.Osm.Xml.v0_6.way();

            wa.idSpecified = false;
            if (way.Id.HasValue)
            {
                wa.idSpecified = true;
                wa.id = way.Id.Value;
            }

            // set changeset.
            wa.changesetSpecified = false;
            if (way.ChangeSetId.HasValue)
            {
                wa.changeset = way.ChangeSetId.Value;
                wa.changesetSpecified = true;
            }

            // set visible.
            wa.visibleSpecified = false;
            if (way.Visible.HasValue)
            {
                wa.visible = way.Visible.Value;
                wa.visibleSpecified = true;
            }

            // set timestamp.
            wa.timestampSpecified = false;
            if (way.TimeStamp.HasValue)
            {
                wa.timestamp = way.TimeStamp.Value;
                wa.timestampSpecified = true;
            }

            // set uid
            wa.uidSpecified = false;
            if (way.UserId.HasValue)
            {
                wa.uid = way.UserId.Value;
                wa.uidSpecified = true;
            }

            // set user
            wa.user = way.UserName;

            // set tags.
            wa.tag = OsmXmlResponse.ConvertToXmlTags(way.Tags);

            // set nodes.
            if (way.Nodes != null)
            {
                wa.nd = new OsmSharp.Osm.Xml.v0_6.nd[way.Nodes.Count];
                for (int idx = 0; idx < way.Nodes.Count; idx++)
                {
                    var nd = new OsmSharp.Osm.Xml.v0_6.nd();
                    nd.refSpecified = true;
                    nd.@ref = way.Nodes[idx];
                    wa.nd[idx] = nd;
                }
            }

            // set version.
            if (way.Version.HasValue)
            {
                wa.version = way.Version.Value;
                wa.versionSpecified = true;
            }

            var emptyNamespace = new XmlSerializerNamespaces();
            emptyNamespace.Add(String.Empty, String.Empty);

            // serialize node.
            var memoryStream = new MemoryStream();
            var writer = XmlWriter.Create(memoryStream, OsmXmlResponse.XmlWriterSettings);
            var serWay = new XmlSerializer(typeof(Osm.Xml.v0_6.way), string.Empty);
            serWay.Serialize(writer, wa, emptyNamespace);
            memoryStream.WriteTo(stream);
        }

        /// <summary>
        /// Adds a relation to the output.
        /// </summary>
        private static void AddRelation(Stream stream, Relation simpleRelation)
        {
            var re = new OsmSharp.Osm.Xml.v0_6.relation();

            re.idSpecified = false;
            if (simpleRelation.Id.HasValue)
            {
                re.idSpecified = true;
                re.id = simpleRelation.Id.Value;
            }

            // set changeset.
            re.changesetSpecified = false;
            if (simpleRelation.ChangeSetId.HasValue)
            {
                re.changeset = simpleRelation.ChangeSetId.Value;
                re.changesetSpecified = true;
            }

            // set visible.
            re.visibleSpecified = false;
            if (simpleRelation.Visible.HasValue)
            {
                re.visible = simpleRelation.Visible.Value;
                re.visibleSpecified = true;
            }

            // set timestamp.
            re.timestampSpecified = false;
            if (simpleRelation.TimeStamp.HasValue)
            {
                re.timestamp = simpleRelation.TimeStamp.Value;
                re.timestampSpecified = true;
            }

            // set uid
            re.uidSpecified = false;
            if (simpleRelation.UserId.HasValue)
            {
                re.uid = simpleRelation.UserId.Value;
                re.uidSpecified = true;
            }

            // set user
            re.user = simpleRelation.UserName;

            // set tags.
            re.tag = OsmXmlResponse.ConvertToXmlTags(simpleRelation.Tags);

            // set members.
            if (simpleRelation.Members != null)
            {
                re.member = new OsmSharp.Osm.Xml.v0_6.member[simpleRelation.Members.Count];
                for (int idx = 0; idx < simpleRelation.Members.Count; idx++)
                {
                    var mem = new OsmSharp.Osm.Xml.v0_6.member();
                    var memberToAdd = simpleRelation.Members[idx];

                    // set memberid
                    mem.refSpecified = false;
                    if (memberToAdd.MemberId.HasValue)
                    {
                        mem.@ref = memberToAdd.MemberId.Value;
                        mem.refSpecified = true;
                    }

                    // set type
                    mem.typeSpecified = false;
                    if (memberToAdd.MemberType.HasValue)
                    {
                        switch (memberToAdd.MemberType.Value)
                        {
                            case OsmGeoType.Node:
                                mem.type = OsmSharp.Osm.Xml.v0_6.memberType.node;
                                break;
                            case OsmGeoType.Way:
                                mem.type = OsmSharp.Osm.Xml.v0_6.memberType.way;
                                break;
                            case OsmGeoType.Relation:
                                mem.type = OsmSharp.Osm.Xml.v0_6.memberType.relation;
                                break;
                        }
                        mem.typeSpecified = true;
                    }

                    mem.role = memberToAdd.MemberRole;

                    re.member[idx] = mem;
                }
            }

            // set version.
            if (simpleRelation.Version.HasValue)
            {
                re.version = simpleRelation.Version.Value;
                re.versionSpecified = true;
            }

            var emptyNamespace = new XmlSerializerNamespaces();
            emptyNamespace.Add(String.Empty, String.Empty);

            // serialize node.
            var memoryStream = new MemoryStream();
            var writer = XmlWriter.Create(memoryStream, OsmXmlResponse.XmlWriterSettings);
            var serRel = new XmlSerializer(typeof(Osm.Xml.v0_6.relation), string.Empty);
            serRel.Serialize(writer, re, emptyNamespace);
            memoryStream.WriteTo(stream);
        }

        /// <summary>
        /// Converts the given tag collection to OSM-XML tags.
        /// </summary>
        private static OsmSharp.Osm.Xml.v0_6.tag[] ConvertToXmlTags(TagsCollectionBase tags)
        {
            if (tags != null)
            {
                var xmlTags = new OsmSharp.Osm.Xml.v0_6.tag[tags.Count];

                int idx = 0;
                foreach (var pair in tags)
                {
                    xmlTags[idx] = new OsmSharp.Osm.Xml.v0_6.tag();
                    xmlTags[idx].k = pair.Key;
                    xmlTags[idx].v = pair.Value;
                    idx++;
                }

                return xmlTags;
            }
            return null;
        }
    }
}