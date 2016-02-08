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

using OsmSharp.Osm.Xml.v0_6;
using System;
using System.Collections.Generic;

namespace OsmSharp.API.Tests.Mocks
{
    /// <summary>
    /// A mock api instance.
    /// </summary>
    class MockApiInstance : IApiInstance
    {
        private List<Node> _nodes;
        private List<Way> _ways;
        private List<Relation> _relations;
        private List<changeset> _changesets;

        public MockApiInstance(IEnumerable<Node> nodes, IEnumerable<Way> ways, IEnumerable<Relation> relations)
        {
            if (nodes != null) { _nodes = new List<Node>(nodes); }
            if (ways != null) { _ways = new List<Way>(ways); }
            if (relations != null) { _relations = new List<Relation>(relations); }

            _changesets = new List<changeset>();
        }

        public ApiResult<diffResult> ApplyChangeset(long id, osmChange osmChange)
        {
            throw new NotImplementedException();
        }

        public ApiResult<long> CreateChangeset(changeset changeset)
        {
            _changesets.Add(changeset);
            return new ApiResult<long>(_changesets.Count);
        }

        public ApiResult<osm> GetCapabilities()
        {
            return new ApiResult<osm>(new osm()
            {
                api = this.Capabilities
            });
        }

        public api Capabilities
        {
            get;
            set;
        }

        public ApiResult<osm> GetMap(double left, double bottom, double right, double top)
        {
            throw new NotImplementedException();
        }

        public ApiResult<osm> GetNode(long id)
        {
            var node = _nodes.Find(x => x.Id == id);
            if(node == null)
            {
                return new ApiResult<osm>(ApiResultStatusCode.NotFound, "Not found.");
            }
            return new ApiResult<osm>(new osm()
            {
                node = new Xml.v0_6.node[]
                {
                    node.ConvertTo()
                }
            });
        }

        public ApiResult<osm> GetRelation(long id)
        {
            var relation = _relations.Find(x => x.Id == id);
            if (relation == null)
            {
                return new ApiResult<osm>(ApiResultStatusCode.NotFound, "Not found.");
            }
            return new ApiResult<osm>(new osm()
            {
                relation = new Xml.v0_6.relation[]
                {
                    relation.ConvertTo()
                }
            });
        }

        public ApiResult<osm> GetUser(long id)
        {
            throw new NotImplementedException();
        }

        public ApiResult<osm> GetWay(long id)
        {
            var way = _ways.Find(x => x.Id == id);
            if (way == null)
            {
                return new ApiResult<osm>(ApiResultStatusCode.NotFound, "Not found.");
            }
            return new ApiResult<osm>(new osm()
            {
                way = new Xml.v0_6.way[]
                {
                    way.ConvertTo()
                }
            });
        }

        public ApiResult<bool> ValidateChangeset(osmChange osmChange)
        {
            throw new NotImplementedException();
        }

        public ApiResult<bool> CloseChangeset(long id)
        {
            throw new NotImplementedException();
        }

        public ApiResult<long> ValidateUser(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
