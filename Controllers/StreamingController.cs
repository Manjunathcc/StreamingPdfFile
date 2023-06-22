using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Web.Http;

namespace StreamingPdfFile.Controllers
{
    [RoutePrefix("api/pdf")]
    public class StreamingController : ApiController
    {

        private const string FilePath = @"Path\to\your\pdf\file.pdf";

        public HttpResponseMessage Get()
        {
            var fileInfo = new FileInfo(FilePath);

            if (!fileInfo.Exists)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fileInfo.Name;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            response.Content.Headers.ContentLength = fileInfo.Length;

            IEnumerable<string> rangeHeaderValues;
            if (Request.Headers.TryGetValues("Range", out rangeHeaderValues))
            {
                var rangeHeaderValue = RangeHeaderValue.Parse(rangeHeaderValues.First());
                if (rangeHeaderValue.Ranges.Count > 0)
                {
                    long totalLength = fileInfo.Length;
                    var range = rangeHeaderValue.Ranges.First();
                    long start = range.From ?? 0;
                    long end = range.To ?? totalLength - 1;
                    long length = end - start + 1;

                    response.Content.Headers.ContentLength = length;
                    response.Content.Headers.ContentRange = new ContentRangeHeaderValue(start, end, totalLength);
                    response.StatusCode = HttpStatusCode.PartialContent;
                }
            }

            return response;
        }

    }

}
