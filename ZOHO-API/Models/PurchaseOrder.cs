using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZOHO_API.Models
{
    [Table("tbl_PurchaseOrders")]
    public class PurchaseOrder
    {
        [Key]
        public string purchaseorder_id { get; set; }
        public string vendor_name { get; set; }
        public string purchaseorder_number { get; set; }
        public string status { get; set; }

        public string total {  get; set; }


    }
}
