using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Models
{
    public class CityModel
    {
        
        public int? CityID { get; set; }
        [Required]
        public int StateID { get; set; }
        [Required]
        public int CountryID { get; set; }
        [Required]

        public string CityName { get; set; }
        [Required]
        public string PinCode { get; set; }
    }
    public class StateDropDownModel
    {
        public int StateID { get; set; }
        public string StateName { get; set; }
    }
    public class CountryDropDownModel
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }
    }
}
