using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Proba.Models;

[assembly: OwinStartup(typeof(Proba.Startup))]

namespace Proba
{
    public class Startup
    {
        IEPBazaEntities db = new IEPBazaEntities();

        public void Configuration(IAppBuilder app)
        {
            Vrednosti silver = db.Vrednosti.Find("Silver");
            if (silver == null)
            {
                silver = new Vrednosti() {
                    Naziv = "Silver",
                    Cena = 1,
                    BrojTokena = 1
                };
                db.Vrednosti.Add(silver);
            }
            Vrednosti gold = db.Vrednosti.Find("Gold");
            if (gold == null)
            {
                gold = new Vrednosti()
                {
                    Naziv = "Gold",
                    Cena = 2,
                    BrojTokena = 2
                };
                db.Vrednosti.Add(gold);
            }
            Vrednosti platinum = db.Vrednosti.Find("Platinum");
            if (platinum == null)
            {
                platinum = new Vrednosti()
                {
                    Naziv = "Platinum",
                    Cena = 3,
                    BrojTokena = 3
                };
                db.Vrednosti.Add(platinum);
            }
            Vrednosti kanal = db.Vrednosti.Find("KomunikacioniKanal");
            if (kanal == null)
            {
                kanal = new Vrednosti()
                {
                    Naziv = "KomunikacioniKanal",
                    Cena = 0,
                    BrojTokena = null
                };
                db.Vrednosti.Add(kanal);
            }

            Korisnik admin = db.Korisnik.Where(a => a.Tip == "Admin").SingleOrDefault();
            if (admin == null)
            {
                admin = new Korisnik() {
                    Ime = "Admin",
                    Prezime = "Admin",
                    Email = "admin@admin.com",
                    Lozinka = "UVW",
                    BrojTokena = 0,
                    Status = "Aktivan",
                    Tip = "Admin"
                };
                db.Korisnik.Add(admin);
            }

            db.SaveChanges();
            app.MapSignalR();
        }
    }
}
