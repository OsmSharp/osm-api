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

using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using OsmSharp.API.Authentication;
using OsmSharp.API.Authentication.Basic;
using OsmSharp.Changesets;
using System;
using System.Collections.Generic;

namespace OsmSharp.API
{
    /// <summary>
    /// Implements a copy of the OSM-API v0.6 according to the documentation found here:
    /// 
    /// http://wiki.openstreetmap.org/wiki/API_v0.6
    /// </summary>
    public class ApiModule : NancyModule
    {
        private static OsmSharp.Logging.Logger _logger = new OsmSharp.Logging.Logger("apimodule");

        /// <summary>
        /// Creates a new api module.
        /// </summary>
        public ApiModule()
        {
			this.SetupAuthentication();

            Get["{instance}/api/capabilities"] = _ => { return this.GetCapabilities(_); };
            Get["{instance}/api/0.6/capabilities"] = _ => { return this.GetCapabilities(_); };
            Get["{instance}/api/0.6/map"] = _ => { return this.GetMap(_); };
            Get["{instance}/api/0.6/permissions"] = _ => { return this.GetPermissions(_); };
            Put["{instance}/api/0.6/changeset/create"] = _ => { return this.PutChangesetCreate(_); };
            Put["{instance}/api/0.6/changeset/{changesetid}/close"] = _ => { return this.PutChangesetClose(_); };
            Get["{instance}/api/0.6/changeset/{changesetid}"] = _ => { return this.GetChangeset(_); };
            Put["{instance}/api/0.6/changeset/{changesetid}"] = _ => { return this.PutChangesetUpdate(_); };
            Get["{instance}/api/0.6/changeset/{changesetid}/download"] = _ => { return this.GetChangesetDownload(_); };
            Post["{instance}/api/0.6/changeset/{changesetid}/expand_bbox"] = _ => { return this.PostChangesetExpandBB(_); };
            Get["{instance}/api/0.6/changesets"] = _ => { return this.GetChangesetQuery(_); };
            Post["{instance}/api/0.6/changeset/{changesetid}/upload"] = _ => { return this.PostChangesetUpload(_); };
            Put["{instance}/api/0.6/[node|way|relation]/create"] = _ => { return this.PutElementCreate(_); };
            Get["{instance}/api/0.6/node/{elementid}"] = _ => { return this.GetElement(_, OsmGeoType.Node); };
			Get["{instance}/api/0.6/nodes"] = _ => { return this.GetElements(_, OsmGeoType.Node); };
            Get["{instance}/api/0.6/way/{elementid}"] = _ => { return this.GetElement(_, OsmGeoType.Way); };
			Get["{instance}/api/0.6/ways"] = _ => { return this.GetElements(_, OsmGeoType.Way); };
            Get["{instance}/api/0.6/relation/{elementid}"] = _ => { return this.GetElement(_, OsmGeoType.Relation); };
			Get["{instance}/api/0.6/relations"] = _ => { return this.GetElements(_, OsmGeoType.Relation); };
            Put["{instance}/api/0.6/node/{elementid}"] = _ => { return this.PutElementUpdate(_); };
            Put["{instance}/api/0.6/way/{elementid}"] = _ => { return this.PutElementUpdate(_); };
            Put["{instance}/api/0.6/relation/{elementid}"] = _ => { return this.PutElementUpdate(_); };
            Delete["{instance}/api/0.6/node/{elementid}"] = _ => { return this.DeleteElement(_); };
            Delete["{instance}/api/0.6/way/{elementid}"] = _ => { return this.DeleteElement(_); };
            Delete["{instance}/api/0.6/relation/{elementid}"] = _ => { return this.DeleteElement(_); };
            Get["{instance}/api/0.6/node/{elementid}/history"] = _ => { return this.GetElementHistory(_); };
            Get["{instance}/api/0.6/way/{elementid}/history"] = _ => { return this.GetElementHistory(_); };
            Get["{instance}/api/0.6/relation/{elementid}/history"] = _ => { return this.GetElementHistory(_); };
            Get["{instance}/api/0.6/node/{elementid}/{version}"] = _ => { return this.GetElementVersion(_); };
            Get["{instance}/api/0.6/way/{elementid}/{version}"] = _ => { return this.GetElementVersion(_); };
            Get["{instance}/api/0.6/relation/{elementid}/{version}"] = _ => { return this.GetElementVersion(_); };
            Get["{instance}/api/0.6/node/{elementid}/relations"] = _ => { return this.GetElementRelations(_); };
            Get["{instance}/api/0.6/way/{elementid}/relations"] = _ => { return this.GetElementRelations(_); };
            Get["{instance}/api/0.6/relation/{elementid}/relations"] = _ => { return this.GetElementRelations(_); };
            Get["{instance}/api/0.6/node/{elementid}/full"] = _ => { return this.GetElementFull(_); };
            Get["{instance}/api/0.6/way/{elementid}/full"] = _ => { return this.GetElementFull(_); };
            Get["{instance}/api/0.6/relation/{elementid}/full"] = _ => { return this.GetElementFull(_); };
            Get["{instance}/api/0.6/node/{id}/ways"] = _ => { return this.GetWaysForNode(_); };
            Get["{instance}/api/0.6/map"] = _ => { return this.GetMap(_); };
            Get["{instance}/api/0.6/user/{id}"] = _ => { return this.GetUserDetails(_); };
            Get["{instance}/api/0.6/user/details"] = _ => { return this.GetCurrentUserDetails(_); };
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
                _logger.Log(Logging.TraceEventType.Verbose, "GetCapabilities");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return this.BuildResponse(instance.GetCapabilities());
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                _logger.Log(Logging.TraceEventType.Verbose, "GetMap");

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

                float left, bottom = 0, right = 0, top = 0;
                if(!float.TryParse(boundsStrings[0], System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out left) ||
                   !float.TryParse(boundsStrings[1], System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out bottom) ||
                   !float.TryParse(boundsStrings[2], System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out right) ||
                   !float.TryParse(boundsStrings[3], System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out top))
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.BadRequest);
                }

