# ElPrisApi

This api regards electricity prices. So far the api has a few methods

## Get Elpris per area
Gets hourly prices for an area (SE1-SE4) in sweden with addition to average price, lowest and highest price during that day. The prices originates from [elprisetjustnu](https://www.elprisetjustnu.se/elpris-api).


Returns: 

```
{
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

