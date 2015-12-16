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

using Nancy;
using OsmSharp.Osm.Xml.v0_6;
using System;

namespace OsmSharp.Osm.API
{
    /// <summary>
    /// Implements a copy of the OSM-API v0.6 according to the documentation found here:
    /// 
    /// http://wiki.openstreetmap.org/wiki/API_v0.6
    /// </summary>
    public class ApiModule : NancyModule
    {
        /// <summary>
        /// Creates a new api module.
        /// </summary>
        public ApiModule()
        {
            Get["{instance}/api/capabilities"] = _ => { return this.GetCapabilities(_); };
            Get["{instance}/api/0.6/capabilities"] = _ => { return this.GetCapabilities(_); };
            Get["{instance}/api/0.6/map"] = _ => { return this.GetMap(_); };
            Get["{instance}/api/0.6/permissions"] = _ => { return this.GetPermissions(_); };
            Put["{instance}/api/0.6/changeset/create"] = _ => { return this.PutChangesetCreate(_); };
            Get["{instance}/api/0.6/changeset/{changesetid}"] = _ => { return this.GetChangeset(_); };
            Put["{instance}/api/0.6/changeset/{changesetid}"] = _ => { return this.PutChangesetUpdate(_); };
            Get["{instance}/api/0.6/changeset/{changesetid}/download"] = _ => { return this.GetChangesetDownload(_); };
            Post["{instance}/api/0.6/changeset/{changesetid}/expand_bbox"] = _ => { return this.PostChangesetExpandBB(_); };
            Get["{instance}/api/0.6/changesets"] = _ => { return this.GetChangesetQuery(_); };
            Post["{instance}/api/0.6/changeset/{changesetid}/upload"] = _ => { return this.PostChangesetUpload(_); };
            Put["{instance}/api/0.6/[node|way|relation]/create"] = _ => { return this.PutElementCreate(_); };
            Get["{instance}/api/0.6/node/{elementid}"] = _ => { return this.GetElement(_, OsmGeoType.Node); };
            Get["{instance}/api/0.6/way/{elementid}"] = _ => { return this.GetElement(_, OsmGeoType.Way); };
            Get["{instance}/api/0.6/relation/{elementid}"] = _ => { return this.GetElement(_, OsmGeoType.Relation); };
            Put["{instance}/api/0.6/node/{elementid}"] = _ => { return this.PutElementUpdate(_); };
            Put["{instance}/api/0.6/way/{elementid}"] = _ => { return this.PutElementUpdate(_); };
            Put["{instance}/api/0.6/relation/{elementid}"] = _ => { return this.PutElementUpdate(_); };
            Delete["{instance}/api/0.6/[node|way|relation]/{elementid}"] = _ => { return this.DeleteElement(_); };
            Get["{instance}/api/0.6/[node|way|relation]/{elementid}/history"] = _ => { return this.GetElementHistory(_); };
            Get["{instance}/api/0.6/[node|way|relation]/{elementid}/{version}"] = _ => { return this.GetElementVersion(_); };
            Get["{instance}/api/0.6/[node|way|relation]"] = _ => { return this.GetElementMultiple(_); };
            Get["{instance}/api/0.6/[node|way|relation]/{elementid}/relations"] = _ => { return this.GetElementFull(_); };
            Get["{instance}/api/0.6/node/{id}/ways"] = _ => { return this.GetWaysForNode(_); };
            Get["{instance}/api/0.6/map"] = _ => { return this.GetMap(_); };
            Get["{instance}/api/0.6/map"] = _ => { return this.GetMap(_); };
        }