                return this.BuildResponse(instance.GetMap(left, bottom, right, top));
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                _logger.Log(Logging.TraceEventType.Verbose, "GetPermissions");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                this.RequiresAuthentication();
                _logger.Log(Logging.TraceEventType.Verbose, "PutChangesetCreate");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                var osm = this.Bind<Osm>();
                
                if (osm == null ||
                    osm.Changesets == null ||
                    osm.Changesets.Length < 1 ||
                    osm.Changesets[0] == null)
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError).WithModel(
                        "No data or body could not be parsed.");
                }

                // get user.
                UserIdentity user = null;
                if (this.Context.CurrentUser == null ||
                  !(this.Context.CurrentUser is UserIdentity))
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError).WithModel(
                        "Current authenticated user could not be identified.");
                }
                user = this.Context.CurrentUser as UserIdentity;

                // add user and time.
                var changeset = osm.Changesets[0];
                changeset.UserId = user.UserId;
                changeset.UserName = user.UserName;
                changeset.Open = true;
                changeset.CreatedAt = DateTime.Now.ToUniversalTime();

                var id = instance.CreateChangeset(changeset);
                if (id.IsError)
                {
                    return this.BuildResponse(id);
                }
                return id.Data.ToInvariantString();
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                _logger.Log(Logging.TraceEventType.Verbose, "GetChangeset");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }
                
                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                this.RequiresAuthentication();
                _logger.Log(Logging.TraceEventType.Verbose, "PutChangesetUpdate");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                this.RequiresAuthentication();
                _logger.Log(Logging.TraceEventType.Verbose, "PutChangesetClose");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                var result = instance.CloseChangeset((long)_.changesetid);
                if(result.IsError)
                {
                    return this.BuildResponse(result);
                }
                return string.Empty;
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                _logger.Log(Logging.TraceEventType.Verbose, "GetChangesetDownload");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                this.RequiresAuthentication();
                _logger.Log(Logging.TraceEventType.Verbose, "PostChangesetExpandBB");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                _logger.Log(Logging.TraceEventType.Verbose, "GetChangesetQuery");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return new Osm()
                {
                    Version = 0.6,
                    Generator = "OsmSharp.API",
                    Changesets = new Changeset[] { 
                        new Changeset()
                        {
                            Id = 1
                        }
                    }
                };
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                this.RequiresAuthentication();
                _logger.Log(Logging.TraceEventType.Verbose, "PostChangesetUpload");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                // get user.
                UserIdentity user = null;
                if (this.Context.CurrentUser == null ||
                  !(this.Context.CurrentUser is UserIdentity))
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError).WithModel(
                        "Current authenticated user could not be identified.");
                }
                user = this.Context.CurrentUser as UserIdentity;

                var osmChange = this.Bind<OsmChange>();

                // prepare changeset.
                if (osmChange.Modify != null)
                {
                    foreach(var modify in osmChange.Modify)
                    {
                        modify.TimeStamp = DateTime.Now.ToUniversalTime();
                        modify.UserId = user.UserId;
                        modify.UserName = user.UserName;
                        modify.Visible = true;
                    }
                }
                if (osmChange.Create != null)
                {
                    foreach (var create in osmChange.Create)
                    {
                        create.TimeStamp = DateTime.Now.ToUniversalTime();
                        create.UserId = user.UserId;
                        create.UserName = user.UserName;
                        create.Visible = true;
                    }
                }

                return this.BuildResponse(instance.ApplyChangeset((long)_.changesetid, osmChange));
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                this.RequiresAuthentication();
                _logger.Log(Logging.TraceEventType.Verbose, "PutElementCreate");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Read: GET /api/0.6/[node|way|relation]/#id
        /// </summary>
        private dynamic GetElement(dynamic _, OsmGeoType type)
        {
            try
            {
                this.EnableCors();
                _logger.Log(Logging.TraceEventType.Verbose, "GetElement");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                // get id.
                long id;
                if (!long.TryParse(_.elementid, out id))
                { // id could not be parsed.
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                ApiResult<Osm> result = null;
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

                return this.BuildResponse(result);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }

		/// <summary>
		/// Read: GET /api/0.6/[nodes|ways|relations]?[nodes|ways|relations]=(ids)
		/// </summary>
		private dynamic GetElements(dynamic _, OsmGeoType type)
		{
			try
			{
				this.EnableCors();
				_logger.Log(Logging.TraceEventType.Verbose, "GetElements");

				// get instance and check if active.
				IApiInstance instance;
				if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
				{ // oeps, instance not active!
					return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
				}

				var idParam = string.Empty;
				switch (type)
				{
					case OsmGeoType.Node:
						idParam = this.Request.Query["nodes"].ToInvariantString();
						break;
					case OsmGeoType.Way:
						idParam = this.Request.Query["ways"].ToInvariantString();
						break;
					case OsmGeoType.Relation:
						idParam = this.Request.Query["relations"].ToInvariantString();
						break;
				}

				var idParamSplit = idParam.Split(',');
				var ids = new long[idParamSplit.Length];
				for (var i = 0; i < ids.Length; i++)
				{
					ids[i] = long.Parse(idParamSplit[i]);
				}

				ApiResult<Osm> result = null;
				switch (type)
				{
					case OsmGeoType.Node:
						result = instance.GetNodes(ids);
						break;
					case OsmGeoType.Way:
						result = instance.GetWays(ids);
						break;
					case OsmGeoType.Relation:
						result = instance.GetRelations(ids);
						break;
				}

				return this.BuildResponse(result);
			}
			catch (Exception ex)
			{ // an unhandled exception!
				_logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                this.RequiresAuthentication();
                _logger.Log(Logging.TraceEventType.Verbose, "PutElementUpdate");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                this.RequiresAuthentication();
                _logger.Log(Logging.TraceEventType.Verbose, "DeleteElement");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                _logger.Log(Logging.TraceEventType.Verbose, "GetElementHistory");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                _logger.Log(Logging.TraceEventType.Verbose, "GetElementVersion");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                _logger.Log(Logging.TraceEventType.Verbose, "GetElementRelations");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                _logger.Log(Logging.TraceEventType.Verbose, "GetWaysForNode");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                _logger.Log(Logging.TraceEventType.Verbose, "GetElementFull");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                _logger.Log(Logging.TraceEventType.Verbose, "GetUserDetails");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                // get id.
                long id;
                if (!long.TryParse(_.id, out id))
                { // id was not parsed.
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return this.BuildResponse(instance.GetUser(id));
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                this.RequiresAuthentication();
                _logger.Log(Logging.TraceEventType.Verbose, "GetCurrentUserDetails");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
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
                this.RequiresAuthentication();
                _logger.Log(Logging.TraceEventType.Verbose, "GetCurrentUserPreferences");

                // get instance and check if active.
                IApiInstance instance;
                if (!ApiBootstrapper.TryGetInstance(_.instance, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                return Negotiate.WithStatusCode(HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            { // an unhandled exception!
                _logger.Log(Logging.TraceEventType.Error, ex.ToInvariantString());
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }
    }
}