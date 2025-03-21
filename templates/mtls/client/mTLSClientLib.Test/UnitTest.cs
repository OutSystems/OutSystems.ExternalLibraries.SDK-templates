using System.Security.Cryptography.X509Certificates;

namespace mTLSClientLib.Test;

public class Tests
{
    private static readonly string BaseUrl = "https://localhost:5051";

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestGetWeatherForecast()
    {
        mTLSClientExample client = new();
        var weatherForecast = client.GetWeatherForecast(BaseUrl);
        Assert.IsNotNull(weatherForecast);
        Assert.IsTrue(weatherForecast.Count == 5, 
            $"Weather Forecast return the forecast for {weatherForecast.Count} days, expecting 5.");
    }
}