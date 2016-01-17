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

namespace OsmSharp.Osm.API
{
    /// <summary>
    /// Represents a result from an API-call an contains error message to prevent the need to throw exceptions.
    /// </summary>
    public class ApiResult<T>
    {
        private readonly T _data;
        private readonly ApiResultStatusCode _status;
        private readonly string _message;

        /// <summary>
        /// Creates a new api result.
        /// </summary>
        public ApiResult(T data)
        {
            _data = data;
            _message = string.Empty;
        }

        /// <summary>
        /// Creates a new api result.
        /// </summary>
        public ApiResult(ApiResultStatusCode status, string message)
        {
            _status = status;
            _message = message;
            _data = default(T);
        }

        /// <summary>
        /// Converts an error api result to an error api result of another type.
        /// </summary>
        public ApiResult<OtherT> Convert<OtherT>()
        {
            return new ApiResult<OtherT>(this.Status, this.Message);
        }

        /// <summary>
        /// Gets the statuscode.
        /// </summary>
        public ApiResultStatusCode Status
        {
            get
            {
                return _status;
            }
        }

        /// <summary>
        /// Returns true if this result respresent an error.
        /// </summary>
        public bool IsError

        {
            get
            {
                return !string.IsNullOrWhiteSpace(_message);
            }
        }

        /// <summary>
        /// Returns the message.
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        public T Data
        {
            get
            {
                return _data;
            }
        }
    }
    
    /// <summary>
    /// A status code.
    /// </summary>
    public enum ApiResultStatusCode
    {
        /// <summary>
        /// Unknown or unset.
        /// </summary>
        Unknown,
        /// <summary>
        /// Not found.
        /// </summary>
        NotFound,
        /// <summary>
        /// An unhandled error.
        /// </summary>
        Exception
    }
}
