﻿// The MIT License (MIT)

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
using Nancy.Responses.Negotiation;
using OsmSharp.Osm.Xml.v0_6;
using System;
using System.Collections.Generic;

namespace OsmSharp.Osm.API.Responses
{
    /// <summary>
    /// A response processor to format OSM-XML.
    /// </summary>
    public class ResponseProcessor : IResponseProcessor
    {
        private static readonly IEnumerable<Tuple<string, MediaRange>> extensionMappings =
            new[] { new Tuple<string, MediaRange>("xml", new MediaRange("application/xml")) };

        /// <summary>
        /// Creates a new OSM-XML repsonse processor.
        /// </summary>
        public ResponseProcessor()
        {

        }

        /// <summary>
        /// Gets a set of mappings that map a given extension (such as .json)
        /// to a media range that can be sent to the client in a vary header.
        /// </summary>
        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get { return extensionMappings; }
        }

        /// <summary>
        /// Determines whether the the processor can handle a given content type and model
        /// </summary>
        /// <param name="requestedMediaRange">Content type requested by the client</param>
        /// <param name="model">The model for the given media range</param>
        /// <param name="context">The nancy context</param>
        /// <returns>A ProcessorMatch result that determines the priority of the processor</returns>
        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (model is osm)
            {
                if (IsExactXmlContentType(requestedMediaRange))
                {
                    return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.ExactMatch
                    };
                }

                if (IsWildcardXmlContentType(requestedMediaRange))
                {
                    return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.NonExactMatch
                    };
                }
            }
            if (model is diffResult)
            {
                if (IsExactXmlContentType(requestedMediaRange))
                {
                    return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.ExactMatch
                    };
                }

                if (IsWildcardXmlContentType(requestedMediaRange))
                {
                    return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.NonExactMatch
                    };
                }
            }
            return new ProcessorMatch
            {
                ModelResult = MatchResult.DontCare,
                RequestedContentTypeResult = MatchResult.NoMatch
            };
        }

        /// <summary>
        /// Process the response
        /// </summary>
        /// <param name="requestedMediaRange">Content type requested by the client</param>
        /// <param name="model">The model for the given media range</param>
        /// <param name="context">The nancy context</param>
        /// <returns>A response</returns>
        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (model is osm)
            {
                return new OsmXmlResponse(model);
            }
            if (model is diffResult)
            {
                return new DiffResultXmlResponse(model);
            }
            throw new ArgumentOutOfRangeException("OsmXmlResponseProcessor can only process osm-objects.");
        }

        private static bool IsExactXmlContentType(MediaRange requestedContentType)
        {
            //if (requestedContentType.Type.IsWildcard && requestedContentType.Subtype.IsWildcard)
            //{
                return true;
            //}

            //return requestedContentType.Matches("application/xml") || requestedContentType.Matches("text/xml");
        }

        private static bool IsWildcardXmlContentType(MediaRange requestedContentType)
        {
            if (!requestedContentType.Type.IsWildcard && !string.Equals("application", 
                requestedContentType.Type, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (requestedContentType.Subtype.IsWildcard)
            {
                return true;
            }

            var subtypeString = requestedContentType.Subtype.ToString();

            return (subtypeString.StartsWith("vnd", StringComparison.InvariantCultureIgnoreCase) &&
                    subtypeString.EndsWith("+xml", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
