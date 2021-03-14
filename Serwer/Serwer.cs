using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Serwer
{
    class Serwer
    {
        public int port_TCP;
        public string lokalizacja;

        public Serwer(int port_TCP, string lokalizacja)
        {
            this.port_TCP = port_TCP;
            this.lokalizacja = lokalizacja;
        }

        static void Main(string[] args)
        {
            Serwer serwer = new Serwer(12345, @"..\..\..\PLIKI SERWER");
            TcpListener nasluchiwacz = new TcpListener(IPAddress.Any, serwer.port_TCP);
            TcpClient aktywny_klient = null;
            nasluchiwacz.Start();
            Console.WriteLine("----------------------------- SERWER -------------------------------");
            while (true)
            {
                TcpClient klient = nasluchiwacz.AcceptTcpClient();
                if (aktywny_klient != klient)
                {
                    Console.WriteLine("\nSerwer zauwazyl klienta\n");
                    aktywny_klient = klient;
                    NetworkStream strumien = klient.GetStream();

                    new Thread(() =>
                    {
                        string dane = null;
                        Metody_Serwer zapytanie = null;
                        byte[] wiadomosc;
                        byte[] bity = new byte[256];

                        while (true)
                        {

                            dane += Encoding.ASCII.GetString(bity, 0, strumien.Read(bity, 0, bity.Length));

                            if (dane.IndexOf('\n') != -1 && dane.Substring(0, 3) == "GET")
                            {

                                zapytanie = new Metody_Serwer(dane.Substring(0, dane.Count() - 1), serwer.lokalizacja);

                                Console.WriteLine("\n{0} ", zapytanie.Odpowiedz());
                                Console.WriteLine("{0}  ", dane.Count() - 5);
                                wiadomosc = Encoding.ASCII.GetBytes(zapytanie.Odpowiedz());
                                strumien.Write(wiadomosc, 0, wiadomosc.Length);
                                dane = null;
                            }
                            if (dane == "OK\n")
                            {
                                zapytanie.Wysyłanie();
                                wiadomosc = Encoding.ASCII.GetBytes("END_DOWNLOAD");
                                Thread.Sleep(2000);
                                strumien.Write(wiadomosc, 0, wiadomosc.Length);
                                dane = null;
                            }
                            else if (dane == "END_SESSION")
                            {
                                break;
                            }
                        }

                        strumien.Close();
                        klient.Close();
                        Console.WriteLine("\nKlient otrzymal zadany plik w calosci");
                    }).Start();
                }
            }

        }

    }
}
