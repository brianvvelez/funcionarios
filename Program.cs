using System;
using System.Windows.Forms;
using GestionFuncionarios.Forms;
using GestionFuncionarios.Util;

namespace GestionFuncionarios
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Verificación temprana de la conexión a la base de datos.
            if (!ConexionDB.ProbarConexion(out string mensaje))
            {
                MessageBox.Show(
                    "No se pudo conectar a la base de datos.\n\nDetalle: " + mensaje +
                    "\n\nRevise la cadena de conexión en App.config.",
                    "Error de conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FrmPrincipal());
        }
    }
}
