using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.SupplierModelViews
{
    public class SupplierModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
    }
    public class CreateSupplierModel
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
    }
    public class UpdateSupplierModel
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
    }

}
