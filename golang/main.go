package main

import (
	"search/api"

	"github.com/gin-gonic/gin"
)

func main() {
	router := gin.Default()
	router.GET("/search", api.GetSearch)

	router.Run("localhost:8080")
}
