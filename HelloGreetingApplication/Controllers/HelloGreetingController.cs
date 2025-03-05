using BusinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HelloGreetingApplication.Controllers
{
    /// <summary>
    /// Class Providing API for HelloGreeting
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HelloGreetingController : ControllerBase
    {
        private static Dictionary<string, string> greetings = new Dictionary<string, string>();
        private readonly IGreetingService _greetingService;

        public HelloGreetingController(IGreetingService greetingService)
        {
            _greetingService = greetingService;
        }

        /// <summary>
        /// Get Method to get the Greeting Message
        /// </summary>
        /// <returns>Hello, World</returns>
        [HttpGet]
        public IActionResult Get()
        {
            ResponseBody<Dictionary<string, string>> ResponseModel = new ResponseBody<Dictionary<string, string>>();

            ResponseModel.Success = true;
            ResponseModel.Message = "Hello to Greeting App API Endpoint";
            ResponseModel.Data = greetings;

            return Ok(ResponseModel);
        }

        [HttpPost]
        public IActionResult Post([FromBody] RequestBody requestModel)
        {
            ResponseBody<string> ResponseModel = new ResponseBody<string>();

            greetings[requestModel.Key] = requestModel.Value;

            ResponseModel.Success = true;
            ResponseModel.Message = "Request received successfully";
            ResponseModel.Data = $"Key: {requestModel.Key}, Value: {requestModel.Value}";

            return Ok(ResponseModel);
        }

        [HttpPut]
        public IActionResult Put([FromBody] RequestBody requestModel)
        {
            ResponseBody<Dictionary<string, string>> ResponseModel = new ResponseBody<Dictionary<string, string>>();

            // Add or update the dictionary
            greetings[requestModel.Key] = requestModel.Value;

            ResponseModel.Success = true;
            ResponseModel.Message = "Greeting updated successfully";
            ResponseModel.Data = greetings;

            return Ok(ResponseModel);
        }

        [HttpPatch]
        public IActionResult Patch(RequestBody requestModel)
        {
            ResponseBody<string> ResponseModel = new ResponseBody<string>();

            if (!greetings.ContainsKey(requestModel.Key))
            {
                ResponseModel.Success = false;
                ResponseModel.Message = "Key not found";
                return NotFound(ResponseModel);
            }

            greetings[requestModel.Key] = requestModel.Value;
            ResponseModel.Success = true;
            ResponseModel.Message = "Value partially updated successfully";
            ResponseModel.Data = $"Key: {requestModel.Key}, Updated Value: {requestModel.Value}";

            return Ok(ResponseModel);
        }

        [HttpDelete("{key}")]
        public IActionResult Delete(string key)
        {
            ResponseBody<string> ResponseModel = new ResponseBody<string>();

            if (!greetings.ContainsKey(key))
            {
                ResponseModel.Success = false;
                ResponseModel.Message = "Key not found";
                return NotFound(ResponseModel);
            }

            greetings.Remove(key);
            ResponseModel.Success = true;
            ResponseModel.Message = "Entry deleted successfully";

            return Ok(ResponseModel);
        }

        [HttpGet]
        [Route("greeting")]
        public IActionResult Greetings([FromQuery] string? firstName, [FromQuery] string? lastName)
        {
            ResponseBody<string> ResponseModel = new ResponseBody<string>();

            ResponseModel.Success = true;
            ResponseModel.Message = "Greeting message fetched successfully";
            ResponseModel.Data = _greetingService.GetGreetingMessage(firstName, lastName);

            return Ok(ResponseModel);
        }

        /// <summary>
        /// Save Greeting Message
        /// </summary>
        [HttpPost]
        [Route("save-greeting")]
        public IActionResult SaveGreeting([FromBody] GreetingEntity greeting)
        {
            ResponseBody<string> ResponseModel = new ResponseBody<string>();

            try
            {
                _greetingService.SaveGreetingMessage(greeting);
                ResponseModel.Success = true;
                ResponseModel.Message = "Greeting saved successfully";
                ResponseModel.Data = $"Greeting for {greeting.FirstName} {greeting.LastName} saved.";
            }
            catch (Exception ex)
            {
                ResponseModel.Success = false;
                ResponseModel.Message = $"Error saving greeting: {ex.Message}";
            }

            return Ok(ResponseModel);
        }


        [HttpGet]
        [Route("get-greetings")]
        public IActionResult GetGreetings()
        {
            ResponseBody<List<GreetingEntity>> ResponseModel = new ResponseBody<List<GreetingEntity>>();

            try
            {
                ResponseModel.Success = true;
                ResponseModel.Message = "Greetings fetched successfully";
                ResponseModel.Data = _greetingService.GetSavedGreetings();
            }
            catch (Exception ex)
            {
                ResponseModel.Success = false;
                ResponseModel.Message = $"Error fetching greetings: {ex.Message}";
            }

            return Ok(ResponseModel);
        }

        [HttpGet]
        [Route("getGreetingById/{id}")]
        public IActionResult GetGreetingById(int id)
        {
            ResponseBody<List<GreetingEntity>> ResponseModel = new ResponseBody<List<GreetingEntity>>();
            var greeting = _greetingService.GetGreetingById(id);

            if (greeting == null)
            {
                ResponseModel.Success = false;
                ResponseModel.Message = "Greeting not found";
                ResponseModel.Data = null;
            }
            else
            {
                ResponseModel.Success = true;
                ResponseModel.Message = "Greeting found";
                ResponseModel.Data = new List<GreetingEntity> { greeting };

            }

            return Ok(ResponseModel);
        }

        [HttpPut]
        [Route("update-greeting/{id}")]
        public IActionResult UpdateGreeting(int id, [FromBody] string NewMessage)
        {
            ResponseBody<string> ResponseModel = new ResponseBody<string>();
            bool isUpdated = _greetingService.UpdateGreeting(id, NewMessage);
            Console.WriteLine(isUpdated);
            try
            {
                if (!isUpdated)
                {
                    ResponseModel.Success = false;
                    ResponseModel.Message = "Greeting not found";
                    ResponseModel.Data = null;
                }
                else
                {
                    ResponseModel.Success = true;
                    ResponseModel.Message = "Greeting updated successfully";
                    ResponseModel.Data = $"Greeting with id {id} updated.";
                }
            }
            catch (Exception ex)
            {
                ResponseModel.Success = false;
                ResponseModel.Message = $"Error updating greeting: {ex.Message}";
            }
            return Ok(ResponseModel);
        }

        [HttpDelete]
        [Route("delete-greeting/{id}")]
        public IActionResult DeleteGreeting(int id)
        {
            ResponseBody<string> ResponseModel = new ResponseBody<string>();
            bool isDeleted = _greetingService.DeleteGreeting(id);

            if (!isDeleted)
            {
                ResponseModel.Success = false;
                ResponseModel.Message = "Greeting not found";
                ResponseModel.Data = null;
            }
            else
            {
                ResponseModel.Success = true;
                ResponseModel.Message = "Greeting deleted successfully";
                ResponseModel.Data = $"Greeting with id {id} deleted.";
            }

            return Ok(ResponseModel);
        }
    }
}
