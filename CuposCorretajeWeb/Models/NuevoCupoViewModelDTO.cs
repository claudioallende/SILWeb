using CuposCorretajeWeb.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Models
{
    public class NuevoCupoViewModelDTO
    {
        public int Producto { get; set; }
        public IList<RelacionGranoStop> Productos { get; set; }
        public string Compcta { get; set; }
        public string CompradorNombre { get; set; }
        public string Puerto { get; set; }
        public IList<RelacionPuertoStop> Puertos { get; set; }
        public IList<SelectListItem> Centros { get; set; }
        public string Centro { get; set; }
        public string Vendcta { get; set; }
        public string VendedorNombre { get; set; }
        public bool VendcyoBoolValue { get; set; }
        [Display(Name = "Titular de CCPP")]
        public string Cuitsolicitante { get; set; }
        [Display(Name = "Nombre")]
        public string Nomsolicitante { get; set; }
        [Display(Name = "Rte Comercial Venta Secundaria ")]
        public string Cuitintermediario { get; set; }
        [Display(Name = "Nombre")]
        public string Nomintermediario { get; set; }
        [Display(Name = "Rte Comercial Venta Secundaria 2")]
        public string Cuitrtecomercial { get; set; }
        [Display(Name = "Nombre")]
        public string Nomrtecomercial { get; set; }
        [Display(Name = "Corredor Venta Secundaria")]
        public string Cuitcorrcomp { get; set; }
        [Display(Name = "Nombre")]
        public string Nomcorrcomp { get; set; }
        [Display(Name = "Mercado a Termino")]
        public string Cuitmat { get; set; }
        [Display(Name = "Nombre")]
        public string Nommat { get; set; }
        [Display(Name = "Corredor Venta Primaria")]
        public string Cuitcorrvta { get; set; }
        [Display(Name = "Nombre")]
        public string Nomcorrvta { get; set; }
        [Display(Name = "Representante Entregador")]
        public string Cuitrteent { get; set; }
        [Display(Name = "Nombre")]
        public string Nomrteent { get; set; }
        [Display(Name = "Destinatario")]
        [Required]
        public string Cuitdestinatario { get; set; }
        [Display(Name = "Nombre")]
        [Required]
        public string Nomdestinatario { get; set; }
        [Display(Name = "Rte Comercial Productor")]
        public string CuitRteComercialProductor { get; set; }

        [Display(Name = "Nombre")]
        public string NomRteComercialProductor { get; set; }

        [Display(Name = "Rte Comercial Venta Primaria")]
        public string CuitRteComercialVentaPrimaria { get; set; }

        [Display(Name = "Nombre")]
        public string NomRteComercialVentaPrimaria { get; set; }
        public IList<CuposPorDiaDTO> CodigosDias { get; set; }
        public string Observaciones { get; set; }
        public string DetalleCupoCNRT { get; set; }
        public int EstadoCupoCNRT { get; set; }
        public IList<string> AlfasConError { get; set; }
        public ServicioGrano ServicioGrano { get; set; }
        public ServicioPuerto ServicioPuerto { get; set; }
        public ServicioCentro ServicioCentro { get; set; }
        public bool EmparejoTurnos { get; set; }
        public string CentroAnterior { get; set; }

        public NuevoCupoViewModelDTO()
        {
            ServicioGrano = new ServicioGrano();
            ServicioCentro = new ServicioCentro();
            ServicioPuerto = new ServicioPuerto();
        }

        public async Task inicializar()
        {
            if (string.IsNullOrEmpty(this.Compcta))
                this.Compcta = "0";
            if (string.IsNullOrEmpty(this.Vendcta))
                this.Vendcta = "0";
            if (string.IsNullOrEmpty(this.Puerto))
                this.Puerto = "0";
            this.Productos = await ServicioGrano.GetEquivalenciasSilDeGranoStop(this.Producto);
            this.Producto = this.Productos.Where(x => x.ValorPorDefecto == 1).Select(x => x.NroGranoSIL).FirstOrDefault();
            this.Puertos = await ServicioPuerto.GetEquivalenciasSilDePuertoStop(this.Puerto);
            this.Puerto = this.Puertos.Where(x => x.ValorPorDefecto == 1).Select(x => x.NroPuertoSIL).FirstOrDefault().ToString();
            this.Centros = (await ServicioCentro.GetCentros()).Select(x => new SelectListItem() { Text = x.Nombre, Value = x.CodigoCentro }).ToList();
        }

        public async Task<IList<SelectListItem>> GetCentros()
        {
            IList<SelectListItem> ListaCentros = new List<SelectListItem>();
            using (WebServiceSILRespository repo = new WebServiceSILRespository())
            {
                var Centros = await repo.RequestGetAndDeserializeAsync<IList<Centro>>("RelacionGranoSILGranoSTOP", "GetRelaciones");
                ListaCentros = Centros.Select(x => new SelectListItem()
                {
                    Value = x.CodigoCentro,
                    Text = x.Nombre
                })
                .ToList();
            }
            return ListaCentros;
        }

        public async Task<NuevoCupoStopViewModel> DTOtoViewModel()
        {
            await inicializar();
            if (!string.IsNullOrEmpty(this.Cuitsolicitante) || !string.IsNullOrEmpty(this.Cuitintermediario) || !string.IsNullOrEmpty(this.Cuitrtecomercial))
            {
                return new NuevoCupoNominadoStopViewModel(long.Parse(Compcta), long.Parse(Vendcta), long.Parse(Puerto), Producto)
                {
                    Productos = this.Productos,
                    Centro = this.Centro,
                    CompradorNombre = this.CompradorNombre,
                    VendedorNombre = this.VendedorNombre,
                    Puertos = this.Puertos,
                    Cuitsolicitante = !string.IsNullOrEmpty(this.Cuitsolicitante) ? this.Cuitsolicitante.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomsolicitante = this.Nomsolicitante,
                    Cuitintermediario = !string.IsNullOrEmpty(this.Cuitintermediario) ? this.Cuitintermediario.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomintermediario = this.Nomintermediario,
                    Cuitrtecomercial = !string.IsNullOrEmpty(this.Cuitrtecomercial) ? this.Cuitrtecomercial.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomrtecomercial = this.Nomrtecomercial,
                    Cuitcorrcomp = !string.IsNullOrEmpty(this.Cuitcorrcomp) ? this.Cuitcorrcomp.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomcorrcomp = this.Nomcorrcomp,
                    Cuitmat = !string.IsNullOrEmpty(this.Cuitmat) ? this.Cuitmat.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nommat = this.Nommat,
                    Cuitcorrvta = !string.IsNullOrEmpty(this.Cuitcorrvta) ? this.Cuitcorrvta.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomcorrvta = this.Nomcorrvta,
                    Cuitrteent = !string.IsNullOrEmpty(this.Cuitrteent) ? this.Cuitrteent.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomrteent = this.Nomrteent,
                    Cuitdestinatario = !string.IsNullOrEmpty(this.Cuitdestinatario) ? this.Cuitdestinatario.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomdestinatario = this.Nomdestinatario,
                    CuitRteComercialProductor = !string.IsNullOrEmpty(this.CuitRteComercialProductor) ? this.CuitRteComercialProductor.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    NomRteComercialProductor = this.NomRteComercialProductor,
                    CuitRteComercialVentaPrimaria = !string.IsNullOrEmpty(this.CuitRteComercialVentaPrimaria) ? this.CuitRteComercialVentaPrimaria.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    NomRteComercialVentaPrimaria = this.NomRteComercialVentaPrimaria,
                    CodigosDias = this.CodigosDias,
                    Observaciones = this.Observaciones,
                    VendcyoBoolValue = this.VendcyoBoolValue,
                    Centros = this.Centros
                };
            }
            else
            {
                return new NuevoCupoNoNominadoStopViewModel(long.Parse(Compcta), long.Parse(Vendcta), long.Parse(Puerto), Producto)
                {
                    Productos = this.Productos,
                    Centro = this.Centro,
                    CompradorNombre = this.CompradorNombre,
                    VendedorNombre = this.VendedorNombre,
                    Puertos = this.Puertos,
                    Cuitsolicitante = !string.IsNullOrEmpty(this.Cuitsolicitante) ? this.Cuitsolicitante.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomsolicitante = this.Nomsolicitante,
                    Cuitintermediario = !string.IsNullOrEmpty(this.Cuitintermediario) ? this.Cuitintermediario.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomintermediario = this.Nomintermediario,
                    Cuitrtecomercial = !string.IsNullOrEmpty(this.Cuitrtecomercial) ? this.Cuitrtecomercial.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomrtecomercial = this.Nomrtecomercial,
                    Cuitcorrcomp = !string.IsNullOrEmpty(this.Cuitcorrcomp) ? this.Cuitcorrcomp.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomcorrcomp = this.Nomcorrcomp,
                    Cuitmat = !string.IsNullOrEmpty(this.Cuitmat) ? this.Cuitmat.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nommat = this.Nommat,
                    Cuitcorrvta = !string.IsNullOrEmpty(this.Cuitcorrvta) ? this.Cuitcorrvta.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomcorrvta = this.Nomcorrvta,
                    Cuitrteent = !string.IsNullOrEmpty(this.Cuitrteent) ? this.Cuitrteent.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomrteent = this.Nomrteent,
                    Cuitdestinatario = !string.IsNullOrEmpty(this.Cuitdestinatario) ? this.Cuitdestinatario.Replace("-", "").Insert(10, "-").Insert(2, "-") : null,
                    Nomdestinatario = this.Nomdestinatario,
                    CodigosDias = this.CodigosDias,
                    Observaciones = this.Observaciones,
                    VendcyoBoolValue = this.VendcyoBoolValue,
                    Centros = this.Centros
                };
            }
        }
    }
}