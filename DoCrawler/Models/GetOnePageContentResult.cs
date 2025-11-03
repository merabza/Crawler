using System;
using System.Net;

namespace DoCrawler.Models;

public class GetOnePageContentResult
{
    public HttpStatusCode StatusCode { get; set; }
    public DateTime? LastModified { get; set; }
    public string? Content { get; set; }
    public string? Location { get; set; }
}