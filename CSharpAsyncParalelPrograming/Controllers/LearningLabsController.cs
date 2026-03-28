using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAsyncParalelPrograming.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class LearningLabsController : ControllerBase
  {
    public LearningLabsController() { }


    // api/learninglabs/basicThread

    [HttpPost("basicThread")]
    public IActionResult BasicThreadSample()
    {
      LearningLabs.ThreadSample.BasicThreadSample();

      return Ok("BasicThreadSample ile Thread kullanım örneği yapıldı");
    }


    [HttpPost("threadWithParameter")]
    public IActionResult ThreadWithParameter()
    {
      LearningLabs.ThreadSample.ThreadWithParameter();

      return Ok("Parametreli Thread kullanım örneği yapıldı");
    }



    [HttpPost("foregroundVsBackgroundThread")]
    public IActionResult ForegroundVsBackgroundThread()
    {
      LearningLabs.ThreadSample.ForegroundVsBackgroundThread();

      return Ok("ForegroundVsBackgroundThread");
    }


    [HttpPost("SenkronSample")]
    public IActionResult SenkronSample()
    {
      LearningLabs.ThreadSample.SenkronSample();

      return Ok("SenkronSample");
    }


    [HttpPost("multiThreadsWithJoin")]
    public IActionResult MultiThreadsWithJoin()
    {
      LearningLabs.ThreadSample.MultiThreadsWithJoin();

      return Ok("MultiThreadsWithJoin");
    }




  }
}
