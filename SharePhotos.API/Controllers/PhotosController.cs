using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharePhotos.API.Data;
using SharePhotos.API.Models;

namespace SharePhotos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PhotosController : ControllerBase
    {

        private readonly ApplicationDbContext contents;
        public PhotosController(ApplicationDbContext contents)
        {
            this.contents = contents;
        }

        
        [HttpGet("Number_Of_Photos")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> get_NumberOfPhotos()
        {
            if (contents == null)
            {
                // Handle the case where contents is null
                return BadRequest("Database context not properly initialized.");
            }
            int numberOfPhotos = await contents.Photos.CountAsync();
            return Ok(numberOfPhotos);

        }

        [HttpPost("Post_Photo")]
        public async Task<ActionResult> post_Photo(Photo foto)
        {

            contents.Add(foto);
            await contents.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("update/{id}")]
        public async Task<ActionResult> update_Photo(Photo foto, int id)
        {
            var existingPhoto = await contents.Photos.FirstOrDefaultAsync(p => p.Id == id);
            if (existingPhoto == null)
            {
                return NotFound(); // If the existing photo is not found, return 404 Not Found
            }
            
            existingPhoto.Title = foto.Title;
            existingPhoto.Description = foto.Description;
            existingPhoto.ImageUrl = foto.ImageUrl;

            await contents.SaveChangesAsync();
            return Ok("Photo replaced successfully");
        }

        [HttpGet("Photo/{id}")]
        public async Task<ActionResult<Photo>> get_Photo(int id)
        {
            var photo = await contents.Photos.Where(i => i.Id == id).FirstOrDefaultAsync();
            if (photo == null)
            {
                return NoContent();
            }
            else
            {
                return Ok(photo);
            }

        }


        [HttpGet("Get_Last_10_Photos")]
        public async Task<ActionResult<Photo>> get_10Photos()
        {
            var last10Photos = await contents.Photos
            .OrderByDescending(p => p.Id) // Order by Id
            .Take(10) // Take the last 10 photos
            .ToListAsync();

            if (last10Photos == null || !last10Photos.Any())
            {
                return NotFound(); // If no photos are found, return a 404 Not Found
            }

            return Ok(last10Photos); // Return the last 10 photos
        }

        [HttpDelete("DeletePhoto/{id}")]
        public async Task<ActionResult> DeletePhoto(int id)
        {
            var photoToDelete = await contents.Photos.Where(p => p.Id == id).FirstOrDefaultAsync();
            if (photoToDelete == null)
            {
                return NotFound(); // If photo not found, return 404 Not Found
            }
            try
            {
                contents.Photos.Remove(photoToDelete);
                await contents.SaveChangesAsync();
                return Ok("Photo deleted successfully");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, $"Error deleting photo: {ex.Message}");
            }
        }


        [HttpDelete("deleteALLphotos")]
        public async Task<ActionResult> DeleteAllPhotos()
        {
            await contents.Database.ExecuteSqlRawAsync("DELETE FROM Photos");

            return Ok("All photos deleted successfully");
        }

    }
}
