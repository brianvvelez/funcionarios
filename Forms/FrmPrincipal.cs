using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GestionFuncionarios.DAO;
using GestionFuncionarios.Exceptions;
using GestionFuncionarios.Models;

namespace GestionFuncionarios.Forms
{
    /// <summary>
    /// Formulario principal: muestra el listado de funcionarios y
    /// expone las acciones del CRUD (crear, editar, eliminar, refrescar).
    /// </summary>
    public class FrmPrincipal : Form
    {
        private readonly IFuncionarioDAO _funcionarioDAO = new FuncionarioDAO();

        private DataGridView dgvFuncionarios = null!;
        private Button       btnNuevo        = null!;
        private Button       btnEditar       = null!;
        private Button       btnEliminar     = null!;
        private Button       btnRefrescar    = null!;
        private Label        lblTotal        = null!;

        public FrmPrincipal()
        {
            ConstruirInterfaz();
            CargarFuncionarios();
        }

        // -------------------------------------------------------------
        // Construcción de la interfaz
        // -------------------------------------------------------------
        private void ConstruirInterfaz()
        {
            Text          = "Gestión de Funcionarios";
            Width         = 1024;
            Height        = 600;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize   = new Size(900, 500);

            var lblTitulo = new Label
            {
                Text      = "Funcionarios",
                Font      = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock      = DockStyle.Top,
                Height    = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(15, 10, 0, 0)
            };

            // Panel inferior con botones
            var panelBotones = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 60,
                FlowDirection = FlowDirection.LeftToRight,
                Padding       = new Padding(10),
                BackColor     = SystemColors.Control
            };

            btnNuevo     = CrearBoton("Nuevo",     Color.SeaGreen,  BtnNuevo_Click);
            btnEditar    = CrearBoton("Editar",    Color.SteelBlue, BtnEditar_Click);
            btnEliminar  = CrearBoton("Eliminar",  Color.Firebrick, BtnEliminar_Click);
            btnRefrescar = CrearBoton("Refrescar", Color.DarkSlateGray, BtnRefrescar_Click);

            panelBotones.Controls.Add(btnNuevo);
            panelBotones.Controls.Add(btnEditar);
            panelBotones.Controls.Add(btnEliminar);
            panelBotones.Controls.Add(btnRefrescar);

            lblTotal = new Label
            {
                AutoSize  = false,
                Width     = 300,
                Height    = 35,
                TextAlign = ContentAlignment.MiddleRight,
                Margin    = new Padding(20, 5, 10, 0),
                Font      = new Font("Segoe UI", 9, FontStyle.Italic)
            };
            panelBotones.Controls.Add(lblTotal);

            // DataGridView
            dgvFuncionarios = new DataGridView
            {
                Dock                       = DockStyle.Fill,
                ReadOnly                   = true,
                AllowUserToAddRows         = false,
                AllowUserToDeleteRows      = false,
                AllowUserToResizeRows      = false,
                AutoGenerateColumns        = false,
                MultiSelect                = false,
                SelectionMode              = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible          = false,
                BackgroundColor            = Color.White,
                AutoSizeColumnsMode        = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvFuncionarios.CellDoubleClick += (_, _) => EditarSeleccionado();

            ConfigurarColumnas();

            Controls.Add(dgvFuncionarios);
            Controls.Add(panelBotones);
            Controls.Add(lblTitulo);
        }

        private static Button CrearBoton(string texto, Color color, EventHandler handler)
        {
            var b = new Button
            {
                Text      = texto,
                Width     = 110,
                Height    = 35,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Margin    = new Padding(5)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += handler;
            return b;
        }

        private void ConfigurarColumnas()
        {
            dgvFuncionarios.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = "IdFuncionario",
                DataPropertyName = "IdFuncionario",
                HeaderText       = "ID",
                Width            = 50,
                AutoSizeMode     = DataGridViewAutoSizeColumnMode.None
            });
            dgvFuncionarios.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = "TipoDocumentoDescripcion",
                DataPropertyName = "TipoDocumentoDescripcion",
                HeaderText       = "Tipo Doc."
            });
            dgvFuncionarios.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = "NumeroDocumento",
                DataPropertyName = "NumeroDocumento",
                HeaderText       = "Documento"
            });
            dgvFuncionarios.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = "NombreCompleto",
                DataPropertyName = "NombreCompleto",
                HeaderText       = "Nombre completo"
            });
            dgvFuncionarios.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = "Email",
                DataPropertyName = "Email",
                HeaderText       = "Correo"
            });
            dgvFuncionarios.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = "CargoNombre",
                DataPropertyName = "CargoNombre",
                HeaderText       = "Cargo"
            });
            dgvFuncionarios.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = "DependenciaNombre",
                DataPropertyName = "DependenciaNombre",
                HeaderText       = "Dependencia"
            });
            dgvFuncionarios.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = "Salario",
                DataPropertyName = "Salario",
                HeaderText       = "Salario",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dgvFuncionarios.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name             = "Activo",
                DataPropertyName = "Activo",
                HeaderText       = "Activo",
                Width            = 60,
                AutoSizeMode     = DataGridViewAutoSizeColumnMode.None
            });
        }

        // -------------------------------------------------------------
        // Lógica
        // -------------------------------------------------------------
        private void CargarFuncionarios()
        {
            try
            {
                List<Funcionario> lista = _funcionarioDAO.Listar();
                dgvFuncionarios.DataSource = null;
                dgvFuncionarios.DataSource = lista;
                lblTotal.Text = $"Total: {lista.Count} funcionario(s)";
            }
            catch (DAOException ex)
            {
                MostrarError(ex);
            }
        }

        private Funcionario? ObtenerFuncionarioSeleccionado()
        {
            if (dgvFuncionarios.CurrentRow?.DataBoundItem is Funcionario f)
                return f;
            return null;
        }

        private void EditarSeleccionado()
        {
            var f = ObtenerFuncionarioSeleccionado();
            if (f == null)
            {
                MessageBox.Show("Seleccione un funcionario.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var frm = new FrmFuncionarioEditor(f);
            if (frm.ShowDialog(this) == DialogResult.OK)
                CargarFuncionarios();
        }

        // -------------------------------------------------------------
        // Eventos
        // -------------------------------------------------------------
        private void BtnNuevo_Click(object? sender, EventArgs e)
        {
            using var frm = new FrmFuncionarioEditor(null);
            if (frm.ShowDialog(this) == DialogResult.OK)
                CargarFuncionarios();
        }

        private void BtnEditar_Click(object? sender, EventArgs e) => EditarSeleccionado();

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            var f = ObtenerFuncionarioSeleccionado();
            if (f == null)
            {
                MessageBox.Show("Seleccione un funcionario.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var resp = MessageBox.Show(
                $"¿Está seguro de eliminar al funcionario '{f.NombreCompleto}'?\n\nEsta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (resp != DialogResult.Yes) return;

            try
            {
                bool ok = _funcionarioDAO.Eliminar(f.IdFuncionario);
                if (ok)
                {
                    MessageBox.Show("Funcionario eliminado correctamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarFuncionarios();
                }
                else
                {
                    MessageBox.Show("No se encontró el registro a eliminar.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (DAOException ex)
            {
                MostrarError(ex);
            }
        }

        private void BtnRefrescar_Click(object? sender, EventArgs e) => CargarFuncionarios();

        private static void MostrarError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
