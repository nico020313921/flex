using System;
using System.Collections.Generic;
using FlexSpace.DAL;

namespace FlexSpace.BLL
{
    public class ReservaNegocio
    {
        private ClienteDatos clienteDatos;
        private PuestoDatos puestoDatos;
        private ReservaDatos reservaDatos;

        public ReservaNegocio()
        {
            clienteDatos = new ClienteDatos();
            puestoDatos = new PuestoDatos();
            reservaDatos = new ReservaDatos();
        }

        public Reserva CalcularReserva(
            int clienteId,
            int puestoId,
            DateTime inicio,
            DateTime fin)
        {
            ValidarFechas(inicio, fin);

            ClienteDTO clienteDTO =
                clienteDatos.ObtenerPorId(clienteId);

            if (clienteDTO == null)
                throw new Exception("El cliente no existe.");

            PuestoDTO puestoDTO =
                puestoDatos.ObtenerPorId(puestoId);

            if (puestoDTO == null)
                throw new Exception("El puesto no existe.");

            if (clienteDTO.SancionesActivas >= 3)
                throw new ClienteSancionadoException();

            if (reservaDatos.ExisteSolapamiento(
                puestoId, inicio, fin))
            {
                throw new Exception(
                    "El puesto ya está reservado en ese horario.");
            }

            Cliente cliente = ConvertirCliente(clienteDTO);
            Puesto puesto = ConvertirPuesto(puestoDTO);

            decimal costo = CalcularCosto(
                cliente,
                puesto,
                inicio,
                fin);

            return new Reserva
            {
                ClienteId = clienteId,
                PuestoId = puestoId,
                FechaInicio = inicio,
                FechaFin = fin,
                Estado = "Confirmada",
                CostoTotal = costo
            };
        }

        public int RegistrarReserva(Reserva reserva)
        {
            ReservaDTO dto = new ReservaDTO
            {
                ClienteId = reserva.ClienteId,
                PuestoId = reserva.PuestoId,
                FechaInicio = reserva.FechaInicio,
                FechaFin = reserva.FechaFin,
                Estado = reserva.Estado,
                CostoTotal = reserva.CostoTotal
            };

            return reservaDatos.Insertar(dto);
        }

        public void CancelarReserva(int reservaId)
        {
            ReservaDTO reserva =
                reservaDatos.ObtenerPorId(reservaId);

            if (reserva == null)
                throw new Exception("La reserva no existe.");

            if (reserva.Estado != "Confirmada")
                throw new Exception(
                    "La reserva no está confirmada.");

            DateTime ahora = DateTime.Now;

            TimeSpan diferencia =
                reserva.FechaInicio - ahora;

            reservaDatos.Cancelar(reservaId);

            if (diferencia.TotalHours < 2)
            {
                clienteDatos.AumentarSancion(
                    reserva.ClienteId);
            }
        }

        public List<Reserva> ConsultarPorPuesto(
            string codigoPuesto)
        {
            List<ReservaDTO> datos =
                reservaDatos.ObtenerActivasPorPuesto(
                    codigoPuesto);

            List<Reserva> resultado =
                new List<Reserva>();

            foreach (ReservaDTO dto in datos)
            {
                resultado.Add(new Reserva
                {
                    Id = dto.Id,
                    ClienteId = dto.ClienteId,
                    PuestoId = dto.PuestoId,
                    FechaInicio = dto.FechaInicio,
                    FechaFin = dto.FechaFin,
                    Estado = dto.Estado,
                    CostoTotal = dto.CostoTotal
                });
            }

            return resultado;
        }

        public List<Cliente> ObtenerClientesSancionados()
        {
            List<ClienteDTO> datos =
                clienteDatos.ObtenerSancionados();

            List<Cliente> resultado =
                new List<Cliente>();

            foreach (ClienteDTO dto in datos)
            {
                resultado.Add(ConvertirCliente(dto));
            }

            return resultado;
        }

        private decimal CalcularCosto(
            Cliente cliente,
            Puesto puesto,
            DateTime inicio,
            DateTime fin)
        {
            decimal horas =
                (decimal)(fin - inicio).TotalHours;

            decimal subtotal =
                horas * puesto.TarifaBasePorHora;

            // Si tiene sanciones, pierde los descuentos
            // y se aplica 20% sobre la tarifa base.
            if (cliente.SancionesActivas > 0)
            {
                return subtotal * 1.20m;
            }

            decimal total = subtotal;

            // 15% por fin de semana
            if (IncluyeFinDeSemana(inicio, fin))
            {
                total *= 1.15m;
            }

            // 10% de descuento por 5 horas o más
            if (horas >= 5)
            {
                total *= 0.90m;
            }

            // 5% de descuento VIP
            if (cliente.TipoCliente == "VIP")
            {
                total *= 0.95m;
            }

            return Math.Round(total, 2);
        }

        private bool IncluyeFinDeSemana(
            DateTime inicio,
            DateTime fin)
        {
            DateTime fecha = inicio.Date;

            while (fecha <= fin.Date)
            {
                if (fecha.DayOfWeek == DayOfWeek.Saturday ||
                    fecha.DayOfWeek == DayOfWeek.Sunday)
                {
                    return true;
                }

                fecha = fecha.AddDays(1);
            }

            return false;
        }

        private void ValidarFechas(
            DateTime inicio,
            DateTime fin)
        {
            if (fin <= inicio)
            {
                throw new Exception(
                    "La fecha de fin debe ser posterior a la fecha de inicio.");
            }
        }

        private Cliente ConvertirCliente(
            ClienteDTO dto)
        {
            return new Cliente
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Email = dto.Email,
                TipoCliente = dto.TipoCliente,
                SancionesActivas = dto.SancionesActivas
            };
        }

        private Puesto ConvertirPuesto(
            PuestoDTO dto)
        {
            return new Puesto
            {
                Id = dto.Id,
                Codigo = dto.Codigo,
                TipoPuesto = dto.TipoPuesto,
                TarifaBasePorHora =
                    dto.TarifaBasePorHora
            };
        }
    }
}