using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Nest;


namespace searchpoc.Controllers
{
    [ApiController]
    [Route("search")]
    public class SearchController : ControllerBase
    {
        private readonly ILogger<SearchController> _logger;

        private const string indexName = "products";
        private const string serverUrl = "http://localhost:9200";


        public SearchController(ILogger<SearchController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetSearch(string? articleId,string? name)
        {            
            var results = DoSearch(articleId,name);
            return new JsonResult(results);
        }

        private IReadOnlyCollection<object> DoSearch(string? articleId,string? name)
        {
            var client = GetESClient();


            MatchPhraseQueryDescriptor<object> queryDescriptor = new MatchPhraseQueryDescriptor<object>();
            

            if(!string.IsNullOrEmpty(articleId))
            {
                queryDescriptor
                    .Field("id")
                    .Query(articleId);                                   
            }

            if(!string.IsNullOrEmpty(name))
            {
                queryDescriptor
                    .Field("name")
                    .Query("*" + name + "*");
            }

            
            var searchResult = client.Search<object>(s => s
                .Query(
                    q => q.MatchPhrase(q => queryDescriptor)                    
                    )
               .AnalyzeWildcard(true)
            );
            

            var products = searchResult.Documents;
            return products;
            
        }


        private ElasticClient GetESClient()
        {
            var settings = new ConnectionSettings(new Uri(serverUrl));
            settings.DefaultIndex(indexName);
           
            return new ElasticClient(settings);
        }

    }
}
