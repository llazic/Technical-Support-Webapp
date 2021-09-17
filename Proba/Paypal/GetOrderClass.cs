using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BraintreeHttp;
using Proba.Models;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using System.Globalization;

namespace IEPProjekat.PayPal
{
    public class GetOrderClass
    {    //2. Set up your server to receive a call from the client
         /*
           You can use this method to retrieve an order by passing the order ID.
          */
        private static IEPBazaEntities db = new IEPBazaEntities();
        public static HttpResponse GetOrder(string orderId, int IDKorisnik, bool debug = false)
        {
            OrdersGetRequest request = new OrdersGetRequest(orderId);
            //3. Call PayPal to get the transaction
            var response = Task.Run(() => PayPalClient.client().Execute(request)).Result;
            //4. Save the transaction in your database. Implement logic to save transaction to your database for future reference.
            var result = response.Result<PayPalCheckoutSdk.Orders.Order>();
            float cena = float.Parse(result.PurchaseUnits[0].AmountWithBreakdown.Value, CultureInfo.InvariantCulture);

            Vrednosti iznosPaketa = db.Vrednosti.Where(v => v.Cena == (int)cena && v.Naziv != "KomunikacioniKanal").OrderByDescending(v => v.BrojTokena).FirstOrDefault();
            Korisnik korisnik = db.Korisnik.Find(IDKorisnik);
            Narudzbina narudzbina = new Narudzbina() {
                DatumPravljenja = DateTime.Now,
                KupljeniPaket = iznosPaketa.Naziv,
                IDKorisnik = IDKorisnik,
                Cena = iznosPaketa.Cena,
                DobijenoTokena = iznosPaketa.BrojTokena
            };

            korisnik.BrojTokena = korisnik.BrojTokena + (int)iznosPaketa.BrojTokena;

            db.Narudzbina.Add(narudzbina);
            db.SaveChanges();
            Console.WriteLine("Status: " + result.Status);
            Console.WriteLine("Retrieved Order Status");
            Console.WriteLine("Status: {0}", result.Status);
            Console.WriteLine("Order Id: {0}", result.Id);
            Console.WriteLine("Intent: {0}", result.CheckoutPaymentIntent);
            Console.WriteLine("Links:");
            foreach (LinkDescription link in result.Links)
            {
                Console.WriteLine("\t{0}: {1}\tCall Type: {2}", link.Rel, link.Href, link.Method);
            }
            AmountWithBreakdown amount = result.PurchaseUnits[0].AmountWithBreakdown;
            Console.WriteLine("Total Amount: {0} {1}", amount.CurrencyCode, amount.Value);

            return response;
        }
    }
}