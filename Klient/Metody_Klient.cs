using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Klient
{
    class Metody_Klient
    {
        public byte[][] czesci { get; set; }
        public int port_UDP { get; set; }
        public int rozmiar_pliku { get; set; }
        public int rozmiar_paczki { get; set; }       
        public int liczba_paczek { get; set; }
        public byte dodatkowe_bajty { get; set; }       

        public Metody_Klient(string wiadomość)
        {
            this.port_UDP = 11001;
            this.dodatkowe_bajty = 4;
            string[] dane = wiadomość.Split(' ');
            if (dane[0] == "SEND")
            {
                this.rozmiar_pliku = Convert.ToInt32( dane[1]);
                this.rozmiar_paczki = Convert.ToInt32( dane[2]);

                if (this.rozmiar_pliku / this.rozmiar_paczki > 0) this.liczba_paczek = this.rozmiar_pliku / this.rozmiar_paczki; else this.liczba_paczek = 1;
                float liczba_p = (float)rozmiar_pliku / (float)rozmiar_paczki;
                if (liczba_p > (float)liczba_paczek) liczba_paczek++;
                this.czesci = new byte[this.liczba_paczek][];   
            }           
        }

        public void Odbieranie()
        {
               UdpClient client_UDP = new UdpClient(port_UDP);
               IPEndPoint punkt_koncowy = new IPEndPoint(IPAddress.Any, port_UDP); 

               byte[] bufor = new byte[4];
               while (true)
               {
                   var czesc = client_UDP.Receive(ref punkt_koncowy);

                   for (int i = 0; i < 4; i++)
                       bufor[i] = czesc[i];

                   int nr = BitConverter.ToInt32(bufor, 0);

                   czesci[nr] = new byte[czesc.Length - 4];

                   for (int i = 4; i < czesc.Length; i++)
                       czesci[nr][i - 4] = czesc[i]; 
               }         
        }

        public void Zapisywanie(string nazwa_pliku)
        {
            byte[] plik = new byte[rozmiar_pliku]; 
            int indeks = 0;
            for (int i = 0; i < czesci.Length; i++)
            {
                for (int j = 0; j < czesci[i].Length; j++)
                {
                    plik[indeks] = czesci[i][j];
                    indeks++;
                }
            }
            File.WriteAllBytes("..\\..\\POBRANE\\ " + nazwa_pliku, plik);          
        }

        public string Sprawdzenie_poprawnosci()
        {
            string braki = null;

            for (int i = 0; i < czesci.GetLength(0); i++)
                if (czesci[i] == null) braki += i + ",";

            if(braki!= null) braki = braki.Substring(0, braki.Length - 1);

            return braki;
        }
    }
}
