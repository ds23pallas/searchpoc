package api

import (
	"bytes"
	"context"
	"crypto/tls"
	"encoding/json"
	"fmt"
	"log"
	"net/http"

	"github.com/elastic/go-elasticsearch/v8"
	"github.com/gin-gonic/gin"
)

var result map[string]interface{}
var indexName = "products"
var serverUrl = "http://localhost:9200"

// Executes a search using ?articleId querystring value and returns a JSON result
func GetSearch(c *gin.Context) {

	esclient, err := getESClient()
	if err != nil {
		fmt.Println("Error getting ES client : ", err)
		panic("Client fail")
	}

	query, err := buildQuery(c.Query("articleid"))

	if err != nil {
		fmt.Println("Error building query ", err)
	}

	fmt.Println("Query is ", &query)

	res, err := esclient.Search(
		esclient.Search.WithContext(context.Background()),
		esclient.Search.WithIndex(indexName),
		esclient.Search.WithBody(&query),
		esclient.Search.WithTrackTotalHits(true),
		esclient.Search.WithPretty(),
	)

	if err != nil {
		fmt.Println("Error executing query %s", err)
		c.IndentedJSON(http.StatusInternalServerError, err)
	}

	defer res.Body.Close()

	if err := json.NewDecoder(res.Body).Decode(&result); err != nil {
		fmt.Println("Error parsing response from Elasticsearch %s", err)
		c.IndentedJSON(http.StatusInternalServerError, err)
	}

	c.IndentedJSON(http.StatusOK, result)

}

// Initiate a connection to Elasticsearch cluster, ignoring SSL
func getESClient() (*elasticsearch.Client, error) {

	cfg := elasticsearch.Config{
		Addresses: []string{
			serverUrl,
		},
		Transport: &http.Transport{
			TLSClientConfig: &tls.Config{InsecureSkipVerify: true},
		},
	}
	es, err := elasticsearch.NewClient(cfg)

	if err != nil {
		log.Fatalf("Error creating the client: %s", err)
	}

	return es, err
}

// Builds an Elasticsearch query for articleId
func buildQuery(articleId string) (bytes.Buffer, error) {

	var buf bytes.Buffer
	query := map[string]interface{}{
		"query": map[string]interface{}{
			"match": map[string]interface{}{
				"id": articleId,
			},
		},
	}
	err := json.NewEncoder(&buf).Encode(query)

	return buf, err
}
