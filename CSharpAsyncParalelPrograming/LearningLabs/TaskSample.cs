using Microsoft.AspNetCore.Mvc;

namespace CSharpAsyncParalelPrograming.LearningLabs
{

  public class UserReponse
  {
    public string Name { get; set; }
  }


  // kendi custom servislerimizde Cancelation Yönetimi

  public class TaskService
  {
    public async Task<List<UserReponse>> getUsersAsync()
    {

      await Task.Delay(2000);


      Console.WriteLine($"[TaskService] {Thread.CurrentThread.ManagedThreadId}");

      return await Task.FromResult(new List<UserReponse> { new UserReponse { Name = "Ali" }, new UserReponse { Name = "Cenk" } });
    }


    // Burada Client isteği iptal ettğini var sayıyoruz.
    // Arka planda uzun çalışan iş isteklerini iptal edilebilir hale getirmek.

    public async Task<List<UserReponse>> getUsersAsync(CancellationToken cancellationToken)
    {
      try
      {

        // Api Call işlemi var.
        // Tam bu bekleme aşamasında Client isteği iptal ederse ne olur ?  Bunu simüle etmeye çalışıyoruz. 
        await Task.Delay(5000);

        // Alttaki Db işlemine ait task arkaplanda başlatılmadan request iptal edilme exception fırlatılacak. Böylece gereksiz yere Db işlemi yapılmamış olur.

        // eğer clientan istek iptal sinyali gönderildiyse
        if (cancellationToken.IsCancellationRequested)
        {
          Console.WriteLine("Cancelation Token is Requested");
          cancellationToken.ThrowIfCancellationRequested(); // eğer request cancel olarak geldiyse hata fırlat.
        }


        Console.WriteLine($"[TaskService] {Thread.CurrentThread.ManagedThreadId}");


      // Db Save Changes
       var task =  Task.Run<List<UserReponse>>(async () =>
       {

         return await Task.FromResult(new List<UserReponse> { new UserReponse { Name = "Ali" }, new UserReponse { Name = "Cenk" } });

       },cancellationToken);



        var response = await task;
        Console.WriteLine($"[Operation is successfull] {response}");

        return response;

      }
      catch (OperationCanceledException ex)
      {

        Console.WriteLine($"[Operation is canceled] {ex.Message}");

        return null;
    
      }


    }



    // GetAwaiter().GetResult() kodları asenkron bir çağırıyı senkron bir kod bloğu içerisinde çağırmamız gereken durumlar için yapılmıştır. 
    public List<UserReponse> getUsers()
    {

      return Task.Run(() =>
      {
        Console.WriteLine($"[Blocked TaskService] {Thread.CurrentThread.ManagedThreadId}");

        return new List<UserReponse> { new UserReponse { Name = "Ali" }, new UserReponse { Name = "Cenk" } };

      }).GetAwaiter().GetResult();


      // 2.Not: aşağıdaki kodda GetAwaiter().GetResult(); gibi kodu bloke eder.
      return Task.Run(() =>
      {
        Console.WriteLine($"[Blocked TaskService] {Thread.CurrentThread.ManagedThreadId}");

        return new List<UserReponse> { new UserReponse { Name = "Ali" }, new UserReponse { Name = "Cenk" } };

      }).Result;


      // 3.Not: Wait WaitAll gibi sonunda Async olmayan methodlarda kodu bloke der. waitAsync bloke etmez ama wait bloke eder. Böyle yazımdan da kaçınmamız gerekir.
      Task.Run(() =>
      {
        Console.WriteLine($"[Blocked TaskService] {Thread.CurrentThread.ManagedThreadId}");

        return new List<UserReponse> { new UserReponse { Name = "Ali" }, new UserReponse { Name = "Cenk" } };

      }).Wait();


    }

  }




  public static class TaskSample
  {


    public static async Task BasicTaskSample()
    {

      Console.WriteLine($"[Main Thread] {Thread.CurrentThread.ManagedThreadId}");

      // non-blocking şekilde çalışırız. awa<it bekletmek değil. Uyutmak yani satırdaki işleme geçemek için main thread bloklamadan uyutmak için kullanılır. 
      await Task.Run(async () =>
      {
        Console.WriteLine($"[Task] Başladı ID: {Thread.CurrentThread.ManagedThreadId}");
        await Task.Delay(2000);
      });

    }


    public static async Task BasicTask2Sample()
    {

      Console.WriteLine($"[Main Thread] {Thread.CurrentThread.ManagedThreadId}");

      // Bu kodda main thread bloke olmuş olur. GetAwaiter().GetResult() ile Task'in sonucunu bekleriz. Ancak bu yöntem genellikle önerilmez çünkü deadlock riskini artırabilir.


      Task.Run(async () =>
      {
        Console.WriteLine($"[Task] Başladı ID: {Thread.CurrentThread.ManagedThreadId}");
        Task.Delay(2000);
      });

      //Task.Run(async () =>
      //{
      //  Console.WriteLine($"[Task] Başladı ID: {Thread.CurrentThread.ManagedThreadId}");
      //  Task.Delay(2000);
      //}).GetAwaiter().GetResult();

    }


    // Task iptal işlemleri ve Hata Yönetimi
    // 1. yöntem olsun
    public static async Task TaskCancelSample()
    {
      CancellationTokenSource cts = new CancellationTokenSource();


      // Task sınıfı ile birlikte Func yada Action delegate'leri kullanarak iptal edilebilir görevler oluşturabiliriz. CancellationTokenSource, iptal işlemlerini yönetmek için kullanılır ve CancellationToken ile birlikte çalışır. CancellationToken, görevlerin iptal edilip edilmediğini kontrol etmek için kullanılır.

      Task longRunningTask = Task.Run(async () =>
      {
        for (int i = 0; i < 10; i++)
        {
          if (cts.Token.IsCancellationRequested)
          {
            Console.WriteLine("Task iptal edildi.");
            return;
          }
          Console.WriteLine($"Çalışıyor... {i}");
          await Task.Delay(1000);
        }
      }, cts.Token);


      // 3 saniye sonra iptal et
      await Task.Delay(3000);
      cts.Cancel();


      try
      {
        await longRunningTask; // longRunningTask tamamlanana kadar bekleriz. Eğer iptal edilmişse, OperationCanceledException fırlatılır.
      }
      catch (OperationCanceledException) // eğer iptal işlemi gerçekleşirse bu exception fırlatılır.
      {
        Console.WriteLine("Task iptal edildiği için yakalandı.");
      }
    }


    public static async Task TaskExceptionSample()
    {
      
        await Task.Run(() =>
        {
          throw new InvalidOperationException("Task içinde bir istinai durum meydana geldi");


          // return Task.FromException<InvalidOperationException>(new InvalidOperationException("Task içinde bir istinai durum meydana geldi"));


        });
      
      
    }




    // Task Methodları (WhenAny,WhenAll,ContinueWith), ValueTask, Methodu Yapay zekaya yorumlatma işlemi 







  }
}
