using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Template_WebAPI.Controllers;


namespace Template_WebAPITest
{
    public class WeatherForecastTest
    { 
        private readonly WeatherForecastController _controller;
         
        public WeatherForecastTest()
        { 
            _controller = new WeatherForecastController(new NullLogger<WeatherForecastController>());
        }


        [Fact]
        public void Get_Ok_anonimo()
        {
            // Preparación
            var result = _controller.anonimo();
            // Estimulo
            

            //Expect
            Assert.IsType<OkObjectResult>(result);
        }
    }
}