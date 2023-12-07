using System.ComponentModel.DataAnnotations;

namespace SharePhotos.API.Models
{
    public class Photo
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "La {0} es requerida")]
        public string User_Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
