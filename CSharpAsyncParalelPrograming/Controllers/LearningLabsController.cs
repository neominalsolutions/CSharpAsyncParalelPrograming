using CSharpAsyncParalelPrograming.LearningLabs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAsyncParalelPrograming.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class LearningLabsController : ControllerBase
  {

    private readonly TaskService _taskService;

    public LearningLabsController(TaskService taskService)
    {
      this._taskService = taskService;

    }


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


    // Task örnekleri
    // Request de non-blocking olmalı. çünkü bu endpoint çok talep aldığında istekleri bloke eder. 

    [HttpPost("basicTaskSample")]

    public async Task<IActionResult> BasicTaskSample()
    {
      await LearningLabs.TaskSample.BasicTaskSample();
      return Ok("BasicTaskSample");
    }

    [HttpPost("basicTaskSample2")]
    public async Task<IActionResult> BasicTask2Sample()
    {
      await LearningLabs.TaskSample.BasicTask2Sample();
      return Ok("BasicTask2Sample");

    }


    [HttpPost("taskFromResult")]
    public async Task<IActionResult> getUsersFromService()
    {
      // response bu aşamada çözümlenmiş oluyor.
      var response = await _taskService.getUsersAsync();

      return Ok(response);
    }


    [HttpPost("taskFromResult2")]
    public async Task<IActionResult> getUsersFromService2()
    {
      // response bu aşamada çözümlenmiş olmuyor. asenkron state de aslında dönüş oluyor
      var response =  _taskService.getUsersAsync();

      // await ile result bekletmediğimiz takdirde async state takibi yapıyoruz.

      if (response.IsCompleted)
      {
        Console.WriteLine("Response bitti");
        return Ok(response.Result);
      }
      else
      {
        if (response.IsFaulted)
        {
          Console.WriteLine("Response da hata var");
          return StatusCode(500);
        }

        if (response.IsCanceled)
        {
          Console.WriteLine("İstek İptal edildi");
          return BadRequest();
        }

        return BadRequest("İstek Bitirilemedi");
      }

     
    }

    // Not: eğer await kullanıyorsak, genel olarak piden bir okuma işlemi yapıyoruz ve sonucu Ok result olarak döndürmek istiyoruzdurç Burada await kullanarak resultı almak zorundayız.
    // Ama eğer amaç arka planda asenkron bir kod bloğunu run ettirmek ise, log, db kayıt atma, mail atma, event fırlatma olabilir bu gibi durumlarda işlemin sonucu almaya gerek yoksa bu durumda await kullanarak response süresini bekletmeye gerek yok. 
    // Sıralı veritabanı operasyonlarında ise await kullanmamız gerekir. 



  }
}
