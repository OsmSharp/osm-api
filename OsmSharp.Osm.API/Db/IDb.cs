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

using OsmSharp.Math.Geo;
using OsmSharp.Osm.API.Db.Domain;
using System.Collections.Generic;

namespace OsmSharp.Osm.API.Db
{
    /// <summary>
    /// Abstract representation of a database.
    /// </summary>
    public interface IDb
    {
        /// <summary>
        /// Gets all objects inside the given box.
        /// </summary>
        IEnumerable<OsmGeo> GetInsideBox(GeoCoordinateBox geoCoordinateBox);

        /// <summary>
        /// Adds a new node.
        /// </summary>
        Node AddNewNode(Node node);

        /// <summary>
        /// Gets the node with the given id.
        /// </summary>
        Node GetNode(long id);

        /// <summary>
        /// Updates the given node.
        /// </summary>
        bool UpdateNode(Node node);

        /// <summary>
        /// Deletes the node with the given id.
        /// </summary>
        bool DeleteNode(long id);

        /// <summary>
        /// Adds a new way.
        /// </summary>
        Way AddNewWay(Way way);

        /// <summary>
        /// Gets the way with the given id.
        /// </summary>
        Way GetWay(long id);

        /// <summary>
        /// Updates the given way.
        /// </summary>
        bool UpdateWay(Way way);

        /// <summary>
        /// Deletes the way with the given id.
        /// </summary>
        bool DeleteWay(long id);

        /// <summary>
        /// Adds a new relation.
        /// </summary>
        Relation AddNewRelation(Relation relation);

        /// <summary>
        /// Gets the relation with the given id.
        /// </summary>
        Relation GetRelation(long id);

        /// <summary>
        /// Updates the given relation.
        /// </summary>
        bool UpdateRelation(Relation relation);

        /// <summary>
        /// Deletes the relation with the given id.
        /// </summary>
        bool DeleteRelation(long id);

        /// <summary>
        /// Gets the user with the given id.
        /// </summary>
        User GetUser(long id);

        /// <summary>
        /// Gets the user with the given username.
        /// </summary>
        User GetUserByName(string username);

        /// <summary>
        /// Gets the user password hash.
        /// </summary>
        string GetUserPasswordHash(long id);

        /// <summary>
        /// Sets the user password hash.
        /// </summary>
        bool SetUserPasswordHash(long id, string hash);
    }
}