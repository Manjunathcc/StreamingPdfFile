using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace StreamingPdfFile.Controllers
{
    [ApiController]
    [Route("controller")]
    public class PdfController : ControllerBase
    {

        private const string FilePath = @"";

        [HttpGet("RangeRequest")]
        public IActionResult Get()
        {
            var fileInfo = new FileInfo(FilePath);
            var contentLength = fileInfo.Length;

            // Check if the request includes a "Range" header
            var rangeHeader = Request.Headers["Range"].ToString();
            if (!string.IsNullOrEmpty(rangeHeader))
            {
                var range = rangeHeader.Replace("bytes=", "").Split('-');
                var start = long.Parse(range[0]);
                var end = range.Length > 1 ? long.Parse(range[1]) : contentLength - 1;

                // Set the response headers for partial content
                Response.StatusCode = 206; // Partial Content
                Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{contentLength}");
                Response.Headers.Add("Accept-Ranges", "bytes");

                // Read the requested range from the PDF file
                using (var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    var length = end - start + 1;
                    var buffer = new byte[length];
                    fileStream.Seek(start, SeekOrigin.Begin);
                    fileStream.Read(buffer, 0, (int)length);
                    return File(buffer, "application/pdf");
                }
            }
            else
            {
                // No range specified, return the entire PDF file
                return PhysicalFile(FilePath, "application/pdf");
            }
        }
    }
}
