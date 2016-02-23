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

namespace OsmSharp.API
{
    /// <summary>
    /// Abstract representation of an API instance.
    /// </summary>
    public interface IApiInstance
    {
        /// <summary>
        /// Even raised when there is a change.
        /// </summary>
        event Action<OsmChange> Change;

        /// <summary>
        /// Gets the api capabilities.
        /// </summary>
        ApiResult<Osm> GetCapabilities();

        /// <summary>
        /// Gets all objects within the given bounding box.
        /// </summary>
        ApiResult<Osm> GetMap(float left, float bottom, float right, float top);

        /// <summary>
        /// Gets the node with the given id.
        /// </summary>
        ApiResult<Osm> GetNode(long id);

        /// <summary>
        /// Gets the way with the given id.
        /// </summary>
        ApiResult<Osm> GetWay(long id);

        /// <summary>
        /// Gets the relation with the given id.
        /// </summary>
        ApiResult<Osm> GetRelation(long id);

        /// <summary>
        /// Gets the user with the given id.
        /// </summary>
        ApiResult<Osm> GetUser(long id);

        /// <summary>
        /// Opens a new changeset.
        /// </summary>
        ApiResult<long> CreateChangeset(Changeset changeset);

        /// <summary>
        /// Applies a changeset.
        /// </summary>
        ApiResult<DiffResult> ApplyChangeset(long id, OsmChange osmChange);

        /// <summary>
        /// Closes the given changeset.
        /// </summary>
        ApiResult<bool> CloseChangeset(long id);

        /// <summary>
        /// Validates a user and returns it's user id. When user is not found, returns -1.
        /// </summary>
        ApiResult<long> ValidateUser(string username, string password);
    }
}