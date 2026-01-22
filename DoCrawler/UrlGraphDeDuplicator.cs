using System.Collections.Generic;
using CrawlerDb.Models;
using LibCrawlerRepositories;

namespace DoCrawler;

public sealed class UrlGraphDeDuplicator
{
    private readonly ICrawlerRepository _repository;
    private readonly SortedDictionary<string, UrlGraphNode> _urlGraphNodes = [];

    // ReSharper disable once ConvertToPrimaryConstructor
    public UrlGraphDeDuplicator(ICrawlerRepository repository)
    {
        _repository = repository;
    }

    public void AddUrlGraph(int fromUrlPageId, UrlModel url, int batchPartId)
    {
        if (fromUrlPageId == 0 || batchPartId == 0)
        {
            return;
        }

        if (!_urlGraphNodes.ContainsKey(url.UrlName))
        {
            _urlGraphNodes.Add(url.UrlName,
                new UrlGraphNode { FromUrlId = fromUrlPageId, GotUrlNavigation = url, BatchPartId = batchPartId });
        }
    }

    public void CopyToRepository()
    {
        foreach (UrlGraphNode urlGraphNode in _urlGraphNodes.Values) //.Distinct()
            //foreach (UrlGraphNode urlGraphNode in _urlGraphNodes
            //  .GroupBy(ugn => new {ugn.BatchPartId, ugn.FromUrlId, ugn.GotUrlId}).Select(group => group.First()))
        {
            _repository.AddUrlGraph(urlGraphNode);
        }

        _urlGraphNodes.Clear();
    }
}
