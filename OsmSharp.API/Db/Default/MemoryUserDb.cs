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

using System;
using OsmSharp.API;
using OsmSharp.API.Db;
using System.Collections.Generic;

namespace OsmSharp.API.Db.Default
{
    /// <summary>
    /// An in-memory implementation of a user db.
    /// </summary>
    public class MemoryUserDb : IUserDb
    {
        private readonly List<User> _users;
        private readonly List<string> _hashes;

        /// <summary>
        /// Creates a new user db.
        /// </summary>
        public MemoryUserDb()
        {
            _users = new List<User>();
            _hashes = new List<string>();
        }

        /// <summary>
        /// Adds a user.
        /// </summary>
        public void AddUser(User user, string hash)
        {
            _users.Add(user);
            _hashes.Add(hash);
        }

        /// <summary>
        /// Gets the user with the given id.
        /// </summary>
        public User GetUser(long id)
        {
            return _users.Find(x => x.Id == id);
        }

        /// <summary>
        /// Get the user by the given username.
        /// </summary>
        public User GetUserByName(string username)
        {
            return _users.Find(x => x.DisplayName == username);
        }

        /// <summary>
        /// Gets the password hash for the user with the given id.
        /// </summary>
        public string GetUserPasswordHash(long id)
        {
            var i = _users.FindIndex(x => x.Id == id);
            if (i < _hashes.Count)
            {
                return _hashes[i];
            }
            return string.Empty;
        }

        /// <summary>
        /// Sets the password hash for the user with the given id.
        /// </summary>
        public bool SetUserPasswordHash(long id, string hash)
        {
            throw new NotImplementedException();
        }
    }
}
