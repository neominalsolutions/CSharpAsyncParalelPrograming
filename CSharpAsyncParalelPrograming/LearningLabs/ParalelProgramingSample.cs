using System.Diagnostics;

namespace CSharpAsyncParalelPrograming.LearningLabs
{
  public class ParalelProgramingSample
  {

    // Paralel programlama, birden fazla işlemi aynı anda yürütme yeteneğidir. Bu, genellikle çok çekirdekli işlemcilerde performansı artırmak için kullanılır. C#'ta paralel programlama, Task Parallel Library (TPL) ve Parallel LINQ (PLINQ) gibi araçlarla desteklenir.
    // ParalelFor ve ParalelForeach, Paralel.Invoke gibi yöntemler, işlemleri paralel olarak yürütmek için kullanılır. Bu yöntemler, işlemlerin bağımsız olduğu durumlarda performansı artırabilir. Ancak, paralel programlama karmaşıktır ve dikkatli yönetilmezse yarış koşulları, ölümler ve diğer sorunlara yol açabilir. Bu nedenle, paralel programlama yaparken senkronizasyon ve hata yönetimi konularına dikkat etmek önemlidir.


    // milyonluk paralel işlemlerde aslında aradaki performans farkını görürüz. Küçük işlemlerde paralel programlama overhead'ı nedeniyle performans düşebilir. Ancak, büyük veri setleri veya uzun süren işlemler için paralel programlama önemli bir performans artışı sağlayabilir. Küçük işlemler için normal for kullanalım.
    public static void BasicParallelFor()
    {

      // Benchmarking Sekron For or ParalelFor (10 item), 1.000.000 civarı bir veri işlemesinde mantıklıdır. 

      Stopwatch paralelSp = Stopwatch.StartNew();

      // Paralel.For, belirli bir aralıkta işlemleri paralel olarak yürütmek için kullanılır. Bu yöntem, genellikle döngülerdeki işlemleri hızlandırmak için tercih edilir. Örneğin, büyük bir dizi üzerinde işlem yaparken, her bir öğeyi paralel olarak işleyebilirsiniz.
      // ParalelFor içerisinde async bir method kod bloğu kullanırsan hatalar yakalanmaz: Bunun yerine Paralel.ForAsync bir yapı vardır onu kullanmak gerekir. 
      Parallel.For(0, 1000000, i =>
      {
        double result = Math.Sqrt(i) * Math.PI * Math.Pow(2, i + 1);
        // CPU-bound işlemler
        // Console.WriteLine($"İşlem {i} - Thread ID: {Thread.CurrentThread.ManagedThreadId}");
      });

      paralelSp.Stop();
      Console.WriteLine($"Paralel Sp Time {paralelSp.ElapsedMilliseconds}");

      Stopwatch forSp = Stopwatch.StartNew();

      for (int i = 0; i < 1000000; i++)
      {
        double result = Math.Sqrt(i) * Math.PI * Math.Pow(2, i + 1);
        // Console.WriteLine($"İşlem {i} - Thread ID: {Thread.CurrentThread.ManagedThreadId}");
      }

      forSp.Stop();
      Console.WriteLine($"For Sp Time {forSp.ElapsedMilliseconds}");


    }

    // Ramdeki milyonlarca veri üzerinde CPU-bound işlemi farklı threadler ile paralel de uygulamak ve kodun hesaplama hızını ciddi oranda artırmak için kullanılabilir. 
    // Not: MaxDegreeOfParallelism eğer emin değilseniz bunun dinamik olarak ThreadPool üzerinden .NET tarafından karar verilmesine izin verin. Cancelation Token önemli uzun süre long running process bu sebeple iptal durumu olmaz ise, threadlerin hepsinin işi bitirmesini bekletir. 

    public static void ParalelForeachBasis()
    {



      List<string> sehirler = ["İstanbul", "İzmir", "Ankara", "Manisa", "Bursa", "Konya", "Antalya"];

      // unmanagement bir resource using kullanalım
      using CancellationTokenSource cts = new CancellationTokenSource();

      ParallelOptions parallelOptions = new ParallelOptions
      {
        CancellationToken = cts.Token,
        MaxDegreeOfParallelism = 3 // bu iş için maksimum 3 thread kullan

      };

      cts.CancelAfter(300); // 300ms sonrasında iptal uygula.


      Parallel.ForEach(sehirler, parallelOptions, sehir =>
      {
        Console.WriteLine($"[Paralel Foreach] tamamlandı {Thread.CurrentThread.ManagedThreadId}");

      });


      // Hata durumunda paralel işlemi iptal etmek için CancellationToken kullanabiliriz. Örneğin, belirli bir koşul gerçekleştiğinde işlemi iptal etmek isteyebiliriz. Bu durumda, ParallelOptions içinde CancellationToken'ı ayarlayarak işlemi iptal edebiliriz.

      try
      {
        // İptal durumunu yakalamak için yazdık.
        Parallel.ForEach(Enumerable.Range(0, 100000), parallelOptions, i =>
        {
          Console.WriteLine($"[Enumerable] ${i}");
        });

      }
      catch (OperationCanceledException ex)
      {
        Console.WriteLine(ex.Message);
      }



      // Senaryo yüz binlerce çalışanın aylık primlerini hesaplayı toplamda ne kadarlık bir prim ödemesi yapılmış bunu elindekiz bilgilerden program tarafında bulmak istiyoruz. 



    }



