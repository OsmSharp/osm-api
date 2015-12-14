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

using System.Linq;
using System.Collections.Generic;

namespace OsmSharp.Osm.API.Db
{
    /// <summary>
    /// An in-memory db.
    /// </summary>
    public class MemoryDb : IDb
    {
        private readonly List<Node> _nodes;
        private readonly List<Way> _ways;
        private readonly List<Relation> _relations;

        public MemoryDb()
        {
            _nodes = new List<Node>();
            _ways = new List<Way>();
            _relations = new List<Relation>();
        }

        /// <summary>
        /// Adds new a new node.
        /// </summary>
        public Node AddNewNode(Node node)
        {
            var id = 1L;
            for(var i = 0; i < _nodes.Count; i++)
            {
                if(_nodes[i].Id > id)
                {
                    id = _nodes[i].Id.Value + 1;
                }
            }
            var newNode = new Node()
                {
                    Id = id,
                    ChangeSetId = node.ChangeSetId,
                    Latitude = node.Latitude,
                    Longitude = node.Longitude,
                    Tags = node.Tags,
                    TimeStamp = node.TimeStamp,
                    UserId = node.UserId,
                    UserName = node.UserName,
                    Version = node.Version,
                    Visible = node.Visible
                };
            _nodes.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// Gets the node with the given id.
        /// </summary>
        public Node GetNode(long id)
        {
            return _nodes.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Gets the way with the given id.
        /// </summary>
        public Way GetWay(long id)
        {
            return _ways.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Gets the relation with the given id.
        /// </summary>
        public Relation GetRelation(long id)
        {
            return _relations.FirstOrDefault(x => x.Id == id);
        }
    }
}
