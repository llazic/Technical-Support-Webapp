//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Proba.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Narudzbina
    {
        public int IDNarudzbina { get; set; }
        public System.DateTime DatumPravljenja { get; set; }
        public string KupljeniPaket { get; set; }
        public int IDKorisnik { get; set; }
        public Nullable<int> Cena { get; set; }
        public Nullable<int> DobijenoTokena { get; set; }
    
        public virtual Korisnik Korisnik { get; set; }
    }
}
