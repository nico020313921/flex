using System;

namespace FlexSpace.DAL
{
    public class ClienteDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string TipoCliente { get; set; }
        public int SancionesActivas { get; set; }
    }

    public class PuestoDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string TipoPuesto { get; set; }
        public decimal TarifaBasePorHora { get; set; }
    }

    public class ReservaDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int PuestoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; }
        public decimal CostoTotal { get; set; }
    }
}