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

using OsmSharp.Osm.API.Db;
using System;

namespace OsmSharp.Osm.API
{
    /// <summary>
    /// A default api instance.
    /// </summary>
    public class DefaultApiInstance : IApiInstance
    {
        private readonly IDb _db;

        /// <summary>
        /// Creates a new api instance.
        /// </summary>
        public DefaultApiInstance(IDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Gets the node with the given id.
        /// </summary>
        public Node GetNode(long id)
        {
            return _db.GetNode(id);
        }

        /// <summary>
        /// Gets the way with the 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Way GetWay(long id)
        {
            throw new NotImplementedException();
        }

        public Relation GetRelation(long id)
        {
            throw new NotImplementedException();
        }
    }
}
