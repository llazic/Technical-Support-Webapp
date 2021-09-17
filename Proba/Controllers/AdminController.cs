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
    public class AdminController : Controller
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
            odgovor.KorenoPitanje = IDPitanje;
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

        public ActionResult MenjajSistem(string message = null)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Vrednosti silver, gold, platinum, kanal;
            silver = db.Vrednosti.Find("Silver");
            gold = db.Vrednosti.Find("Gold");
            platinum = db.Vrednosti.Find("Platinum");
            kanal = db.Vrednosti.Find("KomunikacioniKanal");

            var tuple = new Tuple<Vrednosti, Vrednosti, Vrednosti, Vrednosti>(silver, gold, platinum, kanal);
            ViewBag.Message = message;
            return View(tuple);
        }

        public ActionResult SacuvajSistem(Vrednosti silver, Vrednosti gold, Vrednosti platinum, Vrednosti kanal)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            if (silver.Cena <= 0 || gold.Cena <= 0 || platinum.Cena <= 0 || kanal.Cena < 0 || silver.BrojTokena < 0 || gold.BrojTokena < 0 || platinum.BrojTokena < 0)
            {
                return RedirectToAction("MenjajSistem", new { message = "Vrednosti polja ne smeju biti manje od 0." });
            }

            db.Entry(silver).State = EntityState.Modified;
            db.Entry(gold).State = EntityState.Modified;
            db.Entry(platinum).State = EntityState.Modified;
            db.Entry(kanal).State = EntityState.Modified;

            db.SaveChanges();

            return RedirectToAction("MenjajSistem", new { message = "Uspešno ste promenili vrednosti." });
        }

        public ActionResult IzmeniKorisnika(int? id)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Korisnik korisnik = db.Korisnik.Find(id);
            if (korisnik == null)
            {
                return HttpNotFound();
            }
            return View(korisnik);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IzmeniKorisnika([Bind(Include = "IDKorisnik,Ime,Prezime,Email,Lozinka,BrojTokena,Status,Tip")] Korisnik korisnik)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            if (ModelState.IsValid)
            {
                db.Entry(korisnik).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { message = "Uspešno ste izmenili korisnika." });
            }
            return View(korisnik);
        }

        public ActionResult NapraviKategoriju(string message = null)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            ViewBag.Message = message;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NapraviKategoriju([Bind(Include = "IDKategorija,Naziv")] Kategorija kategorija)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Kategorija vecPostoji = db.Kategorija.Where(k => k.Naziv == kategorija.Naziv).SingleOrDefault();
            if (vecPostoji != null)
            {
                return RedirectToAction("NapraviKategoriju", new { message = "Već postoji kategorija sa tim nazivom." });
            }


            if (ModelState.IsValid)
            {
                db.Kategorija.Add(kategorija);
                db.SaveChanges();
                return RedirectToAction("Index", new { message = "Uspešno ste napravili kategoriju." });
            }

            return View(kategorija);
        }

        // GET: Korisnik/Create
        public ActionResult NapraviAgenta()
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            return View();
        }

        // POST: Korisnik/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NapraviAgenta([Bind(Include = "IDKorisnik,Ime,Prezime,Email,Lozinka,BrojTokena,Status,Tip")] Korisnik korisnik)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            korisnik.BrojTokena = 0;
            korisnik.Status = "Aktivan";
            korisnik.Tip = "Agent";
            korisnik.Lozinka = Encryption.EncryptDecrypt(korisnik.Lozinka);

            if (ModelState.IsValid)
            {
                Korisnik postoji = db.Korisnik.Where(k => k.Email == korisnik.Email).SingleOrDefault();
                if (postoji != null)
                {
                    ViewBag.Message = "Uneti email je već korišćen. Molimo Vas unesite nekorišćeni email.";
                    return View();
                }

                db.Korisnik.Add(korisnik);
                db.SaveChanges();
                return RedirectToAction("Index", new { message = "Uspešno ste se napravili agenta!" });
            }

            return View();
        }

        public ActionResult Korisnici()
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            var tuple = new Tuple<IEnumerable<Korisnik>, Korisnik>(db.Korisnik.ToList(), ulogovan);
            return View(tuple);
        }

        public ActionResult ObrisiPitanje(string id)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            if (id != null)
            {
                var odgovoriZaBrisanje = db.Odgovor.Where(o => o.KorenoPitanje == id);

                foreach (var odg in odgovoriZaBrisanje)
                {
                    var ocenioZaBrisanje = db.Ocenio.Where(o => o.IDOdgovor == odg.IDOdgovor);
                    db.Ocenio.RemoveRange(ocenioZaBrisanje);
                }

                db.Odgovor.RemoveRange(odgovoriZaBrisanje);

                Pitanje pitanje = db.Pitanje.Find(id);
                db.Pitanje.Remove(pitanje);
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { message = "Uspešno ste obrisali pitanje sa svim njegovim odgovorima." });
        }

        public ActionResult ObrisiOdgovor(int? id)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            if (id != null)
            {
                IQueryable<Odgovor> odgovoriNaOdgovor = db.Odgovor.Where(o => o.IDOdgovor == id);

                while (odgovoriNaOdgovor != null)
                {
                    IQueryable<Odgovor> noviOdgovori = null;
                    foreach (var odg in odgovoriNaOdgovor)
                    {
                        var ocenioZaBrisanje = db.Ocenio.Where(o => o.IDOdgovor == odg.IDOdgovor);
                        db.Ocenio.RemoveRange(ocenioZaBrisanje);

                        if (noviOdgovori == null)
                            noviOdgovori = db.Odgovor.Where(o => o.Odgovoreni == odg.IDOdgovor);
                        else
                            noviOdgovori = noviOdgovori.Union(db.Odgovor.Where(o => o.Odgovoreni == odg.IDOdgovor));
                    }

                    db.Odgovor.RemoveRange(odgovoriNaOdgovor);
                    odgovoriNaOdgovor = noviOdgovori;
                }

                db.SaveChanges();
            }
            return RedirectToAction("Index", new { message = "Uspešno ste obrisali odgovor sa svim njegovim odgovorima." });
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
                case "Agent":
                    return RedirectToAction("Index", "Agent");
                case "Klijent":
                    return RedirectToAction("Index", "Klijent");
            }
            return null;
        }
    }
}