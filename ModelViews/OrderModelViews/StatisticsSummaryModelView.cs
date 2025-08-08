using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.OrderModelViews
{
    public class StatisticsSummaryModelView
    {
        public List<RevenueByTimeDto> MonthlyRevenue { get; set; }
        public List<RevenueByTimeDto> WeeklyRevenue { get; set; }
        public List<LowStockProductDto> LowStockProducts { get; set; }
    }
    public class RevenueByTimeDto
    {
        public DateTimeOffset Period { get; set; } // "2025-08" hoặc "2025-W31"
        public int TotalRevenue { get; set; }
    }

    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
    }

}
