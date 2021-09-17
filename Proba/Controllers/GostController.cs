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
    public class GostController : Controller
    {
        private IEPBazaEntities db = new IEPBazaEntities();

        // GET: Gost
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

        public static int BrojPitanjaPoStrani = 3;

        // GET: Pitanje
        public ActionResult Pitanja(string kategorija = null, int strana = 1)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            IEnumerable<Pitanje> pitanje = null;
            if (kategorija != null && kategorija != "Sve")
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

            return View(pitanje);
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

        public static byte[] ZnakPitanja()
        {
            string hex = "89504E470D0A1A0A0000000D49484452000000C8000000C80806000000AD58AE9E0000000473424954080808087C086488000000097048597300000B1300000B1301009A9C1800000AA349444154789CEDDD7DCC57651DC7F1F7FDC0F333422160C84D9122589044B3A1D47454BA66426EADB6FE6A2CD6AA3F5A73D51FAE07B7723DB09CCD4DCD555BB3722E371DF36196582C8D0252282B318430831404A11B81FEB8EE3BD9CDEFFC1EAE739DFBFBBDCEEFF3DABEF30FDDFC9EEB5C9FFB9CDF79B80E888888888888888888888888888888C81B7AAC1BA8A15EE00260113000BC05980BCC01CE036602938149C078A01FE803CE00A780D78141E0187014781538041C1CAA03C00B43B507D8079C1E952DEB420A4839BDC05260157019B01C584298FCA3E504F0376037B01DD8016C035E1CC51E6A4B01E9DC00F001602DB01A9861DB4EA13DC056E071E011E039C2514A24B98B809B809D84499663ED016E23047B5CD2D191AE3405D8003C85FDE44E5D47801F11C2D29F6AC0A43B5C087C973089AC27F268D401E066C2A9A348A145C0DD84AB48D693D6AA1E00DE877E9BCA596603B7D2DDC11859DB80EB5050BA5A1FF019E030F613D26BFD01B80605A5EB2C019EC47E02E6520F0397468DB464A50FF802E1E69AF5A4CBAD4E019B80A91D8FBA646136E18699F544CBBDF601D77638F6E2DC4A602FF693AB4E75073A9AD4C23A744A5555FD1D58D1FEAE106F3E4D78D2D57A22D5B9FE4B78E240327323F693A79B6A13E1228864E0F3D84F986EACFB81096DEC1F31F429EC274A37D723C0C4967B494C5C8D1E19F1508F11DE9614472E025EC17E72A842DD4B0D7E9364BF01432611FE6ACDB36E44FEEF62601AB0D9BA11813BB1FF8BA96A5C9F68B2DF6414DC80FD245015D76BC025857BCFB9DC1F639E05FC99B09C8EF8F547E03D84E58CB292FB6F905B81CBAD9B9096CE1FFAE763A65D44C8F9087205F06BEB26A46D838435C4FE6ADD48277AAD1B88D403DC62DD8474642CE171141905EBB0FFF1A98AAB35E7EE4EBF723CC5EA039E01DE6EDD8844D9025C49088B7B3906641DF00BEB264A38053C0BFC85F012D74B8485234E0CFDBB3EC2037F530957E7E6020B094F0AD4E505A535E8F763257A80DF617F9AD0693D0D7C8DF09733F641BE1E42503E0EFC90B0D2BBF576C5D6BD9163202DACC67EE7B65B2781BB082BBE57610CE1DDF0071D6C6BA7758AF0590849EC27D8EFDC76EA1EC212A6A3E55D845316EBEDEEA4BE54C94874B1E9C071EC776CB33A087CB8AA0168A107D8483EEFDFEF22CFDFC06E6DC47EA736AB6718DDA3469155E4F3FB64594563D095B660BF438B6A3BBE9E07BB943CDE8DB9B1AA01E83673F0BB3AC9DEA1FEBC598BDF311BAE2D956D7D97D980FDCE6C5427098BD279750BF663D46AFCF4FE7A025E2F65DE54E136A73081F0814FEB716A566BAADAF86E3196F03964EB1D39B2FE411E0B135C83FD5835AB2F57B7E9E5E5F034EF4A46F7B3CAEDFA0EE192AA770FE2FB3D8C77583790BBAF60FF576E649D22BCCD988B2BB11FB3A2DA55E1767785CDD8EFC491F554A55B9C5E0FF07BECC7AD51BD4E388D76C9FB29560F709975130DECB06EA0436780DBAD9B28D087E3E59ABC076401BE6EC00DDB67DD40849FE377D104B70F2E7A0F88C7A307C031EB0622BC023C6ADD44010524D252EB060AE4BA1ACC43D60D14F0789600F80F88D7D76AA7593710E909EB060ACCB46EA0880212E7FCD6FF894B3B09578DBC9961DD4011CF01E901165B375160817503910609DF15F4669C7503453C0764063EEFA003BCD5BA8112F65A37D080EE8344707B6D1C980F4CB66E22D2BFAC1B68400189E03920E0F7F4AF95C3D60D3470DABA81220A483CAF17105AF178B3D0E38503C07740DC5E1B1F3260DD40A47EEB061A707BE3D57340BCDF6BB8C0BA814853AC1B68E0A87503453C0764BA75032DCCB66E2092C77B3807AD1B28A280C4CBF55DEA85D60D34F0927503453C2FDCD587EFFE865F9CCAC944E055FCFD61BC0AA70F527AFCC1362CB7C9978395F80B07C0F3D60D14F13858529DABAC1B68601005449CB8CEBA810676E3F86C4101E91ECBF1F97ECD76EB069A5140BAC746EB060A6CB56E40642E7E3F8B704985DB2DD296DBB10F42A3FA27BE2FE54B17584EF8116C1D8646755785DB2DD2D218601BF64128AA6BABDB7491D6BE8E7D088AEA108E5F9492FAFB20BE3FA0F3FDEA365DA4B9A5F8FF04DB92CAB65EA4898584A551AD03D0AC3657B6F5224D2C223CD7641D8056B5A69ACD1729B6027811FBC9DFAA1EAE6A00448AAC27BCD76D3DF95BD569E09D158D81C839FAF1FF25DBB3EB07D50C83C8B9E600BFC27ED2B75BFBF1BF1087D4C47B09CF31594FFA4EEAEA4A4642E42C3DC0E78093D84FF84EEADB550C86C8D926033FC57EB2775ABF458F9448C516034F633FD93BAD1708BF95442A733D7004FBC9DE691D069655301E2240B884FB4DEC277A4CBD06AC4E3F2422C12CC2426AD6133DA64EA02B5652A165C01EEC277A4C1D07D6A61F1291E04384A541AD277A4C1D05DE9F7E4844820DF87D77BC551D0256A51F129170F3EFABD84FF2D87A9E7CBFB625CEF502B7613FC9636B1B3EBF372235D04758F6C67A92C7D6FDE4FBA55F71AE17B813FB491E5BDF22045C24B91EE07BD84FF2983A017C32FD9088BCE18BD84FF498DA8FAE5449C5AEC77EA2C7D413E8A143A9D8C5849B69D693BDD3DA4458C254A432E3819DD84FF64EEA18F0B12A064364A4DC9ECADD8D563E9451B28CBC1E21B9079852C9488834B019FB49DF4E9D043E8B3E6823A36815F613BF9DDA0F5C5ED1188814FA31F693BF553D0ABCA9AA0110293299F0029175009AD5CDE8911131B21EFB0014D571E0A3D56DBA486B5E1F467C19FDDE10079EC33E0C8DC2B1A2CA8D1669C75CECC3D0E8B44A470E71E123D8076264DD50E916D754AF750335B5DCBA811136013FB36E4264D87DD81F31866B17E1614911379EC53E18C37545C5DB2AD29131F8F97EC77D156FAB48C716611F8CE1D207334BD28FF4F406AC1B18F238B0DDBA89DC2920E92DB06E60C8DDD60DD4810292DE7CEB06082F68FDD2BA893A5040D29B67DD00F024F01FEB26EA400149CFC35AB55BAD1BA80B0524BD375B3700ECB06EA02E1490F4665B3740589544125040D29B65DD00E1517B49402B58A4358EB0C0B3A513C044C28D4229494790B4665837001C40E1484601496B9A7503C0BFAD1BA81305242D0F0179D9BA813A5140D29A6ADD0070C4BA813A5140D2F2F0CDBE63D60DD4890292968780585F45AB150524AD89D60D0083D60DD449BF750335F327E01BC63DFCC6F8FF2F22222222222222222222222222222222222222222222222262460BC7A5B31EB8C3BA8911DE4DF85EA244D21B85E92CC1C7B23F679B8702528ADE494F67B175030DCCB56E20770A483A6FB36EA00105A42405248D1E7C1E413C7CCC276B0A481AE701D3AD9B68404790921490343C9E5E8102529A0292860252530A481A1E7F7F400888EE7595A080A4E1F50832017FF766B2A280A4E13520A0D3AC521490F2BC5EE21DA68094A0809437079864DD4413BA17528202529EE7D32BD011A41405A43CCFA757A08094A28094A723488D2920E5292035A68094A753AC1AD35DD6727A095F951D6FDD48138384FECE583792231D41CA998FEF70008C05665A37912B05A41CEFA757C3742F249202528EF71FE8C3F43B249202528E0252730A4839B99C622920911490727404A93905245E3F3060DD449B1490480A48BC0BC967E13D05249202122F97D32B5040A22920F1720AC81CB4AFA368D0E2E572050B600C61ED2EE99002122FA72308E8342B8A02124F01E9020A489C71C002EB263AA480445040E20C90DFD829201172DBC95EE4767A050A8888888888888888888888888888888894F63F04F3A260799EB4880000000049454E44AE426082";
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public ActionResult PregledOdgovora(int? id)
        {
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

        public ActionResult Login()
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            return View();
        }

        [HttpPost]
        public ActionResult Login(Korisnik korisnik)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            string kriptovanaLozinka = Encryption.EncryptDecrypt(korisnik.Lozinka);
            ulogovan = db.Korisnik.Where(k => k.Email == korisnik.Email && k.Lozinka == kriptovanaLozinka && k.Status == "Aktivan").SingleOrDefault();

            if (ulogovan == null)
            {
                ViewBag.Message = "Email i lozinka nisu ispravno uneti.";
                return View();
            }
            else
            {
                Session["Ulogovan"] = ulogovan;

                switch (ulogovan.Tip)
                {
                    case "Klijent":
                        return RedirectToAction("Index", "Klijent", new { message = "Dobrodošli " + ulogovan.Ime + "." });
                    case "Admin":
                        return RedirectToAction("Index", "Admin", new { message = "Dobrodošli " + ulogovan.Ime + "." });
                    case "Agent":
                        return RedirectToAction("Index", "Agent", new { message = "Dobrodošli " + ulogovan.Ime + "." });
                }
            }

            return View(); //ne bi trebalo do ovde da dodje
        }

        // GET: Korisnik/Create
        public ActionResult Registracija()
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
        public ActionResult Registracija([Bind(Include = "IDKorisnik,Ime,Prezime,Email,Lozinka,BrojTokena,Status,Tip")] Korisnik korisnik)
        {
            ActionResult provera = Provera();
            if (provera != null) return provera;

            korisnik.BrojTokena = 0;
            korisnik.Status = "Aktivan";
            korisnik.Tip = "Klijent";
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
                return RedirectToAction("Index", new { message = "Uspešno ste se registrovali! Možete se ulogovati." });
            }

            return View();
        }

        private ActionResult Provera()
        {
            Korisnik ulogovan = (Korisnik)Session["Ulogovan"];
            if (ulogovan != null)
            {
                switch (ulogovan.Tip)
                {
                    case "Klijent":
                        return RedirectToAction("Index", "Klijent");
                    case "Admin":
                        return RedirectToAction("Index", "Admin");
                    case "Agent":
                        return RedirectToAction("Index", "Agent");
                }
            }
            return null;
        }
    }
}