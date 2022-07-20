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

            ISearchResponse<object> result = null;
            IReadOnlyCollection<object> products = null;

            if(!string.IsNullOrEmpty(articleId))
            {
                result = IdSearch(client, articleId);
            }

            if(!string.IsNullOrEmpty(name))
            {
                result = WildcardSearch(client, name);   
            }

            
            if(result!=null)
            {
                products = result.Documents;
            }

            
            return products;
            
        }

        private ISearchResponse<object> WildcardSearch(ElasticClient client, string name)
        {
            var queryDescriptor = new WildcardQueryDescriptor<object>();
            queryDescriptor.Wildcard("*");
            queryDescriptor.Field("name");
            queryDescriptor.Value("*" + name + "*");

            var searchResult = client.Search<object>(s => s
                .Query(
                    q => q.Wildcard(qd => queryDescriptor)                    
                 )               
            );

            return searchResult;
        }

        private ISearchResponse<object> IdSearch(ElasticClient client, string id)
        {
            var queryDescriptor = new MatchPhraseQueryDescriptor<object>();
            queryDescriptor.Field("id");
            queryDescriptor.Query(id);

            var searchResult = client.Search<object>(s => s
               .Query(               
                   q => q.MatchPhrase(q => queryDescriptor)                    
               )              
            );
            return searchResult;
        }


        private ElasticClient GetESClient()
        {
            var settings = new ConnectionSettings(new Uri(serverUrl));
            settings.DefaultIndex(indexName);
           
            return new ElasticClient(settings);
        }

    }
}
