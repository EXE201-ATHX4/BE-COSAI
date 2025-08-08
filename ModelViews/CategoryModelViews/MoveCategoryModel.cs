using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.CategoryModelViews
{
    public class MoveCategoryModel
    {
        public int? NewParentId { get; set; }
        public int NewDisplayOrder { get; set; }
    }
}
