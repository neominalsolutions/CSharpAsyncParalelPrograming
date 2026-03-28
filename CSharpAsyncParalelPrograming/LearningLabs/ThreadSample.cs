using System.Diagnostics;

namespace CSharpAsyncParalelPrograming.LearningLabs
{
  public static class ThreadSample
  {

    // parametre olamayan OS düzeyinde fiziksel bir thread oluşturma şekli.

    public static void BasicThreadSample()
    {
      Console.WriteLine($"[Main Thread] {Thread.CurrentThread.ManagedThreadId}");



      // bizim işi yürüttüğümüz thread
      Thread thread = new Thread(() =>
      {
        Console.WriteLine($"[Yeni Thread]! {Thread.CurrentThread.ManagedThreadId}");
      });

      Thread.Sleep(2000);
      thread.Start();
      thread.Join(); // Main thread bu kodun bitmesini beklesin.

    }

    public static void ThreadWithParameter()
    {
      Console.WriteLine($"[Main Thread] {Thread.CurrentThread.ManagedThreadId}");



      // bizim işi yürüttüğümüz thread
      Thread thread = new Thread((param) =>
      {

        string message = param as string ?? "Parametre Yok";
        Console.WriteLine($"[Yeni Thread]! {Thread.CurrentThread.ManagedThreadId}, message: {message}");
      });

      Thread.Sleep(2000);
      thread.Start();
      thread.Join(); // Main thread bu kodun bitmesini beklesin.
    }


    // Foreground ve Background olarak ayarlama
    // Foreground ne demek ? uygulma, bu thread bitene kadar kapanmaz.
    // Background ise, uygulama kapatılırsa thread zorla sonlandırılır. 
    public static void ForegroundVsBackgroundThread()
    {
      Stopwatch sp = Stopwatch.StartNew();


      Thread foreground = new Thread(() =>
      {
        Thread.Yield(); // CPU kısa bir süreliğine serbest bırak. 
        Console.WriteLine("[Foreground] Tamamlandı");
      });

      Thread background = new Thread(() =>
      {
        Thread.Sleep(2000);
        Console.WriteLine("[Foreground] Tamamlandı");
      });


      background.IsBackground = true; // Default değer  isBackground dalse olarak seçilmiştir. 

      foreground.Start();
      background.Start();
      foreground.Join(); // Foreground threadleri join ile main thread'e bağlamalıyız. Ana thread şuan oluşturduğumuz thread işini bitirmesini bekler.



      Console.WriteLine($"Total time ms {sp.ElapsedMilliseconds}");


    }


    public static void SenkronSample()
    {
      Stopwatch sp = Stopwatch.StartNew();


    
       Thread.Sleep(2000);
       Console.WriteLine("[1.İş] Tamamlandı");
     

     
      // Thread Sleep Main Thread bloke eder.
       Thread.Sleep(2000);
       Console.WriteLine("[2.İş] Tamamlandı");
     

      Console.WriteLine($"Total time ms {sp.ElapsedMilliseconds}");

    }


    // Multi Thread Array with Join
    // Thead Priority Sample
    public static void MultiThreadsWithJoin()
    {
      Thread[] threads = new Thread[3];
      threads[2] = new Thread(() =>
      {
        Console.WriteLine($"Thread Id {Thread.CurrentThread.ManagedThreadId} 3 nolu");
      });

      threads[2].Priority = ThreadPriority.Highest;
      threads[2].Start();

      threads[0] = new Thread(() =>
      {
        Console.WriteLine($"Thread Id {Thread.CurrentThread.ManagedThreadId} 1 nolu");
      });

      threads[0].Priority = ThreadPriority.Lowest;
      threads[0].Start();

      threads[1] = new Thread(() =>
      {
        Console.WriteLine($"Thread Id {Thread.CurrentThread.ManagedThreadId} index: 1");
      });

      threads[1].Priority = ThreadPriority.Normal;
      threads[1].Start();

      threads[0].Join();
      threads[1].Join();
      threads[2].Join();



      // Dizi olarak 
      Thread[] threads2 = new Thread[3];

      for (int i = 0; i < threads2.Count(); i++)
      {

        threads2[i] = new Thread(() =>
        {
          Thread.SpinWait(1000); // Her bir thread için 1000 lik döngü aç. 
          Console.WriteLine($"Thread Id {Thread.CurrentThread.ManagedThreadId} index: {i}");
        });

        threads2[i].Start();

      }


      foreach (var t in threads)
      {
        t.Join(); // Main Thread Theadlerin bitmesini beklesin. 
      }

    }

   





  }
}