    // amaç: shared bir data, ortak kullanılan bir değişkenin, farklı threadlerin sonucu bozmadan (race condition) olmadan çözmek.
    public static void ParalelForWithThreadLocalState()
    {

      long toplamSonuc = 0;

      // kendi localinde toplayacak.
      // arka arkaya işlemlerde risk teşkil ettiğini gördük. bu sebeple Interlocked'suz bir kullanım yapmayalım.

      Parallel.For<long>(fromInclusive: 0, toExclusive: 10000, localInit: () => 0L, body: (i, state, localSum) => localSum + i, localFinally: localSum => // tüm local değerleri burada alıp eşzamanlı olarak işliyoruz.
      {
        // race condition durumu olmasın diye özel concurent bir class kullanıyoruz.

        // Console.WriteLine($"[Thread] {Thread.CurrentThread.ManagedThreadId}");

        // Interlocked.Add(ref toplamSonuc, localSum); -> buna gerek yok çünkü kendi localinde toplantığı için locallerin toplamının toplamını aldığımzıdan dolayı race condition olma ihtimali yok.
        toplamSonuc += localSum;

      });

      // 0 + 1 + 2 + 3 +4 + 5 + 6 + 7 + 8 +9  => 45
      // Beklenen result : 49995000

      Console.WriteLine($"[Thread] Local Toplam: {toplamSonuc}");


      // Amaç burada race condition durumum oluşsun.

      long toplamSonuc2 = 0;

      // kendi localinde toplayacak.
      Parallel.ForEach(Enumerable.Range(0, 10000), i =>
      {
        //Console.WriteLine($"[Thread] Not Local {Thread.CurrentThread.ManagedThreadId}");

        // Interlocked.Add(ref toplamSonuc, localSum); -> buna gerek yok çünkü kendi localinde toplantığı için locallerin toplamının toplamını aldığımzıdan dolayı race condition olma ihtimali yok.
        // bu şekilde oluşunca result istenilen değeri döndürmez. Thread local yoksa burada Interlocked gibi bir sınıfa güvenmek lazım
        Interlocked.Add(ref toplamSonuc2, i);
        // toplamSonuc2 += i;
      });

      Console.WriteLine($"[Thread] Not Local Toplam: {toplamSonuc2}");

    }

    // Birbirinden bağımsız bir şekilde olan işlemleri ise Paralel Invoke ile yönetebiliriz. Örneğin, üç farklı işlemi aynı anda yürütmek istiyorsak, Parallel.Invoke kullanarak bu işlemleri paralel olarak çalıştırabiliriz. Bu yöntem, işlemlerin birbirinden bağımsız olduğu durumlarda performansı artırabilir.


    public static void ParalelInvokeSample()
    {

      Stopwatch sp = Stopwatch.StartNew();

      // senkron methodlar
      // methodlar senkron olmasına rağmen paralel işlemk için kullanabiliriz.
      var action1 = new Action(() =>
      {
        Console.WriteLine($"[Paralel Invoke] İşlem 1 - Thread ID: {Thread.CurrentThread.ManagedThreadId}");
        Thread.Sleep(1000); // Simulate work
        Console.WriteLine($"[Paralel Invoke] İşlem 1 tamamlandı");
      });


      var action2 = new Action(() =>
      {
        Console.WriteLine($"[Paralel Invoke] İşlem 2 - Thread ID: {Thread.CurrentThread.ManagedThreadId}");
        Thread.Sleep(2000); // Simulate work
        Console.WriteLine($"[Paralel Invoke] İşlem 2 tamamlandı");
      });


      var action3 = new Action(() =>
      {
        Console.WriteLine($"[Paralel Invoke] İşlem 3 - Thread ID: {Thread.CurrentThread.ManagedThreadId}");
        Thread.Sleep(2000); // Simulate work
        Console.WriteLine($"[Paralel Invoke] İşlem 3 tamamlandı");
      });



      Parallel.Invoke(action1, action2, action3);

      sp.Start();

      Console.WriteLine($"[Paralel Invoke] Tüm işlemler tamamlandı - Thread ID: {Thread.CurrentThread.ManagedThreadId}");
      Console.WriteLine($"[Paralel Invoke] Tüm işlemler tamamlandı - Thread ID: {sp.ElapsedMilliseconds}");

    }


    // paralelForAsync, ParalelForException Handling, PLINQ ile ilgili kısım, Concurency Collections, Race Conditions



  }
}
