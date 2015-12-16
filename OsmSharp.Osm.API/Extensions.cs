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

using OsmSharp.Osm.API.Db.Domain;
using OsmSharp.Osm.Xml.v0_6;

namespace OsmSharp.Osm.API
{
    /// <summary>
    /// Contains extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts a database user to an xml user.
        /// </summary>
        public static user ToXmlUser(this User user)
        {
            var xmlUser = new user();
            xmlUser.id = user.Id;
            xmlUser.account_created = user.AccountCreated;
            xmlUser.description = user.Description;
            xmlUser.display_name = user.DisplayName;
            if (user.BlocksReceived != null)
            {
                xmlUser.blocks = new block[user.BlocksReceived.Length];
                for(var i = 0; i < xmlUser.blocks.Length; i++)
                {
                    xmlUser.blocks[i] = new block()
                    {
                        active = user.BlocksReceived[i].Active,
                        count = user.BlocksReceived[i].Count
                    };
                }
            }
            xmlUser.changesets = new userchangeset()
            {
                count = user.ChangeSetCount
            };
            xmlUser.contributorterms = new contributorterms()
            {
                agreed = user.ContributorTermsAgreed,
                pd = user.ContributorTermsPublicDomain
            };
            xmlUser.img = new img()
            {
                href = user.Image
            };
            if (user.Roles != null)
            {
                xmlUser.roles = new role[user.Roles.Length];
                for (var i = 0; i < xmlUser.roles.Length; i++)
                {
                    xmlUser.roles[i] = new role()
                    {

                    };
                }
            }
            xmlUser.traces = new traces()
            {
                count = user.TraceCount
            };
            return xmlUser;
        }
    }
}