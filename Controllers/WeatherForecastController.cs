using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace StreamingPdfFile.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetFile()
        {
            var filePath = @"D:\ActivityReactProject\ServerSide\StreamingPdfFile\Files\Balance-Sheet-Example (1).pdf";
            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
            {
                return NotFound();
            }

            var fileSize = fileInfo.Length;

            Response.ContentType = "application/pdf";
            Response.Headers.Add("Content-Disposition", $"attachment; filename={fileInfo.Name}");
            Response.Headers.Add("Accept-Ranges", "bytes");

            var rangeHeader = Request.Headers["Range"].ToString();

            if (!string.IsNullOrEmpty(rangeHeader))
            {
                var range = rangeHeader.Replace("bytes=", "").Split('-');
                var start = int.Parse(range[0]);
                var end = range.Length > 1 && !string.IsNullOrEmpty(range[1]) ? int.Parse(range[1]) : fileSize - 1;

                Response.StatusCode = (int)HttpStatusCode.PartialContent;
                Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileSize}");
                Response.ContentLength = end - start + 1;
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fileStream.Seek(start, SeekOrigin.Begin);

                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        Response.Body.WriteAsync(buffer, 0, bytesRead);
                    }
                }

            }
            else
            {
                Response.ContentLength = fileSize;

                var buffer = new byte[1024 * 16];
                var bytesRead = 0;

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        Response.Body.WriteAsync(buffer, 0, bytesRead);
                    }
                }
            }

            return new EmptyResult();
        }
    }

}