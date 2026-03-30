using CSharpAsyncParalelPrograming.LearningLabs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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


    // Await Main thread'i bloke etmez, sadece o satırdaki işlemi uyutur. O satırdaki işlem tamamlanana kadar main thread diğer işlemleri yapmaya devam eder.
    [HttpPost("taskFromResult")]
    public async Task<IActionResult> getUsersFromService()
    {
      // response bu aşamada çözümlenmiş oluyor.
      var response = await _taskService.getUsersAsync();

      return Ok(response);
    }


    // Uyarı: Servis 2 snlik bir bekleme süresi sonucunda ilgili requesti handle edebiliyor. Aşağıdaki kod hiç bir şekilde resultı beklemediği için response.IsCompleted state geçemez bu durumda istek cevap olarak yanlış sonuç döndürür. 

    [HttpPost("taskFromResultWithDelay")]
    public async Task<IActionResult> getUsersFromServiceWithDelay()
    {
      // response bu aşamada çözümlenmiş olmuyor. asenkron state de aslında dönüş oluyor
      var response = _taskService.getUsersAsync();

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


    // Task Cancelation ve Task Exception Örnekleri

    [HttpPost("taskCancelSample")]
    public async Task<IActionResult> TaskCancelSample()
    {
      await LearningLabs.TaskSample.TaskCancelSample();

      return Ok("TaskCancelSample");

    }


    // Client uzun süren isteği iptal edersek
    // Web uygulamalarında iptal sinyali CancellationToken  cancellationToken
    [HttpGet("ClientCancelRequest")]
    public async Task<IActionResult> ClientCancelRequest(CancellationToken cancellationToken)
    {

      await _taskService.getUsersAsync(cancellationToken);

      return Ok("TaskCancelSample");

    }

    // // throw new InvalidOperationException("Task içinde bir istinai durum meydana geldi"); yada aşağıdaki gibide exception yönetimi yaparsak
    //  return Task.FromException<InvalidOperationException>(new InvalidOperationException("Task içinde bir istinai durum meydana geldi"));
    // try catch bloğu ile hataları yakalarız.

    [HttpPost("taskExceptionSample")]
    public async Task<IActionResult> TaskExceptionSample()
    {
      try
      {

        await LearningLabs.TaskSample.TaskExceptionSample();
      }
      catch (Exception ex)
      {
        Console.WriteLine("Hata");
      }


      // 1. kod örneğinde return OK yukarıdaki işlemleri beklet.

      return Ok("TaskExceptionSample");

    }

    // Try-catch kullanmadan hata yakalamak istersek
    // Serviste Try catch kullanmadan hata yönetimi yaparsak, Task'in IsFaulted propertysini kontrol etmek hatalı çalışmaya sebep oluyor. 
    // try catch kullandıktan sonra taskState.IsFaulted yapmamız gerekiyor.
    // Eğer servis düzeyinde bir exception fırlatıyorsak, controller seviyesinde try catch ile sarmalamamız gerekiyor. 

    [HttpPost("taskExceptionSampleV2")]
    public async Task<IActionResult> TaskExceptionSamplV2()
    {


      var taskState = LearningLabs.TaskSample.TaskExceptionSample();

      // Önerilmeyen örnek
      try
      {
        await taskState;
      }
      catch (Exception ex)
      {
        // HATAYı YA MİDDLEWARE DE MERKEZİ YADA KONTROLLERDA YAKALAMAK LAZIM.
      }


      return Ok("TaskExceptionSample");

    }



    [HttpPost("taskExceptionSampleV3")]
    public async Task<IActionResult> TaskContinueWith()
    {

      // JS deki Promise Chain yapısına benzer şekilde, Task'in ContinueWith methodu ile task tamamlandıktan sonra çalışacak bir kod bloğu tanımlayabiliriz. Bu kod bloğu, task'in sonucunu veya hatasını kontrol etmek için kullanılabilir.
      // kod arkaplanda contrinue with ile devam edip hata durumunda anca loglama yada geri alma işlemi gibi işlemleri tetikleyebilir.
      // JS Callbackgibi çalışır. bi state değişikliği sonrası tetiklenir. 1. işlem bitimini 2.task işleminde bağlayıp 2 sonucu sıralı bir şekilde arka planda işlemi sağlar.
      var taskState = LearningLabs.TaskSample.TaskExceptionSample().ContinueWith(async (task) =>
      {
        // Exception varsa hatayı yakalarız.
        if (task.IsFaulted)
        {
          Console.WriteLine($"[Error3]  ${task.Exception.Message};");
        }

      });


      // return ok beklemez sonuç hızlı döner ama background iş yapılır. 
      return Ok("TaskExceptionSample");

    }


    [HttpPost("TaskWhenAll")]
    public async Task<IActionResult> TaskWhenAll()
    {

      var response = await LearningLabs.TaskSample.TaskWhenAll();

      return Ok(response);
    }

    [HttpPost("TaskWhenAny")]
    public async Task<IActionResult> TaskWhenAny()
    {

      var response = await LearningLabs.TaskSample.TaskWhenAny();

      return Ok(response);
    }



    [HttpPost("BasicParallelFor")]
    public IActionResult BasicParallelFor()
    {

       LearningLabs.ParalelProgramingSample.BasicParallelFor();

      return Ok();
    }

    [HttpPost("ParalelForeachBasis")]
    public IActionResult ParalelForeachBasis()
    {

      LearningLabs.ParalelProgramingSample.ParalelForeachBasis();

      return Ok();
    }


    [HttpPost("ParalelForWithThreadLocalState")]
    public IActionResult ParalelForWithThreadLocalState()
    {

      LearningLabs.ParalelProgramingSample.ParalelForWithThreadLocalState();

      return Ok();
    }


    [HttpPost("ParalelInvokeSample")]
    public IActionResult ParalelInvokeSample()
    {

      LearningLabs.ParalelProgramingSample.ParalelInvokeSample();

      return Ok();
    }


    








  }
}
