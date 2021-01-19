using System;
using System.Collections.Generic;
using System.Text;

namespace LeonardoSouza.RateLimiting.Domain.Model
{
    public class Request
    {
        public int RequestCounter { get; set; }
        public DateTime FirstRequestTime { get; set; }
    }
}
