using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.CategoryModelViews
{
    public class CategoryModel : Core.Base.BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int Level { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool AllowProducts { get; set; }
        public bool ShowInMenu { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryModel> SubCategories { get; set; } = new();
    }
}
