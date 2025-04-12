using imgMetadataSeeker.Model;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace imgMetadataSeeker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageMetadataController: ControllerBase
    {
        [HttpPost("upload")]
        public IActionResult ExtractMetadata([FromForm] FileUploadModel model)
        {
            if (model == null || model.Image == null || model.Image.Length == 0)
                return BadRequest("No image uploaded.");

            using var stream = model.Image.OpenReadStream();

            var directories = ImageMetadataReader.ReadMetadata(stream);

            var metadata = directories
                .SelectMany(d => d.Tags)
                .Select(tag => new
                {
                    Directory = tag.DirectoryName,
                    Tag = tag.Name,
                    Value = tag.Description
                });
            var gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault();

            double? latitude = null;
            double? longitude = null;

            if (gpsDirectory != null)
            {
                var location = gpsDirectory.GetGeoLocation();
                if (location != null)
                {
                    latitude = location.Latitude;
                    longitude = location.Longitude;
                }
            }
            return Ok(metadata);
        }
    }
}
