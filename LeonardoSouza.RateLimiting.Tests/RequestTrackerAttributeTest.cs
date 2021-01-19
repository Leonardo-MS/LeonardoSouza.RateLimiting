using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using LeonardoSouza.RateLimiting.Domain.Enum;

namespace LeonardoSouza.RateLimiting.Tests
{
    [TestClass]
    public class RequestTrackerAttributeTest
    {
        [TestMethod]
        [DataRow(150, 149)]
        [DataRow(101, 10)]
        [DataRow(500, 100)]
        [DataRow(1430, 1)]
        public void OnActionExecutingTest_should_return_error_429(int numberOfRequests, int requestLimit)
        {
            var result = OnActionExecutingTest(numberOfRequests, requestLimit);
            result.Should().NotBeNull().And.BeOfType<ContentResult>();
        }

        [TestMethod]
        [DataRow(150, 150)]
        [DataRow(101, 105)]
        [DataRow(500, 590)]
        [DataRow(1430, 10000)]
        //if OnActionExecuting result is null, the application can call the method, so its success
        public void OnActionExecutingTest_should_return_success(int numberOfRequests, int requestLimit)
        {
            var result = OnActionExecutingTest(numberOfRequests, requestLimit);
            result.Should().BeNull();
        }

        private ContentResult OnActionExecutingTest(int numberOfRequests, int requestLimit)
        {
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("", "error");
            var httpContext = new DefaultHttpContext();
            var context = new ActionExecutingContext(
                new ActionContext(
                    httpContext: httpContext,
                    routeData: new RouteData(),
                    actionDescriptor: new ActionDescriptor(),
                    modelState: modelState
                ),
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new Mock<Controller>().Object);

            context.HttpContext.Request.Path = "/" + numberOfRequests.ToString() + requestLimit.ToString();

            var requestTracker = new RequestTrackerAttribute();

            requestTracker.RequestLimit = requestLimit;
            requestTracker.TimeUnit = TimeUnit.Hour;

            for (int i = 0; i < numberOfRequests; i++)
            {
                requestTracker.OnActionExecuting(context);
            }

            return context.Result as ContentResult;
        }

    }
}
