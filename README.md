# YMH219 - Flappy Bird (GDI+ & OOP)

Nesne Tabanlı Programlama (YMH 219) dersi kapsamında .NET 8.0 Windows Forms ve GDI+ kullanılarak geliştirilmiş, sıfır harici dosya bağımlılığına sahip masaüstü Flappy Bird oyunudur.

## Öğrenci Bilgileri
- **Ad Soyad:** Selahattin Ali Kılıç
- **Öğrenci No:** 250541036
- **Ders:** YMH 219 - Nesne Tabanlı Programlama

## Kullanılan Teknoloji / NuGet Paketleri ve OOP Mimarisi
- **Platform:** .NET 8.0 Windows Forms (`net8.0-windows`)
- **Grafik Motoru:** GDI+ (`System.Drawing`, `DoubleBufferedPanel`)
- **Harici NuGet Paketi:** Yok (yalnızca standart .NET SDK bileşenleri)
- **OOP Mimarisi:**
  - **Abstraction & Interfaces:** `IDrawable`, `IUpdatable`, `ICollidable`, `IHighScoreRepository` arayüzleri ile `GameObject` soyut temel sınıfı (`Models/GameObject.cs`).
  - **Inheritance & Polymorphism:** `Bird`, `PipePair` ve `Cloud` sınıfları `GameObject` sınıfından türer ve `Update(float)` / `Draw(Graphics)` / `IntersectsWith(RectangleF)` metotlarını kendi davranışlarına göre ezer (`override`).
  - **Encapsulation:** Konum, boyut, hız ve boru boşluğu alanları kontrollü özellikler (`property`) üzerinden korunur.
  - **Event-Driven Design:** `ScoreChangedEventArgs`, `GameOverEventArgs` ve `GameStateChangedEventArgs` özel olay sınıfları ile `GameEngine` arayüz katmanından ayrıştırılmıştır.

## Kurulum ve Çalıştırma Adımları
```bash
cd YMH219-FlappyBird
dotnet build
dotnet run
```

## Uygulamanın Temel Özellikleri
- `Space`, `Yukarı Ok` veya fare tıklaması ile yer çekimine karşı zıplama fiziği.
- Sağdan sola akan ve ekran dışına çıktıkça rastgele yüksekliklerle yeniden konumlanan 3 adet boru çifti (`PipePair`).
- Her 5 skorda otomatik artan oyun hızı ve daralan boru aralığı dengesi.
- En yüksek skorun `highscore.txt` dosyasında kalıcı olarak saklanması (`try-catch` hata korumalı).
- Titreşimsiz (`DoubleBuffered`) GDI+ çizim, "Game Over" ekranı, `Yeni Oyun / Yeniden Başlat` butonu ve `R` kısayolu.

## Ekran Görüntüleri
![Ekran 1](screenshots/ekran1.png)
![Ekran 2](screenshots/ekran2.png)

## Bilinen Sınırlamalar / Geliştirme Notları
- Uygulama yalnızca Windows işletim sistemi üzerinde (`net8.0-windows`) çalışır.
- Tüm görseller harici resim dosyası gerektirmeden GDI+ vektör çizimleri ile üretilmektedir.
