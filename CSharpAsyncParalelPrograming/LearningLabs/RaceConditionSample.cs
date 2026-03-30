namespace CSharpAsyncParalelPrograming.LearningLabs
{
  public class RaceConditionSample
  {

    // kilit objesi tanımlamak
    private static readonly object lockObj = new object();

    // Mutex (Thread)
    // Monitor (Thread)
    // Semaphore (Async Task)
    // lock object
    // Interlocked (Atomic operations)
    // Concurency Collections (ConcurrentDictionary, ConcurrentQueue, ConcurentBag etc.)



    // race conditio durumu simüle delim
    // aynı process içerisinde çalışırken lockObject mekanizması ile race condition durumunu engelleyebiliriz.
    // Sayısal değer değil de bir nesne üzerinde çalışırken lock mekanizması kullanmak daha mantıklı olabilir.
    public static void RaceConditionDemo()
    {

      int sayac = 0;

      Thread t1 = new Thread(() =>
      {

        for (int i = 0; i < 1000000; i++)
        {
          lock (lockObj) // sayac değişkenine eriş
            sayac++;
        }

      });


      Thread t2 = new Thread(() =>
      {

        for (int i = 0; i < 1000000; i++)
        {
          lock(lockObj)
          sayac++;
        }


      });



      t1.Start();
      t2.Start();
      t1.Join();
      t2.Join();


      Console.WriteLine($"Sayac degeri: {sayac}"); // Beklenen 11 ama bazen 10 olabilir

    }


    // Add, Increment, Decrement, Exchange, CompareExchange gibi methodlar mevcuttur. Bu methodlar, bir değişken üzerinde atomik işlemler gerçekleştirir ve
    // lock kıyasla daha hızlıdır. Değişken değerinin value type olması gerekir. Interlocked sınıfı, özellikle sayısal değerler üzerinde işlemler yaparken kullanışlıdır.

    public static void InterlockDemo()
    {

      int sayac = 0;

      Thread t1 = new Thread(() =>
      {

        for (int i = 0; i < 1000000; i++)
        {
          Interlocked.Add(ref sayac, 1);
        }

      });


      Thread t2 = new Thread(() =>
      {

        for (int i = 0; i < 1000000; i++)
        {
          Interlocked.Add(ref sayac, -1);
        }


      });



      t1.Start();
      t2.Start();
      t1.Join();
      t2.Join();


      Console.WriteLine($"Sayac degeri: {sayac}"); // Beklenen 11 ama bazen 10 olabilir

    }


    // Monitor 
    // Monitor sınıfı, lock mekanizmasının daha esnek bir versiyonudur. Monitor.Enter ve Monitor.Exit methodları ile kilitleme işlemi yapılır. Monitor, lock mekanizmasına göre daha fazla kontrol sağlar ve bazı durumlarda performans avantajı sunabilir.
    // TryEnter methodu ile zaman aşımı tanımlayarak deadlock önlenebilir. long running işlemler için daha kullanışlı olabilir. 


    // lock ile kitlemek yerine Monitor Enter ve Exit kullanılabilir. 
    public static void MonitorDemo()
    {
      int sayac = 0;
      Thread t1 = new Thread(() =>
      {
        for (int i = 0; i < 1000000; i++)
        {
          Monitor.Enter(lockObj); // kilit alma işlemi
          try
          {
            sayac++;
          }
          finally
          {
            Monitor.Exit(lockObj); // kilit bırakma işlemi
          }
        }
      });

    }


    public static void MonitorDemo2()
    {
      int sayac = 0;

      object monitorObject = new object();

      // aynı process içerisinde çalışırken lockObject
      Thread t1 = new Thread(() =>
      {

        try
        {
          for (int i = 0; i < 1000000; i++)
          {

            bool lockTaken = false;

            // 1000 ms boyunca kilitleme yapmaya çalış, eğer kilit alınamazsa işlemi atla
            Monitor.TryEnter(monitorObject, 1000, ref lockTaken); // kilit alma işlemi

            if (lockTaken)
            {
              sayac++;
            }
            else
            {
              Console.WriteLine("Kilit alınamadı, işlemi atla" + sayac);
            }

          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("hata oluştu" + ex.Message);
        }
        finally
        {
          Monitor.Exit(monitorObject);
        }

       
      });

      t1.Start();
      t1.Join();

      Console.WriteLine($"[Monitor] sayaç {sayac}");

    }



    // Mutex -> Mutual Exclusion: Mutex, birden fazla thread veya process arasında paylaşılan bir kaynağa erişimi kontrol etmek için kullanılan bir senkronizasyon aracıdır. Mutex, sadece tek bir thread veya process'in belirli bir anda belirli bir kaynağa erişmesine izin verir. Mutex, özellikle farklı process'ler arasında senkronizasyon sağlamak için kullanılır. Burada ise birden fazla thread'in aynı kaynağa erişmeye çalıştığı durumlarda kullanılır. Mutex, lock mekanizmasına göre daha ağır bir yapıya sahiptir ve genellikle inter-process senkronizasyonu için tercih edilir.
    // Not: Asenkron Task işlemleri için tercih edilmez. Thread OS düzeyinde uygulanır. 
    // Örnek: Bilgisayarda çalışan 2 farklı exe'nin aynı dosyaya veya kaynağa aynı anda erişmesini enegllemek istersek çok mantıklı olur. 

    public static void MutexDemo()
    {
      using Mutex m = new Mutex(initiallyOwned:false,name:null);


      // Sonuç: 100 farklı thread aynı paylaimli kaynak ile birbirlerin izole paylaimli kaynağı bozmdan çalıabiliyor. 
      int paylasilanKaynak = 0;


      Thread[] threads = new Thread[100]; // Beklenti 10 -> her biri içerisinde 10 kez döner.
      for (int i = 0; i < threads.Length; i++)
      {
        int idx = i;
        threads[idx] = new Thread(() => { 
        
         m.WaitOne(); // Mutex kilidini al
          try
          {
            paylasilanKaynak++;
            //Console.WriteLine($"[Thread {idx} ${Thread.CurrentThread.ManagedThreadId}");
            //Console.WriteLine($"Thread {idx} paylaşılan kaynağı güncelledi: {paylasilanKaynak}");
          }
          finally
          {
            m.ReleaseMutex(); // Mutex kilidini bırak
          }

        });

        threads[idx].Start();

      }


      foreach (var item in threads)
      {
        item.Join();
      }


      Console.WriteLine($"Son paylaşılan kaynak değeri: {paylasilanKaynak}"); // Beklenen 10  


    }



    // Birden fazla process'in olduğu asenkron operasyonlar için hem hız, hemde kaynak tüketiminin verimliliği açısından o zaman semaphoreslim.
    // N sayıda thread aynı anda belilri bir adet kadar işlem yapabilir. Mutex (1,1) -> 100 adet birbirinden izole olan thread OS düzyinde aynı veri kümesi ile çalıştırıyordu. Ama SemaphoreSlim ise (4, 25) -> aynı operasyon altında 3 farklı thread ile işi paralelde yürütüp daha az adımda asenkron olarak bunu dinamik bir şekilde ayarlıyabiliyor. 
    // 4 farklı task birbirinde izole olarak 25'lik işlemleri çalıştıracak şekilde bir yapıda çalışsın. 
    // OS düzeyinde bir kilitleme mekanizması yoksa, sadece In-process uygulama düzeyinde bir işlem yapıalcak ise mantıklı ve hızlı bir çözüm.
    // Senaryo olarak: Bir api endpointe aynı anda eş zamanlı olarak 5 adet istek yapılabilsin. (Throttling)

    public static async Task SemaphoreSlimSample()
    {

    
      using SemaphoreSlim semaphoreSlim = new SemaphoreSlim(initialCount: 4,maxCount: 8);

      // 100 birim masamız var
      // ilk 4 müşteri masaları rezerve etmiş. direk oturabilir.
      // geri kalan 96 müşteriden fazla müşterinin oturabilmesi için masaların boşalmasını beklemesi gerekir.
      // bekletme işlemi sadece masalar dolu ise hiç boşta masa yoksa yapılıyor. 
      // boş masa varsa bu restoran maksimum 8 müşterinin siparişini aynı anda alabilir. 

      Task[] gorevler = Enumerable.Range(0, 100).Select(async i =>
      {

        await semaphoreSlim.WaitAsync(); // slot boşalana kadar beklet, kilit mekanizması

        try
        {
          Console.WriteLine($"[Semaphore] Görev-{i}, Kalan Slot: {semaphoreSlim.CurrentCount}");
          await Task.Delay(300);

        } 
        finally
        {
          semaphoreSlim.Release(); // slotu serberst bırak.
        }


      }).ToArray();


      await Task.WhenAll(gorevler);

    }






  }
}
