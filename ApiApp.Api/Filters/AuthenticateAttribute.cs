﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ApiApp.Core.Interfaces;
using ApiApp.Data;
using System.IO;

namespace ApiApp.Api.Filters
{
    public class AuthenticateAttribute : ActionFilterAttribute
    {
        private const string AuthenticationHeaderName = "Authentication";
        private const string TimestampHeaderName = "Timestamp";

        public IAccountRepository Repository { get; set; }

        private static string ComputeHash(string hashedPassword, string message)
        {
            var key = Encoding.UTF8.GetBytes(hashedPassword.ToUpper());
            string hashString;

            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                hashString = Convert.ToBase64String(hash);
            }

            return hashString;
        }

        private static void AddNameValuesToCollection(List<KeyValuePair<string, string>> parameterCollection,
            NameValueCollection nameValueCollection)
        {
            if (!nameValueCollection.AllKeys.Any())
                return;

            foreach (var key in nameValueCollection.AllKeys)
            {
                var value = nameValueCollection[key];
                var pair = new KeyValuePair<string, string>(key, value);

                parameterCollection.Add(pair);
            }
        }

        private static List<KeyValuePair<string, string>> BuildParameterCollection(HttpActionContext actionContext)
        {
            // Use the list of keyvalue pair in order to allow the same key instead of dictionary
            var parameterCollection = new List<KeyValuePair<string, string>>();

            var queryStringCollection = actionContext.Request.RequestUri.ParseQueryString();
            var formCollection = HttpContext.Current.Request.Form;

            AddNameValuesToCollection(parameterCollection, queryStringCollection);
            AddNameValuesToCollection(parameterCollection, formCollection);

            return parameterCollection.OrderBy(pair => pair.Key).ToList();
        }

        private static string GetHttpRequestHeader(HttpHeaders headers, string headerName)
        {
            if (!headers.Contains(headerName))
                return string.Empty;

            return headers.GetValues(headerName)
                            .SingleOrDefault();
        }

        private static string BuildBaseString(HttpActionContext actionContext)
        {
            var headers = actionContext.Request.Headers;
            string date = GetHttpRequestHeader(headers, TimestampHeaderName);

            string methodType = actionContext.Request.Method.Method;

            var uri = actionContext.Request.RequestUri.AbsoluteUri.ToLower();

            // TODO: Enable content hash
            // Authorization hash requires exactly 1 request object
            // var content = actionContext.ActionArguments.Count == 1 ? Newtonsoft.Json.JsonConvert.SerializeObject(actionContext.ActionArguments["postRequest"]) : string.Empty;
            var content = string.Empty;

            string message = string.Join("\n", methodType, date, uri, content);

            return message;
        }

        private static bool IsAuthenticated(string hashedPassword, string message, string signature)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                return false;

            var verifiedHash = ComputeHash(hashedPassword, message);
            if (signature != null && signature.Equals(verifiedHash))
                return true;

            return false;
        }

        private static bool IsDateValidated(string timestampString)
        {
            DateTime timestamp;

            bool isDateTime = DateTime.TryParseExact(timestampString, "U", null,
                DateTimeStyles.AdjustToUniversal, out timestamp);

            if (!isDateTime)
                return false;

            var now = DateTime.UtcNow;

            // TimeStamp should not be in 5 minutes behind
            if (timestamp < now.AddMinutes(-5))
                return false;

            if (timestamp > now.AddMinutes(5))
                return false;

            return true;
        }

        private static bool IsSignatureValidated(string signature)
        {
            var memoryCache = MemoryCache.Default;
            if (memoryCache.Contains(signature))
                return false;

            return true;
        }

        private static void AddToMemoryCache(string signature)
        {
            var memoryCache = MemoryCache.Default;
            if (!memoryCache.Contains(signature))
            {
                var expiration = DateTimeOffset.UtcNow.AddMinutes(5);
                memoryCache.Add(signature, signature, expiration);
            }
        }

        private string GetHashedPassword(string username)
        {
            Repository = new AccountRepository();
            return Repository.GetHashedPassword(username);
        }

        private bool IsAuthenticated(HttpActionContext actionContext)
        {
            var headers = actionContext.Request.Headers;

            var timeStampString = GetHttpRequestHeader(headers, TimestampHeaderName);
            if (!IsDateValidated(timeStampString))
                return false;

            var authenticationString = headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authenticationString))
                return false;

            var authenticationheaderParts = authenticationString.Split(new[] { " " },
                    StringSplitOptions.RemoveEmptyEntries);

            if (authenticationheaderParts.Length != 2)
                return false;

            var authenticationKeyParts = authenticationheaderParts[1].Split(new[] { ":" },
                    StringSplitOptions.RemoveEmptyEntries);

            if (authenticationKeyParts.Length != 2)
                return false;

            var username = authenticationKeyParts[0];
            var signature = authenticationKeyParts[1];

            if (!IsSignatureValidated(signature))
                return false;

            AddToMemoryCache(signature);

            var hashedPassword = GetHashedPassword(username);
            var baseString = BuildBaseString(actionContext);

            return IsAuthenticated(hashedPassword, baseString, signature);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var isAuthenticated = IsAuthenticated(actionContext);

            if (!isAuthenticated)
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                actionContext.Response = response;
            }
        }
    }
}
