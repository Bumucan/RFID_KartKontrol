Ardunio IDE 

#include <SPI.h>
#include <MFRC522.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <WiFiClient.h>

// --- AYARLAR ---
const char* ssid = "...";           // Wi-Fi Adın
const char* password = "...";     // Wi-Fi Şifren
String serverIP = "...";         // ADIM 2'de bulduğun IPv4 Adresin (Bilgisayarın IP'si)
int serverPort = ...;                    // Visual Studio çalışınca açılan port (Genelde 5000-5200 arası)
// ----------------

#define SS_PIN D4
#define RST_PIN D3

MFRC522 mfrc522(SS_PIN, RST_PIN);

void setup() {
  Serial.begin(9600);
  SPI.begin();
  mfrc522.PCD_Init();

  // Wi-Fi Bağlantısı
  Serial.println();
  Serial.print("WiFi Baglaniyor: ");
  Serial.println(ssid);
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.println("WiFi Baglandi!");
  Serial.println(WiFi.localIP());
  Serial.println("Kart okutmaya hazir...");
}

void loop() {
  // Yeni kart kontrolü
  if (!mfrc522.PICC_IsNewCardPresent()) return;
  if (!mfrc522.PICC_ReadCardSerial()) return;

  // UID'yi String'e çevir (Aradaki boşlukları kaldırarak)
  String kartUID = "";
  for (byte i = 0; i < mfrc522.uid.size; i++) {
    if(mfrc522.uid.uidByte[i] < 0x10) {
      kartUID += "0";
    }
    kartUID += String(mfrc522.uid.uidByte[i], HEX);
  }
  kartUID.toUpperCase(); // Hepsini büyük harf yap (Örn: 94CEF44)

  Serial.print("Okunan UID: ");
  Serial.println(kartUID);

  // Sunucuya gönder
  SendCardID(kartUID);

  // Kartı tekrar okumasın diye biraz bekle ve kartı durdur
  mfrc522.PICC_HaltA();
  mfrc522.PCD_StopCrypto1();
  delay(2000); // 2 saniye bekle
}

void SendCardID(String uid) {
  if (WiFi.status() == WL_CONNECTED) {
    WiFiClient client;
    HTTPClient http;

    // URL Oluştur
    String url = "http://" + serverIP + ":" + String(serverPort) + "/Rfid/Okut?uid=" + uid;
    
    Serial.print("Istek gonderiliyor: ");
    Serial.println(url);

    http.begin(client, url);
    int httpCode = http.GET();

    if (httpCode > 0) {
      String payload = http.getString(); // Sunucudan gelen cevap
      Serial.println("Sunucu Cevabi: " + payload);
      
      // -- SENARYOLAR --
      if (payload == "UID_YOK") {
        Serial.println(">> Admin Onayi Bekleniyor (Yetkisiz Kart)");
        // Buraya LCD kodu eklenebilir: lcd.print("Admin Onayi Bekle");
      } 
      else if (payload == "PERSONEL_YOK") {
        Serial.println(">> Kart Yetkili ama Personel Atanmamis");
      }
      else if (payload.startsWith("OK_GIRIS")) {
        // Gelen format: OK_GIRIS_AhmetYilmaz
        String isim = payload.substring(9); // "OK_GIRIS_" sonrasını al
        Serial.println(">> HOSGELDIN " + isim);
        // lcd.print("Hosgeldin"); lcd.setCursor(0,1); lcd.print(isim);
      }
      else if (payload.startsWith("OK_CIKIS")) {
        // Gelen format: OK_CIKIS_AhmetYilmaz
        String isim = payload.substring(9);
        Serial.println(">> GORUSURUZ " + isim);
      }
      else {
        Serial.println(">> Bilinmeyen Hata");
      }
      
    } else {
      Serial.println("Hata: Sunucuya baglanilamadi");
    }
    http.end();
  } else {
    Serial.println("Hata: WiFi bagli degil");
  }
}
