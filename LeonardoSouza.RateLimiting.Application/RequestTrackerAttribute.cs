using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Runtime.Caching;
using Microsoft.AspNetCore.Mvc;
using LeonardoSouza.RateLimiting.Domain.Enum;
using LeonardoSouza.RateLimiting.Domain.Model;

namespace LeonardoSouza.RateLimiting
{

    //[RequestTracker(RequestLimit = 100, TimeUnit = TimeUnit.Minute)]

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequestTrackerAttribute : ActionFilterAttribute
    {
        public TimeUnit TimeUnit { get; set; }
        public int RequestLimit { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var seconds = Convert.ToInt32(TimeUnit);

            //Creates the key for the cache
            var key = string.Join("-",
                context.HttpContext.Request.Path,
                context.HttpContext.Connection.RemoteIpAddress
                );

            //requestCounter starts at one because of the current request and could increment with data in the cache
            int requestCounter = 1;
            //implements worst case by default to try again and could decrease with data in the cache
            int secondsLeft = seconds;

            if (MemoryCache.Default[key] != null)
            {
                Request request = (Request)MemoryCache.Default[key];
                secondsLeft = CalculateTimeLeft(seconds, request.FirstRequestTime);

                //if there is still time to clean the cache item, increse requests
                if (secondsLeft > 0)
                {
                    request.RequestCounter++;
                    requestCounter = request.RequestCounter;
                    MemoryCache.Default[key] = request;
                }
                //if the seconds left to try again expires, reset the cache item
                else
                {
                    AddNewCacheItem(key, seconds);
                }
            }
            else
            {
                AddNewCacheItem(key, seconds);
            }

            if(requestCounter > RequestLimit)
            {
                context.Result = new ContentResult
                {
                    Content = $"Rate limit exceeded. Try again in {secondsLeft} seconds.",
                    StatusCode = 429
                };
            }
        }

        private static int CalculateTimeLeft(int seconds, DateTime firstRequestDate)
        {
            return seconds - (int)(DateTime.Now - firstRequestDate).TotalSeconds;
        }

        private static void AddNewCacheItem(string key, int seconds)
        {
            var cachePolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(seconds)
            };

            Request request = new Request
            {
                RequestCounter = 1,
                FirstRequestTime = DateTime.Now,
            };

            MemoryCache.Default.Add(key, request, cachePolicy);
        }
    }
}