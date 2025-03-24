using System.Security.Claims;
using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authorization;
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
        private readonly RedisCacheService _redisCacheService;
        private readonly RabbitMQService _rabbitMQService;

        public HelloGreetingController(IGreetingService greetingService, RedisCacheService redisCacheService, RabbitMQService rabbitMQService)
        {
            _greetingService = greetingService;
            _redisCacheService = redisCacheService;
            _rabbitMQService = rabbitMQService;
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

        private int GetUserIdFromToken()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null && identity.IsAuthenticated)
            {
                Console.WriteLine("TOKEN CLAIMS: ");
                foreach (var claim in identity.Claims)
                {
                    Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
                }

                var userIdClaim = identity.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .LastOrDefault();
                if (userIdClaim != null)
                {
                    return int.Parse(userIdClaim.Value);
                }
            }
            throw new UnauthorizedAccessException("User ID not found in token.");
        }



        /// <summary>
        /// Save Greeting Message
        /// </summary>
        [Authorize]
        [HttpPost]
        [Route("save-greeting")]
        public IActionResult SaveGreeting([FromBody] GreetingRequestDTO request)
        {
            ResponseBody<string> responseModel = new ResponseBody<string>();

            try
            {
                var userId = GetUserIdFromToken();

                 //Debugging log
                Console.WriteLine($"Received Request: {request.FirstName}, {request.LastName}, {request.Message}");

                if (string.IsNullOrWhiteSpace(request.FirstName) ||
                    string.IsNullOrWhiteSpace(request.LastName) ||
                    string.IsNullOrWhiteSpace(request.Message))
                {
                    responseModel.Success = false;
                    responseModel.Message = "First name, last name, and message are required.";
                    return BadRequest(responseModel);
                }

                GreetingEntity greeting = new GreetingEntity()
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Message = request.Message,
                    UserId = userId
                };

                _greetingService.SaveGreetingMessage(greeting, userId);

                responseModel.Success = true;
                responseModel.Message = "Greeting saved successfully";
                responseModel.Data = $"Greeting for {greeting.FirstName} {greeting.LastName} saved.";
            }
            catch (Exception ex)
            {
                responseModel.Success = false;
                responseModel.Message = $"Error saving greeting: {ex.Message}";
            }

            return Ok(responseModel);
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

        [HttpGet("cache/{key}")]
        public async Task<IActionResult> GetFromCache(string key)
        {
            var value = await _redisCacheService.GetAsync(key);
            if (value == null)
            {
                return NotFound();
            }
            return Ok(value);
        }

        [HttpPost("cache")]
        public async Task<IActionResult> SetCache([FromBody] RequestBody request)
        {
            await _redisCacheService.SetAsync(request.Key, request.Value);
            return Ok();
        }

        [HttpPost("publish")]
        public IActionResult PublishMessage([FromBody] RequestBody request)
        {
            _rabbitMQService.Publish("greetingQueue", request.Value);
            return Ok();
        }
    }
}
