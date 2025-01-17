﻿using System;
using System.Net;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;

namespace IRuettae.GeoCalculations.RouteCalculation
{
    public class GoogleRouteCalculator : IRouteCalculator
    {
        private const string BaseUrl = @"https://maps.googleapis.com/maps/api/directions/json?";

        private readonly string apiKey;
        private readonly string region;
        private const string Units = "metric";

        /// <summary>
        /// Implementation of RouteCalculator Interface using the Google API
        /// </summary>
        /// <param name="apiKey">Your Google API Key to use Directions API</param>
        /// <param name="region"></param>
        public GoogleRouteCalculator(string apiKey, string region = null)
        {
            this.apiKey = apiKey;
            this.region = region;
        }

        public (double distance, double duration) CalculateWalkingDistance(string from, string to)
        {
            return CalculateDistance(from, to, "walking");
        }

        public (double distance, double duration) CalculateWalkingDistance(double fromLat, double fromLong, double toLat, double toLong)
        {
            return CalculateWalkingDistance($"{fromLat},{fromLong}", $"{toLat},{toLong}");
        }

        private (double distance, double duration) CalculateDistance(string from, string to, string mode)
        {
            
            var callUrl = $"{BaseUrl}origin={from}&destination={to}&mode={mode}&units={Units}&key={apiKey}";

            if (!string.IsNullOrEmpty(region))
            {
                callUrl += $"&region={region}";
            }

            var retJson = new WebClient().DownloadString(callUrl);

            dynamic retData = JObject.Parse(retJson);

            if (retData.status != "OK")
            {
                throw new RouteNotFoundException();
            }

            try
            {
                double distance = retData.routes[0].legs[0].distance.value;
                double duration = retData.routes[0].legs[0].duration.value;

                return (distance, duration);
            }
            catch (RuntimeBinderException)
            {
                // if binding failed for dynamic data (double => null)
                throw new RouteNotFoundException();
            }
            catch (Exception)
            {
                // if accessing the route failed (generally)
                throw new RouteNotFoundException();
            }
        }

    }
}
