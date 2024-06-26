# ElPrisApi

This api regards electricity prices. So far the api has a few methods

## Get Elpris per area
Gets hourly prices for an area (SE1-SE4) in sweden with addition to average price, lowest and highest price during that day. The prices originates from [elprisetjustnu](https://www.elprisetjustnu.se/elpris-api).

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
Checks whether a gps coordinate is within one of the 4 areas in sweden and return price information for that gps coordinate

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