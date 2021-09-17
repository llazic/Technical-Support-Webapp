using Proba.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Net;

namespace Proba.Controllers
{
    public class AgentController : Controller
    {
        private IEPBazaEntities db = new IEPBazaEntities();

        // GET: Klijent
        public ActionResult Index(string message = null)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            ViewBag.Message = message;
            return View();
        }

        public ActionResult Pretraga(string tekst)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            var pitanja = db.Pitanje.Where(p => p.Naslov.Contains(tekst) && p.Kanal == null);
            return View("RezultatPretrage", pitanja.ToList());
        }

        // GET: Pitanje
        public ActionResult Pitanja(string kategorija = null, int strana = 1)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;


            IEnumerable<Pitanje> pitanje = null;
            if (kategorija != null && kategorija != "Sve" && kategorija != "Sve kategorije")
            {
                pitanje = db.Pitanje.Include(p => p.Kanal).Include(p => p.Kategorija).Include(p => p.Korisnik).Where(p => p.Kanal == null && p.Kategorija.Naziv == kategorija).OrderByDescending(p => p.DatumPravljenja);
            }
            else
            {
                pitanje = db.Pitanje.Include(p => p.Kanal).Include(p => p.Kategorija).Include(p => p.Korisnik).Where(p => p.Kanal == null).OrderByDescending(p => p.DatumPravljenja);
            }
            int BrojStrana = (pitanje.Count() - 1) / GostController.BrojPitanjaPoStrani + 1;
            pitanje = pitanje.Skip(GostController.BrojPitanjaPoStrani * (strana - 1)).Take(GostController.BrojPitanjaPoStrani);
            var kategorije = db.Kategorija.ToList();
            var tuple = new Tuple<IEnumerable<Proba.Models.Pitanje>, IEnumerable<Proba.Models.Kategorija>, string, int>(pitanje.ToList(), kategorije, kategorija, BrojStrana);
            return View(tuple);
        }

        // GET: Pitanje/Details/5
        public ActionResult PregledPitanja(string id)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pitanje pitanje = db.Pitanje.Find(id);
            if (pitanje == null)
            {
                return HttpNotFound();
            }

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            var tuple = new Tuple<Pitanje, Korisnik>(pitanje, ulogovan);
            return View(tuple);
        }

        public FileContentResult DohvatiSliku(string id)
        {
            ActionResult provera = Provera();
            if (provera != null) return null;

            Pitanje pitanje = db.Pitanje.Find(id);
            if (pitanje.Slika != null)
            {
                return File(pitanje.Slika, "image/" + pitanje.EkstenzijaSlike);
            }
            else
            {
                return File(GostController.ZnakPitanja(), "image/png");
            }
        }

        public ActionResult PregledOdgovora(int? id)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Odgovor odgovor = db.Odgovor.Find(id);
            if (odgovor == null)
            {
                return HttpNotFound();
            }
            return View(odgovor);
        }

        public ActionResult OtkljucajPitanje(string IDPitanje)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Pitanje pitanje = db.Pitanje.Find(IDPitanje);
            if (pitanje == null) return RedirectToAction("Index");

            pitanje.Status = "Otkljucano";
            db.SaveChanges();
            return RedirectToAction("Index", new { message = "Uspešno ste otključali pitanje." });
        }

        public ActionResult Logout()
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Session["Ulogovan"] = null;
            return RedirectToAction("Index", "Gost", new { message = "Uspešno ste se odjavili." });
        }

        // GET: Korisnik/Details/5
        public ActionResult PregledProfila()
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];

            return View(ulogovan);
        }

        public ActionResult NapraviOdgovor(string IDPitanje, int? Odgovoreni)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Pitanje pitanje = db.Pitanje.Find(IDPitanje);
            if (pitanje == null || pitanje.Status == "Zakljucano" || pitanje.IDKanal != null)
            {
                return RedirectToAction("Index");
            }
            Odgovor odgovor = db.Odgovor.Find(Odgovoreni);

            var tuple = new Tuple<Pitanje, Odgovor>(pitanje, odgovor);
            return View(tuple);
        }

        public ActionResult SacuvajOdgovor(string IDPitanje, int? Odgovoreni, string Tekst)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Pitanje pitanje = db.Pitanje.Find(IDPitanje);
            if (pitanje == null || pitanje.Status == "Zakljucano" || pitanje.IDKanal != null)
            {
                return RedirectToAction("Index");
            }

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];

            Odgovor odgovor = new Odgovor();
            odgovor.BrojNegativnihOcena = 0;
            odgovor.BrojPozitivnihOcena = 0;
            odgovor.DatumPravljenja = DateTime.Now;
            odgovor.IDKorisnik = ulogovan.IDKorisnik;
            if (Odgovoreni != -1) odgovor.Odgovoreni = (int)Odgovoreni;
            else odgovor.Odgovoreni = null;
            odgovor.KorenoPitanje = (string)IDPitanje;
            odgovor.Tekst = Tekst;

            db.Odgovor.Add(odgovor);
            db.SaveChanges();

            if (Odgovoreni == -1)
            {
                return RedirectToAction("PregledPitanja", new { id = IDPitanje });
            }
            else
            {
                return RedirectToAction("PregledOdgovora", new { id = Odgovoreni });
            }
        }

        public string Oceni(int? IDOdgovor, string vrsta)
        {
            ActionResult provera = Provera();
            if (provera != null) return "Nedozvoljeni pristup.";
            if (IDOdgovor == null) return "";
            if (vrsta != "Pozitivna" && vrsta != "Negativna") return "";


            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];


            Ocenio ocenio = db.Ocenio.Where(o => o.IDKorisnik == ulogovan.IDKorisnik && o.IDOdgovor == IDOdgovor).FirstOrDefault();
            if (ocenio == null)
            {
                Ocenio novi = new Ocenio();
                novi.IDKorisnik = ulogovan.IDKorisnik;
                novi.IDOdgovor = (int)IDOdgovor;
                novi.Ocena = vrsta;

                db.Ocenio.Add(novi);

                Odgovor odgovor = db.Odgovor.Find(IDOdgovor);
                switch (vrsta)
                {
                    case "Pozitivna":
                        odgovor.BrojPozitivnihOcena++;
                        break;
                    case "Negativna":
                        odgovor.BrojNegativnihOcena++;
                        break;
                }

                db.SaveChanges();
                return "true";
            }
            return "false";

        }
        
        public ActionResult Kanali()
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            var kanal = db.Kanal.Where(k => k.Status == "Otvoren").Include(k => k.Korisnik).Include(k => k.Korisnik1);
            return View(kanal.ToList());
        }

        public ActionResult Kanal(string id)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];

            Kanal najskorijiKanal = db.Kanal.Find(id);
            IEnumerable<Pitanje> pitanja = null;
            if (najskorijiKanal != null && najskorijiKanal.Status == "Otvoren")
            {
                pitanja = db.Pitanje.Where(p => p.IDKanal == najskorijiKanal.IDKanal).OrderBy(p => p.DatumPravljenja);
            }

            var tuple = new Tuple<Korisnik, Kanal, IEnumerable<Pitanje>>(ulogovan, najskorijiKanal, pitanja);
            return View(tuple);
        }


        private ActionResult Provera()
        {
            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            if (ulogovan == null)
            {
                return RedirectToAction("Index", "Gost");
            }
            switch (ulogovan.Tip)
            {
                case "Admin":
                    return RedirectToAction("Index", "Admin");
                case "Klijent":
                    return RedirectToAction("Index", "Klijent");
            }
            return null;
        }
    }
}