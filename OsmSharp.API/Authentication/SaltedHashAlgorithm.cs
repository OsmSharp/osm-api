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
using System.Security.Cryptography;

namespace OsmSharp.API.Authentication
{
    /// <summary>
    /// A double-salted hashing algorithm.
    /// </summary>
    public static class SaltedHashAlgorithm
    {
        /// <summary>
        /// Keeps the username salt.
        /// </summary>
        public static string GlobalSalt = string.Empty;

        /// <summary>
        /// Creates a salt based on the global salt for the given string.
        /// </summary>
        public static string CreateSalt(this string value)
        {
            if(value == null) { throw new ArgumentNullException("value"); }
            if(GlobalSalt == null) { throw new Exception("Global salt value not set."); }

            return value.HashWithSalt(GlobalSalt);
        }

        /// <summary>
        /// Hashes the given value using the given salt.
        /// </summary>
        public static string HashWithSalt(this string value, string salt)
        {
            if (value == null) { throw new ArgumentNullException("value"); }
            if (salt == null) { throw new ArgumentNullException("salt"); }

            var hasher = new Rfc2898DeriveBytes(value,
                System.Text.Encoding.Default.GetBytes(salt), 10000);
            return Convert.ToBase64String(hasher.GetBytes(25));
        }

        /// <summary>
        /// Hashes the given password using the username to generate a salt.
        /// </summary>
        public static string HashPassword(string username, string password)
        {
            if (username == null) { throw new ArgumentNullException("username"); }
            if (password == null) { throw new ArgumentNullException("password"); }

            var salt = username.CreateSalt();
            return password.HashWithSalt(salt);
        }
    }
}