        /// <summary>
        /// This API call is meant to provide information about the capabilities and limitations of the current API. 
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetCapabilities(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return instance.GetCapabilities();
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Retrieving map data by bounding box: GET /api/0.6/map
        /// </summary>
        /// <param name="_"></param>
        /// <returns>
        /// - All nodes that are inside a given bounding box and any relations that reference them.
        /// - All ways that reference at least one node that is inside a given bounding box, any relations that reference them [the ways], and any nodes outside the bounding box that the ways may reference.
        /// - All relations that reference one of the nodes, ways or relations included due to the above rules. (Does not apply recursively, see explanation below.)
        /// </returns>
        private dynamic GetMap(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                // try to parse bounds.
                var boundsString = (string)this.Request.Query["bbox"];
                if(string.IsNullOrWhiteSpace(boundsString))
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.BadRequest);
                }
                var boundsStrings = boundsString.Split(',');
                if(boundsStrings.Length != 4)
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.BadRequest);
                }

                double left, bottom = 0, right = 0, top = 0;
                if(!double.TryParse(boundsStrings[0], System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out left) ||
                   !double.TryParse(boundsStrings[1], System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out bottom) ||
                   !double.TryParse(boundsStrings[2], System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out right) ||
                   !double.TryParse(boundsStrings[3], System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out top))
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.BadRequest);
                }

                var result = instance.GetMap(left, bottom, right, top);
                if(result == null)
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
                }
                return result;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Retrieving permissions: GET /api/0.6/permissions
        /// </summary>
        /// <param name="_"></param>
        /// <returns> 
        /// - If the API client is not authorized, an empty list of permissions will be returned.
        /// - If the API client uses Basic Auth, the list of permissions will contain all permissions.
        /// - If the API client uses OAuth, the list will contain the permissions actually granted by the user.
        /// </returns>
        private dynamic GetPermissions(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Create: PUT /api/0.6/changeset/create
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic PutChangesetCreate(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Read: GET /api/0.6/changeset/#id?include_discussion=true
        /// </summary>
        /// <param name="_"></param>
        /// <returns>Returns the changeset with the given id in OSM-XML format. </returns>
        private dynamic GetChangeset(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Update: PUT /api/0.6/changeset/#id
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic PutChangesetUpdate(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Close: PUT /api/0.6/changeset/#id/close
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic PutChangesetClose(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Download: GET /api/0.6/changeset/#id/download
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetChangesetDownload(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Expand Bounding Box: POST /api/0.6/changeset/#id/expand_bbox
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic PostChangesetExpandBB(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Query: GET /api/0.6/changesets
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetChangesetQuery(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return new osm()
                {
                    version = 0.6,
                    versionSpecified = true,
                    changeset = new changeset[] { 
                        new changeset()
                        {
                            id = 1
                        }
                    }
                };
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Diff upload: POST /api/0.6/changeset/#id/upload
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic PostChangesetUpload(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Create: PUT /api/0.6/[node|way|relation]/create
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic PutElementCreate(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Read: GET /api/0.6/[node|way|relation]/#id
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetElement(dynamic _, OsmGeoType type)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                // get id.
                long id;
                if (!long.TryParse(_.elementid, out id))
                { // id was parsed.
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                osm result = null;
                switch(type)
                {
                    case OsmGeoType.Node:
                        result = instance.GetNode(id);
                        break;
                    case OsmGeoType.Way:
                        result = instance.GetWay(id);
                        break;
                    case OsmGeoType.Relation:
                        result = instance.GetRelation(id);
                        break;
                }

                if(result == null)
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return result;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Update: PUT /api/0.6/[node|way|relation]/#id
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic PutElementUpdate(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Delete: DELETE /api/0.6/[node|way|relation]/#id
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic DeleteElement(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// History: GET /api/0.6/[node|way|relation]/#id/history
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetElementHistory(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Version: GET /api/0.6/[node|way|relation]/#id/#version
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetElementVersion(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Multi fetch: GET /api/0.6/[nodes|ways|relations]?#parameters
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetElementMultiple(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Relations for element: GET /api/0.6/[node|way|relation]/#id/relations
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetElementRelations(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Ways for node: GET /api/0.6/node/#id/ways
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetWaysForNode(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Full: GET /api/0.6/[way|relation]/#id/full
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private  dynamic GetElementFull(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// GET /api/0.6/user/#id
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetUserDetails(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// GET /api/0.6/user/details
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetCurrentUserDetails(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///  GET /api/0.6/user/preferences
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetCurrentUserPreferences(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return null;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }
    }
}