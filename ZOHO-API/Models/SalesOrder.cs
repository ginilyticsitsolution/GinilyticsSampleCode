using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime;

namespace ZOHO_API.Models
{
    [Table("tbl_Salesorder")]
    public class SalesOrder
    {
        [Key]
        public string salesorder_id { get; set; }

        public string customer_name { get; set; }
        public string salesorder_number { get; set; }
        public string status { get; set; }
        public string order_status { get; set; }
        public string invoiced_status { get; set; }
        public string paid_status { get; set; }
        public string salesperson_name { get; set; }
        public string total { get; set; }
        public string bcy_total { get; set; }
        public string total_invoiced_amount { get; set; }





    }
}
