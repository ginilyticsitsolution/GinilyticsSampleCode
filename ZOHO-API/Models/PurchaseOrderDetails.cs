using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZOHO_API.Models
{
    [Table("tbl_PurchaseorderDetail")]
    public class PurchaseOrderDetails
    {
        [Key]
        [Column("purchaseorder_id")]
        public string purchaseorder_id { get; set; }
        public string purchaseorder_number { get; set; }
        public string vendor_name { get; set; }
        public string status { get; set; }
        public string adjustment { get; set; }
        public string discount_amount { get; set; }
        public string discount { get; set; }
        public string discount_applied_on_amount { get; set; }
        public string discount_type { get; set; }
        public string sub_total { get; set; }
        public string sub_total_inclusive_of_tax { get; set; }
        public string tax_total { get; set; }
        public string total { get; set; }

        public List<Puchaseorderlineitems> line_items { get; set; } = new List<Puchaseorderlineitems>();

    }

    [Table("tbl_purchaseOrderlineitems")]
    public class Puchaseorderlineitems
    {
        [Key]
        public string line_item_id { get; set; }
        [Column("purchaseorder_id")]

        [ForeignKey("PurchaseOrderDetails")]
        public string purchaseorder_id { get; set; }

        public PurchaseOrderDetails PurchaseOrderDetails { get; set; }
        public string name { get; set; }
        public string account_name { get; set; }
        public string bcy_rate { get; set; }
        public string rate { get; set; }
        public string unit { get; set; }
        public decimal quantity { get; set; }
        public string discount {  get; set; }
        public string tax_name {  get; set; }

        public string tax_type { get; set; }
        public string tax_percentage { get; set; }
        public string item_total { get; set; }
        public string item_type { get; set; }
        public string project_id { get; set; } 
    }
}
