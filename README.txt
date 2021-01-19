In this Solution you'll find 4 projects:

	Libraries - Where the logic of the request limitation is. Can be loaded in any WebApi project.
	LeonardoSouza.RateLimiting.Application
	LeonardoSouza.RateLimiting.Domain

	Test project - Used for the unit testing.
	LeonardoSouza.RateLimiting.Tests

	WebApi - Standard WebApi created by Visual Studio only to demonstrate how it works
	LeonardoSouza.RateLimiting.Usage.WebApi

How to test the code:
	- If you just run the WebApi, the rate-limiting already works using the default 100 requests per hour.
	- If you Add the libraries as references(or the DLLs) in any WebApi project and call OnActionExecuting method as shown below.
	- If you want a more easy way to see how it works in runtime, just debug the tests in the test project.

The Solution:
	In .netcore(also in .net framework, but in this case was made for .netcore) there is a Class called ActionFilterAttribute,
with virtual methods that can be implemented to run every time before even reach the endpoint method. This can be used to check if
the caller can access this route, to limit the number of requests, and other things. 
	The implementation consists in do a override of OnActionExecuting method from ActionFilterAttribute to check if the caller
still can do one more request in a period of time. I choose this way because its a cleaner addition to the WebApis(one line of code) 
and the limit can be set for every route. 

How to use in a WebAPi
	Once the libraries are add as references, add the RequestTracker filter for each route that you want as shown below:
(TimeUnit variable accepts Second, Minute, Hour, Day):

        [HttpGet]
        [RequestTracker(RequestLimit = 100, TimeUnit = TimeUnit.Minute)]
        public IEnumerable<WeatherForecast> Get()

Limitation:
	This solution uses in memory cache to verify the number of requests made. In a architecture with multiple instances of the same 
application, a NoSql cache like Redis could be a better solution.
	