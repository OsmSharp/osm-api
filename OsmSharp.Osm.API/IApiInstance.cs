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

namespace OsmSharp.Osm.API
{
    /// <summary>
    /// Abstract representation of an API instance.
    /// </summary>
    public interface IApiInstance
    {
        /// <summary>
        /// Gets the api capabilities.
        /// </summary>
        /// <returns></returns>
        ApiResult<osm> GetCapabilities();

        /// <summary>
        /// Gets all objects within the given bounding box.
        /// </summary>
        /// <returns></returns>
        ApiResult<osm> GetMap(double left, double bottom, double right, double top);

        /// <summary>
        /// Gets the node with the given id.
        /// </summary>
        ApiResult<osm> GetNode(long id);

        /// <summary>
        /// Gets the way with the given id.
        /// </summary>
        ApiResult<osm> GetWay(long id);

        /// <summary>
        /// Gets the relation with the given id.
        /// </summary>
        ApiResult<osm> GetRelation(long id);

        /// <summary>
        /// Gets the user with the given id.
        /// </summary>
        ApiResult<osm> GetUser(long id);

        /// <summary>
        /// Opens a new changeset.
        /// </summary>
        ApiResult<long> CreateChangeset(changeset changeset);

        /// <summary>
        /// Applies a changeset.
        /// </summary>
        ApiResult<diffResult> ApplyChangeset(long id, osmChange osmChange);

        /// <summary>
        /// Closes the given changeset.
        /// </summary>
        ApiResult<bool> CloseChangeset(long id);

        /// <summary>
        /// Validates a changeset against the current state of the data.
        /// </summary>
        ApiResult<bool> ValidateChangeset(osmChange osmChange);

        /// <summary>
        /// Validates a user and returns it's user id. When user is not found, returns -1.
        /// </summary>
        ApiResult<long> ValidateUser(string username, string password);
    }
}