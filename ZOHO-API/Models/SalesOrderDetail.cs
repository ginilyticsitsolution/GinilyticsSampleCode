using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZOHO_API.Models
{
    [Table("tbl_SalesOrderDetails")]
    public class SalesOrderDetail
    {
        [Key]
        public string salesorder_id { get; set; }

        public string salesorder_number { get; set; }

        public string customer_name { get; set; }
        public string status { get; set; }
        public string adjustment { get; set; }

        public string discount { get; set; }
        public string discount_percent { get; set; }
        public string discount_applied_on_amount { get; set; }
        public string discount_type { get; set; }

        public string sub_total { get; set; }

        public string sub_total_inclusive_of_tax { get; set; }

        public string tax_total { get; set; }
        public string total { get; set; }

        public List<Salesorderlineitem> line_items { get; set; } = new List<Salesorderlineitem>();


    }
    [Table("tbl_SalesOrderLineItem")]
    public class Salesorderlineitem
    {
        [Key]
        public string line_item_id { get; set; }

        [ForeignKey("detail")]
        public string salesorder_id { get; set; }

        public SalesOrderDetail detail { get; set; }    
        public string name { get; set; }
        public string sales_rate { get; set; }

        public string bcy_rate { get; set; }

        public string rate { get; set; }
        public string unit { get; set; }

        public decimal quantity { get; set; }
        public string discount { get; set; }
        public string tax_name { get; set; }
        public string tax_type { get; set; }
        public string tax_percentage { get; set; }
        public string item_total { get; set; }

        public string item_type { get; set; }
        public string project_id { get; set; }

    }
}
