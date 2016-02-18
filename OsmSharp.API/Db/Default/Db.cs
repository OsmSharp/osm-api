﻿// The MIT License (MIT)

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

using OsmSharp.Db;
using System.Collections.Generic;
using OsmSharp.Changesets;
using OsmSharp.Streams;

namespace OsmSharp.API.Db.Default
{
    /// <summary>
    /// A default db implementation that is based on an OsmSharp IDataSource-implementation and a IUserDb implementation.
    /// </summary>
    public class Db : IDb
    {
        private readonly IHistoryDb _dataSource;
        private readonly IUserDb _userDb;

        /// <summary>
        /// Creates a new db.
        /// </summary>
        public Db(IHistoryDb dataSource, IUserDb userDb)
        {
            _dataSource = dataSource;
            _userDb = userDb;
        }

        /// <summary>
        /// Clears all data from this db.
        /// </summary>
        public void Clear()
        {
            _dataSource.Clear();
        }

        /// <summary>
        /// Adds the given osm object in the db exactly as given.
        /// </summary>
        public void Add(OsmGeo osmGeo)
        {
            _dataSource.Add(osmGeo);
        }

        /// <summary>
        /// Adds the given osm objects in the db exactly as given.
        /// </summary>
        public void Add(IEnumerable<OsmGeo> osmGeos)
        {
            _dataSource.Add(osmGeos);
        }

        /// <summary>
        /// Adds the given changeset in the Db exactly as given.
        /// </summary>
        public void Add(Changeset meta, OsmChange changes)
        {
            _dataSource.Add(meta, changes);
        }

        /// <summary>
        /// Gets all the objects in the form of a stream source.
        /// </summary>
        /// <returns></returns>
        public OsmStreamSource Get()
        {
            return _dataSource.Get();
        }

        /// <summary>
        /// Gets all latest versions of osm objects with the given types and the given id's.
        /// </summary>
        public IList<OsmGeo> Get(OsmGeoType type, IList<long> id)
        {
            return _dataSource.Get(type, id);
        }

        /// <summary>
        /// Gets an osm object of the given type, the given id and the given version #.
        /// </summary>
        public OsmGeo Get(OsmGeoType type, long id, int version)
        {
            return _dataSource.Get(type, id, version);
        }

        /// <summary>
        /// Gets all osm objects with the given types, the given id's and the given version #'s.
        /// </summary>
        public IList<OsmGeo> Get(OsmGeoType type, IList<long> id, IList<int> version)
        {
            return _dataSource.Get(type, id, version);
        }

        /// <summary>
        /// Gets all latest versions of osm objects within the given bounding box.
        /// </summary>
        public IEnumerable<OsmGeo> Get(float minLatitude, float minLongitude, float maxLatitude, float maxLongitude)
        {
            return _dataSource.Get(minLatitude, minLongitude, maxLatitude, maxLongitude);
        }

        /// <summary>
        /// Opens a new changeset.
        /// </summary>
        public long OpenChangeset(Changeset info)
        {
            return _dataSource.OpenChangeset(info);
        }

        /// <summary>
        /// Applies the given changeset.
        /// </summary>
        public DiffResultResult ApplyChangeset(long id, OsmChange changeset, bool bestEffort = true)
        {
            return _dataSource.ApplyChangeset(id, changeset, bestEffort);
        }

        /// <summary>
        /// Updates the changeset with the new info.
        /// </summary>
        public bool UpdateChangesetInfo(Changeset info)
        {
            return _dataSource.UpdateChangesetInfo(info);
        }

        /// <summary>
        /// Closes the changeset with the given id.
        /// </summary>
        public bool CloseChangeset(long id)
        {
            return _dataSource.CloseChangeset(id);
        }

        /// <summary>
        /// Returns an OsmGeo obejct with the given id and type from this source.
        /// </summary>
        public OsmGeo Get(OsmGeoType type, long id)
        {
            return _dataSource.Get(type, id);
        }

        /// <summary>
        /// Gets the user with the given id.
        /// </summary>
        public User GetUser(long id)
        {
            return _userDb.GetUser(id);
        }

        /// <summary>
        /// Gets the user with the given name.
        /// </summary>
        public User GetUserByName(string username)
        {
            return _userDb.GetUserByName(username);
        }

        /// <summary>
        /// Gets the password hash for the given user.
        /// </summary>
        public string GetUserPasswordHash(long id)
        {
            return _userDb.GetUserPasswordHash(id);
        }

        /// <summary>
        /// Sets the password hash for the given user.
        /// </summary>
        public bool SetUserPasswordHash(long id, string hash)
        {
            return _userDb.SetUserPasswordHash(id, hash);
        }
    }
}