using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Klient
{
    class Klient
    {
        static void Main(string[] args)
        {
             
            TcpClient client_TCP = new TcpClient("localhost", 12345);
            NetworkStream strumien = client_TCP.GetStream();

            Console.WriteLine("--------------------------------- KLIENT -------------------------");
            Console.WriteLine("By pobrać plik, prosze uzyc polecenia GET plik AdresIpSerwera \n");
            string wiadomosc = Console.ReadLine();
            Metody_Klient zapytanie = new Metody_Klient("tmp");
            wiadomosc += " " + zapytanie.port_UDP;

            byte[] dane;
            dane = Encoding.ASCII.GetBytes(wiadomosc + " ALL\n");
            strumien.Write(dane, 0, dane.Length);
            Console.Write("Wyslano zapytanie o plik: {0}", wiadomosc);
            byte[] odpowiedz_surowa = new byte[256];
            string odpowiedz = null;
            

            Thread watek_odbioru = new Thread(zapytanie.Odbieranie);

            while (true) 
            {
                odpowiedz += Encoding.ASCII.GetString(odpowiedz_surowa, 0, strumien.Read(odpowiedz_surowa, 0, odpowiedz_surowa.Length));

                if ((odpowiedz.Contains("SEND")) || (odpowiedz == "Dosylane czesci"))
                {
                    if (odpowiedz.Contains("SEND"))
                    {
                        zapytanie = new Metody_Klient(odpowiedz);
                        watek_odbioru = new Thread(zapytanie.Odbieranie);
                        watek_odbioru.Start();
                    } else watek_odbioru.Resume();

                    dane = Encoding.ASCII.GetBytes("OK\n");
                    strumien.Write(dane, 0, dane.Length);
                    odpowiedz = null;
                }
                else if (odpowiedz == "END_DOWNLOAD")
                {
                    watek_odbioru.Suspend();
                    string braki = zapytanie.Sprawdzenie_poprawnosci();
                    if (braki != null)
                    {
                        dane = Encoding.ASCII.GetBytes(wiadomosc + " " + braki + "\n");
                        strumien.Write(dane, 0, dane.Length);
                        odpowiedz = null;
                    }
                    else
                    {
                        dane = Encoding.ASCII.GetBytes("END_SESSION");
                        strumien.Write(dane, 0, dane.Length);
                        string[] czesci_wiadomosci = wiadomosc.Split(' ');
                        zapytanie.Zapisywanie(czesci_wiadomosci[1]);
                        Console.WriteLine("\n\nPobrano i zapisano plik: " + czesci_wiadomosci[1]);                    
                        break;
                    }
                }
            }

            watek_odbioru.Abort();
            Console.WriteLine("\nTestowa linia");
            client_TCP.Close();
            strumien.Close();
        }

    }
}
