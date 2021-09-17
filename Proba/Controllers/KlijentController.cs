using Proba.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Net;
using IEPProjekat.PayPal;

namespace Proba.Controllers
{
    public class KlijentController : Controller
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
            if (kategorija != null && kategorija != "Sve kategorije" && kategorija != "Sve")
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

        public ActionResult MojaPitanja(string kategorija = null, int strana = 1)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];

            IEnumerable<Pitanje> pitanje = null;
            if (kategorija != null && kategorija != "Sve kategorije" && kategorija != "Sve")
            {
                pitanje = db.Pitanje.Include(p => p.Kanal).Include(p => p.Kategorija).Include(p => p.Korisnik).Where(p => p.Kanal == null && p.Kategorija.Naziv == kategorija && p.Autor == ulogovan.IDKorisnik).OrderByDescending(p => p.DatumPravljenja);
            }
            else
            {
                pitanje = db.Pitanje.Include(p => p.Kanal).Include(p => p.Kategorija).Include(p => p.Korisnik).Where(p => p.Kanal == null).Where(p => p.Autor == ulogovan.IDKorisnik && p.Autor == ulogovan.IDKorisnik).OrderByDescending(p => p.DatumPravljenja);
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

        // GET: Pitanje/Create
        public ActionResult NapraviPitanje()
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            ViewBag.IDKategorija = new SelectList(db.Kategorija, "IDKategorija", "Naziv");
            ViewBag.Autor = ulogovan.IDKorisnik;

            ViewBag.Kategorije = db.Kategorija.ToList();
            return View();
        }

        // POST: Pitanje/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NapraviPitanje([Bind(Include = "IDPitanje,Tekst,Slika,IDKategorija,Autor,DatumPravljenja,DatumZakljucavanja,IDKanal,Naslov,Status")] Pitanje pitanje, HttpPostedFileBase fajl)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            pitanje.IDPitanje = Guid.NewGuid().ToString();
            pitanje.DatumPravljenja = DateTime.Now;
            pitanje.Status = "Otkljucano";
            pitanje.IDKanal = null;
            pitanje.DatumZakljucavanja = null;
            pitanje.Slika = null;
            pitanje.EkstenzijaSlike = null;

            if (ModelState.IsValid)
            {
                if (fajl != null)
                {
                    var ekstenzija = Path.GetExtension(fajl.FileName);
                    if (ekstenzija.ToLower() == ".png" || ekstenzija.ToLower() == ".jpg" || ekstenzija.ToLower() == ".jpeg")
                    {
                        Stream stream = fajl.InputStream;
                        BinaryReader binaryReader = new BinaryReader(stream);
                        byte[] bytes = binaryReader.ReadBytes((int)stream.Length);

                        pitanje.Slika = bytes;
                        pitanje.EkstenzijaSlike = ekstenzija;
                    }
                    db.SaveChanges();
                }
                db.Pitanje.Add(pitanje);
                db.SaveChanges();
                return RedirectToAction("Index", new { message = "Uspešno ste napravili pitanje." });
            }

            ViewBag.IDKanal = new SelectList(db.Kanal, "IDKanal", "Naziv", pitanje.IDKanal);
            ViewBag.IDKategorija = new SelectList(db.Kategorija, "IDKategorija", "Naziv", pitanje.IDKategorija);
            ViewBag.Autor = new SelectList(db.Korisnik, "IDKorisnik", "Ime", pitanje.Autor);
            return View(pitanje);
        }

        public ActionResult ObrisiPitanje(string IDPitanje)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            Pitanje pitanje = db.Pitanje.Find(IDPitanje);

            if (pitanje != null && pitanje.Autor != ulogovan.IDKorisnik)
            {
                return RedirectToAction("Index");
            }


            var odgovoriZaBrisanje = db.Odgovor.Where(o => o.KorenoPitanje == IDPitanje);

            foreach (var odg in odgovoriZaBrisanje)
            {
                var ocenioZaBrisanje = db.Ocenio.Where(o => o.IDOdgovor == odg.IDOdgovor);
                db.Ocenio.RemoveRange(ocenioZaBrisanje);
            }

            db.Odgovor.RemoveRange(odgovoriZaBrisanje);

            db.Pitanje.Remove(pitanje);
            db.SaveChanges();

            return RedirectToAction("Index", new { message = "Uspešno ste obrisali pitanje sa svim njegovim odgovorima." });
        }

        public ActionResult ZakljucajPitanje(string IDPitanje)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            Pitanje pitanje = db.Pitanje.Find(IDPitanje);

            if (pitanje == null || pitanje.Autor != ulogovan.IDKorisnik)
            {
                return RedirectToAction("Index");
            }

            pitanje.Status = "Zakljucano";
            db.SaveChanges();
            return RedirectToAction("Index", new { message = "Uspešno ste zaključali pitanje." });
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
            Korisnik trenutnoStanje = db.Korisnik.Find(ulogovan.IDKorisnik);

            return View(trenutnoStanje);
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

        public ActionResult Kanal()
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Vrednosti kanal = db.Vrednosti.Find("KomunikacioniKanal");

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            Korisnik svezeStanje = db.Korisnik.Find(ulogovan.IDKorisnik);

            Kanal najskorijiKanal = db.Kanal.Where(k => k.IDKorisnik == ulogovan.IDKorisnik).OrderByDescending(k => k.DatumOtvaranja).FirstOrDefault();
            IEnumerable<Pitanje> pitanja = null;
            if (najskorijiKanal != null && najskorijiKanal.Status == "Otvoren")
            {
                pitanja = db.Pitanje.Where(p => p.IDKanal == najskorijiKanal.IDKanal).OrderBy(p => p.DatumPravljenja);
            }

            var tuple = new Tuple<Korisnik, Vrednosti, Kanal, IEnumerable<Pitanje>>(svezeStanje, kanal, najskorijiKanal, pitanja);
            return View(tuple);
        }

        // GET: Kanal/Create
        public ActionResult NapraviKanal()
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            Kanal najskorijiKanal = db.Kanal.Where(k => k.IDKorisnik == ulogovan.IDKorisnik).OrderByDescending(k => k.DatumOtvaranja).FirstOrDefault();
            if (najskorijiKanal != null && najskorijiKanal.Status == "Otvoren")
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        // POST: Kanal/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NapraviKanal([Bind(Include = "IDKanal,Naziv,DatumOtvaranja,IDKorisnik,Status")] Kanal kanal)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            Kanal najskorijiKanal = db.Kanal.Where(k => k.IDKorisnik == ulogovan.IDKorisnik).OrderByDescending(k => k.DatumOtvaranja).FirstOrDefault();
            if (najskorijiKanal != null && najskorijiKanal.Status == "Otvoren")
            {
                return RedirectToAction("Index");
            }

            Vrednosti cenaKanala = db.Vrednosti.Find("KomunikacioniKanal");
            Korisnik korisnikSaTrenutnimBrojemTokena = db.Korisnik.Find(ulogovan.IDKorisnik);

            if (korisnikSaTrenutnimBrojemTokena.BrojTokena < cenaKanala.Cena)
            {
                return RedirectToAction("Index", new { message = "Nemate dovoljno tokena." });
            }

            kanal.IDKanal = Guid.NewGuid().ToString();
            kanal.DatumOtvaranja = DateTime.Now;
            kanal.Status = "Otvoren";
            kanal.IDKorisnik = ulogovan.IDKorisnik;

            if (ModelState.IsValid)
            {
                korisnikSaTrenutnimBrojemTokena.BrojTokena -= (int)(cenaKanala.Cena);
                db.Kanal.Add(kanal);
                db.SaveChanges();
                return RedirectToAction("Kanal", new { message = "Uspešno ste napravili kanal." });
            }

            return View(kanal);
        }

        public ActionResult ZatvoriKanal(string IDKanal)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            Kanal kanal = db.Kanal.Find(IDKanal);

            if (kanal.IDKorisnik != ulogovan.IDKorisnik)
            {
                return RedirectToAction("Index");
            }

            kanal.Status = "Zatvoren";
            db.SaveChanges();
            return RedirectToAction("Index", new { message = "Uspešno ste zatvorili kanal!" });
        }

        public ActionResult Kupi()
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;


            ViewBag.Silver = db.Vrednosti.Find("Silver");
            ViewBag.Gold = db.Vrednosti.Find("Gold");
            ViewBag.Platinum = db.Vrednosti.Find("Platinum");
            return View();
        }

        public ActionResult Narudzbine()
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            var narudzbina = db.Narudzbina.Where(n => n.IDKorisnik == ulogovan.IDKorisnik).Include(n => n.Korisnik);
            return View(narudzbina.ToList());
        }

        public BraintreeHttp.HttpResponse ZavrsenaKupovina(string orderId)
        {
            ActionResult provera = Provera();
            if (provera != null) return null;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            return GetOrderClass.GetOrder(orderId, ulogovan.IDKorisnik);
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
                case "Agent":
                    return RedirectToAction("Index", "Agent");
            }
            return null;
        }
    }
}