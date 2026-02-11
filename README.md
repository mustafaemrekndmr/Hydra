# Unity Underwater Simulation Framework

Bu proje su altı için tasarlanmış yüksek sadakatli bir simülasyon ortamıdır. Unity oyun motoru üzerinde geliştirilen bu framework, gerçekçi hidro-fizik modellemeleri ve modüler bir mimari sunar.

## Proje Özeti

Simülasyon, kullanıcıya hem kontrollü ortamları (havuz) hem de zorlu doğal koşulları (okyanus) sağlayarak araç dinamiği ve kontrollerinin test edilmesine ve deneyimlenmesine olanak tanır.

---

## Temel Özellikler

### 🛠️ Gelişmiş Fizik Motoru
*   **6-DOF Hareket Dinamiği:** Aracın 6 serbestlik derecesindeki (Surge, Sway, Heave, Roll, Pitch, Yaw) hareketleri fizik tabanlı olarak hesaplanır.
*   **Hidrodinamik Modelleme:** Su kaldırma kuvveti (Buoyancy) ve sürüklenme (Drag) katsayıları gerçekçi bir şekilde simüle edilir.
*   **Stabilizasyon Sisteleri:** Derinlik sabitleme (Depth Hold) ve otomatik dengeleme (Auto-Stabilization) algoritmaları entegre edilmiştir.

### 🌐 Simülasyon Ortamları
*   **Okyanus Ortamı (Ocean Scene):** Biyolüminesans, bulanıklık, değişken ışık koşulları ve akıntı gibi çevresel faktörlerin simüle edildiği açık deniz senaryosu.
*   **Havuz Ortamı (Pool Scene):** Net görüş mesafesi ve referans noktaları (kulvarlar) sunan, manevra kabiliyetlerinin test edildiği kontrollü iç mekan ortamı.

---

## Kontrol Şeması

Araç kontrolü, standart ROV pilotaj prensiplerine uygun olarak yapılandırılmıştır.

| Komut | Tuş Ataması | Fonksiyon |
| :--- | :---: | :--- |
| **Surge** | `W` / `S` | İleri - Geri İtiş |
| **Sway** | `A` / `D` | Yanal Kayma |
| **Heave** | `Q` / `E` | Dikey Hareket (Derinlik) |
| **Yaw** | `C` / `V` | Eksenel Dönüş |
| **Camera Tilt** | `⬆️` / `⬇️` | Kamera Açısı |
| **Depth Hold** | `Space` | Derinlik Kilidi (Oto-Pilot) |

---

## Teknik Mimari

Proje, **Separation of Concerns (SoC)** prensibi gözetilerek modüler bir yapıda geliştirilmiştir. Bu sayede farklı ortamlar ve araçlar birbirini etkilemeden geliştirilebilir.

*   `Assets/Scripts/Shared`: Temel ROV fiziği, kontrolcü mantığı ve ortak yardımcı sınıflar.
*   `Assets/Scripts/Ocean`: Okyanus sahnesine özgü çevresel efektler ve prosedürel üretim scriptleri.
*   `Assets/Scripts/Pool`: Havuz inşası ve iç mekan aydınlatma sistemleri.
*   `Assets/Scripts/UI`: Kullanıcı arayüzü ve sahne yönetimi.

---
