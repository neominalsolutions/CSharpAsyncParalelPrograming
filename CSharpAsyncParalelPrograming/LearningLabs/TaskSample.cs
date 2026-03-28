namespace CSharpAsyncParalelPrograming.LearningLabs
{

  public class UserReponse
  {
    public string Name { get; set; }
  }



  public class TaskService
  {
    public Task<List<UserReponse>> getUsersAsync()
    {
      return Task.FromResult(new List<UserReponse> { new UserReponse { Name = "Ali" }, new UserReponse { Name = "Cenk" } });
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
