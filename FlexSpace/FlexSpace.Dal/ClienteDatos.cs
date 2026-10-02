using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FlexSpace.DAL
{
    public class ClienteDatos
    {
        private readonly string conexion =
            "Server=localhost;Database=flexspace;Uid=root;Pwd=;";

        public ClienteDTO ObtenerPorId(int id)
        {
            string sql = @"
                SELECT Id, Nombre, Email, TipoCliente, SancionesActivas
                FROM clientes
                WHERE Id = @Id";

            using (MySqlConnection cn = new MySqlConnection(conexion))
            using (MySqlCommand cmd = new MySqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                cn.Open();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new ClienteDTO
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nombre = reader["Nombre"].ToString(),
                        Email = reader["Email"].ToString(),
                        TipoCliente = reader["TipoCliente"].ToString(),
                        SancionesActivas =
                            Convert.ToInt32(reader["SancionesActivas"])
                    };
                }
            }
        }

        public void AumentarSancion(int clienteId)
        {
            string sql = @"
                UPDATE clientes
                SET SancionesActivas = SancionesActivas + 1
                WHERE Id = @Id";

            using (MySqlConnection cn = new MySqlConnection(conexion))
            using (MySqlCommand cmd = new MySqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Id", clienteId);

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<ClienteDTO> ObtenerSancionados()
        {
            List<ClienteDTO> clientes = new List<ClienteDTO>();

            string sql = @"
                SELECT Id, Nombre, Email, TipoCliente, SancionesActivas
                FROM clientes
                WHERE SancionesActivas > 0
                ORDER BY SancionesActivas DESC";

            using (MySqlConnection cn = new MySqlConnection(conexion))
            using (MySqlCommand cmd = new MySqlCommand(sql, cn))
            {
                cn.Open();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientes.Add(new ClienteDTO
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString(),
                            Email = reader["Email"].ToString(),
                            TipoCliente = reader["TipoCliente"].ToString(),
                            SancionesActivas =
                                Convert.ToInt32(reader["SancionesActivas"])
                        });
                    }
                }
            }

            return clientes;
        }
    }
}