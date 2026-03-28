namespace CSharpAsyncParalelPrograming.LearningLabs
{

  public class UserReponse
  {
    public string Name { get; set; }
  }



  public class TaskService
  {
    public async Task<List<UserReponse>> getUsersAsync()
    {

      await Task.Delay(2000);


      Console.WriteLine($"[TaskService] {Thread.CurrentThread.ManagedThreadId}");

      return await Task.FromResult(new List<UserReponse> { new UserReponse { Name = "Ali" }, new UserReponse { Name = "Cenk" } });
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



  }
}
