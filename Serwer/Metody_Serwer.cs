using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace Serwer
{
    class Paczka
    {
        public uint poczatek { get; set; }
        public uint koniec { get; set; }

        public Paczka(uint od_, uint do_)
        {
            this.poczatek = od_;
            this.koniec = do_;
        }
    }

    class Metody_Serwer
    {
        public string nazwa_pliku { get; set; }
        public int rozmiar_paczki { get; set; }
        public long rozmiar_pliku { get; set; }
        public string adres_UDP { get; set; }
        public int port_UDP { get; set; }
        public int blad { get; set; }
        public List<Paczka> braki{get; set;}
        public bool czy_caly_plik { get; set; }


        public Metody_Serwer(string dane, string lokalizacja)
        {
            this.rozmiar_paczki = 16000;
            this.blad = 0;
            this.czy_caly_plik = true;

            string[] zapytanie = dane.Split(' ');
            if (zapytanie.Length != 5) this.blad = 1;
            if (!zapytanie[0].Contains("GET")) this.blad = 2;

            nazwa_pliku = lokalizacja + @"\" + zapytanie[1]; //Path.Combine(lokalizacja, zapytanie[1]);
            if (File.Exists(nazwa_pliku)) this.rozmiar_pliku = new FileInfo(nazwa_pliku).Length; else this.blad = 3;

            try { this.adres_UDP = zapytanie[2]; } catch { this.blad = 4; }
            try { this.port_UDP = int.Parse(zapytanie[3]); } catch { this.blad = 5; }
            try { this.braki = Brakujace_czesci(zapytanie[4]); } catch { this.blad = 6; }
        }

        private List<Paczka> Brakujace_czesci(string braki)       //tworzenie paczek
        {
            var wynik = new List<Paczka>();

            if (braki.Contains("ALL"))
            wynik.Add(new Paczka(0, (uint)((rozmiar_pliku) / rozmiar_paczki)));
            else
            {
                czy_caly_plik = false;
                string[] czesci = braki.Split(',');

                for (int i = 0; i < czesci.Length; i++)
                {
                      int pozycja = czesci[i].IndexOf('-');

                     if (pozycja == -1)
                    wynik.Add(new Paczka(uint.Parse(czesci[i]), uint.Parse(czesci[i])));
                    else wynik.Add(new Paczka(uint.Parse(czesci[i].Substring(0, pozycja)), uint.Parse(czesci[i].Substring(pozycja + 1))));
                }
            }
            return wynik;
        }

        public void Wysyłanie()
        {
            UdpClient client_UDP = new UdpClient();
            client_UDP.Connect(adres_UDP, port_UDP);

            byte[] bufor = new byte[rozmiar_paczki + 4];

            Mutex blokada_pliku = new Mutex(false, "Blokada-pliku-asdfgqwer5678-7490g93-09495");

            blokada_pliku.WaitOne(); //ciągle otwieranie pliku nie jest efektywne, jednak zapis całości do tablicy wymagałoby polecenia od klienta w celu jej zwolnienia po zakończeniu przesyłu, co spowodowałoby utworzenie komunikajci połączeniowej, a jest to sprzeczne z ideą UDP
                FileStream plik = File.Open(nazwa_pliku, FileMode.Open);
                using (BinaryReader br = new BinaryReader(plik))
                {
                    for (int i = 0; i < braki.Count(); i++)
                        for (uint indeks = braki[i].poczatek; indeks <= braki[i].koniec; indeks++)
                        {
                            for (int k = 0; k < 4; k++) bufor[k] = (byte)(indeks >> (k * 8));
                            plik.Seek(indeks * rozmiar_paczki, SeekOrigin.Begin);
                            int rozmiar = br.Read(bufor, 4, rozmiar_paczki);
                            client_UDP.Send(bufor, rozmiar + 4);
                        }                   
                }
                client_UDP.Close();
            blokada_pliku.ReleaseMutex();
        }

        public string Odpowiedz()
        {
            if (blad != 0) return "Blad nr " + blad + ": " + Obsluga_bledu();
            if (czy_caly_plik == false) return "Dosylane czesci";

            return "SEND " + rozmiar_pliku + " " + rozmiar_paczki;
        }

        private string Obsluga_bledu()
        {
            switch (blad)
            {
                case 1: return "Nieprawidlowa skladnia zapytania\n";
                case 2: return "Nieprawidlowe polecenie\n";
                case 3: return "Zadany plik nie istnieje\n";
                case 4: return "Nieprawidlowy adres IP\n";
                case 5: return "Nieprawidlowy port UDP\n";
                case 6: return "Nieprawidlowo zdefiniowane brakujace czesci\n";
                default: return "Blad nieznany/nieobslugiwany\n";
            }
        }
    }
}
