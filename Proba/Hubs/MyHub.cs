using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Proba.Models;

namespace Proba.Hubs
{
    public class MyHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }

        private IEPBazaEntities db = new IEPBazaEntities();

        public void Posalji(int IDKorisnik, string poruka, string IDKanal)
        {
            Korisnik korisnik = db.Korisnik.Find(IDKorisnik);
            Kanal kanal = db.Kanal.Find(IDKanal);

            if (kanal.Status == "Zatvoren")
            {
                Clients.Group("" + IDKanal).kanalZatvoren();
                return;
            }

            Pitanje pitanje = new Pitanje();
            pitanje.IDPitanje = Guid.NewGuid().ToString();
            pitanje.IDKanal = IDKanal;
            pitanje.Autor = IDKorisnik;
            pitanje.Tekst = poruka;
            pitanje.DatumPravljenja = DateTime.Now;
            pitanje.Status = "Otkljucano";
            pitanje.Naslov = null;
            pitanje.Slika = null;
            pitanje.DatumZakljucavanja = null;
            pitanje.EkstenzijaSlike = null;
            pitanje.IDKategorija = null;

            db.Pitanje.Add(pitanje);
            db.SaveChanges();

            Clients.Group("" + IDKanal).prikaziPoruku(korisnik.Ime, poruka, korisnik.Tip);
        }

        public void PrikljuciSe(string IDKanal, int IDKorisnik)
        {
            Kanal kanal = db.Kanal.Find(IDKanal);
            if (kanal == null || kanal.Status == "Zatvoren")
            {
                Clients.Group("" + IDKanal).kanalZatvoren();
                return;
            }

            Korisnik korisnik = db.Korisnik.Find(IDKorisnik);

            if (korisnik.Tip == "Agent" && kanal.Korisnik1.Contains(korisnik) == false)
            {
                kanal.Korisnik1.Add(korisnik);
                db.SaveChanges();
            }
            this.Groups.Add(this.Context.ConnectionId, "" + IDKanal);
        }

        public void Napusti(string IDKanal, int IDKorisnik)
        {
            Kanal kanal = db.Kanal.Find(IDKanal);
            if (kanal.Status == "Zatvoren")
            {
                Clients.Group("" + IDKanal).kanalZatvoren();
                return;
            }

            Korisnik korisnik = db.Korisnik.Find(IDKorisnik);
            if (kanal.Korisnik1.Contains(korisnik))
            {
                kanal.Korisnik1.Remove(korisnik);
                db.SaveChanges();
            }
            this.Groups.Remove(this.Context.ConnectionId, "" + IDKanal);
        }
    }
}