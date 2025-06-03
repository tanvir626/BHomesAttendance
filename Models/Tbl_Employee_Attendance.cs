using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BHomesAttendance.Models
{
    [Table("Tbl_Employee_Attendance")]
    public class Tbl_Employee_Attendance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AttID { get; set; }

        [StringLength(30)]
        public string fp_id { get; set; }

        public DateTime? io_time { get; set; }

        [Column(TypeName = "date")]
        public DateTime? fp_date { get; set; }

        [StringLength(20)]
        public string fp_time { get; set; }
    }
}
