using mTLSclient;
using OutSystems.ExternalLibraries.SDK;

namespace mTLSClientLib;

[OSInterface(Description = "Consume a mTLS web service in OutSystems Developer Cloud (ODC).", Name = "mTLS_Client")]
public interface ImTLSClientExample {
    [OSAction(Description = "Get Weather Forecast for the next 5 days.", ReturnName = "WeatherForecast")]
    ICollection<WeatherForecast> GetWeatherForecast(
        [OSParameter(Description = "Service Base Url.", DataType = OSDataType.Text)] 
        string baseUrl);
}

public class mTLSClientExample : ImTLSClientExample {
    public ICollection<WeatherForecast> GetWeatherForecast(string baseUrl) {
        var client = new ExampleClient(baseUrl);
        return client.GetWeatherForecastAsync().GetAwaiter().GetResult();
    }
}
