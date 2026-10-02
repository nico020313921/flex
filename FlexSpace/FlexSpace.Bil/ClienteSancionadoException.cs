using System;

namespace FlexSpace.BLL
{
    public class ClienteSancionadoException : Exception
    {
        public ClienteSancionadoException()
            : base("El cliente tiene 3 o más sanciones activas y no puede realizar reservas.")
        {
        }
    }
}