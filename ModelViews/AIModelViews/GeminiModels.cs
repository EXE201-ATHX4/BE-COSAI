using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModelViews.AIModelViews.GeminiModels;

namespace ModelViews.AIModelViews
{
    public class GeminiModels
    {
        public class Part
        {
            public string text { get; set; }
        }

        public class Content
        {
            public string role { get; set; }
            public List<Part> parts { get; set; }
        }

        public class Candidate
        {
            public Content content { get; set; }
        }

        public class GeminiResponse
        {
            public List<Candidate> candidates { get; set; }
        }
      
    }
    public class Chatresponse()
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public DateTimeOffset Timestamp { get; set; }

    }
}
