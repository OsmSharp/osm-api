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

using OsmSharp.Changesets;
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
        private List<Changeset> _changesets;

        public MockApiInstance(IEnumerable<Node> nodes, IEnumerable<Way> ways, IEnumerable<Relation> relations)
        {
            if (nodes != null) { _nodes = new List<Node>(nodes); }
            if (ways != null) { _ways = new List<Way>(ways); }
            if (relations != null) { _relations = new List<Relation>(relations); }

            _changesets = new List<Changeset>();
        }
        
        public event Action<OsmChange> Change;

        public ApiResult<DiffResult> ApplyChangeset(long id, OsmChange osmChange)
        {
            throw new NotImplementedException();
        }

        public ApiResult<long> CreateChangeset(Changeset changeset)
        {
            _changesets.Add(changeset);
            return new ApiResult<long>(_changesets.Count);
        }

        public ApiResult<Osm> GetCapabilities()
        {
            return new ApiResult<Osm>(new Osm()
            {
                Api = this.Capabilities
            });
        }

        public Capabilities Capabilities
        {
            get;
            set;
        }

        public ApiResult<Osm> GetMap(float left, float bottom, float right, float top)
        {
            throw new NotImplementedException();
        }

        public ApiResult<Osm> GetNode(long id)
        {
            var node = _nodes.Find(x => x.Id == id);
            if(node == null)
            {
                return new ApiResult<Osm>(ApiResultStatusCode.NotFound, "Not found.");
            }
            return new ApiResult<Osm>(new Osm()
            {
                Nodes = new Node[]
                {
                    node
                }
            });
        }

        public ApiResult<Osm> GetRelation(long id)
        {
            var relation = _relations.Find(x => x.Id == id);
            if (relation == null)
            {
                return new ApiResult<Osm>(ApiResultStatusCode.NotFound, "Not found.");
            }
            return new ApiResult<Osm>(new Osm()
            {
                Relations = new Relation[]
                {
                    relation
                }
            });
        }

        public ApiResult<Osm> GetUser(long id)
        {
            throw new NotImplementedException();
        }

        public ApiResult<Osm> GetWay(long id)
        {
            var way = _ways.Find(x => x.Id == id);
            if (way == null)
            {
                return new ApiResult<Osm>(ApiResultStatusCode.NotFound, "Not found.");
            }
            return new ApiResult<Osm>(new Osm()
            {
                Ways = new Way[]
                {
                    way
                }
            });
        }

        public ApiResult<bool> ValidateChangeset(OsmChange osmChange)
        {
            throw new NotImplementedException();
        }

        public ApiResult<bool> CloseChangeset(long id)
        {
            return new ApiResult<bool>(true);
        }

        public ApiResult<long> ValidateUser(string username, string password)
        {
            return new ApiResult<long>(1);
        }
    }
}