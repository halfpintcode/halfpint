using System.ComponentModel.DataAnnotations;

namespace hpMvc.Models
{
    public class SensorModel
    {
        [System.Web.Mvc.HiddenInput(DisplayValue = false)]
        public int SensorType { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Monitor Date")]
        public string MonitorDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Monitor Time")]
        public string MonitorTime { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Monitor ID")]
        public string MonitorId  { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Transmitter ID")]
        public string TransmitterId { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Sensor Lot")]
        public string SensorLot { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Sensor Expiration Date")]
        public string ExpirationDate { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Inserter First Name")]
        public string InserterFirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Inserter Last Name")]
        public string InserterLastName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Sensor Location")]
        public string SensorLocation { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Sensor Reason")]        
        public string SensorReason { get; set; }

        [ScaffoldColumn(false)]
        [DataType(DataType.DateTime)]
        public string DateCreated { get; set; }
    }
}