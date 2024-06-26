# ElPrisApi

This api regards electricity prices. So far the api has a few methods.


In order to be able to debug it one need access to a azure subscription with service bus where one can create a queue. In addition to this one needs either to use azurite to emulate azure tablestorage locally or enter a connectionstring to a tablestorage in azure. 

## Get Elpris per area
Gets hourly prices for an area (SE1-SE4) in sweden with addition to average price, lowest and highest price during that day. The prices originates from [elprisetjustnu](https://www.elprisetjustnu.se/elpris-api). The prices are cached in azure tablestorage, but if not present it will create a service bus message to the background worker function which updates the tablestorage for the current date. Meanwhile the user will get the information straight from the api for this call.

Calls can look like this
http://localhost:5023/ElPris?area=SE2

Or this

http://localhost:5023/ElPris?area=1


Returns: 

```
{
  "Area" : "SE1",
  "averagePrice": 0.24180624999999997,
  "highestPrice": 0.41423,
  "lowestPrice": 0.00101,
  "prices": 
  [
    {
      "SEK_per_kWh": 0.34742,
      "EUR_per_kWh": 0.03089,
      "EXR": 11.246989,
      "time_start": "2024-06-22T22:00:00+00:00",
      "time_end": "2024-06-22T23:00:00+00:00"
    },
    {
      "SEK_per_kWh": 0.33955,
      "EUR_per_kWh": 0.03019,
      "EXR": 11.246989,
      "time_start": "2024-06-22T23:00:00+00:00",
      "time_end": "2024-06-23T00:00:00+00:00"
    }
  ]
}
 
```


## Get Elrpis for GPS coordinate 
Checks whether a gps coordinate is within one of the 4 areas in sweden and return price information for that gps coordinate.

Returns: 

```
{
  "Area" : "SE1",
  "averagePrice": 0.24180624999999997,
  "highestPrice": 0.41423,
  "lowestPrice": 0.00101,
  "prices": 
  [
    {
      "SEK_per_kWh": 0.34742,
      "EUR_per_kWh": 0.03089,
      "EXR": 11.246989,
      "time_start": "2024-06-22T22:00:00+00:00",
      "time_end": "2024-06-22T23:00:00+00:00"
    },
    {
      "SEK_per_kWh": 0.33955,
      "EUR_per_kWh": 0.03019,
      "EXR": 11.246989,
      "time_start": "2024-06-22T23:00:00+00:00",
      "time_end": "2024-06-23T00:00:00+00:00"
    }
  ]
}
 
```


## Healthchecks 
http://localhost:5023/healthy

http://localhost:5023/healthy/self

http://localhost:5023/healthy/underlying-api

http://localhost:5023/healthy/tablestorage


Returns: 
```
{
  "status": "Healthy",
  "results": {
    "self": {
      "status": "Healthy",
      "description": "Healthy",
      "data": {}
    },
    "TableStorage": {
      "status": "Healthy",
      "description": "Table Storage is healthy",
      "data": {}
    },
    "Underlying API": {
      "status": "Healthy",
      "description": null,
      "data": {}
    }
  }
}
 
```
Or individual call
```
{
  "status": "Healthy",
  "results": {
    "TableStorage": {
      "status": "Healthy",
      "description": "Table Storage is healthy",
      "data": {}
    }
  }
}
```


All http calls can be tried out when running the app from visual studio from the .http file.


## Background functions
In addition to having those 2 api methods and the healthchecks the solution also contains a backgroundworker function app project with a couple of functions. 

### CleanOutOldRecordsFunction
This function fetches a list of table names to clearout from the appsettings field TablesToClean. The function app is timertriggered based of the value in the appsettings field TablesToCleanCronSchema. For each table there's a corresponding DaysToRetain number in a commaseparated field in the appsettings. 

The function cleans out old records from azure tablestorage in order to save space over time. 


### DailyPriceUpdateFunction
The function is timertriggered where one can decide the sceduel from the appsettings field DailyPriceUpdateCronSchema. It Checks if there's data present in azure tablestorage for the current date and if not it puts a message on the servicebus queue specified in the appsettings field PriceUpdateQueueName. 

### UpdatePriceQueueListenerFunction
Listenes to the queue specified in the appsettings field PriceUpdateQueueName and fetches electricalprices for all areas for the current date from the BaseUrl appsettings api and saves to azure tablestorage specified in AzureWebJobsStorage appsettings. 


### HealthCheckPingerFunction
A healthchecker that takes urls from config file and pings. Also reports to discord hook entered in config if service responds with 502.