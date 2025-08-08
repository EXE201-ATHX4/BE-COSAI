using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.AIModelViews
{
    public class GeminiContent
    {
        public string role { get; set; }
        public List<GeminiPart> parts { get; set; }
    }
}
