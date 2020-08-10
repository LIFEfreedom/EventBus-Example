using EventGenerator.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.ComponentModel.DataAnnotations;

namespace EventGenerator.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ObjectController : ControllerBase
	{
		private readonly ILogger<ObjectController> _logger;
		private readonly CustomObjectManager _objectManager;

		public ObjectController(ILogger<ObjectController> logger, CustomObjectManager objectManager)
		{
			_logger = logger;
			_objectManager = objectManager;
		}

		[HttpGet]
		public void Get([Required][FromQuery] int? id, [FromQuery] string description)
		{
			_objectManager.PublishNewObjectCreatedEvent(id.Value, description);
		}
	}
}
