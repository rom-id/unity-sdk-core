﻿/**
* Copyright 2019 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.Connection;
using IBM.Cloud.SDK.Authentication;
using IBM.Cloud.SDK.Authentication.NoAuth;
using System;
using System.Collections.Generic;

namespace IBM.Cloud.SDK
{
    public class BaseService
    {
        protected Authenticator authenticator;
        protected string Url;
        public string ServiceId { get; set; }
        protected Dictionary<string, string> customRequestHeaders = new Dictionary<string, string>();
        public static string PropNameServiceUrl = "URL";
        public static string propnameDisableSsl = "DISABLE_SSL";
        private const string ErrorMessageNoAuthenticator = "Authentication information was not properly configured.";
        public BaseService(string serviceId)
        {
            authenticator = ConfigBasedAuthenticatorFactory.GetAuthenticator(serviceId);
        }

        public BaseService(string versionDate, string serviceId) : this(serviceId) { }

        public BaseService(string versionDate, Authenticator authenticator, string serviceId) : this(authenticator, serviceId) { }

        public BaseService(Authenticator authenticator, string serviceId) {
            ServiceId = serviceId;

            this.authenticator = authenticator ?? throw new ArgumentNullException(ErrorMessageNoAuthenticator);

            // Try to retrieve the service URL from either a credential file, environment, or VCAP_SERVICES.
            Dictionary<string, string> props = CredentialUtils.GetServiceProperties(serviceId);
            props.TryGetValue(PropNameServiceUrl, out string url);
            if (!string.IsNullOrEmpty(url))
            {
                SetEndpoint(url);
            }

            // Check to see if "disable ssl" was set in the service properties.
            // bool disableSsl = false;
            // props.TryGetValue(PropNameServiceDisableSslVerification, out string tempDisableSsl);
            // if (!string.IsNullOrEmpty(tempDisableSsl))
            // {
            //     bool.TryParse(tempDisableSsl, out disableSsl);
            // }

            // DisableSslVerification(disableSsl);
        }

        protected BaseService(string versionDate, string serviceId, string url)
        {
            ServiceId = serviceId;
            authenticator = new NoAuthAuthenticator();

            if (!string.IsNullOrEmpty(url))
                Url = url;
        }

        protected void SetAuthentication(RESTConnector connector)
        {
            if (authenticator != null)
            {
                authenticator.Authenticate(connector);
            }
            else
            {
                throw new ArgumentException("Authentication information was not properly configured.");
            }
        }

        public void SetEndpoint(string url)
        {
            Url = url;
        }

        /// <summary>
        /// Returns the authenticator for the service.
        /// </summary>
        public Authenticator GetAuthenticator()
        {
            return authenticator;
        }

        public void WithHeader(string name, string value)
        {
            if (!customRequestHeaders.ContainsKey(name))
            {
                customRequestHeaders.Add(name, value);
            }
            else
            {
                customRequestHeaders[name] = value;
            }
        }

        public void WithHeaders(Dictionary<string, string> headers)
        {
            foreach (KeyValuePair<string, string> kvp in headers)
            {
                if (!customRequestHeaders.ContainsKey(kvp.Key))
                {
                    customRequestHeaders.Add(kvp.Key, kvp.Value);
                }
                else
                {
                    customRequestHeaders[kvp.Key] = kvp.Value;
                }
            }
        }

        protected void ClearCustomRequestHeaders()
        {
            customRequestHeaders = new Dictionary<string, string>();
        }
    }
}
