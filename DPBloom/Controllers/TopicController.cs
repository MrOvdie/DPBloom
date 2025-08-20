using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DPBloom.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class TopicController : ControllerBase
{
    
}