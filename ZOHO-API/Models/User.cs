using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZOHO_API.Models
{
    [Table("tbl_Users")]
    public class User
    {
        [Key]
        public string user_id { get; set; }
        public string name { get; set; }

        public string email { get; set; }

        public string user_role { get; set; }
        public string status { get; set; }

        public bool is_current_user { get; set; }
    }
}
