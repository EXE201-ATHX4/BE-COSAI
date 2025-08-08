using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.BrandModelViews
{
    public class BrandModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class CreateBrandModel
    {
        public string Name { get; set; }
    }
    public class UpdateBrandModel
    { 
        public string Name { get; set; }
    }
    
}
