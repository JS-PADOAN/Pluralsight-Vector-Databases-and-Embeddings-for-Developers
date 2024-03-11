# Pluralsight-Vector-Databases-and-Embeddings-for-Developers

Welcome to the Repository that contains demo code for my course : 

To run the demo, you need an Azure Subscription with :
- a storage account with anonymous access enabled (or any other web hosting solution for accessing images through HTTPS requests)
- an AI Vision service (available in free tier)
  - https://learn.microsoft.com/en-us/azure/ai-services/computer-vision/overview
  - https://azure.microsoft.com/en-us/pricing/details/cognitive-services/computer-vision/
- an AI Search service (also available in free tier)
  - https://azure.microsoft.com/en-us/products/ai-services/ai-search
  - https://azure.microsoft.com/en-us/pricing/details/search/




To create the index, in the demo, I have used the following payload :

```
{
    "name": "my-index",
    "fields": [
        {
            "name": "id",
            "type": "Edm.String",
            "key": true,
            "filterable": true
        },
        {
            "name": "url",
            "type": "Edm.String",
            "searchable": true,
            "filterable": true,
            "sortable": true,
            "retrievable": true
        },
        {
            "name": "contentVector",
            "type": "Collection(Edm.Single)",
            "searchable": true,
            "retrievable": true,
            "dimensions": 1024,
            "vectorSearchProfile": "my-default-vector-profile"
        }
    ],
    "vectorSearch": {
        "algorithms": [
            {
                "name": "my-hnsw-config-1",
                "kind": "hnsw",
                "hnswParameters": {
                    "m": 4,
                    "efConstruction": 400,
                    "efSearch": 500,
                    "metric": "cosine"
                }
            }
        ],
        "profiles": [
            {
                "name": "my-default-vector-profile",
                "algorithm": "my-hnsw-config-1"
            }
        ]
    }
}
```

with a PUT HTTP Request on that URL :
 https://xxxxxxxxxxxxxxxxx.search.windows.net/indexes/my-index?api-version=2023-11-01&allowIndexDowntime=true

 (just add one header "api-key" with your api key)